using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MSNP
{
	public class MutexValue<T>
	{
		private T value;
		private Mutex mut;

		public void Do(Func<T, T> func)
		{
			mut.WaitOne();
			value = func(value);
			mut.ReleaseMutex();
		}
	}
}
