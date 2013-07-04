using System;
using System.Globalization;
using System.Windows.Forms;
using Reversals.DateFormats;
using Reversals.Forms;

namespace Reversals
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetCompatibleTextRenderingDefault(false);
            DateFormatsManager.CurrentShortDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
            var culture = new CultureInfo("en-US", false) {DateTimeFormat = {ShortTimePattern = "HH:mm:ss"}};
            Application.CurrentCulture = culture;
            Application.Run(new MainFormMetroApp());
        }
    }
}
