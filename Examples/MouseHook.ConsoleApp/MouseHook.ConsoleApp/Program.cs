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
			var hook = new MouseHookRx();
			//HookAllEventHandlers(hook);
			// HookGlobalEventHandler(hook);

			hook.SetHook();
			HookObservable(hook);
			
		}

		private void HookObservable(BK.Plugins.MouseHook.Logic.MouseHookRx hookRx)
		{
			hookRx.MouseObservable.Subscribe(param =>
			{
				Console.WriteLine(param);
			});
		}

		private void HookAllEventHandlers(BK.Plugins.MouseHook.Logic.MouseHookRx hookRx)
		{
			hookRx.LDownEvent += OnLDownEvent;
			hookRx.LUpEvent += OnLUpEvent;
			hookRx.LDoubleEvent += OnLDoubleEvent;

			hookRx.MDownEvent += OnMDownEvent;
			hookRx.MUpEvent += OnMUpEvent;
			hookRx.MDoubleEvent += OnMDoubleEvent;

			hookRx.RDownEvent += OnRDownEvent;
			hookRx.RUpEvent += OnRUpEvent;
			hookRx.RDoubleEvent += OnRDoubleEvent;

			hookRx.MoveEvent += OnMoveEvent;
			hookRx.WheelEvent += OnWheelEvent;

			hookRx.UnhandledEvent += OnUnhandledEvent;
		}

		private void HookGlobalEventHandler(BK.Plugins.MouseHook.Logic.MouseHookRx hookRx)
		{
			hookRx.GlobalEvent += OnGlobalEvent;
		}

		#region Handlers
		private void OnGlobalEvent(object sender, MouseParameter e) => Console.WriteLine(e);
		private void OnUnhandledEvent(object sender, MouseParameter e) => Console.WriteLine(e);
		private void OnWheelEvent(object sender, MouseParameter e) => Console.WriteLine(e);
		private void OnMoveEvent(object sender, MouseParameter e) => Console.WriteLine(e);
		private void OnRUpEvent(object sender, MouseParameter e) => Console.WriteLine(e);
		private void OnRDownEvent(object sender, MouseParameter e) => Console.WriteLine(e);
		private void OnMUpEvent(object sender, MouseParameter e) => Console.WriteLine(e);
		private void OnMDownEvent(object sender, MouseParameter e) => Console.WriteLine(e);
		private void OnLUpEvent(object sender, MouseParameter e) => Console.WriteLine(e);
		private void OnLDownEvent(object sender, MouseParameter e) => Console.WriteLine(e);
		private void OnLDoubleEvent(object sender, MouseParameter e) => Console.WriteLine(e);
		private void OnMDoubleEvent(object sender, MouseParameter e) => Console.WriteLine(e);
		private void OnRDoubleEvent(object sender, MouseParameter e) => Console.WriteLine(e);

		#endregion
		
	}
}