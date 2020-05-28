using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using BK.Plugins.MouseHook.Core;
using BK.Plugins.PInvoke.Core;

namespace BK.Plugins.MouseHook.Common
{
	internal class MouseDictionaryMapper : DictionaryMapper<MouseHookType, MouseInfo>
	{
		public MouseDictionaryMapper()
			: base(new Dictionary<MouseHookType, MouseInfo>
			{
				{ MouseHookType.WM_MOUSEWHEEL,  MouseInfo.Wheel},
				{ MouseHookType.WM_MOUSEMOVE,   MouseInfo.Move },
				{ MouseHookType.WM_LBUTTONDOWN, MouseInfo.LeftButton   | MouseInfo.Down },
				{ MouseHookType.WM_LBUTTONUP,   MouseInfo.LeftButton   | MouseInfo.Up },			
				{ MouseHookType.WM_RBUTTONDOWN, MouseInfo.RightButton  | MouseInfo.Down },
				{ MouseHookType.WM_RBUTTONUP,   MouseInfo.RightButton  | MouseInfo.Up },
				{ MouseHookType.WM_MBUTTONDOWN, MouseInfo.MiddleButton | MouseInfo.Down },
				{ MouseHookType.WM_MBUTTONUP,   MouseInfo.MiddleButton | MouseInfo.Up },

			}) { }

		public override MouseInfo FallBack => MouseInfo.Unknown;
		internal new MouseInfo Map(MouseHookType source) => base.Map(source);

	}
}