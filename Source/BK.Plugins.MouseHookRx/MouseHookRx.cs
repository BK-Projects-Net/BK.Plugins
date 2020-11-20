using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using BK.Plugins.MouseHook.Core;
using BK.Plugins.MouseHook.Extensons;
using BK.Plugins.PInvoke.Core;

namespace BK.Plugins.MouseHookRx
{
	public sealed class MouseHookRx : MouseHook.MouseHook
	{
		private Subject<Unit> _unHookIndicator = new Subject<Unit>();
		private CompositeDisposable _disposable = new CompositeDisposable();
		private Subject<(LowLevelMouseInfo info, MouseParameter parameter)> _source = new Subject<(LowLevelMouseInfo info, MouseParameter parameter)>();

		public IScheduler ObserveOnScheduler { get; set; }
		public IScheduler SubscribeOnScheduler { get; set; }

		public override void SetHook()
		{
			if (IsHooked)
			{
				Debug.WriteLine($"### {nameof(MouseHookRx)}.{nameof(SetHook)}: The hook is already set! If you need a new hook then call {nameof(UnHook)} before you call {nameof(SetHook)}!");
				return;
			}
			IsHooked = true;
			_disposable = new CompositeDisposable();
			_source = new Subject<(LowLevelMouseInfo info, MouseParameter parameter)>();
			_unHookIndicator = new Subject<Unit>();
			MouseObservable = new Subject<MouseParameter>();

			base.SetHook();

			var tolerance = DoubleClickTime.Ticks / 100 * 100 * 2;
			var doubleClickTime = TimeSpan.FromTicks(DoubleClickTime.Ticks + tolerance);

			var pipe = _source.Buffer(doubleClickTime, 4);

			pipe.Where(buffer => buffer.Count > 0)
				.Select(buffer => (List<(LowLevelMouseInfo, MouseParameter)>)buffer)
				.ObserveOn(ObserveOnScheduler)
				.SubscribeOn(SubscribeOnScheduler)
				.Subscribe(EvaluateEvents)
				.DisposeWith(_disposable);

		}

		public Subject<MouseParameter> MouseObservable { get; private set; }


		private void EvaluateEvents(List<(LowLevelMouseInfo info, MouseParameter parameter)> buffer)
		{
			Debug.WriteLine($"### started! count: {buffer.Count}"); 

			var rest = buffer.Count % 4;
			bool hasLeftOver = rest != 0;
			int count = hasLeftOver ? buffer.Count - rest : buffer.Count;

			// pairwise iteration of 4 items 
			for (int index = 0; index < count; index += 4)
			{
				var item1 = buffer[index];
				var item2 = buffer[index + 1];
				var item3 = buffer[index + 2];
				var item4 = buffer[index + 3];

				// double click
				if (IsDoubleClick(item1.info.Type, item3.info.Type))
				{
					GetParameterAndInvoke(in item1.info, in item1.parameter, true);
				}
				else // single click
				{
					GetParameterAndInvoke(in item1.info, in item1.parameter, false);
					GetParameterAndInvoke(in item2.info, in item2.parameter, false);
					GetParameterAndInvoke(in item3.info, in item3.parameter, false);
					GetParameterAndInvoke(in item4.info, in item4.parameter, false);
				}
			}

			// last item if uneven
			if (hasLeftOver)
			{
				var leftOver = buffer.Skip(count).Take(rest);
				foreach (var pair in leftOver)
				{
					GetParameterAndInvoke(in pair.info, in pair.parameter, false);
				}

			}

			Debug.WriteLine($"### End");
		}

		private static bool IsDoubleClick(MouseHookType type1, MouseHookType type2)
		{
			if (type1 == MouseHookType.WM_LBUTTONDOWN && type2 == MouseHookType.WM_LBUTTONDOWN) return true;
			if (type1 == MouseHookType.WM_MBUTTONDOWN && type2 == MouseHookType.WM_MBUTTONDOWN) return true;
			if (type1 == MouseHookType.WM_RBUTTONDOWN && type2 == MouseHookType.WM_RBUTTONDOWN) return true;
			if (type1 == MouseHookType.WM_XBUTTONDOWN && type2 == MouseHookType.WM_XBUTTONDOWN) return true;
			return false;
		}

		internal override void MouseClickDelegateOverride(in LowLevelMouseInfo info, in MouseParameter parameter)
		{
			_source.OnNext((info, parameter));
		}

		public override void UnHook()
		{
			if (!IsHooked) return;
			IsHooked = false;
			base.UnHook();
			_unHookIndicator.OnNext(Unit.Default);
			_unHookIndicator.OnCompleted();
			_disposable.Dispose();
		}

		private void GetParameterAndInvoke(in LowLevelMouseInfo info, in MouseParameter parameter, bool isDoubleClick)
		{
			var mouseInfo = parameter.MouseInfo;
			var time = info.HookStruct.time;
			var position = parameter.Position;

			if (isDoubleClick)
			{
				var param = MouseParameter.Factory.CreateHasDoubleClick(mouseInfo, position, time);
				InvokeDoubleClickHandler(mouseInfo, param);
			}
			else
			{
				InvokeHandler(info.Type, this, parameter);
			}
		}


		internal void InvokeDoubleClickHandler(MouseInfo info, in MouseParameter parameter)
		{
			switch (info)
			{
				case MouseInfo.LeftButton:
					Invoke(LDoubleEvent, this, parameter);
					break;
				case MouseInfo.MiddleButton:
					Invoke(MDoubleEvent, this, parameter);
					break;
				case MouseInfo.RightButton:
					Invoke(RDoubleEvent, this, parameter);
					break;
				case MouseInfo.Mouse4:
					Invoke(Mouse4DoubleEvent, this, parameter);
					break;
				case MouseInfo.Mouse5:
					Invoke(Mouse5DoubleEvent, this, parameter);
					break;
				default:
					InvokeUnhandled(this, parameter);
					break;
			}
		}

		protected override void Invoke(EventHandler<MouseParameter> handler, object sender, MouseParameter param)
		{
			base.Invoke(handler, sender, param);
			if (MouseObservable.HasObservers)
				MouseObservable.OnNext(param);
		}

		public event EventHandler<MouseParameter> LDoubleEvent;
		public event EventHandler<MouseParameter> MDoubleEvent;
		public event EventHandler<MouseParameter> RDoubleEvent;
		public event EventHandler<MouseParameter> Mouse4DoubleEvent;
		public event EventHandler<MouseParameter> Mouse5DoubleEvent;
	}
}
