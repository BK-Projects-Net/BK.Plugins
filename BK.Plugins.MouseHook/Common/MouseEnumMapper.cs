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
				{ (int)MouseHookType.WM_MOUSEWHEEL, (int)MouseInfo.Wheel},
				{ (int)MouseHookType.WM_MOUSEMOVE, (int)MouseInfo.Move },
				{ (int)MouseHookType.WM_LBUTTONDOWN, (int)MouseInfo.LeftButton | (int)MouseInfo.Down },
				{ (int)MouseHookType.WM_LBUTTONUP, (int)MouseInfo.LeftButton | (int)MouseInfo.Up },			
				{ (int)MouseHookType.WM_RBUTTONDOWN, (int)MouseInfo.RightButton | (int)MouseInfo.Down },
				{ (int)MouseHookType.WM_RBUTTONUP, (int)MouseInfo.RightButton | (int)MouseInfo.Up },
				{ (int)MouseHookType.WM_MBUTTONDOWN, (int)MouseInfo.MiddleButton | (int)MouseInfo.Down },
				{ (int)MouseHookType.WM_MBUTTONUP, (int)MouseInfo.MiddleButton | (int)MouseInfo.Up },

			}) { }

		internal MouseInfo Map(MouseHookType source) => (MouseInfo)Map((int)source);
	}
}