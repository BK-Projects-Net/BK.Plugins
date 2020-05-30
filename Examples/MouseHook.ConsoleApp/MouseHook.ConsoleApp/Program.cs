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
			HookAllEventHandlers(hook);
			// HookGlobalEventHandler(hook);

			hook.SetHook();
			//HookObservable(hook);
			
		}

		private void HookObservable(BK.Plugins.MouseHook.Logic.MouseHook hook)
		{
			hook.MouseObservable.Subscribe(param =>
			{
				Console.WriteLine(param);
			});
		}

		private void HookAllEventHandlers(BK.Plugins.MouseHook.Logic.MouseHook hook)
		{
			hook.LDownEvent += OnLDownEvent;
			hook.LUpEvent += OnLUpEvent;
			hook.LDoubleEvent += OnLDoubleEvent;

			hook.MDownEvent += OnMDownEvent;
			hook.MUpEvent += OnMUpEvent;
			hook.MDoubleEvent += OnMDoubleEvent;

			hook.RDownEvent += OnRDownEvent;
			hook.RUpEvent += OnRUpEvent;
			hook.RDoubleEvent += OnRDoubleEvent;

			hook.MoveEvent += OnMoveEvent;
			hook.WheelEvent += OnWheelEvent;

			hook.UnhandledEvent += OnUnhandledEvent;
		}

		private void HookGlobalEventHandler(BK.Plugins.MouseHook.Logic.MouseHook hook)
		{
			hook.GlobalEvent += OnGlobalEvent;
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