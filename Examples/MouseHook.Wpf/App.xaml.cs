using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using System.Windows;
using BK.Plugins.MouseHook.Core;

namespace MouseHook.Wpf
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			MainWindow = new MainWindow();
			MainWindow.Show();
			
			var hook = BK.Plugins.MouseHook.Logic.MouseHook.Instance;
			hook.LDownEvent += HookOnLDownEvent;

			hook.SubscribeOnScheduler = new DispatcherScheduler(Dispatcher);
			hook.ObserveOnScheduler = new DispatcherScheduler(Dispatcher);

			hook.SetHook();

		}

		private void HookOnLDownEvent(object sender, MouseParameter e)
		{
			
		}
	}
}
