using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BK.Plugins.MouseHook.Core;

namespace MouseHook.Wpf
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		private string _lastEvent;

		public MainWindow()
		{
			InitializeComponent();
			DataContext = this;

			var hook = new BK.Plugins.MouseHook.MouseHook();
			hook.GlobalEvent += HookOnGlobalEvent;
			hook.SetHook();
		}

		private void HookOnGlobalEvent(object sender, MouseParameter e)
		{
			LastEvent = e.ToString();
		}

		public string LastEvent
		{
			get => _lastEvent;
			set => SetProperty(ref _lastEvent, in value);
		}

		private void SetProperty<T>(ref T field, in T value, [CallerMemberName] string caller = null)
		{
			if (EqualityComparer<T>.Default.Equals(field, value)) return;
			field = value;
			OnPropertyChanged(caller);
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
