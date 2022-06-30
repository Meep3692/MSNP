using MSNP.Commands;
using System;

namespace Test
{
	class Program
	{
		static void Main(string[] args)
		{
			VER ver = new VER()
			{
				Versions = new Arglist("MSNP8 CVR0")
			};
			Console.WriteLine(CommandSerialization.Serialize(ver, 7));
		}
	}
}
