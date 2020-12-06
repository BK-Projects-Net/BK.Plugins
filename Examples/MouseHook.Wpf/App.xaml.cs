using System.Windows;

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


		}

	}
}
