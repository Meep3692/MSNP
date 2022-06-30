using MSNP.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MSNP
{
	public class MSNPConnection
	{

		public delegate void CommandEventHandler(object sender, CommandEventArgs e);
		public event CommandEventHandler CommandRecieved;

		//private Dictionary<string, Type> commands;
		private MutexValue<uint> commandId;
		private Dictionary<uint, SemaphoreSlim> commandSemaphores;
		private Dictionary<uint, object> commandResponses;
		private MutexValue<TcpClient> client;
		private static readonly byte[] lineSep = { 13, 10 };
		private Thread listenThread;

		private uint NextId()
		{
			uint nextId = 0;
			commandId.Do((id) => { nextId = id + 1; return nextId; });
			return nextId;
		}

		public async Task Connect(Host dispatch)
		{
			connectToServer(dispatch.Hostname, dispatch.Port);
			string[] serverVersions = await SendVersions(new string[] { "MSNP8", "CVR0" });
			//TODO: check versions worked

		}

		private void connectToServer(string hostname, int port)
		{
			client.Do((socket) =>
			{
				if (socket.Connected) socket.Close();
				socket.Connect(hostname, port);
				listenThread = new Thread(new ThreadStart(listen));

				return socket;
			});
		}

		//Send a command to the server and wait for the server's response if applicable
		async Task<object?> SendCommand(object command)
		{
			//Generate a unique id for this command
			uint id = NextId();
			//Serialize for sending
			string commandString = CommandSerialization.Serialize(command, id);
			//Send command down the socket
			client.Do((socket) =>
			{
				byte[] command = Encoding.UTF8.GetBytes(commandString);
				socket.GetStream().Write(command);
				socket.GetStream().Write(lineSep);
				return socket;
			});
			//If the command has a TrID that means we need to wait for a response
			if(((CommandAttribute)command.GetType().GetCustomAttributes(typeof(CommandAttribute), false).First()).TrID)
			{
				//Create a semaphore we can wait on for the response
				SemaphoreSlim responseSem = new(0);
				commandSemaphores.Add(id, responseSem);
				await responseSem.WaitAsync();
				//The listen thread has now gotten a response and put it in the commandResponse table
				object response = commandResponses[id];
				//Clean up
				commandResponses.Remove(id);
				commandSemaphores.Remove(id);
				return response;
			}
			//Return null if the command gets no response
			return null;
		}

		#region Commands

		public async Task<string[]> SendVersions(string[] versions)
		{
			VER clientVer = new VER()
			{
				Versions = new Arglist(versions)
			};
			VER serverVer = (VER)await SendCommand(clientVer);
			return serverVer.Versions.AsArray();
		}

		#endregion Commands

		//Listen thread
		private void listen()
		{
			//Pull the client out of the mutex, the mutex is for writing so we can do this weird thing
			TcpClient socket = null;
			client.Do((tc) => { socket = tc; return socket; });
			while (socket.Connected)
			{
				object command = CommandSerialization.Deserialize(socket.GetStream());
				CommandRecieved?.Invoke(this, new CommandEventArgs() { command = command });
				uint? id = CommandSerialization.GetCommandTrID(command);
				if (id != null)
				{
					commandResponses.Add((uint)id, command);
					commandSemaphores[(uint)id].Release();
				}

			}
		}
	}
}
