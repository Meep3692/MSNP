using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSNP.Commands
{
	[Command(Code = "VER", TrID = true, Payload = false)]
	public class VER
	{
		[Argument(true, true)]
		public uint TrID;

		[Argument(true, true)]
		public Arglist Versions;
	}
}
