using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

using ClosedXML.Excel;

namespace Arad.Sms.Core.Bulk.Win;

public partial class Form : System.Windows.Forms.Form
{
    private List<BulkMessage> _bulkMessages = new();
    private List<string> _blackList = new();
    private bool _stopClick;
    private readonly UserInfo _userInfo;

    public Form()
    {
        InitializeComponent();

        _userInfo = Helpers.UserInfo();

        lblName.Text = $"{_userInfo.FirstName} {_userInfo.LastName}";
        lblCredit.Text = $"{_userInfo.Credit:#,0.####}";
        txtTake.Text = _userInfo.Mps.ToString(CultureInfo.CurrentCulture);

        foreach (string senderId in _userInfo.SenderIds)
        {
            cmbSendNumberTest.Items.Add(senderId);
            cmbSendNumber.Items.Add(senderId);
        }

        DateTime dateTime = DateTime.Now;
        PersianCalendar calendar = new();
        txtDate.Text = $"{calendar.GetYear(dateTime):0000}/{calendar.GetMonth(dateTime):00}/{calendar.GetDayOfMonth(dateTime):00}";
        txtStartTime.Text = $"{dateTime.Hour:00}:{dateTime.Minute:00}";
        btnBlackList.Enabled = false;
    }

    private void RefreshUserInfo()
    {
        try
        {
            UserInfo userInfo = Helpers.UserInfo();

            lblName.Text = $"{userInfo.FirstName} {userInfo.LastName}";
            lblCredit.Text = $"{userInfo.Credit:#,0.##}";
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void btnSentTest_Click(object sender, EventArgs e)
    {

        string[] reviverTest = txtReciverTest.Text.Split(';', ',');

        List<BulkMessage> bulkMessages = reviverTest
            .Select(dest => new BulkMessage { SourceAddress = cmbSendNumberTest.SelectedItem.ToString(), DestinationAddress = Helpers.CorrectMobileNumber(dest), MessageText = txtMessageTest.Text })
            .ToList();

        List<string> result = Helpers.SendSms(bulkMessages);

        List<ResultSend> resultSends = new();
        resultSends.AddRange(bulkMessages.Select((t, i) => new ResultSend { DestinationAddress = t.DestinationAddress, Status = GetStatus(result[i]) }));

        if (result.Count == 1)
        {
            MessageBox.Show("پیام ارسال شد.", "ارسال", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);
            RefreshUserInfo();
        }
        else
        {
            MessageBox.Show("خطا در ارسال پیام.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);
        }

        string path = $"{Environment.CurrentDirectory}\\Log";

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        using (StreamWriter sw = File.CreateText($"{path}\\Test {cmbSendNumberTest.SelectedItem} {DateTime.Now:yyyyMMdd hhmmss}.txt"))
        {
            sw.WriteLine($"==== Send Count : {resultSends.Count(r => r.Status == SmsSendStatus.Sent)} - Error Count : {resultSends.Count(r => r.Status != SmsSendStatus.Sent)}");
            sw.WriteLine("=========================================================================");

            foreach (ResultSend resultSend in resultSends)
            {
                sw.WriteLine($"{resultSend.DestinationAddress}   {resultSend.Status}");
            }
        }

        using (StreamWriter sw = File.CreateText($"{path}\\Test Error {cmbSendNumberTest.SelectedItem} {DateTime.Now:yyyyMMdd hhmmss}.txt"))
        {
            sw.WriteLine($"==== Count : {resultSends.Count(r => r.Status != SmsSendStatus.Sent)}");
            sw.WriteLine("=========================================================================");

            foreach (ResultSend resultSend in resultSends.Where(r => r.Status != SmsSendStatus.Sent))
            {
                sw.WriteLine($"{resultSend.DestinationAddress}   {resultSend.Status}");
            }
        }

        using (StreamWriter sw = File.CreateText($"{path}\\Test Send {cmbSendNumberTest.SelectedItem} {DateTime.Now:yyyyMMdd hhmmss}.txt"))
        {
            sw.WriteLine($"==== Count : {resultSends.Count(r => r.Status == SmsSendStatus.Sent)}");
            sw.WriteLine("=========================================================================");

            foreach (ResultSend resultSend in resultSends.Where(r => r.Status == SmsSendStatus.Sent))
            {
                sw.WriteLine($"{resultSend.DestinationAddress}   {resultSend.Status}");
            }
        }
    }

    private void btnSelectedFile_Click(object sender, EventArgs e)
    {
        _bulkMessages.Clear();
        txtFromRow.Text = "0";
        txtToRow.Text = "0";

        if (!rbtnSendCorresponding.Checked && string.IsNullOrEmpty(txtMessage.Text))
        {
            MessageBox.Show("متن پیام را وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);

            return;
        }

        if (cmbSendNumber.SelectedItem == null)
        {
            MessageBox.Show("شماره ارسال را انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);

            return;
        }

        openFileDialog1.Filter = "Excel Files|*.xls;*.xlsx|txt files (*.txt)|*.txt";
        openFileDialog1.Title = "انتخاب فایل";
        DialogResult result = openFileDialog1.ShowDialog();
        txtLog.Clear();
        int paramCount = 0;
        progressBar1.Value = 0;
        Application.DoEvents();

        if (result == DialogResult.OK)
        {
            txtLog.AppendText($"در حال خواندن فایل ... {Environment.NewLine}{Environment.NewLine}");
            progressBar1.Maximum = 100;
            progressBar1.Value = 20;
            panel1.Enabled = false;
            btnSend.Enabled = false;
            btnStop.Enabled = false;

            txtFilePath.Text = openFileDialog1.FileName;
            string extension = Path.GetExtension(openFileDialog1.FileName);
            _bulkMessages = new();

            if (rbtnDynamic.Checked)
            {
                try
                {
                    int[] matches = Regex.Matches(txtMessage.Text, @"{\d+}").Select(x => x.Value.Replace("{", "").Replace("}", "")).Select(x => Convert.ToInt32(x)).OrderBy(x => x).ToArray();
                    paramCount = matches.Length;

                    if (matches.Any())
                    {
                        if (matches.Any(item => matches[item] != item))
                        {
                            MessageBox.Show("الگو نادرست نمی باشد.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);

                            return;
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("الگو نادرست نمی باشد.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);

                    return;
                }
            }

            if (extension is ".txt")
            {
                ReadText();
            }
            else
            {
                ReadExcel();
            }

            panel1.Enabled = true;
            btnSend.Enabled = true;
            btnStop.Enabled = true;
            int temp = _bulkMessages.Count;

            if (rbtnNormal.Checked)
            {
                _bulkMessages = _bulkMessages.GroupBy(b => b.DestinationAddress)
                    .Select(b => new BulkMessage { DestinationAddress = b.Key, MessageText = b.First().MessageText, SourceAddress = b.First().SourceAddress })
                    .ToList();
            }

            txtToRow.Text = _bulkMessages.Count.ToString();

            txtLog.AppendText($"تعداد : {_bulkMessages.Count} ردیف خوانده شد{Environment.NewLine}");
            txtLog.AppendText($"تعداد شماره های تکراری : {temp - _bulkMessages.Count} ");
            btnSearch.Enabled = true;
        }

        void ReadExcel()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Application.DoEvents();
            using XLWorkbook wb = new(openFileDialog1.FileName);

            IXLWorksheet ws = wb.Worksheets.FirstOrDefault();

            if (ws is null)
            {
                return;
            }

            Application.DoEvents();
            progressBar1.Value = 0;
            IXLRange range = ws.RangeUsed();
            progressBar1.Maximum = range.RowCount();

            if (rbtnSendCorresponding.Checked && range.ColumnCount() != 2)
            {
                MessageBox.Show("فایل دارای خطا است.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);

                return;
            }

            if (rbtnDynamic.Checked && paramCount != range.ColumnCount() - 1)
            {
                MessageBox.Show("تعداد ستون های فایل اکسل با تعداد پارامتر های الگو وارد شده برابر نیست.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);
            }

            for (int row = 1; row < range.RowCount() + 1; row++)
            {
                string messageText = "";

                if (rbtnSendCorresponding.Checked)
                {
                    messageText = ws.Cell(row, 2).Value.ToString();
                }

                if (rbtnNormal.Checked)
                {
                    messageText = txtMessage.Text;
                }

                if (rbtnDynamic.Checked)
                {
                    messageText = txtMessage.Text;

                    for (int i = 0; i <= paramCount + 1; i++)
                    {
                        messageText = messageText.Replace($"{{{i}}}", ws.Cell(row, i + 2).Value.ToString());
                    }
                }

                BulkMessage bulkMessage = new() { SourceAddress = cmbSendNumber.SelectedItem.ToString(), DestinationAddress = Helpers.CorrectMobileNumber(ws.Cell(row, 1).Value.ToString()), MessageText = messageText };

                if (_blackList.Any(b => b.Equals(bulkMessage.DestinationAddress)))
                {
                    continue;
                }

                _bulkMessages.Add(bulkMessage);
                progressBar1.Value++;
            }

            stopwatch.Stop();
            txtLog.AppendText($" زمان خواندن فایل : {stopwatch.ElapsedMilliseconds} {Environment.NewLine}");
        }

        void ReadText()
        {
            Application.DoEvents();

            foreach (string line in File.ReadLines(openFileDialog1.FileName))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    break;
                }

                if (line.Split('|').Length != (rbtnSendCorresponding.Checked ? 2 : 1))
                {
                    MessageBox.Show("فایل دارای خطا است.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);

                    return;
                }

                BulkMessage bulkMessage = new()
                {
                    SourceAddress = cmbSendNumber.SelectedItem.ToString(),
                    DestinationAddress = Helpers.CorrectMobileNumber(line.Split(';').First()),
                    MessageText = rbtnSendCorresponding.Checked ? line.Split(';').Last() : txtMessage.Text
                };

                if (_blackList.Any(b => b.Equals(bulkMessage.DestinationAddress)))
                {
                    continue;
                }

                _bulkMessages.Add(bulkMessage);
            }
        }
    }

    private void btnSend_Click(object sender, EventArgs e)
    {
        _stopClick = false;

        if (!_bulkMessages.Any())
        {
            MessageBox.Show("فایل را انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);

            return;
        }

        if (string.IsNullOrEmpty(txtTake.Text))
        {
            MessageBox.Show("طول هر بسته را وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);

            return;
        }

        int packageCount = Convert.ToInt32(txtTake.Text);

        if (packageCount == 0)
        {
            MessageBox.Show("طول هر بسته را بزرگتر از صفر وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);

            return;
        }

        if (packageCount > _userInfo.Mps)
        {
            MessageBox.Show($"طول هر بسته را کوچکتر از {_userInfo.Mps}  وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);

            return;
        }

        if (!string.IsNullOrWhiteSpace(txtToRow.Text) && !string.IsNullOrWhiteSpace(txtFromRow.Text) && Convert.ToInt32(txtToRow.Text) < Convert.ToInt32(txtFromRow.Text))
        {
            MessageBox.Show("بازه ردیف ها را درست وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);

            return;
        }

        List<int> date = txtDate.Text.Split('/').Where(x => int.TryParse(x, out _)).Select(int.Parse).ToList();
        List<int> startTime = txtStartTime.Text.Split(':').Where(x => int.TryParse(x, out _)).Select(int.Parse).ToList();
        List<int> endTime = txtEndTime.Text.Split(':').Where(x => int.TryParse(x, out _)).Select(int.Parse).ToList();

        if (startTime.Any() && endTime.Any())
        {
            DateTime now = DateTime.Now;
            DateTime startDate = new(date[0], date[1], date[2], startTime[0], startTime[1], now.Second, new PersianCalendar());
            DateTime endDate = new(date[0], date[1], date[2], endTime[0], endTime[1], now.Second, new PersianCalendar());

            if ((endDate - startDate).TotalMilliseconds <= 0)
            {
                MessageBox.Show("زمان پایان را بزرگتر از زمان شروع ارسال وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);

                return;
            }
        }

        int totalMilliseconds = 0;

        if (date.Any() && startTime.Any())
        {
            DateTime now = DateTime.Now;
            DateTime startDate = new(date[0], date[1], date[2], startTime[0], startTime[1], 0, new PersianCalendar());
            totalMilliseconds = (int)(startDate - now).TotalMilliseconds;
        }

        panel1.Enabled = false;
        btnSend.Enabled = false;
        txtTake.Enabled = false;
        timer1.Enabled = true;
        timer1.Interval = totalMilliseconds < 0 ? 1 : totalMilliseconds;

        txtLog.Clear();
        progressBar1.Value = 0;
        txtLog.AppendText("آماده ارسال ....");
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        timer1.Enabled = false;
        txtLog.Clear();
        progressBar1.Value = 0;

        Application.DoEvents();

        int skip = 0;
        int take = _bulkMessages.Count;
        long tempStart = Convert.ToInt64(txtFromRow.Text) == 1 ? 0 : Convert.ToInt64(txtFromRow.Text);

        if (!string.IsNullOrWhiteSpace(txtFromRow.Text) && Convert.ToInt32(txtFromRow.Text) > 0)
        {
            skip = Convert.ToInt32("0" + txtFromRow.Text) - 1;
        }

        if (!string.IsNullOrWhiteSpace(txtToRow.Text) && Convert.ToInt32("0" + txtToRow.Text) > 0)
        {
            take = Convert.ToInt32("0" + txtToRow.Text);
            take = take - skip + 1;
        }

        List<BulkMessage> bulkMessages = new();
        List<List<BulkMessage>> package = new();
        int smsCount = 0;

        foreach (BulkMessage message in _bulkMessages.Skip(skip).Take(take))
        {
            int messageCount = Helpers.TryGet(message.MessageText).Count();

            if (smsCount + messageCount <= Convert.ToInt32(txtTake.Text))
            {
                bulkMessages.Add(message);
                smsCount += messageCount;
            }
            else
            {
                package.Add(bulkMessages);
                smsCount = 0;
                bulkMessages = new();
            }
        }

        if (bulkMessages.Any())
        {
            package.Add(bulkMessages);
        }

        List<ResultSend> resultSends = new();
        string errorMessage = string.Empty;

        progressBar1.Maximum = package.Count;

        string path = $"{Environment.CurrentDirectory}\\Log";

        for (int index = 0; index < package.Count; index++)
        {
            List<BulkMessage> item = package[index];

            try
            {
                progressBar1.Value++;

                Application.DoEvents();

                txtLog.AppendText($"ارسال بسته پیام شماره {index + 1} {Environment.NewLine}");
                Stopwatch sw1 = new();
                sw1.Start();
                List<string> result = Helpers.SendSms(item);
                sw1.Stop();

                if (result.Count == 0)
                {
                    using StreamWriter sw = File.CreateText($"{path}\\SendSmsError {DateTime.Now:yyyyMMdd hhmmss}.txt");
                    sw.WriteLine(Helpers.ErrorMessageLog);
                }

                try
                {
                    resultSends.AddRange(item.Select((t, i) => new ResultSend { DestinationAddress = t.DestinationAddress, Status = GetStatus(result[i]) }));
                    txtLog.AppendText($"تعداد پیام های ارسالی {result.Count} تعداد پیام های ارسال نشده {result.Count(s => s.Length < 4)} {Environment.NewLine}");
                    txtFromRow.Text = (tempStart + ((index == 0 ? 1 : index) * item.Count)).ToString();
                }
                catch (Exception exception)
                {
                    txtLog.AppendText($"Error: {exception.Message}");
                }

                if (chkStopWhenError.Checked && result.Count(s => s.Length < 4) > 0)
                {
                    break;
                }

                if (1000 > sw1.ElapsedMilliseconds)
                {
                    Thread.Sleep((int)(1000 - sw1.ElapsedMilliseconds));
                }

                Application.DoEvents();
                RefreshUserInfo();
                Application.DoEvents();

                if (_stopClick)
                {
                    break;
                }

                List<int> date = txtDate.Text.Split('/').Where(x => int.TryParse(x, out _)).Select(int.Parse).ToList();
                List<int> endTime = txtEndTime.Text.Split(':').Where(x => int.TryParse(x, out _)).Select(int.Parse).ToList();

                if (date.Any() && endTime.Any())
                {
                    DateTime now = DateTime.Now;
                    DateTime endDate = new(date[0], date[1], date[2], endTime[0], endTime[1], now.Second, new PersianCalendar());

                    if ((endDate - now).TotalMilliseconds < 0)
                    {
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                errorMessage = $"{exception} {Environment.NewLine} شماره بسته : {index + 1} {Environment.NewLine}";
                errorMessage = item.Aggregate(errorMessage, (current, bulkMessage) => current + $"{bulkMessage} {Environment.NewLine}");

                using StreamWriter sw = File.CreateText($"{path}\\AppError {cmbSendNumber.SelectedItem} {DateTime.Now:yyyyMMdd hhmmss}.txt");
                sw.WriteLine(exception.ToString());
            }
        }

        txtLog.AppendText(string.IsNullOrWhiteSpace(errorMessage) ? "پیام ها ارسال شد" : errorMessage);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        using (StreamWriter sw = File.CreateText($"{path}\\{cmbSendNumber.SelectedItem} {DateTime.Now:yyyyMMdd hhmmss}.txt"))
        {
            sw.WriteLine($"==== Send Count : {resultSends.Count(r => r.Status == SmsSendStatus.Sent)} - Error Count : {resultSends.Count(r => r.Status != SmsSendStatus.Sent)}");
            sw.WriteLine("=========================================================================");

            foreach (ResultSend resultSend in resultSends)
            {
                sw.WriteLine($"{resultSend.DestinationAddress}   {resultSend.Status}");
            }

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                sw.WriteLine(errorMessage);
            }
        }

        using (StreamWriter sw = File.CreateText($"{path}\\Error {cmbSendNumber.SelectedItem} {DateTime.Now:yyyyMMdd hhmmss}.txt"))
        {
            sw.WriteLine($"==== Count : {resultSends.Count(r => r.Status != SmsSendStatus.Sent)}");
            sw.WriteLine("=========================================================================");

            foreach (ResultSend resultSend in resultSends.Where(r => r.Status != SmsSendStatus.Sent))
            {
                sw.WriteLine($"{resultSend.DestinationAddress}   {resultSend.Status}");
            }
        }

        using (StreamWriter sw = File.CreateText($"{path}\\Send {cmbSendNumber.SelectedItem} {DateTime.Now:yyyyMMdd hhmmss}.txt"))
        {
            sw.WriteLine($" ==== Count : {resultSends.Count(r => r.Status == SmsSendStatus.Sent)}");
            sw.WriteLine("=========================================================================");

            foreach (ResultSend resultSend in resultSends.Where(r => r.Status == SmsSendStatus.Sent))
            {
                sw.WriteLine($"{resultSend.DestinationAddress}   {resultSend.Status}");
            }
        }

        panel1.Enabled = true;
        btnSend.Enabled = true;
        txtTake.Enabled = true;
    }

    private SmsSendStatus GetStatus(string value)
    {
        try
        {
            return value.Length < 4 ? (SmsSendStatus)Convert.ToInt32(value) : SmsSendStatus.Sent;
        }
        catch
        {
            return SmsSendStatus.Sent;
        }
    }

    private void txtTime_Leave(object sender, EventArgs e)
    {
        bool valid = TimeSpan.TryParse(txtStartTime.Text, out TimeSpan _);

        if (!valid)
        {
            MessageBox.Show("ساعت وارد شده اشتباه است.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);
            txtStartTime.Focus();
        }
    }

    private void txtDate_Leave(object sender, EventArgs e)
    {
        try
        {
            List<int> date = txtDate.Text.Split('/').Select(int.Parse).ToList();
            DateTime startDate = new(date[0], date[1], date[2], new PersianCalendar());
            Console.WriteLine(startDate);
        }
        catch
        {
            MessageBox.Show("تاریخ وارد شده اشتباه است.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);
            txtDate.Focus();
        }
    }

    private void txtMessage_TextChanged(object sender, EventArgs e)
    {
        string tmp = txtMessage.Text.Trim().Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n\n", "\n");

        txtMessageCount.Text = Helpers.TryGet(tmp).Count().ToString();
        txtMessageTyp.Text = Helpers.HasUniCodeCharacter(tmp) ? "فارسی" : "انگلیسی";
        txtCharCount.Text = tmp.Length.ToString();
    }

    private void txtMessageTest_TextChanged(object sender, EventArgs e)
    {
        string tmp = txtMessageTest.Text.Trim().Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n\n", "\n");

        txtMessageCountTest.Text = Helpers.TryGet(tmp).Count().ToString();
        txtMessageTypeTest.Text = Helpers.HasUniCodeCharacter(tmp) ? "فارسی" : "انگلیسی";
        txtCharCountTest.Text = tmp.Length.ToString();
    }

    private void btnStop_Click(object sender, EventArgs e)
    {
        _stopClick = true;
        timer1.Enabled = false;
        txtLog.Clear();
        progressBar1.Value = 0;
        panel1.Enabled = true;
        btnSend.Enabled = true;
    }

    private void btnSearch_Click(object sender, EventArgs e)
    {
        Search search = new(_bulkMessages);
        search.ShowDialog();
    }

    private void rbtnSendCorresponding_CheckedChanged(object sender, EventArgs e)
    {
        txtMessage.Enabled = !rbtnSendCorresponding.Checked;
    }

    private void CheckBlackList_CheckedChanged(object sender, EventArgs e)
    {
        btnBlackList.Enabled = CheckBlackList.Checked;
        _blackList = new();
    }

    private void btnBlackList_Click(object sender, EventArgs e)
    {
        _blackList = new();
        openFileDialog1.Filter = "txt files (*.txt)|*.txt";
        openFileDialog1.Title = "انتخاب فایل";
        DialogResult result = openFileDialog1.ShowDialog();

        if (result == DialogResult.OK)
        {
            Application.DoEvents();

            foreach (string line in File.ReadLines(openFileDialog1.FileName))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    break;
                }

                _blackList.Add(line);
            }
        }
    }
}