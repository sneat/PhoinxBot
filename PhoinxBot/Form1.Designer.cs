namespace PhoinxBot
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.main = new System.Windows.Forms.TabPage();
            this.btnSend = new System.Windows.Forms.Button();
            this.txtTerminal = new System.Windows.Forms.RichTextBox();
            this.fldQuery = new System.Windows.Forms.TextBox();
            this.tabs = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.TextBoxCmdList = new System.Windows.Forms.RichTextBox();
            this.UsernameTextBox = new System.Windows.Forms.TextBox();
            this.PasswordTextBox = new System.Windows.Forms.TextBox();
            this.UsernamePasswordLabel = new System.Windows.Forms.Label();
            this.ConnectIRCButton = new System.Windows.Forms.Button();
            this.TimeoutPeriodTextBox = new System.Windows.Forms.TextBox();
            this.GeneralToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.main.SuspendLayout();
            this.tabs.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // main
            // 
            this.main.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.main.Controls.Add(this.btnSend);
            this.main.Controls.Add(this.txtTerminal);
            this.main.Controls.Add(this.fldQuery);
            this.main.Location = new System.Drawing.Point(4, 22);
            this.main.Name = "main";
            this.main.Padding = new System.Windows.Forms.Padding(3);
            this.main.Size = new System.Drawing.Size(741, 706);
            this.main.TabIndex = 0;
            this.main.Text = "irc.twitch.tv";
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(660, 679);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(75, 20);
            this.btnSend.TabIndex = 2;
            this.btnSend.Tag = "main";
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // txtTerminal
            // 
            this.txtTerminal.BackColor = System.Drawing.SystemColors.Control;
            this.txtTerminal.Location = new System.Drawing.Point(7, 6);
            this.txtTerminal.Name = "txtTerminal";
            this.txtTerminal.ReadOnly = true;
            this.txtTerminal.Size = new System.Drawing.Size(728, 668);
            this.txtTerminal.TabIndex = 1;
            this.txtTerminal.Text = "";
            // 
            // fldQuery
            // 
            this.fldQuery.Location = new System.Drawing.Point(6, 680);
            this.fldQuery.Name = "fldQuery";
            this.fldQuery.Size = new System.Drawing.Size(648, 24);
            this.fldQuery.TabIndex = 0;
            this.fldQuery.KeyDown += new System.Windows.Forms.KeyEventHandler(this.fldQuery_KeyDown);
            // 
            // tabs
            // 
            this.tabs.Controls.Add(this.tabPage1);
            this.tabs.Controls.Add(this.main);
            this.tabs.Font = new System.Drawing.Font("Lucida Sans Unicode", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabs.Location = new System.Drawing.Point(12, 12);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(749, 732);
            this.tabs.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.TextBoxCmdList);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(741, 704);
            this.tabPage1.TabIndex = 1;
            this.tabPage1.Text = "Command List";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // TextBoxCmdList
            // 
            this.TextBoxCmdList.BackColor = System.Drawing.SystemColors.Control;
            this.TextBoxCmdList.Font = new System.Drawing.Font("Lucida Sans Unicode", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxCmdList.Location = new System.Drawing.Point(7, 6);
            this.TextBoxCmdList.Name = "TextBoxCmdList";
            this.TextBoxCmdList.ReadOnly = true;
            this.TextBoxCmdList.Size = new System.Drawing.Size(728, 697);
            this.TextBoxCmdList.TabIndex = 2;
            this.TextBoxCmdList.Text = resources.GetString("TextBoxCmdList.Text");
            // 
            // UsernameTextBox
            // 
            this.UsernameTextBox.Location = new System.Drawing.Point(385, 7);
            this.UsernameTextBox.Name = "UsernameTextBox";
            this.UsernameTextBox.Size = new System.Drawing.Size(119, 20);
            this.UsernameTextBox.TabIndex = 1;
            // 
            // PasswordTextBox
            // 
            this.PasswordTextBox.Location = new System.Drawing.Point(510, 7);
            this.PasswordTextBox.Name = "PasswordTextBox";
            this.PasswordTextBox.PasswordChar = '*';
            this.PasswordTextBox.Size = new System.Drawing.Size(89, 20);
            this.PasswordTextBox.TabIndex = 2;
            this.PasswordTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PasswordTextBox_KeyDown);
            // 
            // UsernamePasswordLabel
            // 
            this.UsernamePasswordLabel.AutoSize = true;
            this.UsernamePasswordLabel.Location = new System.Drawing.Point(218, 10);
            this.UsernamePasswordLabel.Name = "UsernamePasswordLabel";
            this.UsernamePasswordLabel.Size = new System.Drawing.Size(161, 13);
            this.UsernamePasswordLabel.TabIndex = 3;
            this.UsernamePasswordLabel.Text = "Twitch.TV Username/Password:";
            // 
            // ConnectIRCButton
            // 
            this.ConnectIRCButton.Location = new System.Drawing.Point(605, 5);
            this.ConnectIRCButton.Name = "ConnectIRCButton";
            this.ConnectIRCButton.Size = new System.Drawing.Size(75, 23);
            this.ConnectIRCButton.TabIndex = 3;
            this.ConnectIRCButton.Text = "Connect";
            this.ConnectIRCButton.UseVisualStyleBackColor = true;
            this.ConnectIRCButton.Click += new System.EventHandler(this.ConnectIRCButton_Click);
            // 
            // TimeoutPeriodTextBox
            // 
            this.TimeoutPeriodTextBox.Location = new System.Drawing.Point(707, 7);
            this.TimeoutPeriodTextBox.Name = "TimeoutPeriodTextBox";
            this.TimeoutPeriodTextBox.Size = new System.Drawing.Size(50, 20);
            this.TimeoutPeriodTextBox.TabIndex = 4;
            this.GeneralToolTip.SetToolTip(this.TimeoutPeriodTextBox, "Length in seconds to timeout a user for a banned word.\r\nPress <Enter> to set.");
            this.TimeoutPeriodTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TimeoutPeriodTextBox_KeyDown);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(776, 756);
            this.Controls.Add(this.TimeoutPeriodTextBox);
            this.Controls.Add(this.ConnectIRCButton);
            this.Controls.Add(this.UsernamePasswordLabel);
            this.Controls.Add(this.PasswordTextBox);
            this.Controls.Add(this.UsernameTextBox);
            this.Controls.Add(this.tabs);
            this.Name = "Form1";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "PhoinxBot";
            this.main.ResumeLayout(false);
            this.main.PerformLayout();
            this.tabs.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabPage main;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.RichTextBox txtTerminal;
        private System.Windows.Forms.TextBox fldQuery;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.RichTextBox TextBoxCmdList;
        private System.Windows.Forms.TextBox UsernameTextBox;
        private System.Windows.Forms.TextBox PasswordTextBox;
        private System.Windows.Forms.Label UsernamePasswordLabel;
        private System.Windows.Forms.Button ConnectIRCButton;
        private System.Windows.Forms.TextBox TimeoutPeriodTextBox;
        private System.Windows.Forms.ToolTip GeneralToolTip;
    }
}

