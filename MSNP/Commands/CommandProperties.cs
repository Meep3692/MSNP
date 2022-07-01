using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSNP.Commands
{
	public struct CommandProperties
	{
		public bool HasTrIDFromServer;
		public bool HasTrIDFromClient;
		public bool HasPayload;

		public CommandProperties(bool hasTrIDFromServer, bool hasTrIDFromClient, bool hasPayload)
		{
			HasTrIDFromServer = hasTrIDFromServer;
			HasTrIDFromClient = hasTrIDFromClient;
			HasPayload = hasPayload;
		}
	}
}
