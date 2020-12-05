using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;
using BK.Plugins.MouseHook.Core;
using BK.Plugins.PInvoke;
using BK.Plugins.PInvoke.Core;
using Timer = System.Timers.Timer;

// https://stackoverflow.com/questions/50055814/how-to-detect-double-click-tap-when-handling-wm-pointer-message/50057917#50057917
// https://devblogs.microsoft.com/oldnewthing/20041018-00/?p=37543

namespace BK.Plugins.MouseHook
{
	public class MouseHook : IDisposable
	{
		private readonly IUser32 _user32 = new User32();
		private readonly MouseInfoFactory _mouseInfoFactory = new MouseInfoFactory();

		private User32.HookProc _mouseHookProc;
		private GCHandle _mouseHookProcHandle;	// used to pin an instance to not get GC // has to be released
		private IntPtr _mouseHook = IntPtr.Zero;
		private TimerPool _timerPool;
		private readonly SynchronizationContext _syncContext = SynchronizationContext.Current;

		private int? _doubleClickTicks;
		private int? _doubleClickWidth;
		private int? _doubleClickHeight;

		public int DoubleClickTicks => _doubleClickTicks ??= (int)_user32.GetDoubleClickTime();
		public int DoubleClickWidth => _doubleClickWidth ??= _user32.GetSystemMetrics(SystemMetric.SM_CXDOUBLECLK);
		public int DoubleClickHeight => _doubleClickHeight ??= _user32.GetSystemMetrics(SystemMetric.SM_CYDOUBLECLK);

		public bool IsHooked { get; protected set; }

		private void ElapsedSingleClickThreshold(object sender, ElapsedEventArgs args, in LowLevelMouseInfo capturedMouseClick) => 
			InvokeHandler(capturedMouseClick.Type, sender, capturedMouseClick.MouseParameter);

		public MouseHook()
		{
			_timerPool = new TimerPool(false, DoubleClickTicks);
			_timerPool.Elapsed += (sender, args) =>
			{
				Interlocked.Exchange(ref _clickCount, 0);
				if (!_capturedMouseClicks.TryDequeue(out var click)) return;

				if (_syncContext != null)
					_syncContext.Send(state => ElapsedSingleClickThreshold(sender, args, in click), null);
				else
				{
					ElapsedSingleClickThreshold(sender, args, in click);
				}

			};
		}

		/// <summary>
		/// Used for testing
		/// </summary>
		internal MouseHook(IUser32 user32) : this() => _user32 = user32;


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

		public event EventHandler<MouseParameter> LDoubleEvent;
		public event EventHandler<MouseParameter> MDoubleEvent;
		public event EventHandler<MouseParameter> RDoubleEvent;
		public event EventHandler<MouseParameter> Mouse4DoubleEvent;
		public event EventHandler<MouseParameter> Mouse5DoubleEvent;

		public event EventHandler<MouseParameter> GlobalEvent;
		public event EventHandler<MouseParameter> UnhandledEvent;
		#endregion

		public virtual void SetHook()
		{
			_mouseHookProc = MouseClickDelegate;
			_mouseHookProcHandle = GCHandle.Alloc(_mouseHookProc);

			var mouseHook = HookType.WH_MOUSE_LL;
			_mouseHook = _user32.SetWindowsHookEx((int) mouseHook, _mouseHookProc, IntPtr.Zero, 0);
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

		// this delgate has to be quickly returned and all events have to be invoked on the dispatcher when one exists
		private IntPtr MouseClickDelegate(int code, IntPtr wparam, IntPtr lparam)
		{
			var hookType = (MouseHookType)wparam; 
			var mouseHookStruct = (MSLLHOOKSTRUCT) Marshal.PtrToStructure(lparam, typeof(MSLLHOOKSTRUCT));
			
			MouseClickDelegateImpl(hookType, mouseHookStruct);

			return _user32.CallNextHookEx(_mouseHook, code, wparam, lparam);
		}

		private int _clickCount = 0;
		private LowLevelMouseInfo _last;
		private ConcurrentQueue<LowLevelMouseInfo> _capturedMouseClicks = new ConcurrentQueue<LowLevelMouseInfo>();
		internal void MouseClickDelegateImpl(MouseHookType type, MSLLHOOKSTRUCT mouseHookStruct)
		{
			var time = mouseHookStruct.time;
			var point = new MousePoint(mouseHookStruct.pt.X, mouseHookStruct.pt.Y);
			var info = _mouseInfoFactory.Create(type, mouseHookStruct);
			var parameter = MouseParameter.Factory.Create(info, point, time);

			if (type == MouseHookType.WM_MOUSEMOVE)
			{
				Invoke(MoveEvent, this, parameter);
			}
			else if (type == MouseHookType.WM_MOUSEWHEEL)
			{
				Invoke(WheelEvent, this, parameter);
			}
			else
			{
				_clickCount++;
				// double click
				var last = _last.HookStruct;
				if ( _clickCount == 4
				    && mouseHookStruct.time - last.time < DoubleClickTicks
				    && Math.Abs(last.pt.X - point.X) < DoubleClickWidth 
				    && Math.Abs(last.pt.Y - point.Y) < DoubleClickHeight)
				{
					_timerPool.Stop();
					_capturedMouseClicks = new ConcurrentQueue<LowLevelMouseInfo>();
					InvokeDoubleClickHandler(info, parameter.ToDoubleClick());
					_clickCount = 0;
				}
				else // single click
				{
					_capturedMouseClicks.Enqueue(new LowLevelMouseInfo(type, mouseHookStruct, parameter));
					_timerPool.Start();

				}

				if (IsDown(type))
					_last = new LowLevelMouseInfo(type, mouseHookStruct, parameter);
			}

			// ThreadPool.QueueUserWorkItem(_ => _buffer.Enqueue(in mouseTuple));
			// MouseClickDelegateOverride(mouseTuple, parameter);
			
		}

		private bool IsDown(MouseHookType type) =>
			type == MouseHookType.WM_LBUTTONDOWN || type == MouseHookType.WM_MBUTTONDOWN ||
			type == MouseHookType.WM_RBUTTONDOWN || type == MouseHookType.WM_XBUTTONDOWN;

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
		internal void InvokeDoubleClickHandler(MouseInfo info, in MouseParameter parameter)
		{
			if (info.HasFlag(MouseInfo.LeftButton))
				Invoke(LDoubleEvent, this, parameter);
			else if (info.HasFlag(MouseInfo.MiddleButton))
				Invoke(MDoubleEvent, this, parameter);
			else if (info.HasFlag(MouseInfo.RightButton))
				Invoke(RDoubleEvent, this, parameter);
			else if (info.HasFlag(MouseInfo.Mouse4))
				Invoke(Mouse4DoubleEvent, this, parameter);
			else if (info.HasFlag(MouseInfo.Mouse5))
				Invoke(Mouse5DoubleEvent, this, parameter);
			else
				InvokeUnhandled(this, parameter);
		}

		protected void InvokeUnhandled(object sender, MouseParameter parameter) => Invoke(UnhandledEvent, sender, parameter);

		protected virtual void Invoke(EventHandler<MouseParameter> handler, object sender, MouseParameter param, Dispatcher dispatcher = null)
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

	internal class TimerPool : IDisposable
	{
		private readonly List<Timer> _timers = new List<Timer>();
		private readonly bool _autoReset;
		private readonly int _interval;

		public TimerPool(bool autoReset, int interval)
		{
			_autoReset = autoReset;
			_interval = interval;
		}
		
		public event ElapsedEventHandler Elapsed;

		public void Start()
		{
			var timer = _timers.Find(t => !t.Enabled);
			if (timer != null)
			{
				timer.Start();
			}
			else
			{
				var t = new Timer(_interval);
				t.Elapsed += Elapsed;
				t.AutoReset = _autoReset;
				t.Start();
				_timers.Add(t);
			}
		}

		public void Stop()
		{
			foreach (var timer in _timers)
			{
				timer.Stop();
			}
		}

		public void Dispose()
		{
			foreach (var timer in _timers)
			{
				timer.Stop();
				timer.Elapsed -= Elapsed;
				timer.Dispose();
			}
		}
	}
}
