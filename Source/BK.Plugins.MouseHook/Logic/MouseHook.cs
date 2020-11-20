using System;
using System.Runtime.InteropServices;
using BK.Plugins.PInvoke;
using BK.Plugins.MouseHook.Common;
using BK.Plugins.MouseHook.Core;
using BK.Plugins.PInvoke.Core;

namespace BK.Plugins.MouseHook.Logic
{
	public abstract class MouseHook : IDisposable
	{
		private readonly IUser32 _user32 = new User32();
		internal readonly MouseInfoFactory _mouseInfoFactory = new MouseInfoFactory();

		private User32.HookProc _mouseHookProc;
		private GCHandle _mouseHookProcHandle;	// used to pin an instance to not get GC // has to be released
		private IntPtr _mouseHook = IntPtr.Zero;

		public bool IsHooked { get; protected set; }
		public TimeSpan DoubleClickTime => TimeSpan.FromMilliseconds(_user32.GetDoubleClickTime());
		//public Subject<MouseParameter> MouseObservable { get; protected set; }
		
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

		public virtual void UnHook()
		{
			_mouseHookProcHandle.Free();
			_user32.UnhookWindowsHookEx(_mouseHook);
		}

		/// <summary>
		/// Make Sure that you Subscribe to this observable **after** you called "SetHook"
		/// </summary>
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

		private IntPtr MouseClickDelegate(int code, IntPtr wparam, IntPtr lparam)
		{
			var hookType = (MouseHookType)wparam; 
			var mouseHookStruct = (MSLLHOOKSTRUCT) Marshal.PtrToStructure(lparam, typeof(MSLLHOOKSTRUCT));

			MouseClickDelegateImpl(hookType, mouseHookStruct);

			return _user32.CallNextHookEx(_mouseHook, code, wparam, lparam);
		}

		internal void MouseClickDelegateImpl(MouseHookType type, MSLLHOOKSTRUCT mouseHookStruct)
		{
			var mouseTuple = new LowLevelMouseTuple(type, mouseHookStruct); // TODO: get rid of this later
			var time = mouseHookStruct.time;
			var point = new MousePoint(mouseHookStruct.pt.X, mouseHookStruct.pt.Y);
			var info = _mouseInfoFactory.Create(type, mouseHookStruct);
			var parameter = MouseParameter.Factory.Create(info, point, time);

			MouseClickDelegateOverride(mouseTuple, parameter);
			MouseClickDelegateTemplateMethod(in mouseTuple); // TODO: remove this later

		}

		// TODO: get rid of this later
		internal abstract void MouseClickDelegateTemplateMethod(in LowLevelMouseTuple lowLevelMouseTuple);

		/// <summary>
		/// do not call the base implementation when overriding this
		/// </summary>
		/// <param name="parameter"></param>
		internal virtual void MouseClickDelegateOverride(in LowLevelMouseTuple tuple, in MouseParameter parameter)
		{
			InvokeHandler(tuple.Type, parameter);
		}

		private void InvokeHandler(MouseHookType key, in MouseParameter parameter)
		{
			var info = parameter.MouseInfo;

			switch (key)
			{
				case MouseHookType.WM_LBUTTONDOWN:
					Invoke(LDownEvent, this, parameter);
					break;
				case MouseHookType.WM_LBUTTONUP:
					Invoke(LUpEvent, this, parameter);
					break;
				case MouseHookType.WM_MOUSEMOVE:
					Invoke(MoveEvent, this, parameter);
					break;
				case MouseHookType.WM_MOUSEWHEEL:
					Invoke(WheelEvent, this, parameter);
					break;
				case MouseHookType.WM_RBUTTONDOWN:
					Invoke(RDownEvent, this, parameter);
					break;
				case MouseHookType.WM_RBUTTONUP:
					Invoke(RUpEvent, this, parameter);
					break;
				case MouseHookType.WM_MBUTTONDOWN:
					Invoke(MDownEvent, this, parameter);
					break;
				case MouseHookType.WM_MBUTTONUP:
					Invoke(MUpEvent, this, parameter);
					break;
				case MouseHookType.WM_XBUTTONDOWN when (info & MouseInfo.Mouse4) != 0:
					Invoke(Mouse4DownEvent, this, parameter);
					break;
				case MouseHookType.WM_XBUTTONDOWN when (info & MouseInfo.Mouse5) != 0:
					Invoke(Mouse5DownEvent, this, parameter);
					break;
				case MouseHookType.WM_XBUTTONUP when (info & MouseInfo.Mouse4) != 0:
					Invoke(Mouse4UpEvent, this, parameter);
					break;
				case MouseHookType.WM_XBUTTONUP when (info & MouseInfo.Mouse5) != 0:
					Invoke(Mouse5UpEvent, this, parameter);
					break;
				default:
					Invoke(UnhandledEvent, this, parameter);
					break;
			}
		}

		protected void InvokeUnhandled(object sender, MouseParameter parameter) => 
			Invoke(UnhandledEvent, sender, parameter);

		protected virtual void Invoke(EventHandler<MouseParameter> handler, object sender, MouseParameter param)
		{
			handler?.Invoke(sender, param);
			GlobalEvent?.Invoke(sender, param);
			//if(MouseObservable.HasObservers)
			//	MouseObservable.OnNext(param);
		}

		public virtual void Dispose()
		{
			if (!IsHooked) return;
			//MouseObservable?.Dispose();
			UnHook();
		}
	}
}
