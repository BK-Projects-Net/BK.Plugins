using System.Runtime.InteropServices;

namespace BK.Plugins.PInvoke.Core
{

	// https://docs.microsoft.com/en-us/windows/win32/api/windef/ns-windef-point
	// http://pinvoke.net/default.aspx/Structures/POINT.html
	[StructLayout(LayoutKind.Sequential)]
	public struct Point
	{
		public int X;
		public int Y;
	}
}