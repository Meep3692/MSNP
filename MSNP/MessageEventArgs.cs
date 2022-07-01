using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSNP
{
	public class MessageEventArgs : EventArgs
	{
		public Dictionary<string, string> Headers;
		public string Body;//TODO: can the body be something other than text?
	}
}
