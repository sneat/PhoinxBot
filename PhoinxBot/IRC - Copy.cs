using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;


namespace PhoinxBot
{
    public class ServerData
    {
        string host;
        int port;
        public string ServerURL
        {
            get
            {
                return host;
            }
        }
        public int ServerPort
        {
            get
            {
                return port;
            }
        }
        public ServerData(string host, int port)
        {
            this.host = host;
            this.port = port;
        }
    }

    class IRC
    {
        string ircHost;
        int ircPort;
        string nick;
        string ident;
        string realname;

        TcpClient serverconnection;
        StreamReader serverreader;
        StreamWriter serverwriter;

        public IRC(ServerData server, string nick, string ident, string realname)
        {
            this.ircHost = server.ServerURL;
            this.ircPort = server.ServerPort;
            this.nick = nick;
            this.ident = ident;
            this.realname = realname;
            this.serverconnection = new TcpClient();
        }

        public void Connect()
        {
            serverconnection.Connect(ircHost, ircPort);

            serverreader = new StreamReader(serverconnection.GetStream());
            serverwriter = new StreamWriter(serverconnection.GetStream());

            serverwriter.WriteLine("PASS 123qweasd");

            Console.WriteLine("Connecting");

            Thread thread = new Thread(new ThreadStart(Listen));
            thread.Name = "Listen thread";
            thread.Start();
        }

        private void Listen()
        {
            Console.WriteLine("Establishing");
            while (serverconnection.Connected)
            {
                Console.WriteLine("Preparing");
                string message = serverreader.ReadLine();
                Console.WriteLine(message);
            }
        }

        public void SendMessage(string channel, string message)
        {
            if (serverconnection.Connected)
            {
                serverwriter.WriteLine("PRIVMSG", channel, message);
            }
        }

    }
}
