using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSNP.Commands
{
	public class Command
	{
		public string Code;
		public uint TrID;
		public string[] Args;
		public byte[] Payload;
	}
}
