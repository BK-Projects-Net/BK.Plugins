using System.Runtime.InteropServices;

namespace BK.Plugins.PInvoke.Core
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct MouseHookStruct
	{
		public Point Point;
		public int HWnd;
		public int WHitTestCode;
		public int DwExtraInfo;
	}
}