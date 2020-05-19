using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;
using BK.Plugins.MouseHook.Core;
using BK.Plugins.MouseHook.Extensons;

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

			MouseObservable = Observable.FromEvent<EventHandler<MouseParameter>, MouseParameter>(
					conversion: (conversion) => (sender, arg) => conversion.Invoke(arg),
					addHandler: h => MouseHookEvent += h,
					removeHandler: h => MouseHookEvent -= h)
				.TakeUntil(_unHookIndicator);

			var doubleClickObservable = MouseObservable
				.Where(param => !param.Is(MouseInfo.Move) && !param.Is(MouseInfo.Wheel) && !param.Is(MouseInfo.Unknown))
				.Window(DoubleClickTime).Switch();

			var leftDoubleObs = doubleClickObservable.Window(2).Switch()
				.Aggregate((current, next) => current.Is(MouseInfo.LeftButton) && next.Is(MouseInfo.LeftButton)
					? next.ToDoubleClick()
					: next)
				.Where(param => param.Is(MouseInfo.Double))
				.Do(param => LDoubleEvent?.Invoke(this, param))
				.Subscribe()
				.DisposeWith(_disposable);

			var middleDoubleObs = doubleClickObservable.Window(2).Switch()
				.Aggregate((current, next) => current.Is(MouseInfo.MiddleButton) && next.Is(MouseInfo.MiddleButton)
					? next.ToDoubleClick()
					: next)
				.Where(param => param.Is(MouseInfo.Double))
				.Do(param => MDoubleEvent?.Invoke(this, param))
				.Subscribe()
				.DisposeWith(_disposable);

			var rightDoubleObs = doubleClickObservable.Window(2).Switch()
				.Aggregate((current, next) => current.Is(MouseInfo.RightButton) && next.Is(MouseInfo.RightButton)
					? next.ToDoubleClick()
					: next)
				.Where(param => param.Is(MouseInfo.Double))
				.Do(param => RDoubleEvent?.Invoke(this, param))
				.Subscribe()
				.DisposeWith(_disposable);
		}

		public override void UnHook()
		{
			base.UnHook();
			_unHookIndicator.OnNext(Unit.Default);
			_unHookIndicator.OnCompleted();
		}
	}
}