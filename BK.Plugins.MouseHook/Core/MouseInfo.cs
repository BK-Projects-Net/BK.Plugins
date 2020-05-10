using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.Plugins.MouseHook.Core
{
	[Flags]
	public enum MouseInfo
	{
		Unknown = 0,
		Move = 1,
		Wheel = 2,
		LeftButton = 4, MiddleButton = 8, RightButton = 16,
		Up = 32, Down = 64,
		Single = 128, Double = 256
	}
}
