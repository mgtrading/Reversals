using System;
using System.Windows.Forms;
using Reversals.DbDataManager;

namespace Reversals.Forms
{
    public partial class FormSettings : DevComponents.DotNetBar.Metro.MetroForm
    {
        public FormSettings()
        {
            InitializeComponent();
        }

        
        private void FormSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.sHost = uiTBSettingHost.Text;
            Properties.Settings.Default.sDB = uiTBSettingDB.Text;
            Properties.Settings.Default.sUser = uiTBSettingUser.Text;
            Properties.Settings.Default.sPassword = uiTBSettingPassword.Text;
            Properties.Settings.Default.sTimeZone = uiTimeZoneSettingsOffsetValue.Text;

            Properties.Settings.Default.Save();
        }

        private void FormSettings_Shown(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reload();
            uiTBSettingHost.Text = Properties.Settings.Default.sHost;
            uiTBSettingDB.Text = Properties.Settings.Default.sDB;
            uiTBSettingUser.Text = Properties.Settings.Default.sUser;
            uiTBSettingPassword.Text = Properties.Settings.Default.sPassword;
            uiTimeZoneSettingsOffsetValue.Text = Properties.Settings.Default.sTimeZone;
        }

        private void buttonX_test_connection_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.sHost = uiTBSettingHost.Text;
            Properties.Settings.Default.sDB = uiTBSettingDB.Text;
            Properties.Settings.Default.sUser = uiTBSettingUser.Text;
            Properties.Settings.Default.sPassword = uiTBSettingPassword.Text;
            Properties.Settings.Default.sTimeZone = uiTimeZoneSettingsOffsetValue.Text;


            if (DataManager.Initialize(Properties.Settings.Default.sHost, Properties.Settings.Default.sDB, Properties.Settings.Default.sUser, Properties.Settings.Default.sPassword))
            {
                
                DialogResult = DialogResult.OK;
                
            }
            else
            {
                const int visibleTime = 3 * 1000; //in milliseconds

                var tt1 = new ToolTip();
                tt1.Show("Please, enter a valid data.", uiTBSettingHost, 0, -20, visibleTime);

                var tt2 = new ToolTip();
                var tt3 = new ToolTip();
                var tt4 = new ToolTip();

                tt2.Show("Please, enter a valid data.", uiTBSettingDB, 0, -20, visibleTime);
                tt3.Show("Please, enter a valid data.", uiTBSettingUser, 0, -20, visibleTime);
                tt4.Show("Please, enter a valid data.", uiTBSettingPassword, 0, -20, visibleTime);
            }
        }
    }
}