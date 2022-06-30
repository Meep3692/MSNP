using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSNP
{
	public class Host
	{
		public string Hostname;
		public int Port;
		public Host(string hostString)
		{
			string[] vs = hostString.Split(':');
			Hostname = vs[0];
			Port = int.Parse(vs[1]);
		}

		public override string ToString()
		{
			return Hostname + ":" + Port;
		}
	}
}
