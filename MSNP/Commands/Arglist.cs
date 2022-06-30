using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSNP.Commands
{
	//Basically a string array wrapper with a better ToString
	public class Arglist : IEnumerable<string>
	{
		private string[] array;

		public Arglist(params string[] strings)
		{
			array = strings;
		}

		public string[] AsArray()
		{
			return array;
		}

		public IEnumerator<string> GetEnumerator()
		{
			return array.AsEnumerable().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return array.GetEnumerator();
		}

		public override string ToString()
		{
			return array.Aggregate((acc, str) => acc + " " + str);
		}
	}
}
