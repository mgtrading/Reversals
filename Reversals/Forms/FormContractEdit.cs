using System;
using System.Windows.Forms;

namespace Reversals.Forms
{
    public partial class FormContractEdit : Form
    {
        public FormContractEdit()
        {
            InitializeComponent();
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            if (textBoxX_contract_name.Text != "")
            {
                DialogResult = DialogResult.OK;
            }
            else
            {
                var tt1 = new ToolTip();
                tt1.Show("Please, enter name of contract.", textBoxX_contract_name, 0, -20, 3000);
            }
        }

        private void FormContractAdd_Shown(object sender, EventArgs e)
        {
            textBoxX_contract_name.Focus();            

        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void textBoxX_contract_name_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
            if (e.KeyCode == Keys.Enter)
            {
                if (textBoxX_contract_name.Text != "")
                {
                    DialogResult = DialogResult.OK;
                }
                else
                {
                    var tt1 = new ToolTip();
                    tt1.Show("Please, enter name of contract.", textBoxX_contract_name, 0, -20, 3000);
                }
            }
        }

    }
}
