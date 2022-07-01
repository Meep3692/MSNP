using MSNP.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MSNP
{
	public class MSNPConnection
	{

		public delegate void CommandEventHandler(object sender, CommandEventArgs e);
		public event CommandEventHandler CommandRecieved;

		//private Dictionary<string, Type> commands;
		private readonly MutexValue<uint> commandId;
		private readonly Dictionary<uint, SemaphoreSlim> commandSemaphores;
		private readonly Dictionary<uint, Command> commandResponses;
		private readonly MutexValue<TcpClient> client;
		private static readonly byte[] lineSep = { 13, 10 };
		private Thread listenThread;

		private string displayName;


		public MSNPConnection()
		{
			commandId = new(0);
			commandSemaphores = new();
			commandResponses = new();
			client = new(new TcpClient());
			CommandRecieved += MSGHandler;
		}

		private uint NextId()
		{
			uint nextId = 0;
			commandId.Do((id) => { nextId = id + 1; return nextId; });
			return nextId;
		}

		#region Connection

		public async Task Connect(Host dispatch, Uri passportNexus, string passport, string password)
		{
			connectToServer(dispatch.Hostname, dispatch.Port);
			string[] serverVersions = await SendVersions(new string[] { "MSNP8", "CVR0" });
			//TODO: check versions worked
			await SendCVR(passport);
			Command USR = new("USR", NextId(), "TWN", "I", passport);
			Command response = await SendCommand(USR);
			if(response.Code == "XFR")
			{
				Host NS = new(response.Args[1]);
				await Connect(NS, passportNexus, passport, password);
			}else if(response.Code == "USR")
			{
				string token = response.Args[2];
				string ticket = await PassportAuth(passportNexus, passport, password, token);
				USR = new("USR", NextId(), "TWN", "S", ticket);
				response = await SendCommand(USR);
				//TODO: check that response.Args[1] == "OK"
				displayName = HttpUtility.UrlDecode(response.Args[2]);

			}
		}

		private async Task<string> PassportAuth(Uri passportNexus, string passport, string password, string token)
		{
			//Send a request to get the login url
			HttpWebRequest request = WebRequest.CreateHttp(passportNexus);
			HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
			//We have to support the header "Passporturls" because of a deviation in escargot that has always been there and was never fixed
			string passportUrls = response.Headers["PassportURLs"] ?? response.Headers["Passporturls"];
			//Use regex to get DALogin
			string regex = @"(?'Pair'(?'Key'[\w-]+)=(?'Value'.*?))(?:,|$)";
			string daLogin = Regex.Matches(passportUrls, regex).Where((match) => match.Groups["Key"].Value == "DALogin").First().Groups["Value"].Value;
			UriBuilder ub = new UriBuilder(daLogin);
			ub.Scheme = "https";//It doesn't always specify this
			Uri daLoginUri = ub.Uri;
			//Make request to log in
			HttpWebRequest loginRequest = WebRequest.CreateHttp(daLoginUri);
			loginRequest.AllowAutoRedirect = true;
			loginRequest.Headers[HttpRequestHeader.Authorization] =
				string.Format("Passport1.4 OrgVerb=GET,OrgURL={0},sign-in={1},pwd={2},{3}",
				HttpUtility.UrlEncode("http://messenger.msn.com"),
				HttpUtility.UrlEncode(passport),
				HttpUtility.UrlEncode(password),
				token);
			HttpWebResponse loginResponse = (HttpWebResponse)loginRequest.GetResponse();
			//TODO: handle failed login
			string authinfoHeader = loginResponse.Headers["Authentication-Info"];
			string ticket = Regex.Matches(authinfoHeader, regex).Where((match) => match.Groups["Key"].Value == "from-PP").First().Groups["Value"].Value;
			ticket = ticket.Trim('\'');
			return ticket;
		}

		private void connectToServer(string hostname, int port)
		{
			client.Do((socket) =>
			{
				if (socket.Connected) socket.Close();
				if (listenThread != null) listenThread.Abort();
				socket.Connect(hostname, port);
				listenThread = new Thread(new ThreadStart(listen));
				listenThread.Start();
				return socket;
			});
		}

		#endregion Connection

		#region Commands

		//Send a command to the server and wait for the server's response if applicable
		async Task<Command?> SendCommand(Command command)
		{
			//Generate a unique id for this command
			//uint id = NextId();
			//Serialize for sending
			string commandString = CommandSerialization.Serialize(command);
			Console.WriteLine(">>> {0}", commandString);
			//Send command down the socket
			client.Do((socket) =>
			{
				byte[] commandBytes = Encoding.UTF8.GetBytes(commandString);
				socket.GetStream().Write(commandBytes);
				socket.GetStream().Write(lineSep);
				if(CommandSerialization.commandProperties.ContainsKey(command.Code) && CommandSerialization.commandProperties[command.Code].HasPayload)
				{
					socket.GetStream().Write(command.Payload);
				}
				return socket;
			});
			//If the command has a TrID that means we need to wait for a response
			if(CommandSerialization.commandProperties.ContainsKey(command.Code) && CommandSerialization.commandProperties[command.Code].HasTrIDFromClient)
			{
				//Create a semaphore we can wait on for the response
				SemaphoreSlim responseSem = new(0);
				commandSemaphores.Add(command.TrID, responseSem);
				await responseSem.WaitAsync();
				//The listen thread has now gotten a response and put it in the commandResponse table
				Command response = commandResponses[command.TrID];
				//Clean up
				commandResponses.Remove(command.TrID);
				commandSemaphores.Remove(command.TrID);
				return response;
			}
			//Return null if the command gets no response
			return null;
		}

		public async Task<string[]> SendVersions(string[] versions)
		{
			Command clientVer = new Command("VER", NextId(), versions);
			Command serverVer = await SendCommand(clientVer);
			return serverVer.Args;
		}

		public async Task<Command> SendCVR(string Passport)
		{
			string localeID = "0x1009";
			string osType = Environment.OSVersion.Platform.ToString();
			string osVersion = Environment.OSVersion.Version.ToString();
			string arch = RuntimeInformation.ProcessArchitecture.ToString();
			string clientName = Assembly.GetEntryAssembly().GetName().Name;
			string clientVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
			string MSMSGS = "MSMSGS";
			Command command = new("CVR", NextId(), localeID, osType, osVersion, arch, clientName, clientVersion, MSMSGS, Passport);
			return await SendCommand(command);
		}

		#endregion Commands

		#region Handlers

		private void MSGHandler(object sender, CommandEventArgs e)
		{
			if(e.command.Code == "MSG")
			{
				string from = e.command.Args[0];
				string fromDisplay = e.command.Args[1];
				string message = Encoding.UTF8.GetString(e.command.Payload);
				Console.WriteLine(message);
			}
		}

		#endregion Handlers

		//Listen thread
		private void listen()
		{
			//Pull the client out of the mutex, the mutex is for writing so we can do this weird thing
			TcpClient socket = null;
			client.Do((tc) => { socket = tc; return socket; });
			BufferedStream bs = new(socket.GetStream());
			while (socket.Connected)
			{
				string commandLine = "";
				{//Reading a line of text without StreamReader because StreamReader's buffering fucks everything up for payload commands
					List<byte> commandBytes = new List<byte>();
					int next;
					while ((next = bs.ReadByte()) != 13)
					{
						commandBytes.Add((byte)next);
					}
					bs.ReadByte();
					commandLine = Encoding.UTF8.GetString(commandBytes.ToArray());
				}
				Command command = CommandSerialization.Deserialize(commandLine);
				if (command != null)
				{
					//Read payload
					if (CommandSerialization.commandProperties.ContainsKey(command.Code) && CommandSerialization.commandProperties[command.Code].HasPayload)
					{
						int length = int.Parse(command.Args.Last());
						command.Payload = new byte[length];
						bs.Read(command.Payload, 0, length);
						command.Args = command.Args.SkipLast(1).ToArray();//Remove payload size from args list
					}
					Console.WriteLine("<<< {0}", commandLine);
					CommandRecieved?.Invoke(this, new CommandEventArgs() { command = command });
					uint id = command.TrID;
					if (id != 0)
					{
						commandResponses.Add(id, command);
						commandSemaphores[id].Release();
					}
					
				}
			}
			Console.WriteLine("<o> Server Closes Connection");
		}
	}
}
