using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Data.SQLite;
using System.Text.RegularExpressions;


namespace PhoinxBot
{
    class IRC
    {
        //Client vars
        TcpClient Client;
        NetworkStream NwStream;
        StreamReader Reader;
        StreamWriter Writer;

        //Special vars
        Dictionary<string, bool> pollOpen;
        Dictionary<string, Dictionary<string, Dictionary<string, int>>> pollVotes;
        Dictionary<string, bool> antiSpoil;
        Dictionary<string, Dictionary<string, int>> blackList;

        //Spoiler regex
        Regex spoilers;

        //Listen thread
        Thread listen;

        //Constructor
        public IRC()
        {
            //Opens connection to the twitch IRC
            try
            {
                Client = new TcpClient("irc.twitch.tv", 6667);
                NwStream = Client.GetStream();
                Reader = new StreamReader(NwStream, Encoding.GetEncoding("iso8859-1"));
                Writer = new StreamWriter(NwStream, Encoding.GetEncoding("iso8859-1"));

                //Starts a thread that reads all data from IRC
                listen = new Thread(new ThreadStart(Listen));
                listen.Start();

                //Special vars initialized
                pollOpen = new Dictionary<string, bool>();
                pollVotes = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();
                antiSpoil = new Dictionary<string, bool>();
                blackList = new Dictionary<string, Dictionary<string, int>>();

                //Writes userdata to twitch - remember your username and password!
                Writer.WriteLine("USER " + Properties.Settings.Default.Username + "tmi twitch :" + Properties.Settings.Default.Username);
                Writer.Flush();
                Writer.WriteLine("PASS " + Properties.Settings.Default.Password);
                Writer.Flush();
                Writer.WriteLine("NICK " + Properties.Settings.Default.Username);
                Writer.Flush();

                //Regex to check for spoilers initialized
                spoilers = new Regex("^(.*)[0-9]+[_ ]*[-:][_ ]*[0-9](.*)$");

                //Initializes all channels
                using (SQLiteConnection dbCon = new SQLiteConnection("Data Source=Database.sqlite;Version=3;"))
                {
                    dbCon.Open();
                    string query = "SELECT name FROM channels ORDER BY name ASC";
                    SQLiteCommand command = new SQLiteCommand(query, dbCon);
                    SQLiteDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        InitChan((string)reader["name"]);
                    }
                    dbCon.Close();
                }
            }
            catch
            {
                Console.WriteLine("Could not connect to irc.twitch.tv");
            }
        }

        //Initializes a new channel
        private void InitChan(string name)
        {
            pollOpen["#" + name] = false;
            pollVotes["#" + name] = null;
            antiSpoil["#" + name] = false;

            Dictionary<string, int> tmpBL = new Dictionary<string, int>();

            //Builds the chans blacklist table
            using (SQLiteConnection dbCon = new SQLiteConnection("Data Source=Database.sqlite;Version=3;"))
            {
                dbCon.Open();
                string qSelect = "SELECT * FROM blacklist WHERE channel = '#" + name + "'";
                SQLiteCommand command = new SQLiteCommand(qSelect, dbCon);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    tmpBL.Add((string)reader["text"], (int)reader["type"]);
                }
                dbCon.Close();
            }
            //Anti ascii spam thingy
            tmpBL.Add("â", 0);

            //Adds to "global" scope var
            blackList.Add("#" + name, tmpBL);

            Writer.WriteLine("JOIN #" + name + "\n");
            Writer.Flush();
        }

        //Checks is a person is op/owner in a channel
        private bool isOp(string channel, string username)
        {
            //If owner
            bool _xist = false;
            if(username == channel.Replace("#", "")) {
                return true;
            }

            //If bot
            if (username == BotName()) { return true; }

            //If op
            using (SQLiteConnection dbCon = new SQLiteConnection("Data Source=Database.sqlite;Version=3;"))
            {
                dbCon.Open();
                string qSelect = "SELECT * FROM permits WHERE user = '" + username + "' AND channel = '" + channel + "'";
                SQLiteCommand command = new SQLiteCommand(qSelect, dbCon);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read() && _xist == false)
                {
                    _xist = true;
                }
                dbCon.Close();
            }

            return _xist;
        }

        //Checks if a user is the owner
        private bool isOwner(string channel, string username)
        {
            return username == channel.Replace("#", "");
        }

        public string BotName()
        {
            return Properties.Settings.Default.Username;
        }

        //Reads all the data from the IRC server
        public void Listen()
        {
            string Data = "";
            try
            {
                while ((Data = Reader.ReadLine()) != null)
                {
                    //Initializing vars
                    string _nick = "";
                    string _type = "";
                    string _channel = "";
                    string _message = "";
                    string[] ex;

                    //Writes to console and main terminal
                    Console.WriteLine(Data);
                    Program.mainForm.AddLine("", Data);

                    //Respond PINGs with PONGs and the same message
                    ex = Data.Split(new char[] { ' ' }, 5);
                    if (ex[0] == "PING")
                    {
                        SendMessage("PONG " + ex[1]);
                        Console.WriteLine("PONG " + ex[1]);
                    }

                    //Splitting message and the meta data
                    string[] split1 = Data.Split(':');
                    if (split1.Length > 1)
                    {
                        //Splitting nick, type, chan and message
                        string[] split2 = split1[1].Split(' ');

                        //Nick consists of various things - we only want the nick itself
                        _nick = split2[0];
                        _nick = _nick.Split('!')[0];

                        //Type = PRIVMSG for normal messages. Only thing we need
                        _type = split2[1];

                        //Channel posted to
                        _channel = split2[2];

                        //Get message
                        if (split1.Length > 2)
                        {
                            for (int i = 2; i < split1.Length; i++)
                            {
                                _message += split1[i] + " ";
                            }
                        }

                        //Print nicely in seperate channel
                        if (_type == "PRIVMSG" && _channel.Contains("#"))
                        {
                            Program.mainForm.AddLine(_channel, "<" + DateTime.Now.ToString("HH:mm:ss") + "> <" + _nick + "> " + _message);
                        }

                        /*
                        * CHECK FOR ALL THE VARIOUS FUNCTIONS IN THE BOT - THIS IS THE FUN PART!
                        */
                        if (_message.StartsWith("!")) ProcessCmds(_message, _channel, _nick);

                        //Check all messages for blacklisted commands (and ascii art-crap) and spoilers (if enabled)
                        if(_channel.Contains('#'))
                        {
                            /*
                            //Antispoiler
                            if (antiSpoil[_channel] && spoilers.IsMatch(_message))
                            {
                                Console.WriteLine("Spoiler by " + _nick);
                                SendMessage("PRIVMSG " + _channel + " :/timeout " + _nick + " " + Properties.Settings.Default.TimeoutPeriod);
                                SendMessage("PRIVMSG " + _channel + " :" + _nick + " no spoilers pls!");
                            }
                            */

                            bool blacklistmatched = false;
                            //Check blacklist
                            foreach (KeyValuePair<string, Dictionary<string, int>> y in blackList)
                            {
                                foreach (KeyValuePair<string, int> x in y.Value)
                                {
                                    if (!blacklistmatched && (_message.Contains(x.Key) || _message.ToLower().Contains(x.Key.ToLower())))
                                    {
                                        blacklistmatched = true;
                                        if (x.Value == 1)
                                        {
                                            SendMessage("PRIVMSG " + _channel + " :/ban " + _nick);
                                            Console.WriteLine("Banning {0} because of banned word: {1}", _nick, _message);
                                            //SendMessage("PRIVMSG " + _channel + " :" + _nick + " received ban for blacklisted message");
                                        }
                                        else
                                        {
                                            SendMessage("PRIVMSG " + _channel + " :/timeout " + _nick + " " + Properties.Settings.Default.TimeoutPeriod);
                                            Console.WriteLine("Timing out {0} for {1}seconds because of banned word: {2}", _nick, Properties.Settings.Default.TimeoutPeriod, _message);
                                            //SendMessage("PRIVMSG " + _channel + " :" + _nick + " received 5 minute timeout for blacklisted message");
                                        }

                                        break; // Don't keep looping
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                listen.Abort();
            }
        }

        //Sends a message and adds it to the mainform
        public void SendMessage(string message)
        {
        {
            Writer.WriteLine(message);
            Program.mainForm.AddLine("", message);
            Writer.Flush();
        }
        }

        public void ProcessCmds(string _message, string _channel, string _nick)
        {
            //if (_message.StartsWith("!cmd")) Cmd_Custom(_message, _channel, _nick);
            if (_message.StartsWith("!permit")) Cmd_Permit(_message, _channel, _nick);
            if (_message.StartsWith("!blist")) Cmd_Blist(_message, _channel, _nick);
        }

        private void Cmd_Custom(string _message, string _channel, string _nick)
        {
            //Custom commands
            if (isOp(_channel, _nick))
            {
                string[] cmd = _message.Replace("!cmd", "").TrimStart(' ').TrimEnd(' ').Split(' ');
                if (cmd.Length > 1)
                {
                    //Gets name and text from the cmd posted
                    string _name = cmd[0];
                    string _text = String.Join(" ", cmd).Replace(_name, "").TrimEnd(' ').TrimStart(' ');

                    //Check if command exists in channel
                    bool _xist = false;
                    using (SQLiteConnection dbCon = new SQLiteConnection("Data Source=Database.sqlite;Version=3;"))
                    {
                        dbCon.Open();
                        string qSelect = "SELECT * FROM commands WHERE command = '" + _name + "' AND channel = '" + _channel + "'";
                        SQLiteCommand command = new SQLiteCommand(qSelect, dbCon);
                        SQLiteDataReader reader = command.ExecuteReader();
                        while (reader.Read() && _xist == false)
                        {
                            _xist = true;
                        }

                        //Either update, remove or add it
                        if (_xist)
                        {
                            string qUpdate = "";
                            if (_text == "remove")
                            {
                                qUpdate = "DELETE FROM commands WHERE channel = '" + _channel + "' AND command = '" + _name + "'";
                                Console.WriteLine("CMD DELETED!");
                                SendMessage("PRIVMSG " + _channel + " :Command " + _name + " deleted!");
                            }
                            else
                            {
                                qUpdate = "UPDATE commands SET text = '" + _text + "' WHERE channel = '" + _channel + "' AND command = '" + _name + "'";
                                Console.WriteLine("CMD UPDATED! (" + _name + " -> " + _text + ")");
                                SendMessage("PRIVMSG " + _channel + " :Command " + _name + " updated!");
                            }

                            SQLiteCommand cUpdate = new SQLiteCommand(qUpdate, dbCon);
                            cUpdate.ExecuteNonQuery();
                        }
                        else
                        {
                            string qInsert = "INSERT INTO commands (command, text, channel) VALUES ('" + _name + "', '" + _text + "', '" + _channel + "')";
                            SQLiteCommand cInsert = new SQLiteCommand(qInsert, dbCon);
                            cInsert.ExecuteNonQuery();
                            Console.WriteLine("CMD ADDED! (" + _name + " -> " + _text + ")");
                            SendMessage("PRIVMSG " + _channel + " :Command " + _name + " added!");
                        }
                        dbCon.Close();
                    }
                }
            }
        }
        
        private void Cmd_Poll(string _message, string _channel, string _nick)
        {
            //Poll commands
            if (isOp(_channel, _nick))
            {
                string[] cmd = _message.Replace("!poll", "").TrimStart(' ').TrimEnd(' ').Split(' ');
                //If you just enter !poll it posts the current status (not running or cmds to vote with)
                if (cmd[0] == "")
                {
                    if (pollOpen[_channel])
                    {
                        string answers = "";
                        foreach (KeyValuePair<string, Dictionary<string, int>> entry in pollVotes[_channel])
                        {
                            answers += "\"!" + entry.Key + "\", ";
                        }

                        answers = answers.TrimEnd(' ').TrimEnd(',');

                        SendMessage("PRIVMSG " + _channel + " :You can vote using " + answers);
                    }
                    else
                    {
                        SendMessage("PRIVMSG " + _channel + " :No poll running!");
                    }
                }
                //Open a poll with minimum 2 possible answers
                else if (cmd[0] == "open" && cmd.Length > 2)
                {
                    //Poll is already open
                    if (pollOpen[_channel])
                    {
                        SendMessage("PRIVMSG " + _channel + " :Poll is already open!");
                    }
                    else
                    {
                        //Inserts the answers into our kinda deep array with strings as keys w00t
                        Dictionary<string, Dictionary<string, int>> votes = new Dictionary<string, Dictionary<string, int>>();
                        for (int i = 1; i < cmd.Length; i++)
                        {
                            string tmpan = cmd[i].TrimEnd(' ').TrimStart(' ');
                            if (tmpan != "")
                            {
                                votes[tmpan] = new Dictionary<string, int>();
                            }
                        }

                        //Inserts this into our special vars
                        pollVotes[_channel] = votes;
                        pollOpen[_channel] = true;

                        //Posts the initial message - opened, how to vote
                        string answers = "";
                        foreach (KeyValuePair<string, Dictionary<string, int>> entry in pollVotes[_channel])
                        {
                            answers += "\"!" + entry.Key + "\", ";
                        }

                        answers = answers.TrimEnd(' ').TrimEnd(',');
                        SendMessage("PRIVMSG " + _channel + " :Poll started! You can vote using " + answers);
                    }
                }
                //Displays the current result (result/close) and can close if (close)
                else if (cmd[0] == "result" || cmd[0] == "close")
                {
                    //Poll not open
                    if (!pollOpen[_channel])
                    {
                        SendMessage("PRIVMSG " + _channel + " :No poll running!");
                    }
                    else
                    {
                        //Array with the results
                        Dictionary<string, int> res = new Dictionary<string, int>();
                        int total = 0;

                        //Find the number of votes for each answer and gets the total amount of votes
                        foreach (KeyValuePair<string, Dictionary<string, int>> entry in pollVotes[_channel])
                        {
                            res[entry.Key] = pollVotes[_channel][entry.Key].Count;
                            total += pollVotes[_channel][entry.Key].Count;
                        }

                        string votes = "";

                        //Then we round it and insert it nicely into a string
                        foreach (KeyValuePair<string, int> entry in res)
                        {
                            double pc = 0;
                            int perc = 0;

                            if (total > 0)
                            {
                                pc = (double)entry.Value / (double)total * (double)100;
                                perc = (int)Math.Round(pc);
                            }

                            votes += "\"" + entry.Key + "\" (" + perc + "%), ";
                        }

                        votes = votes.TrimEnd(' ').TrimEnd(',');

                        //We either just display the result or display result AND close the poll
                        if (cmd[0] == "result")
                        {
                            SendMessage("PRIVMSG " + _channel + " :Current result is: " + votes + " - Total votes: " + total);
                        }
                        else if (cmd[0] == "close")
                        {
                            pollOpen[_channel] = false;
                            pollVotes[_channel] = null;
                            SendMessage("PRIVMSG " + _channel + " :Poll closed! Result is: " + votes + " - Total votes: " + total);
                        }
                    }
                }
            }
        }
        
//        template function definition to copy paste        
//        private void Cmd_Blist(string _message, string _channel, string _nick)
//        private void Cmd_Blist(string _message, string _channel, string _nick)

        private void Cmd_Blist(string _message, string _channel, string _nick)
        {
            if (!_channel.StartsWith("#"))
            {
                _channel = "#" + _channel;
            }
            //Blacklists certain words
            if (isOp(_channel, _nick))
            {
                string[] cmd = _message.Replace("!blist", "").TrimStart(' ').TrimEnd(' ').Split(' ');
                if (cmd.Length > 1)
                {
                    //Add or remove
                    if (cmd[0] == "add" && cmd.Length > 2 || cmd[0] == "remove")
                    {
                        //If add you need to post the type aswell (ban, timeout)
                        string _btype = "";
                        string _btext = "";
                        if (cmd[0] == "add")
                        {
                            _btype = cmd[1];
                            _btext = cmd[2];

                            if (_btype == "ban")
                            {
                                _btype = "1";
                            }
                            else
                            {
                                _btype = "0";
                            }
                        }
                        else
                        {
                            _btext = cmd[1];
                        }
                        //Check to see if we can lowercase this string
                        string sOut = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(_btext));
                        if (_btext == sOut)
                        {
                            _btext = _btext.ToLower();
                        }

                        if (_btext.Length > 0)
                        {

                            //Check if blacklist already exists
                            bool _xist = false;
                            using (SQLiteConnection dbCon = new SQLiteConnection("Data Source=Database.sqlite;Version=3;"))
                            {
                                dbCon.Open();
                                string qSelect = "SELECT * FROM blacklist WHERE text = '" + _btext + "' AND channel = '" + _channel + "'";
                                SQLiteCommand command = new SQLiteCommand(qSelect, dbCon);
                                SQLiteDataReader reader = command.ExecuteReader();
                                while (reader.Read() && _xist == false)
                                {
                                    _xist = true;
                                }

                                //Either add or remove it
                                if (!_xist && cmd[0] == "add")
                                {
                                    string qUpdate = "INSERT INTO blacklist (type, text, channel) VALUES ('" + _btype + "', '" + _btext + "', '" + _channel + "')";
                                    Console.WriteLine("Blacklist added! (" + _btext + ")");

                                    SQLiteCommand cUpdate = new SQLiteCommand(qUpdate, dbCon);
                                    cUpdate.ExecuteNonQuery();

                                    blackList[_channel].Add(_btext, Convert.ToInt16(_btype));

                                    //SendMessage("PRIVMSG " + _channel + " :Blacklist for " + _btext + " added!");
                                    PrintToChatTab("Blacklist for " + _btext + " added!", _channel);
                                }
                                else if (_xist && cmd[0] == "remove")
                                {
                                    string qUpdate = "DELETE FROM blacklist WHERE text = '" + _btext + "' AND channel = '" + _channel + "'";
                                    //Console.WriteLine("Blacklist removed! (" + _btext + ")");

                                    SQLiteCommand cUpdate = new SQLiteCommand(qUpdate, dbCon);
                                    cUpdate.ExecuteNonQuery();

                                    blackList[_channel].Remove(_btext);

                                    //SendMessage("PRIVMSG " + _channel + " :Blacklist for " + _btext + " removed!");
                                    PrintToChatTab("Blacklist for " + _btext + " removed!", _channel);
                                }
                                dbCon.Close();
                            }
                        }
                    }
                }
                //Displays all the current blacklisted words
                else if (cmd[0] == "show")
                {
                    //Displays all the words nicely
                    string blist = "";
                    using (SQLiteConnection dbCon = new SQLiteConnection("Data Source=Database.sqlite;Version=3;"))
                    {
                        dbCon.Open();
                        string qSelect = "SELECT * FROM blacklist WHERE channel = '" + _channel + "'";
                        SQLiteCommand command = new SQLiteCommand(qSelect, dbCon);
                        SQLiteDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            blist += "\"" + reader["text"] + "\", ";
                        }
                        dbCon.Close();
                    }

                    blist = blist.TrimEnd(' ').TrimEnd(',');

                    // *todo* change this to a debug message
                    //SendMessage("PRIVMSG " + _channel + " :Blacklist is: " + blist);
                    PrintToChatTab("Blacklist is: " + blist, _channel);

                }
            }

        }
       
        private void Cmd_Permit(string _message, string _channel, string _nick)
        {
            //Adds a new op to the bot (can use all the above commands) - this can only be used by the channel owner
            if (isOwner(_channel, _nick) || _nick == BotName())
            {
                string[] cmd = _message.Replace("!permit", "").TrimStart(' ').TrimEnd(' ').Split(' ');
                Console.WriteLine(cmd.Length);
                //List command doesn't work... throws exception
                if (cmd[0] == "list")
                {
                    PrintToChatTab("Command !permit list not implemented yet", _channel);
                    /*
                    //Displays all permitted operators
                    string permitlist = "";
                    using (SQLiteConnection dbCon = new SQLiteConnection("Data Source=Database.sqlite;Version=3;"))
                    {
                        dbCon.Open();
                        string qSelect = "SELECT * FROM permits WHERE channel = '#" + _channel + "'";
                        SQLiteCommand commandlist = new SQLiteCommand(qSelect, dbCon);
                        SQLiteDataReader reader = commandlist.ExecuteReader();
                        while (reader.Read())
                        {
                            permitlist += "\"" + reader["text"] + "\", ";
                        }
                        dbCon.Close();
                    }

                    permitlist = permitlist.TrimEnd(' ').TrimEnd(',');

                    //SendMessage("PRIVMSG " + _channel + " :Blacklist is: " + blist);
                    PrintToChat(" : OP List is: " + permitlist, _channel);
                    */
                }

                if (cmd.Length == 2)
                {
                    //Check if user exists as mod
                    bool _xist = false;
                    using (SQLiteConnection dbCon = new SQLiteConnection("Data Source=Database.sqlite;Version=3;"))
                    {
                        dbCon.Open();
                        string qSelect = "SELECT * FROM permits WHERE user = '" + cmd[1] + "' AND channel = '" + _channel + "'";
                        SQLiteCommand command = new SQLiteCommand(qSelect, dbCon);
                        SQLiteDataReader reader = command.ExecuteReader();
                        while (reader.Read() && _xist == false)
                        {
                            _xist = true;
                            PrintToChatTab("User is currently OP: " + cmd[1], _channel);
                        }

                        //Either add or remove him
                        if (_xist && cmd[0] == "remove")
                        {
                            string qUpdate = "DELETE FROM permits WHERE user = '" + cmd[1] + "' AND channel = '" + _channel + "'";

                            SQLiteCommand cUpdate = new SQLiteCommand(qUpdate, dbCon);
                            cUpdate.ExecuteNonQuery();
                            PrintToChatTab("User Deleted: " + cmd[1], _channel);
                        }
                        else
                        {
                            string _username;
                            if (!_xist && cmd[0] == "add")
                            {
                                _username = cmd[1];
                                string qUpdate = "INSERT INTO permits (user, channel) VALUES ('" + _username + "', '" + _channel + "')";

                                SQLiteCommand cUpdate = new SQLiteCommand(qUpdate, dbCon);
                                cUpdate.ExecuteNonQuery();
                                PrintToChatTab("User Added: " + _username, _channel);

                            }
                        }
                        dbCon.Close();
                    }
                }

            }
        }

        // Blocked funcitons
        private void Cmd_Disabled(string _message, string _channel, string _nick)
        {
            /*
            //Antispoiler command - timeouts people posting spoilers
            else if (_message.StartsWith("!antispoil") && isOp(_channel, _nick))
            {
                string[] cmd = _message.Replace("!antispoil", "").TrimStart(' ').TrimEnd(' ').Split(' ');
                if (cmd[0] == "on")
                {
                    SendMessage("PRIVMSG " + _channel + " :Anti-spoiler enabled! Avoid typing result-like comments - you will be timed out.");
                    antiSpoil[_channel] = true;
                }
                else if (cmd[0] == "off")
                {
                    SendMessage("PRIVMSG " + _channel + " :Anti-spoiler disabled! Go crazy!");
                    antiSpoil[_channel] = false;
                }

            }


            //Displays all the custom commands created with !cmd
            else if (_message.StartsWith("!commands"))
            {
                string cmds = "";
                //Displays them nicely
                using (SQLiteConnection dbCon = new SQLiteConnection("Data Source=Database.sqlite;Version=3;"))
                {
                    dbCon.Open();
                    string qSelect = "SELECT * FROM commands WHERE channel = '" + _channel + "'";
                    SQLiteCommand command = new SQLiteCommand(qSelect, dbCon);
                    SQLiteDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        cmds += "!" + reader["command"] + " | ";
                    }
                    cmds = cmds.TrimEnd(' ').TrimEnd('|').TrimEnd(' ');
                    dbCon.Close();
                }
                SendMessage("PRIVMSG " + _channel + " :Commands are: " + cmds);
            }

            //Displays the current time in CEST
            else if (_message.StartsWith("!time"))
            {
                SendMessage("PRIVMSG " + _channel + " :Current time: " + DateTime.Now.ToString("HH:mm:ss") + " CEST");
            }

            //Check for votes and custom commands - votes will override custom cmds, since they are temporary
            else if (_message.StartsWith("!"))
            {
                string _cmd = _message.Replace("!", "").TrimEnd(' ').TrimStart(' ');

                //Votes
                if (pollVotes[_channel].ContainsKey(_cmd))
                {
                    string[] ans = _message.TrimEnd(' ').TrimStart(' ').Split(' ');
                    if (ans.Length > 1 && pollVotes[_channel].ContainsKey(ans[1]))
                    {
                        if (!pollVotes[_channel][ans[1]].ContainsKey(_nick))
                        {
                            foreach (KeyValuePair<string, Dictionary<string, int>> entry in pollVotes[_channel])
                            {
                                if (pollVotes[_channel][entry.Key].ContainsKey(_nick))
                                {
                                    pollVotes[_channel][entry.Key].Remove(_nick);
                                }
                            }
                            pollVotes[_channel][ans[1]].Add(_nick, 1);
                        }
                    }
                }
                //Custom cmds
                else
                {
                    //Check if it exists and post the text
                    using (SQLiteConnection dbCon = new SQLiteConnection("Data Source=Database.sqlite;Version=3;"))
                    {
                        dbCon.Open();
                        string query = "SELECT * FROM commands WHERE channel = '" + _channel + "' AND command = '" + _cmd + "'";
                        SQLiteCommand command = new SQLiteCommand(query, dbCon);
                        SQLiteDataReader reader = command.ExecuteReader();

                        string _text = "";

                        while (reader.Read())
                        {
                            _text = (string)reader["text"];
                        }

                        if (_text != "")
                        {
                            SendMessage("PRIVMSG " + _channel + " :" + _text);
                        }
                        dbCon.Close();
                    }
                }
            }
        */ 
        }

        public void PrintToChatTab(string _message, string _channel, string _nick = "*BOT_ECHO*")
        {
            Program.mainForm.AddLine(_channel, "<" + DateTime.Now.ToString("HH:mm:ss") + "> <" + _nick + "> " + _message);
        }
    }
}
