using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Reversals.DateFormats;
using Reversals.Forms;

namespace Reversals
{
    static class Program
    {
        static Mutex mutex = new Mutex(true, "{8F6F0AC4-B9A1-45fd-A8CF-72F04E6BDE8F}");

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWndm, int nCmdShow);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {

                Application.SetCompatibleTextRenderingDefault(false);
                DateFormatsManager.CurrentShortDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                var culture = new CultureInfo("en-US", false) {DateTimeFormat = {ShortTimePattern = "HH:mm:ss"}};
                Application.CurrentCulture = culture;
                Application.Run(new MainFormMetroApp());
            }
            else
            {
                Process current = Process.GetCurrentProcess();
                foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                {
                    if (process.Id != current.Id)
                    {
                        SetForegroundWindow(process.MainWindowHandle);
                        ShowWindow(process.MainWindowHandle, 5);
                        break;
                    }
                }
            }
        }
    }
}
