using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using BK.Plugins.PInvoke;
using BK.Plugins.PInvoke.Core;

namespace BK.Plugins.MouseHook.Core
{
	[Flags]
	public enum MouseInfo
	{
		Unknown = 0,
		Move = 1,
		Wheel = 2,
		LeftButton = 4, MiddleButton = 8, RightButton = 16,
		Mouse4 = 32, Mouse5 = 64,
		Up = 128, Down = 256,
		Double = 512
	}

	internal static class MouseInfoFactory
	{
		internal static MouseInfo CreateWheelInfo(MSLLHOOKSTRUCT hookStruct)
		{
			var info = MouseInfo.Wheel;
						
			if (hookStruct.mouseData > 0)      // up scroll
				info |= MouseInfo.Up;
			else if (hookStruct.mouseData < 0) // down scroll
				info |= MouseInfo.Down;

			return info;
		}

	}
}
