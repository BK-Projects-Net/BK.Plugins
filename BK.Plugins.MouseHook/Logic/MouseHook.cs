using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;
using BK.Plugins.MouseHook.Core;
using BK.Plugins.MouseHook.Extensons;
using BK.Plugins.PInvoke.Core;

namespace BK.Plugins.MouseHook.Logic
{
	public sealed class MouseHook : MouseHookBase<MouseHook>
	{
		public IObservable<MouseParameter> MouseObservable { get; private set; }
		private readonly Subject<Unit> _unHookIndicator = new Subject<Unit>();
		private CompositeDisposable _disposable = new CompositeDisposable();

		private MouseHook() { }

		public EventHandler<MouseParameter> LDoubleEvent;
		public EventHandler<MouseParameter> MDoubleEvent;
		public EventHandler<MouseParameter> RDoubleEvent;

		public override void SetHook()
		{
			base.SetHook();
			_disposable = new CompositeDisposable();

			var mouseParamFilter = new Func<MouseHookType, bool>(type =>
				type == MouseHookType.WM_LBUTTONDOWN || type == MouseHookType.WM_MBUTTONDOWN||
				type == MouseHookType.WM_RBUTTONDOWN);

			//_hook = CreateMouseObservable().Where(mouseParamFilter)
			//	.Buffer(DoubleClickTime)
			//	.Where(buffer => buffer.Count > 1)
			//	.Subscribe(buffer =>
			//	{
			//		if (buffer.Count == 0) return;
			//		var item = buffer.GroupBy(type => type)
			//			.OrderBy(group => group.Count())
			//			.Take(1).First().First();

			//		var mouseInfo = _dictionaryMapper.Map(item);
			//		var mouseParameter = new MouseParameter(mouseInfo,);

			//		if(mouseInfo == (MouseInfo.LeftButton | MouseInfo.Down))
			//			LDoubleEvent?.Invoke(this, parameter);
			//		else if(mouseInfo == MouseInfo.MDown)
			//			MDoubleEvent?.Invoke(item.MouseInfo, parameter);
			//		else if(mouseInfo == MouseInfo.RDown)
			//			RDoubleEvent?.Invoke(item.MouseInfo,parameter);

			//	},  exception => Console.WriteLine(exception.Message), () => Console.WriteLine("Completed"));
		}

		internal override void MouseClickDelegateTemplateMethod(MouseInfo info, Point point)
		{
			
		}

		public override void UnHook()
		{
			base.UnHook();
			_unHookIndicator.OnNext(Unit.Default);
			_unHookIndicator.OnCompleted();
		}

		private IObservable<MouseHookType> CreateMouseObservable() => 
			Observable.FromEvent<EventHandler<MouseHookType>, MouseHookType>(Conversion, 
				h => MouseHookInternal += h, 
				h => MouseHookInternal -= h);

		private EventHandler<MouseHookType> Conversion(Action<MouseHookType> arg) => 
			new Func<EventHandler<MouseHookType>>(() => (sender, parameter) => arg(parameter)).Invoke();
	}
}