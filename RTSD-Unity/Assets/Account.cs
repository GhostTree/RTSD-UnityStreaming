using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiblinphonedotNET
{
    public class Account
    {
		public string Username {get; set;}
		public string Password {get; set;}
		public string Server {get; set;}
		public int Port {get; set;}

		public string Identity
		{
			get
			{
				return "sip:" + this.Username + "@" + this.Server;
			}
		}

		public Account(string username, string password, string server, int port)
		{
			this.Username = username;
			this.Password = password;
			this.Server = server;
			this.Port = port;
		}

		public Account(string username, string password, string server) : this(username, password, server, 5060) {}
    }
}
