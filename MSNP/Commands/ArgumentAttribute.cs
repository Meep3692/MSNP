using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSNP.Commands
{
	[AttributeUsage(AttributeTargets.Field)]
	public class ArgumentAttribute : Attribute
	{
		public bool FromClient;
		public bool FromServer;
		public ArgumentAttribute(bool fromClient, bool fromServer)
		{
			FromClient = fromClient;
			FromServer = fromServer;
		}
	}
}
