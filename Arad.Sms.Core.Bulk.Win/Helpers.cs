using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

using Newtonsoft.Json;

using RestSharp;

namespace Arad.Sms.Core.Bulk.Win;

public static class Helpers
{
    public enum ApiResponse
    {
        [Description("Succeeded")] Succeeded = 100,

        [Description("DatabaseError")] DatabaseError = 101,

        [Description("RepositoryError")] RepositoryError = 102,

        [Description("ModelError")] ModelError = 103,

        [Description("Status connecting to api")]
        ApiConnectionAttemptFailure = 104,

        [Description("Service unavailable at the moment")]
        ServiceUnAvailable = 105,

        [Description("Confirm email failed")] ConfirmEmailFailed = 106,

        [Description("User not found")] UserNotFound = 107,

        [Description("Item already exists")] DuplicateError = 108,

        [Description("Item not found!")] NotFound = 109,

        [Description("Could not json convert the result!")]
        UnableToCreateResultApiError = 110,

        [Description("Status mapping object!")] AutoMapperError = 111,

        [Description("GeneralFailure")] GeneralFailure = 112,

        [Description("Status in 3rd party web service.")]
        WebServiceError = 113,

        [Description("Unable to recieve token from identity provider.")]
        ErrorGettingTokenFromIdp = 114,

        [Description("Can not read appsettings.json")]
        ErrorRetrievingDataFromAppSettings = 115,

        [Description("Headers of request is not correctly set.")]
        HeaderError = 116,

        [Description("Bad request.")] BadRequestError = 117,

        [Description("RangeLimitExceed")] RangeLimitExceedResponse = 118
    }

    public static string ErrorMessageLog;

    public static void GetToken(string userName, string password, string baseUrl)
    {
        try
        {
            RestClient client = new($"{baseUrl}/connect/token");
            RestRequest request = new(Method.Get.ToString());
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("username", userName);
            request.AddParameter("password", password);
            request.AddParameter("scope", "ApiAccess");
            var response = client.Execute(request);

            string path = $"{Environment.CurrentDirectory}\\Log";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            TokenResponseModel data = JsonConvert.DeserializeObject<TokenResponseModel>(response.Content);
            if (data != null && !string.IsNullOrWhiteSpace(data.access_token))
            {
                Program.AccessToken = data.access_token;
            }
            else
            {
                MessageBox.Show("نام کاربری یا گذر واژه شما نادرست است.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);
            }
        }
        catch
        {
            MessageBox.Show("خطا براقرای ارتباط با سرور", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);
        }
    }

    public static UserInfo UserInfo()
    {
        try
        {
            RestClient client = new ($"{Program.Domain}/api/user/userinfo");
            RestRequest request = new (Method.Get.ToString());
            request.AddHeader("Authorization", $"Bearer {Program.AccessToken}");
            var response = client.Execute(request);
               
            ResultApiClass<UserInfo> userInfo = JsonConvert.DeserializeObject<ResultApiClass<UserInfo>>(response.Content);
            return userInfo?.Data;
        }
        catch
        {
            MessageBox.Show("خطا براقرای ارتباط با سرور", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);
        }

        return new();
    }

    public static List<string> SendSms(List<BulkMessage> bulkMessages)
    {
        try
        {
            string url = $"{Program.Domain}/api/message/P2PBulk";
            RestClient client = new(url);
            RestRequest request = new(Method.Get.ToString());
            request.AddHeader("Authorization", $"Bearer {Program.AccessToken}");
            request.AddHeader("Content-Type", "application/json");

            string body = JsonConvert.SerializeObject(bulkMessages);
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            var response = client.Execute(request);

            List<string> batchIds = new();

            ErrorMessageLog = $"StatusCode => {response.StatusCode}{Environment.NewLine} ErrorMessage => {response.ErrorMessage} {Environment.NewLine} ErrorException=> {response.ErrorException?.Message} {Environment.NewLine}";
                
            switch(response.StatusCode)
            {
                case HttpStatusCode.OK:
                    ErrorMessageLog += $"Content => {response.Content}{Environment.NewLine}";
                    batchIds = JsonConvert.DeserializeObject<ResultApiClass<List<string>>>(response.Content)?.Data;
                    break;

                case HttpStatusCode.Unauthorized:
                    GetToken(Program.UserName, Program.Password, Program.Domain);
                    Thread.Sleep(1000);
                    batchIds = SendSms(bulkMessages);
                    break;
                            
                case 0:
                    Thread.Sleep(1000);
                    batchIds = SendSms(bulkMessages);
                    break;
            }

            return batchIds;
        }
        catch (Exception e)
        {
            string path = $"{Environment.CurrentDirectory}\\Log";
            using StreamWriter sw = File.CreateText($"{path}\\SendSms {DateTime.Now:yyyyMMdd hhmmss}.txt");
            sw.WriteLine(e.ToString());

            throw;
        }
    }

    public static bool HasUniCodeCharacter(string text)
    {
        return Regex.IsMatch(text, "[^\u0000-\u00ff]");
    }

    public static string CorrectMobileNumber(string cellPhone)
    {
        if (cellPhone.StartsWith("09"))
            cellPhone = "98" + cellPhone.Substring(1);
        else if (cellPhone.StartsWith("+98"))
            cellPhone = cellPhone.Substring(1);
        else if (cellPhone.StartsWith("0098"))
            cellPhone = cellPhone.Substring(2);
        else if (cellPhone.StartsWith("98"))
            cellPhone = cellPhone.Substring(0);
        else if (cellPhone.StartsWith("9"))
            cellPhone = "98" + cellPhone;
        return cellPhone;
    }

    public static IEnumerable<string> TryGet(string text)
    {
        List<string> list = new ();
        int standardSmsLen = 140;
        int standardUdhLen = 6;

        if (HasUniCodeCharacter(text))
        {
            standardSmsLen = 70;
            standardUdhLen = 3;
        }

        if (text.Length <= standardSmsLen)
        {
            list.Add(text);
        }
        else
        {
            standardSmsLen -= standardUdhLen;

            while (text.Length >= standardSmsLen)
            {
                string subString = text.Substring(0, standardSmsLen);
                list.Add(subString);
                text = text.Substring(standardSmsLen);
            }

            if (text.Length > 0)
            {
                list.Add(text);
            }
        }

        return list;
    }
        
    public static int GetSmsCount(string text)
    {
        int standardSmsLen = 140;
        int standardUdhLen = 6;

        if (HasUniCodeCharacter(text))
        {
            standardSmsLen = 70;
            standardUdhLen = 3;
        }

        double smsLen = text.Replace("\r\n", "\n").Length;
        double smsCount = smsLen > standardSmsLen ? Math.Ceiling(smsLen / (standardSmsLen - standardUdhLen)) : Math.Ceiling(smsLen / standardSmsLen);

        return (int)smsCount;
    }
}

public class UserInfo
{
    public string UserName { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public decimal Credit { get; set; }

    public decimal Mps { get; set; }

    public string[] SenderIds { get; set; }
}

public class BulkMessage
{
    public string SourceAddress { get; set; }

    public string DestinationAddress { get; set; }

    public string MessageText { get; set; }
}

public class ResultApiClass<TClass> where TClass : class
{
    public string Message { get; set; }

    public bool Succeeded { get; set; }

    public TClass Data { get; set; }

    public Helpers.ApiResponse ResultCode { get; set; }
}

public class ResultApiStruct<TStruct> where TStruct : struct
{
    public string Message { get; set; }

    public bool Succeeded { get; set; }

    public TStruct Data { get; set; }

    public Helpers.ApiResponse ResultCode { get; set; }
}

public class ResultApi
{
    public string Message { get; set; }

    public bool Succeeded { get; set; }

    public Helpers.ApiResponse ResultCode { get; set; }
}

public class ResultSend
{
    public string DestinationAddress { get; set; }

    public SmsSendStatus Status { get; set; }
}

public enum SmsSendStatus
{
    Sent = 1,
    SendError = 0,
    NotEnoughCredit = -1,
    ServerError = -2,
    DeActiveAccount = -3,
    ExpiredAccount = -4,
    InvalidUsernameOrPassword = -5,
    AuthenticationFailure = -6,
    ServerBusy = -7,
    NumberAtBlackList = -8,
    LimitedInSendDay = -9,
    LimitedInVolume = -10,
    InvalidSenderNumber = -11,
    InvalidReceiverNumber = -12,
    InvalidDestinationNetwork = -13,
    UnreachableNetwork = -14,
    DeActiveSenderNumber = -15,
    InvalidFormatOfSenderNumber = -16,
    TariffNotFound = -17,
    InvalidIpAddress = -18,
    InvalidPattern = -19,
    ExpiredSenderNumber = -20,
    MessageContainsLink = -21,
    InvalidPort = -22,
    MessageTooLong = -23,
    FilterWord = -24,
    InvalidReferenceNumberType = -25,
    InvalidTargetUDH = -26,
    LimitedInSendMonth = -27,
    DataCodingNotAllowed = -28,
    NotFoundRoute = -29,
    None = -100
}

public class TokenResponseModel
{
    public string access_token { get; set; }
    public DateTime expires_at { get; set; }
    public string scope { get; set; }
}