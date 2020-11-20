using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.ExceptionServices;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;
using BK.Plugins.MouseHook.Core;
using BK.Plugins.MouseHook.Extensons;
using BK.Plugins.PInvoke;
using BK.Plugins.PInvoke.Core;

namespace BK.Plugins.MouseHook.Logic
{
	internal readonly struct MouseTuple
	{
		public readonly MouseHookType Type;
		public readonly MSLLHOOKSTRUCT HookStruct;

		public MouseTuple(MouseHookType type, MSLLHOOKSTRUCT hookStruct)
		{
			Type = type;
			HookStruct = hookStruct;
		}
	}

	public sealed class MouseHookRx : MouseHook
	{
		private Subject<Unit> _unHookIndicator = new Subject<Unit>();
		private CompositeDisposable _disposable = new CompositeDisposable();
		private Subject<MouseTuple> _source = new Subject<MouseTuple>();

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
			_source	= new Subject<MouseTuple>();
			_unHookIndicator = new Subject<Unit>();
			MouseObservable = new Subject<MouseParameter>();

			base.SetHook();

			var tolerance = DoubleClickTime.Ticks / 100 * 100 * 2;
			var doubleClickTime = TimeSpan.FromTicks(DoubleClickTime.Ticks + tolerance);

			var pipe = _source.Buffer(doubleClickTime, 4);

			pipe.Where(buffer => buffer.Count > 0)
				.Select(buffer => (List<MouseTuple>)buffer)
				.ObserveOn(ObserveOnScheduler)
				.SubscribeOn(SubscribeOnScheduler)
				.Subscribe(EvaluateEvents)
				.DisposeWith(_disposable);

		}


		private void EvaluateEvents(List<MouseTuple> buffer)
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
				if (IsDoubleClick(item1.Type, item3.Type))
				{
					GetParameterAndInvoke(item1, item1.HookStruct.GetMousePoint(), item1.HookStruct.time,true);
				}
				else // single click
				{
					GetParameterAndInvoke(item1, item1.HookStruct.GetMousePoint(), item1.HookStruct.time);
					GetParameterAndInvoke(item2, item2.HookStruct.GetMousePoint(), item2.HookStruct.time);
					GetParameterAndInvoke(item3, item3.HookStruct.GetMousePoint(), item3.HookStruct.time);
					GetParameterAndInvoke(item4, item4.HookStruct.GetMousePoint(), item4.HookStruct.time);
				}
			}

			// last item if uneven
			if (hasLeftOver)
			{
				// TODO: Use getrange for performance
				var leftOver = buffer.Skip(count).Take(rest);
				foreach (var pair in leftOver)
				{
					GetParameterAndInvoke(pair, pair.HookStruct.GetMousePoint(), pair.HookStruct.time);
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

		internal override void MouseClickDelegateTemplateMethod(in MouseTuple mouseTuple) => 
			_source.OnNext(mouseTuple);

		public override void UnHook()
		{
			if(!IsHooked) return;
			IsHooked = false;
			base.UnHook();
			_unHookIndicator.OnNext(Unit.Default);
			_unHookIndicator.OnCompleted();
			_disposable.Dispose();
		}

		private void GetParameterAndInvoke(in MouseTuple tuple, in MousePoint point, int time, bool isDoubleCLick = false)
		{
			if (isDoubleCLick)
			{
				var mouseInfo = _mouseInfoFactory.Create(tuple.Type, tuple.HookStruct);
				var param = MouseParameter.Factory.Create(mouseInfo, point, time).ToDoubleClick();
				var handler = GetDoubleClickHandler(mouseInfo);
				Invoke(handler, this, param);
			}
			else
			{
				var mouseInfo = _mouseInfoFactory.Create(tuple.Type, tuple.HookStruct);
				var param = MouseParameter.Factory.Create(mouseInfo, point, time);
				var handler = GetHandler(tuple.Type, mouseInfo);
				Invoke(handler, this, param);
			}
		}

	}
}