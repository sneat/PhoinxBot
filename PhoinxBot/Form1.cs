using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;

namespace PhoinxBot
{
    public partial class Form1 : Form
    {
        static IRC bot;

        //Delegate
        delegate void SetTextCallback(string form, string text);

        //Constructor
        public Form1()
        {
            InitializeComponent();
            UsernameTextBox.Text = Properties.Settings.Default.Username;
            PasswordTextBox.Text = Properties.Settings.Default.Password;
            TimeoutPeriodTextBox.Text = Properties.Settings.Default.TimeoutPeriod;
        }

        private void ConnectIRCButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Username = UsernameTextBox.Text;
            Properties.Settings.Default.Password = PasswordTextBox.Text;
            Properties.Settings.Default.Save();
            UsernamePasswordLabel.Visible = false;
            InitDBData();
            InitIRC();
            InitTabs();
            //AddChannel("your_channel_here");
            FillCmdListTab();
        }

        //Create tables if new db
        private void InitDBData() {
            using (SQLiteConnection dbCon = new SQLiteConnection("Data Source=Database.sqlite;Version=3;"))
            {
                dbCon.Open();
                string query = "CREATE TABLE IF NOT EXISTS channels (name varchar(50), UNIQUE(name));";
                query += "CREATE TABLE IF NOT EXISTS commands (command varchar(50), text varchar(255), channel varchar(50));";
                query += "CREATE TABLE IF NOT EXISTS permits (user varchar(50), channel varchar(50));";
                query += "CREATE TABLE IF NOT EXISTS blacklist (type int(1), text varchar(255), channel varchar(50));";

                SQLiteCommand command = new SQLiteCommand(query, dbCon);
                command.ExecuteNonQuery();
                dbCon.Close();
            }
        }

        private void AddChannel(string chan)
        {
            using (SQLiteConnection dbCon = new SQLiteConnection("Data Source=Database.sqlite;Version=3;"))
            {
                dbCon.Open();
                //channel names are case sensitive.  Twitch uses lowercase
                string query = "INSERT OR IGNORE INTO channels (name) values ('" + chan.ToLower() + "');";
                SQLiteCommand command = new SQLiteCommand(query, dbCon);
                command.ExecuteNonQuery();
                dbCon.Close();
            }
        }

        //Connect to IRC
        private void InitIRC()
        {
            bot = new IRC();
        }

        //Creates tabs
        private void InitTabs()
        {
            using (SQLiteConnection dbCon = new SQLiteConnection("Data Source=Database.sqlite;Version=3;"))
            {
                dbCon.Open();
                string query = "SELECT name FROM channels ORDER BY name ASC";
                SQLiteCommand command = new SQLiteCommand(query, dbCon);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string name = (string)reader["name"];
                    TabPage tab = new TabPage(name);

                    tab.Name = name;

                    System.Windows.Forms.Button b;
                    System.Windows.Forms.RichTextBox r;
                    System.Windows.Forms.TextBox t;

                    b = new System.Windows.Forms.Button();
                    t = new System.Windows.Forms.TextBox();
                    r = new System.Windows.Forms.RichTextBox();

                    //Send button
                    b.Location = new System.Drawing.Point(660, 679);
                    b.Name = "send[" + name + "]";
                    b.Size = new System.Drawing.Size(75, 20);
                    b.TabIndex = 2;
                    b.Text = "Send";
                    b.UseVisualStyleBackColor = true;
                    b.Click += new System.EventHandler(this.btnSend_Click);
                    b.Tag = name;

                    //Query area
                    t.Location = new System.Drawing.Point(6, 680);
                    t.Name = "query[" + name + "]";
                    t.Size = new System.Drawing.Size(648, 20);
                    t.TabIndex = 0;
                    t.KeyDown += (s, e) => {
                        if (e.KeyCode == Keys.Enter)
                        {
                            b.PerformClick();
                        }
                    };

                    //Terminal/console
                    r.Location = new System.Drawing.Point(7, 6);
                    r.Name = "terminal[" + name + "]";
                    r.Size = new System.Drawing.Size(728, 668);
                    r.TabIndex = 1;
                    r.Text = "";
                    r.ReadOnly = true;
                    r.BackColor = System.Drawing.SystemColors.Control;
                    r.Font = new System.Drawing.Font("Lucida Sans Unicode", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

                    tab.Controls.Add(b);
                    tab.Controls.Add(t);
                    tab.Controls.Add(r);

                    tabs.TabPages.Add(tab);
                }
                dbCon.Close();
            }

        }

        private void FillCmdListTab() {
            string CmdListText = "Hi";
            CmdListText += "";
            //TextBoxCmdList.Text = CmdListText;
        }

        

        //Sends message
        private void btnSend_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button from = (System.Windows.Forms.Button) sender;
            string tag = from.Tag.ToString();

            TabPage tab = null;

            //Get current tab
            for(int i = 1; i < this.tabs.TabPages.Count; i++) {
                TabPage temptab = this.tabs.TabPages[i];
                if (temptab.Name == tag)
                {
                    tab = temptab;
                }
            }

            System.Windows.Forms.TextBox query = null;

            //Send message to channel
            if (tab == null)
            {
                //tab = this.tabs.TabPages[0];
                tab = this.tabs.TabPages["main"];
                query = (System.Windows.Forms.TextBox)tab.Controls[2];
            }
            else
            {
                query = (System.Windows.Forms.TextBox)tab.Controls["query[" + tag + "]"];
            }

            if (tab.Name == "main") { tag = Properties.Settings.Default.Username; }

            if (tab.Name != "main" && query.Text.StartsWith("!"))
            {
                bot.PrintToChatTab(query.Text, tag);
                bot.ProcessCmds(query.Text, tag, bot.BotName());
            }
            else
            {
                if (tag == Properties.Settings.Default.Username)
                {
                    query = (System.Windows.Forms.TextBox)tab.Controls[2];
                    bot.SendMessage(query.Text);  //send message to server
                }
                else
                {
                    bot.PrintToChatTab(query.Text, tag, bot.BotName());  //display message in chat tab
                    bot.SendMessage("PRIVMSG #" + tag + " :" + query.Text); //send message to channel
                }
            }

            query.Text = "";
        }

        //Add a line to the terminal chosen
        public void AddLine(string form, string line)
        {
            if (form == "")
            {
                this.SetText(form, "> " + line + Environment.NewLine);
            }
            else
            {
                this.SetText(form, "" + line + Environment.NewLine);
            }
        }

        //Private function that invokes via SetTextCallback (to get outside thread)
        private void SetText(string form, string text)
        {
            if (this.txtTerminal.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { form, text });
            }
            else
            {
                if (form == "")
                {
                    this.txtTerminal.SelectionFont = this.txtTerminal.Font;
                    this.txtTerminal.AppendText(text);
                    this.txtTerminal.ScrollToCaret();
                }
                else
                {
                    string name = form.Replace("#", "");

                    TabPage tab = null;
                    for (int i = 1; i < this.tabs.TabPages.Count; i++)
                    {
                        TabPage temptab = this.tabs.TabPages[i];
                        if (temptab.Name == name)
                        {
                            tab = temptab;
                        }
                    }

                    ((System.Windows.Forms.RichTextBox)tab.Controls["terminal[" + name + "]"]).AppendText(text);
                    ((System.Windows.Forms.RichTextBox)tab.Controls["terminal[" + name + "]"]).ScrollToCaret();
                }
            }
        }

        private void PasswordTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ConnectIRCButton.PerformClick();
            }
        }

        private void fldQuery_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // The Send button on the irc.twitch.tv doesn't seem to work...not sure what sending commands to irc.twitch.tv channel is meant to do...
                btnSend.PerformClick();
            }
        }

        private void TimeoutPeriodTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                 Properties.Settings.Default.TimeoutPeriod = TimeoutPeriodTextBox.Text;
                 Properties.Settings.Default.Save();
            }
        }
   }
}
