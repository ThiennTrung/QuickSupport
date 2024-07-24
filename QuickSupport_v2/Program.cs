using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace QuickSupport_v2
{
    internal static class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);
            Application.ThreadException += new ThreadExceptionEventHandler(ThreadException);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //var client = new WebClient();
            //if (!client.DownloadString("https://www.dropbox.com/scl/fi/64vnjlj47seqf3ojxd9rs/Version.txt?rlkey=9c58b1nokrhb9q8pbijdy78wu&dl=1").Contains("3.0.0"))
            //{
            //    string pathDown = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "{374DE290-123F-4565-9164-39C4925E467B}", String.Empty).ToString();

            //    UpdateVer form = new UpdateVer();
            //    form.ShowDialog();

            //    Process pro = new Process();
            //    pro.StartInfo.FileName = "msiexec";
            //    pro.StartInfo.Arguments = String.Format("/i QuickSupportSetup.msi");
            //    pro.StartInfo.WorkingDirectory = pathDown;
            //    log.Info("Cập nhật...........................");
            //    pro.Start();
            //    Application.Exit();
            //}
            //else { 
            Application.Run(new XtraForm1());
            //}
        }
        static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            log.Error(e.Message);
        }
        static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            log.Error(ex.Message);
        }

        static void ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            log.Error(ex.Message);
        }
    }
}
