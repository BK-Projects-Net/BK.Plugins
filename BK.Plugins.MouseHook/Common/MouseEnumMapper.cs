using System.Collections.Generic;
using BK.Plugins.MouseHook.Core;
using BK.Plugins.PInvoke.Core;

namespace BK.Plugins.MouseHook.Common
{
	internal class MouseEnumMapper : FlagMapper
	{
		public MouseEnumMapper()
			: base(new Dictionary<int, int>
			{
				{ (int)MouseHookType.WM_MOUSEWHEEL, (int)PrimaryMouseInfo.Wheel},
				{ (int)MouseHookType.WM_MOUSEMOVE, (int)PrimaryMouseInfo.Move },
				{ (int)MouseHookType.WM_LBUTTONDOWN, (int)PrimaryMouseInfo.LeftButton | (int)PrimaryMouseInfo.Down },
				{ (int)MouseHookType.WM_LBUTTONUP, (int)PrimaryMouseInfo.LeftButton | (int)PrimaryMouseInfo.Up },			
				{ (int)MouseHookType.WM_RBUTTONDOWN, (int)PrimaryMouseInfo.RightButton | (int)PrimaryMouseInfo.Down },
				{ (int)MouseHookType.WM_RBUTTONUP, (int)PrimaryMouseInfo.RightButton | (int)PrimaryMouseInfo.Up },
				{ (int)MouseHookType.WM_MBUTTONDOWN, (int)PrimaryMouseInfo.MiddleButton | (int)PrimaryMouseInfo.Down },
				{ (int)MouseHookType.WM_MBUTTONUP, (int)PrimaryMouseInfo.MiddleButton | (int)PrimaryMouseInfo.Up },

			}) { }

		internal PrimaryMouseInfo Map(MouseHookType source) => (PrimaryMouseInfo)Map((int)source);
	}
}