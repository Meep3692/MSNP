using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSNP.Commands
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class CommandAttribute : Attribute
	{
		public string Code;
		public bool TrID;
		public bool Payload;
	}
}
