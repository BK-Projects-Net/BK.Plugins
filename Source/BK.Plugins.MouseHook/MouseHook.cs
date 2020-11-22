using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using BK.Plugins.MouseHook.Core;
using BK.Plugins.PInvoke;
using BK.Plugins.PInvoke.Core;

namespace BK.Plugins.MouseHook
{
	internal class MouseBuffer
	{
		private readonly Queue<MouseParameter> _buffer = new Queue<MouseParameter>();

		internal void Add(in MouseParameter parameter)
		{
			_buffer.Enqueue(parameter);
		}


	}

	internal class AccurateTimer
	{
		private delegate void TimerEventDel(int id, int msg, IntPtr user, int dw1, int dw2);
		private const int TIME_PERIODIC = 1;
		private const int EVENT_TYPE = TIME_PERIODIC;// + 0x100;  // TIME_KILL_SYNCHRONOUS causes a hang ?!
		[DllImport("winmm.dll")]
		private static extern int timeBeginPeriod(int msec);
		[DllImport("winmm.dll")]
		private static extern int timeEndPeriod(int msec);
		[DllImport("winmm.dll")]
		private static extern int timeSetEvent(int delay, int resolution, TimerEventDel handler, IntPtr user, int eventType);
		[DllImport("winmm.dll")]
		private static extern int timeKillEvent(int id);

		private readonly Action _action;
		private readonly int _timerId;
		private readonly TimerEventDel _handler;  // NOTE: declare at class scope so garbage collector doesn't release it!!!
		private readonly Dispatcher _dispatcher;

		public AccurateTimer(Action action, int delay, Dispatcher dispatcher) : this(action, delay)
		{
			_dispatcher = dispatcher;
		}

		public AccurateTimer(Action action, int delay)
		{
			_action = action;
			timeBeginPeriod(1);
			_handler = new TimerEventDel(TimerCallback);
			_timerId = timeSetEvent(delay, 0, _handler, IntPtr.Zero, EVENT_TYPE);
		}

		public void Stop()
		{
			int err = timeKillEvent(_timerId);
			timeEndPeriod(1);
			System.Threading.Thread.Sleep(100);// Ensure callbacks are drained
		}

		private void TimerCallback(int id, int msg, IntPtr user, int dw1, int dw2)
		{
			if (_timerId != 0)
				_dispatcher.BeginInvoke(_action);
		}
	}

	public abstract class MouseHook : IDisposable
	{
		private readonly IUser32 _user32 = new User32();
		private readonly MouseInfoFactory _mouseInfoFactory = new MouseInfoFactory();

		private User32.HookProc _mouseHookProc;
		private GCHandle _mouseHookProcHandle;	// used to pin an instance to not get GC // has to be released
		private IntPtr _mouseHook = IntPtr.Zero;

		public bool IsHooked { get; protected set; }
		public TimeSpan DoubleClickTime => TimeSpan.FromMilliseconds(_user32.GetDoubleClickTime());

		#region Events
		public event EventHandler<MouseParameter> MoveEvent;
		public event EventHandler<MouseParameter> LDownEvent;
		public event EventHandler<MouseParameter> LUpEvent;
		public event EventHandler<MouseParameter> MDownEvent;
		public event EventHandler<MouseParameter> MUpEvent;
		public event EventHandler<MouseParameter> RDownEvent;
		public event EventHandler<MouseParameter> RUpEvent;
		public event EventHandler<MouseParameter> WheelEvent;
		public event EventHandler<MouseParameter> Mouse4UpEvent;
		public event EventHandler<MouseParameter> Mouse4DownEvent;
		public event EventHandler<MouseParameter> Mouse5UpEvent;
		public event EventHandler<MouseParameter> Mouse5DownEvent;

		public event EventHandler<MouseParameter> GlobalEvent;
		public event EventHandler<MouseParameter> UnhandledEvent;
		#endregion

		public virtual void SetHook()
		{
			_mouseHookProc = MouseClickDelegate;
			_mouseHookProcHandle = GCHandle.Alloc(_mouseHookProc);

			var mouseHook = HookType.WH_MOUSE_LL;
			_mouseHook = _user32.SetWindowsHookEx((int)mouseHook, _mouseHookProc, IntPtr.Zero, 0);

			if (_mouseHook == IntPtr.Zero)
			{
				var error = Marshal.GetLastWin32Error();
				throw new InvalidComObjectException($"Cannot set the mouse hook! error: {error}");
			}
		}

		public virtual void UnHook()
		{
			_mouseHookProcHandle.Free();
			_user32.UnhookWindowsHookEx(_mouseHook);
		}

		public bool TryGetMousePosition(out MousePoint point)
		{
			var success = _user32.GetCursorPos(out var p);
			point = new MousePoint(p.X, p.Y);
			return success;
		}

		private IntPtr MouseClickDelegate(int code, IntPtr wparam, IntPtr lparam)
		{
			var hookType = (MouseHookType)wparam; 
			var mouseHookStruct = (MSLLHOOKSTRUCT) Marshal.PtrToStructure(lparam, typeof(MSLLHOOKSTRUCT));

			MouseClickDelegateImpl(hookType, mouseHookStruct);

			return _user32.CallNextHookEx(_mouseHook, code, wparam, lparam);
		}

		private readonly Queue<MouseParameter> _queue = new Queue<MouseParameter>();

		internal void MouseClickDelegateImpl(MouseHookType type, MSLLHOOKSTRUCT mouseHookStruct)
		{
			var mouseTuple = new LowLevelMouseInfo(type, mouseHookStruct);
			var time = mouseHookStruct.time;
			var point = new MousePoint(mouseHookStruct.pt.X, mouseHookStruct.pt.Y);
			var info = _mouseInfoFactory.Create(type, mouseHookStruct);
			var parameter = MouseParameter.Factory.Create(info, point, time);

			_queue.Enqueue(parameter);
			

			//MouseClickDelegateOverride(mouseTuple, parameter);
		}

		/// <summary>
		/// do not call the base implementation when overriding this
		/// otherwise the events will be invoked 2 times for the concretion
		/// </summary>
		internal virtual void MouseClickDelegateOverride(in LowLevelMouseInfo info, in MouseParameter parameter) =>
			InvokeHandler(info.Type,this, parameter);

		protected void InvokeHandler(MouseHookType key, object sender, in MouseParameter parameter)
		{
			var info = parameter.MouseInfo;

			switch (key)
			{
				case MouseHookType.WM_LBUTTONDOWN:
					Invoke(LDownEvent, sender, parameter);
					break;
				case MouseHookType.WM_LBUTTONUP:
					Invoke(LUpEvent, sender, parameter);
					break;
				case MouseHookType.WM_MOUSEMOVE:
					Invoke(MoveEvent, sender, parameter);
					break;
				case MouseHookType.WM_MOUSEWHEEL:
					Invoke(WheelEvent, sender, parameter);
					break;
				case MouseHookType.WM_RBUTTONDOWN:
					Invoke(RDownEvent, sender, parameter);
					break;
				case MouseHookType.WM_RBUTTONUP:
					Invoke(RUpEvent, sender, parameter);
					break;
				case MouseHookType.WM_MBUTTONDOWN:
					Invoke(MDownEvent, sender, parameter);
					break;
				case MouseHookType.WM_MBUTTONUP:
					Invoke(MUpEvent, sender, parameter);
					break;
				case MouseHookType.WM_XBUTTONDOWN when (info & MouseInfo.Mouse4) != 0:
					Invoke(Mouse4DownEvent, sender, parameter);
					break;
				case MouseHookType.WM_XBUTTONDOWN when (info & MouseInfo.Mouse5) != 0:
					Invoke(Mouse5DownEvent, sender, parameter);
					break;
				case MouseHookType.WM_XBUTTONUP when (info & MouseInfo.Mouse4) != 0:
					Invoke(Mouse4UpEvent, sender, parameter);
					break;
				case MouseHookType.WM_XBUTTONUP when (info & MouseInfo.Mouse5) != 0:
					Invoke(Mouse5UpEvent, sender, parameter);
					break;
				default:
					InvokeUnhandled(sender, parameter);
					break;
			}
		}

		protected void InvokeUnhandled(object sender, MouseParameter parameter) => 
			Invoke(UnhandledEvent, sender, parameter);

		protected virtual void Invoke(EventHandler<MouseParameter> handler, object sender, MouseParameter param)
		{
			handler?.Invoke(sender, param);
			GlobalEvent?.Invoke(sender, param);
		}

		public virtual void Dispose()
		{
			if (!IsHooked) return;
			UnHook();
		}
	}
}
