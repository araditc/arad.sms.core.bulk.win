using System;
using System.Windows.Forms;

namespace Arad.Sms.Core.Bulk.Win;

internal static class Program
{
    public static string AccessToken;
    public static string Domain;
    public static string UserName;
    public static string Password;
        
    [STAThread]
    private static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        Login login = new();
        Application.Run(login);

        if (login.DialogResult == DialogResult.OK)
        {
            Application.Run(new Form());
        }
    }
}