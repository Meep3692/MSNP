using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MSNP.Commands
{
	public static class CommandSerialization
	{
		public static readonly Dictionary<string, CommandProperties> commandProperties = new()
		{
			{ "VER", new(true, true, false) },
			{ "USR", new(true, true, false) },
			{ "CVR", new(true, true, false) },
			{ "MSG", new(false, true, true) },
			{ "ACK", new(true, true, false) },
		};

		public static string Serialize(Command command)
		{
			StringBuilder sb = new();
			sb.Append(command.Code);
			sb.Append(' ');
			if (commandProperties.ContainsKey(command.Code) && commandProperties[command.Code].HasTrIDFromClient)
			{
				sb.Append(command.TrID);
				sb.Append(' ');
			}
			for(int i = 0; i < command.Args.Length; i++)
			{
				sb.Append(command.Args[i]);
				sb.Append(' ');
			}
			if(commandProperties.ContainsKey(command.Code) && commandProperties[command.Code].HasPayload)
			{
				sb.Append(command.Payload.Length);
			}
			return sb.ToString();
		}

		public static Command Deserialize(String line)
		{
			Command command = new();
			//Read command
			if (line == null) return null;//I guess readline returns null if the stream closes
			//Regex to parse
			string pattern = @"^(?'Code'[A-Z0-9]{3}) +(?:(?'TrID'[0-9]+) +)?(?:(?'Arg'[^\s]+) *)+$";
			Match match = Regex.Match(line, pattern);
			command.Code = match.Groups["Code"].Value;
			command.TrID = uint.Parse("0" + match.Groups["TrID"]?.Value);//Kinda a hack if there is no TrID, the leading zero will zero it
			command.Args = match.Groups["Arg"].Captures.Select((capture) => capture.Value).ToArray();
			return command;
		}
	}
}
