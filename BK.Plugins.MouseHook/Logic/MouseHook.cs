using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
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

	public sealed class MouseHook : MouseHookBase<MouseHook>
	{
		
		private Subject<Unit> _unHookIndicator = new Subject<Unit>();
		private CompositeDisposable _disposable = new CompositeDisposable();
		private Subject<MouseTuple> _source = new Subject<MouseTuple>();

		private MouseHook() { }

		public override void SetHook()
		{
			if (IsHooked)
			{
				Debug.WriteLine($"### {nameof(MouseHook)}.{nameof(SetHook)}: The hook is already set! If you need a new hook then call {nameof(UnHook)} before you call {nameof(SetHook)}!");
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

			_source.Buffer(doubleClickTime, 4)
				.Where(buffer => buffer.Count > 0)
				.Select(buffer => (List<MouseTuple>)buffer)
				.Subscribe(EvaluateEvents)
				.DisposeWith(_disposable);

		}


		private void EvaluateEvents(IList<MouseTuple> buffer)
		{
			Console.WriteLine($"### started! count: {buffer.Count}");

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
					var param = GetParameterAndInvoke(item1.Type, item1.HookStruct.GetMousePoint(), item1.HookStruct.time,true);
					MouseObservable.OnNext(param);
				}
				else // single click
				{
					var param1 = GetParameterAndInvoke(item1.Type, item1.HookStruct.GetMousePoint(), item1.HookStruct.time);
					var param2 = GetParameterAndInvoke(item2.Type, item2.HookStruct.GetMousePoint(), item2.HookStruct.time);
					var param3 = GetParameterAndInvoke(item3.Type, item3.HookStruct.GetMousePoint(), item3.HookStruct.time);
					var param4 = GetParameterAndInvoke(item4.Type, item4.HookStruct.GetMousePoint(), item4.HookStruct.time);
					if (MouseObservable.HasObservers)
					{
						MouseObservable.OnNext(param1);
						MouseObservable.OnNext(param2);
						MouseObservable.OnNext(param3);
						MouseObservable.OnNext(param4);
					}
				}
			}

			// last item if uneven
			if (hasLeftOver)
			{
				var leftOver = buffer.Skip(count).Take(rest);
				foreach (var pair in leftOver)
				{
					var param = GetParameterAndInvoke(pair.Type, pair.HookStruct.GetMousePoint(), pair.HookStruct.time);

					if (MouseObservable.HasObservers)
					{
						MouseObservable.OnNext(param);
					}
				}

			}

			Console.WriteLine($"### End");
		}

		private static bool IsDoubleClick(MouseHookType type1, MouseHookType type2)
		{
			if (type1 == MouseHookType.WM_LBUTTONDOWN && type2 == MouseHookType.WM_LBUTTONDOWN) return true;
			if (type1 == MouseHookType.WM_MBUTTONDOWN && type2 == MouseHookType.WM_MBUTTONDOWN) return true;
			if (type1 == MouseHookType.WM_RBUTTONDOWN && type2 == MouseHookType.WM_RBUTTONDOWN) return true;
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

		private MouseParameter GetParameterAndInvoke(MouseHookType type, in MousePoint point, int time, bool isDoubleCLick = false)
		{
			if (isDoubleCLick)
			{
				var mouseInfo = _dictionaryMapper.Map(type);
				var param = MouseParameter.Factory.Create(mouseInfo, point, time).ToDoubleClick();
				OnGlobalEvent(param);
				GetDoubleClickHandler(mouseInfo)?.Invoke(this, param);
				return param;
			}
			else
			{
				var mouseInfo = _dictionaryMapper.Map(type);
				var param = MouseParameter.Factory.Create(mouseInfo, point, time);
				OnGlobalEvent(param);
				GetHandler(type)?.Invoke(this, param);
				return param;
			}
		}
	}
}