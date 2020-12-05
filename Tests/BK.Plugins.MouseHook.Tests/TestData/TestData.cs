using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BK.Plugins.PInvoke.Core;

namespace BK.Plugins.MouseHook.Tests.TestData
{
	class TestData
	{
		internal static MSLLHOOKSTRUCT GetMouse4() => new MSLLHOOKSTRUCT {time = (int) DateTime.Now.Ticks, mouseData = 0x010000 };
		internal static MSLLHOOKSTRUCT GetMouse5() => new MSLLHOOKSTRUCT {time = (int) DateTime.Now.Ticks, mouseData = 0x020000};

	}
}
