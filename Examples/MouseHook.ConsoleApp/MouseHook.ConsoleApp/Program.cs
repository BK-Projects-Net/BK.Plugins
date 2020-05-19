using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BK.Plugins.MouseHook.Core;
using BK.Plugins.MouseHook.Logic;
using System.Windows.Forms;

// https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.application.run?view=netcore-3.1#System_Windows_Forms_Application_Run_System_Windows_Forms_ApplicationContext_

namespace MouseHook.ConsoleApp
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			var context = new AppContext();
			Application.Run(context);

		}

	}

	class AppContext : ApplicationContext
	{
		public AppContext()
		{
			var hook = new MouseHookLogic();
			hook.MouseHookEvent += HookOnMouseHookEvent;
			hook.SetHook();
		}

		private static void HookOnMouseHookEvent(object sender, MouseParameter e)
		{
			
		}
	}
}