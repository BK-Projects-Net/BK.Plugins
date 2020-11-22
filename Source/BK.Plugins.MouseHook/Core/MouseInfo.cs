using System;
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

	internal class MouseInfoFactory
	{
		

		internal MouseInfo Create(MouseHookType hookType, MSLLHOOKSTRUCT hookStruct)
		{
			MouseInfo result;
			int mouseData;

			switch (hookType)
			{
				case MouseHookType.WM_LBUTTONDOWN:
					return MouseInfo.LeftButton | MouseInfo.Down;
				case MouseHookType.WM_LBUTTONUP:
					return MouseInfo.LeftButton | MouseInfo.Up;
				case MouseHookType.WM_MOUSEMOVE:
					return MouseInfo.Move;

				case MouseHookType.WM_MOUSEWHEEL:
					return CreateWheelInfo(hookStruct);

				case MouseHookType.WM_RBUTTONDOWN:
					return MouseInfo.RightButton | MouseInfo.Down;
				case MouseHookType.WM_RBUTTONUP:
					return MouseInfo.RightButton | MouseInfo.Up;
				case MouseHookType.WM_MBUTTONDOWN:
					return MouseInfo.MiddleButton | MouseInfo.Down;
				case MouseHookType.WM_MBUTTONUP:
					return MouseInfo.MiddleButton | MouseInfo.Up;

				case MouseHookType.WM_XBUTTONDOWN:
					result = MouseInfo.Down;
					mouseData = hookStruct.mouseData;
					result = GetMouse4Or5(mouseData, ref result);
					return result;

				case MouseHookType.WM_XBUTTONUP:
					result = MouseInfo.Up;
					mouseData = hookStruct.mouseData;
					result = GetMouse4Or5(mouseData, ref result);
					return result;

				default:
					return MouseInfo.Unknown;
			}
		}

		private static MouseInfo CreateWheelInfo(MSLLHOOKSTRUCT hookStruct)
		{
			var info = MouseInfo.Wheel;

			if (hookStruct.mouseData > 0)      // up scroll
				info |= MouseInfo.Up;
			else if (hookStruct.mouseData < 0) // down scroll
				info |= MouseInfo.Down;

			return info;
		}

		private static MouseInfo GetMouse4Or5(in int mouseData, ref MouseInfo result)
		{
			if (mouseData.GetHighWord() == 0x01)
				result |= MouseInfo.Mouse4;
			else if (mouseData.GetHighWord() == 0x02)
				result |= MouseInfo.Mouse5;
			else result = MouseInfo.Unknown;
			return result;
		}
	}
}
