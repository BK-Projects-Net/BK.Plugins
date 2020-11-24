using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;
using BK.Plugins.MouseHook.Core;
using BK.Plugins.PInvoke;
using BK.Plugins.PInvoke.Core;


namespace BK.Plugins.MouseHook
{
	public class MouseHook : IDisposable
	{
		private readonly IUser32 _user32 = new User32();
		private readonly MouseInfoFactory _mouseInfoFactory = new MouseInfoFactory();
		private readonly Buffer<LowLevelMouseInfo> _buffer;

		private User32.HookProc _mouseHookProc;
		private GCHandle _mouseHookProcHandle;	// used to pin an instance to not get GC // has to be released
		private IntPtr _mouseHook = IntPtr.Zero;

		public bool IsHooked { get; protected set; }
		public TimeSpan DoubleClickTime => TimeSpan.FromMilliseconds(_user32.GetDoubleClickTime());

		private readonly Dispatcher _dispatcher;
		public MouseHook(Dispatcher dispatcher) : this()
		{
			_dispatcher = dispatcher;
		}

		public MouseHook()
		{
			_buffer = new Buffer<LowLevelMouseInfo>(4, DoubleClickTime.Milliseconds);
			_buffer.ThresholdReached += buffer =>
			{
				if (buffer.Length == 4)
				{
					var item1 = buffer[0];
					var item2 = buffer[1];
					var item3 = buffer[2];
					var item4 = buffer[3];

					if (Enum.Equals(item1.HookStruct, item3.HookStruct) &&
					    Enum.Equals(item2.HookStruct, item4.HookStruct))
					{
						var doubleClickParameter = item1.MouseParameter.ToDoubleClick();
						InvokeDoubleClickHandler(doubleClickParameter.MouseInfo, doubleClickParameter);
					}
					else
					{
						InvokeSingleClicks(buffer);
					}
				}
				else
				{
					InvokeSingleClicks(buffer);
				}

				void InvokeSingleClicks(LowLevelMouseInfo[] lowLevelMouseInfos)
				{
					foreach (var item in lowLevelMouseInfos) 
						InvokeHandler(item.Type, this, item.MouseParameter);
				}
			};
		}

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

		private readonly BackgroundWorker _worker = new BackgroundWorker();

		// this delgate has to be quickly returned and all events have to be invoked on the dispatcher when one exists
		private IntPtr MouseClickDelegate(int code, IntPtr wparam, IntPtr lparam)
		{
			var hookType = (MouseHookType)wparam; 
			var mouseHookStruct = (MSLLHOOKSTRUCT) Marshal.PtrToStructure(lparam, typeof(MSLLHOOKSTRUCT));

			ThreadPool.QueueUserWorkItem(_ =>
			{ 
				MouseClickDelegateImpl(hookType, mouseHookStruct);
			});

			return _user32.CallNextHookEx(_mouseHook, code, wparam, lparam);
		}

		internal void MouseClickDelegateImpl(MouseHookType type, MSLLHOOKSTRUCT mouseHookStruct)
		{
			var time = mouseHookStruct.time;
			var point = new MousePoint(mouseHookStruct.pt.X, mouseHookStruct.pt.Y);
			var info = _mouseInfoFactory.Create(type, mouseHookStruct);
			var parameter = MouseParameter.Factory.Create(info, point, time);
			var mouseTuple = new LowLevelMouseInfo(type, mouseHookStruct, parameter);

			_buffer.Enqueue(in mouseTuple);

			// MouseClickDelegateOverride(mouseTuple, parameter);
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

		protected void InvokeUnhandled(object sender, MouseParameter parameter)
		{
			if (_dispatcher != null)
				_dispatcher.BeginInvoke(new Action(() => Invoke(UnhandledEvent, sender, parameter)));
			else Invoke(UnhandledEvent, sender, parameter);
		}

		protected virtual void Invoke(EventHandler<MouseParameter> handler, object sender, MouseParameter param)
		{
			if (_dispatcher != null)
				_dispatcher.BeginInvoke(new Action(() =>
				{
					handler?.Invoke(sender, param);
					GlobalEvent?.Invoke(sender, param);
				}));
			else
			{
				handler?.Invoke(sender, param);
				GlobalEvent?.Invoke(sender, param);
			}
			
		}

		public virtual void Dispose()
		{
			if (!IsHooked) return;
			UnHook();
		}
	}
}
