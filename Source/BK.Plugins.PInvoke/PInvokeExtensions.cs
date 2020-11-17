using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.Plugins.PInvoke
{
	internal static class PInvokeExtensions
	{
		internal static unsafe int GetLowWord(this int value) => ((ushort *) &value)[0];
		internal static unsafe int GetHighWord(this int value) => ((ushort *) &value)[1];

	}
}
