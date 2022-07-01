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

		public Command(string code, uint TrID, params string[] args)
		{
			this.Code = code;
			this.TrID = TrID;
			this.Args = args;
		}

		public Command() { }
	}
}
