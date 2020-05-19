using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BK.Plugins.MouseHook.Core;
using BK.Plugins.MouseHook.Logic;
using System.Windows.Forms;

namespace MouseHook.ConsoleApp
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			var hook = new MouseHookLogic();
			hook.MouseHookEvent += HookOnMouseHookEvent;
			hook.SetHook();
			
			Application.Run();
			
		}

		private static void HookOnMouseHookEvent(object sender, MouseParameter e)
		{
			
		}
	}
}