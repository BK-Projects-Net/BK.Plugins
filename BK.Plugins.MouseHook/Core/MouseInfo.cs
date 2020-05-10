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
		Move,
		Wheel,
		LeftButton, MiddleButton, RightButton,
		Up, Down,
		Single, Double
	}
}
