using System;
using BK.Plugins.MouseHook.Core;
using System.Windows.Forms;
using BK.Plugins.MouseHookRx;

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
			var hook = new MouseHookRx();
			hook.SetHook();

			//HookAllEventHandlers(hook);
			HookObservable(hook);
		}

		private void HookObservable(MouseHookRx hookRx) => 
			hookRx.MouseObservable
				.Subscribe(param => Console.WriteLine(param));

		private void HookAllEventHandlers(MouseHookRx hookRx)
		{
			hookRx.LDownEvent += OnInvoke;
			hookRx.LUpEvent += OnInvoke;
			hookRx.LDoubleEvent += OnInvoke;

			hookRx.MDownEvent += OnInvoke;
			hookRx.MUpEvent += OnInvoke;
			hookRx.MDoubleEvent += OnInvoke;

			hookRx.RDownEvent += OnInvoke;
			hookRx.RUpEvent += OnInvoke;
			hookRx.RDoubleEvent += OnInvoke;

			hookRx.MoveEvent += OnInvoke;
			hookRx.WheelEvent += OnInvoke;

			hookRx.UnhandledEvent += OnInvoke;
			hookRx.GlobalEvent += OnInvoke;
		}

		private void OnInvoke(object sender, MouseParameter e) => Console.WriteLine(e);

		
	}
}