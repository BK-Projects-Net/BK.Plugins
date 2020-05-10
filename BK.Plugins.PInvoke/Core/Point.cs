using System.Runtime.InteropServices;

namespace BK.Plugins.PInvoke.Core
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Point
	{
		public int X;
		public int Y;
	}
}
