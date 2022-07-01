using MSNP;
using MSNP.Commands;
using System;

namespace Test
{
	class Program
	{
		static void Main(string[] args)
		{
			MSNPConnection connection = new MSNPConnection();
			connection.Connect(new("m1.escargot.chat:1863"), new("https://m1.escargot.chat/nexus-mock"), "Silverhawk@escargot.chat", "FifteenCars").Wait();
		}
	}
}
