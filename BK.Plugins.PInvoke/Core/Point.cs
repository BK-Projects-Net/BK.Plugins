using System.Runtime.InteropServices;

namespace BK.Plugins.PInvoke.Core
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct Point
	{
		public int X;
		public int Y;
	}
}
