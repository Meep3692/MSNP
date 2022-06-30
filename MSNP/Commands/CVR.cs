using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSNP.Commands
{
	[Command(Code = "CVR", TrID = true, Payload = false)]
	public class CVR
	{
		[Argument(true, true)]
		public uint TrID;

		[Argument(true, false)]
		public string LocaleID;

		[Argument(true, false)]
		public string OSType;

		[Argument(true, false)]
		public string OSVer;

		[Argument(true, false)]
		public string Arch;

		[Argument(true, false)]
		public string ClientName;

		[Argument(true, false)]
		public string ClientVer;

		[Argument(true, false)]
		public string MSMSGS;

		[Argument(true, false)]
		public string Passport;

		[Argument(false, true)]
		public string RecommendedVer;

		[Argument(false, true)]
		public string RecommendedVer2;

		[Argument(false, true)]
		public string MinimumVer;

		[Argument(false, true)]
		public string DownloadUrl;

		[Argument(false, true)]
		public string InfoUrl;
	}
}
