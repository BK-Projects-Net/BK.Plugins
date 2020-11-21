using System.Diagnostics;

namespace BK.Plugins.PInvoke
{
	public static class PInvokeExtensions
	{
		public static unsafe int GetLowWord(this int value) => ((ushort *) &value)[0];
		public static unsafe int GetHighWord(this int value) => ((ushort *) &value)[1];

	}
}
