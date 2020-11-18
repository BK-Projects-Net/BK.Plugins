using System.Diagnostics;

namespace BK.Plugins.PInvoke
{
	[DebuggerStepThrough]
	internal static class PInvokeExtensions
	{
		internal static unsafe int GetLowWord(this int value) => ((ushort *) &value)[0];
		internal static unsafe int GetHighWord(this int value) => ((ushort *) &value)[1];

	}
}
