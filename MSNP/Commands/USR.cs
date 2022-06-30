using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSNP.Commands
{
	[Command(Code = "USR", TrID = true, Payload = false)]
	public class USR
	{
		[Argument(true, true)]
		public uint TrID;

		[Argument(true, true)]
		public string AuthSystem;

		[Argument(true, true)]
		public string Initial;

		[Argument(true, false)]
		public string Passport;

		[Argument(false, true)]
		public string TweenAuthString;
	}
}
