using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Arad.Sms.Core.Bulk.Win;

public partial class Login : System.Windows.Forms.Form
{
    public Login()
    {
        InitializeComponent();

        List<string> domains = GetDomain();
        cmbDomain.Items.Clear();
        if (domains != null)
        {
            cmbDomain.Items.AddRange(domains.ToArray());
        }

        string path = Path.GetDirectoryName(Application.ExecutablePath) + "\\UserData.txt";

        if (!File.Exists(path))
        {
            return;
        }

        List<string> users = File.ReadLines(path).ToList();
            
        if (users.Count != 3)
        {
            return;
        }

        txtUserName.Text = users[0];
        txtPassword.Text = users[1];
        cmbDomain.SelectedIndex = Convert.ToInt32(users[2]);
    }

    private static List<string> GetDomain()
    {
        List<string> domains = new();
        try
        {
            StreamReader sr = new (Path.GetDirectoryName(Application.ExecutablePath) + "\\Domains.txt");
            string line = sr.ReadLine();
            if (!string.IsNullOrWhiteSpace(line))
            {
                domains.Add(line);
            }
            while (line != null)
            {
                line = sr.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    domains.Add(line);
                }
            }

            sr.Close();
               
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);   
        }
        return domains;
    }

    private void btnExit_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void btnLogin_Click(object sender, EventArgs e)
    {
        bool isInvalid = false;
        if (string.IsNullOrWhiteSpace(txtUserName.Text))
        {
            errorProvider1.SetError(txtUserName, "نام کاربری را وارد کنید");
            isInvalid = true;
        }

        if (string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            errorProvider1.SetError(txtPassword, "گذر واژه را وارد کنید");
            isInvalid = true;
        }

        if (cmbDomain.SelectedIndex < 0)
        {
            errorProvider1.SetError(cmbDomain, "دامنه را انتخاب کنید");
            isInvalid = true;
        }

        if (isInvalid)
        {
            return;
        }

        Helpers.GetToken(txtUserName.Text, txtPassword.Text, cmbDomain.SelectedItem.ToString());
        Program.Domain = cmbDomain.SelectedItem.ToString();
        Program.UserName = txtUserName.Text;
        Program.Password = txtPassword.Text;
        if (!string.IsNullOrWhiteSpace(Program.AccessToken))
        {
            string path = Path.GetDirectoryName(Application.ExecutablePath) + "\\UserData.txt";
                
            using (StreamWriter sw = File.Exists(path) ? File.AppendText(path) : File.CreateText(path))
            {
                sw.WriteLine($"{Program.UserName}");
                sw.WriteLine($"{Program.Password}");
                sw.WriteLine($"{cmbDomain.SelectedIndex}");
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}