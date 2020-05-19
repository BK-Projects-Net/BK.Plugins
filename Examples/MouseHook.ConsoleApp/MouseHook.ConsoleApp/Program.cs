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
			var hook = BK.Plugins.MouseHook.Logic.MouseHook.Instance;
			hook.LDoubleEvent += LDoubleEvent;
			hook.MDoubleEvent += MDoubleEvent;
			hook.RDoubleEvent += RDoubleEvent;
			hook.SetHook();
		}

		private void LDoubleEvent(object sender, MouseParameter e)
		{
			
		}

		private void MDoubleEvent(object sender, MouseParameter e)
		{
			
		}

		private void RDoubleEvent(object sender, MouseParameter e)
		{
			
		}

		
	}
}