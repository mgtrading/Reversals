namespace Reversals.Forms
{
    partial class FormContractEdit
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonX_data_archive_stop = new DevComponents.DotNetBar.ButtonX();
            this.buttonX_data_archive_start = new DevComponents.DotNetBar.ButtonX();
            this.textBoxX_contract_name = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.uiBlackSInterestRateLabel = new DevComponents.DotNetBar.LabelX();
            this.SuspendLayout();
            // 
            // buttonX_data_archive_stop
            // 
            this.buttonX_data_archive_stop.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.buttonX_data_archive_stop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonX_data_archive_stop.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.buttonX_data_archive_stop.Location = new System.Drawing.Point(161, 63);
            this.buttonX_data_archive_stop.Margin = new System.Windows.Forms.Padding(2);
            this.buttonX_data_archive_stop.Name = "buttonX_data_archive_stop";
            this.buttonX_data_archive_stop.Size = new System.Drawing.Size(61, 35);
            this.buttonX_data_archive_stop.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.buttonX_data_archive_stop.TabIndex = 31;
            this.buttonX_data_archive_stop.TabStop = false;
            this.buttonX_data_archive_stop.Text = "Cancel";
            this.buttonX_data_archive_stop.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // buttonX_data_archive_start
            // 
            this.buttonX_data_archive_start.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.buttonX_data_archive_start.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonX_data_archive_start.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.buttonX_data_archive_start.Location = new System.Drawing.Point(20, 63);
            this.buttonX_data_archive_start.Margin = new System.Windows.Forms.Padding(2);
            this.buttonX_data_archive_start.Name = "buttonX_data_archive_start";
            this.buttonX_data_archive_start.Size = new System.Drawing.Size(61, 35);
            this.buttonX_data_archive_start.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.buttonX_data_archive_start.TabIndex = 30;
            this.buttonX_data_archive_start.TabStop = false;
            this.buttonX_data_archive_start.Text = "Save";
            this.buttonX_data_archive_start.Click += new System.EventHandler(this.btn_save_Click);
            // 
            // textBoxX_contract_name
            // 
            this.textBoxX_contract_name.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxX_contract_name.BackColor = System.Drawing.Color.White;
            // 
            // 
            // 
            this.textBoxX_contract_name.Border.Class = "TextBoxBorder";
            this.textBoxX_contract_name.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.textBoxX_contract_name.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.textBoxX_contract_name.ForeColor = System.Drawing.Color.Black;
            this.textBoxX_contract_name.Location = new System.Drawing.Point(20, 36);
            this.textBoxX_contract_name.MaxLength = 50;
            this.textBoxX_contract_name.Name = "textBoxX_contract_name";
            this.textBoxX_contract_name.Size = new System.Drawing.Size(202, 22);
            this.textBoxX_contract_name.TabIndex = 194;
            this.textBoxX_contract_name.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxX_contract_name.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxX_contract_name_KeyDown);
            // 
            // uiBlackSInterestRateLabel
            // 
            this.uiBlackSInterestRateLabel.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.uiBlackSInterestRateLabel.BackgroundStyle.BackColor = System.Drawing.Color.Transparent;
            this.uiBlackSInterestRateLabel.BackgroundStyle.BackColor2 = System.Drawing.Color.Transparent;
            this.uiBlackSInterestRateLabel.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.uiBlackSInterestRateLabel.Font = new System.Drawing.Font("Segoe UI Symbol", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uiBlackSInterestRateLabel.ForeColor = System.Drawing.Color.White;
            this.uiBlackSInterestRateLabel.Location = new System.Drawing.Point(20, 12);
            this.uiBlackSInterestRateLabel.Name = "uiBlackSInterestRateLabel";
            this.uiBlackSInterestRateLabel.Size = new System.Drawing.Size(100, 23);
            this.uiBlackSInterestRateLabel.TabIndex = 193;
            this.uiBlackSInterestRateLabel.Text = "Contract";
            // 
            // FormContractEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.SteelBlue;
            this.ClientSize = new System.Drawing.Size(244, 110);
            this.Controls.Add(this.textBoxX_contract_name);
            this.Controls.Add(this.uiBlackSInterestRateLabel);
            this.Controls.Add(this.buttonX_data_archive_stop);
            this.Controls.Add(this.buttonX_data_archive_start);
            this.Font = new System.Drawing.Font("Segoe UI Symbol", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormContractEdit";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Edit Contract";
            this.Shown += new System.EventHandler(this.FormContractAdd_Shown);
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.ButtonX buttonX_data_archive_stop;
        private DevComponents.DotNetBar.ButtonX buttonX_data_archive_start;
        private DevComponents.DotNetBar.LabelX uiBlackSInterestRateLabel;
        public DevComponents.DotNetBar.Controls.TextBoxX textBoxX_contract_name;
    }
}