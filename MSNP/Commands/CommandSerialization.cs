using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MSNP.Commands
{
	public static class CommandSerialization
	{
		public static string Serialize(object command, uint id)
		{
			CommandAttribute attribute = (CommandAttribute)command.GetType().GetCustomAttributes(typeof(CommandAttribute), false).First();
			string commandString = "";
			commandString += attribute.Code + " ";
			if (attribute.TrID)
			{
				commandString += id + " ";
			}
			string args = command.GetType().GetFields()
				.Where((field) => (field.GetCustomAttribute<ArgumentAttribute>() != null) && field.Name != "TrID")
				.Select((field) => field.GetValue(command).ToString())
				.Aggregate((acc, val) => acc + " " + val);
			commandString += args;
			return commandString;
		}

		public static object Deserialize(Stream stream)
		{
			throw new NotImplementedException();
		}

		public static uint? GetCommandTrID(object command)
		{
			CommandAttribute attribute = (CommandAttribute)command.GetType().GetCustomAttributes(typeof(CommandAttribute), false).First();
			if (attribute.TrID)
			{
				return (uint?)command.GetType().GetFields().Where((field) => (field.GetCustomAttribute<ArgumentAttribute>() != null) && field.Name == "TrID").First()?.GetValue(command);
			}
			return null;
		}
	}
}
