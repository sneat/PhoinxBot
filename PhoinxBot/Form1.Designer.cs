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
            this.main = new System.Windows.Forms.TabPage();
            this.btnSend = new System.Windows.Forms.Button();
            this.txtTerminal = new System.Windows.Forms.RichTextBox();
            this.fldQuery = new System.Windows.Forms.TextBox();
            this.tabs = new System.Windows.Forms.TabControl();
            this.main.SuspendLayout();
            this.tabs.SuspendLayout();
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
            this.fldQuery.Size = new System.Drawing.Size(648, 20);
            this.fldQuery.TabIndex = 0;
            // 
            // tabs
            // 
            this.tabs.Controls.Add(this.main);
            this.tabs.Location = new System.Drawing.Point(12, 12);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(749, 732);
            this.tabs.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(776, 756);
            this.Controls.Add(this.tabs);
            this.Name = "Form1";
            this.Text = "PhoinxBot";
            this.main.ResumeLayout(false);
            this.main.PerformLayout();
            this.tabs.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage main;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.RichTextBox txtTerminal;
        private System.Windows.Forms.TextBox fldQuery;
        private System.Windows.Forms.TabControl tabs;
    }
}

