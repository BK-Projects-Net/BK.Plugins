using System;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using BK.Plugins.PInvoke;
using BK.Plugins.MouseHook.Common;
using BK.Plugins.MouseHook.Core;
using BK.Plugins.PInvoke.Core;

namespace BK.Plugins.MouseHook.Logic
{
	public abstract class MouseHook<T> : Singleton<T> where T : class
	{
		private readonly IUser32 _user32 = new User32();
		private readonly IKernel32 _kernel32 = new Kernel32();
		internal readonly MouseInfoFactory _mouseInfoFactory = new MouseInfoFactory();

		private static User32.HookProc _mouseHookProc;
		private static IntPtr _mouseHook = IntPtr.Zero;
		private TimeSpan? _doubleClickTime;
		protected MouseHook()
		{
			_mouseHookProc = MouseClickDelegate;
		}

		public bool IsHooked { get; protected set; }

		
		public TimeSpan DoubleClickTime => _doubleClickTime ??= TimeSpan.FromMilliseconds(_user32.GetDoubleClickTime());
		
		public Subject<MouseParameter> MouseObservable { get; protected set; }
		
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

		public virtual void UnHook()
		{
			_user32.UnhookWindowsHookEx(_mouseHook);
		}

		/// <summary>
		/// Make Sure that you Subscribe to this observable **after** you called "SetHook"
		/// </summary>
		public virtual void SetHook()
		{
			var mouseHook = HookType.WH_MOUSE_LL;
			_mouseHook = _user32.SetWindowsHookEx((int)mouseHook, _mouseHookProc, IntPtr.Zero, 0);
			
			if ((IntPtr) _mouseHook == IntPtr.Zero)
			{
				var error = Marshal.GetLastWin32Error(); 
				throw new InvalidComObjectException($"Cannot set the mouse hook! error-code: {error}");
			}
		}

		private IntPtr MouseClickDelegate(int code, IntPtr wparam, IntPtr lparam)
		{
			var hookType = (MouseHookType)wparam; 
			var mouseHookStruct = (MSLLHOOKSTRUCT) Marshal.PtrToStructure(lparam, typeof(MSLLHOOKSTRUCT));

			MouseClickDelegateImpl(hookType, mouseHookStruct);

			return (IntPtr)_user32.CallNextHookEx(_mouseHook, code, wparam, lparam);
		}

		internal void MouseClickDelegateImpl(MouseHookType type, MSLLHOOKSTRUCT mouseHookStruct)
		{
			var mouseTuple = new MouseTuple(type, mouseHookStruct);
			var time = mouseHookStruct.time;
			var point = new MousePoint(mouseHookStruct.pt.X, mouseHookStruct.pt.Y);

			if (type == MouseHookType.WM_MOUSEMOVE)
			{
				var info = _mouseInfoFactory.Create(type, mouseHookStruct);
				var param = MouseParameter.Factory.Create(info, point, time);
				Invoke(MoveEvent, this, param);
				return;
			}
			if (type == MouseHookType.WM_MOUSEWHEEL)
			{
				var wheelInfo = _mouseInfoFactory.Create(type, mouseHookStruct);
				var param = MouseParameter.Factory.Create(wheelInfo, point, time);
				Invoke(WheelEvent, this, param);
				return;
			}
			
			MouseClickDelegateTemplateMethod(in mouseTuple);

		}

		internal abstract void MouseClickDelegateTemplateMethod(in MouseTuple mouseTuple);

		internal EventHandler<MouseParameter> GetHandler(MouseHookType key, in MouseInfo info) =>
			key switch
			{
				MouseHookType.WM_LBUTTONDOWN => LDownEvent,
				MouseHookType.WM_LBUTTONUP => LUpEvent,
				MouseHookType.WM_MOUSEMOVE => MoveEvent,
				MouseHookType.WM_MOUSEWHEEL => WheelEvent,
				MouseHookType.WM_RBUTTONDOWN => RDownEvent,
				MouseHookType.WM_RBUTTONUP => RUpEvent,
				MouseHookType.WM_MBUTTONDOWN => MDownEvent,
				MouseHookType.WM_MBUTTONUP => MUpEvent,
				MouseHookType.WM_XBUTTONDOWN when (info & MouseInfo.Mouse4) != 0 => Mouse4DownEvent,
				MouseHookType.WM_XBUTTONDOWN when (info & MouseInfo.Mouse5) != 0 => Mouse5DownEvent,
				MouseHookType.WM_XBUTTONUP when (info & MouseInfo.Mouse4) != 0 => Mouse4UpEvent,
				MouseHookType.WM_XBUTTONUP when (info & MouseInfo.Mouse5) != 0 => Mouse5UpEvent,
				_ => UnhandledEvent
			};


		internal EventHandler<MouseParameter> GetDoubleClickHandler(MouseInfo info) =>
			info switch
			{
				MouseInfo.LeftButton => LDoubleEvent,
				MouseInfo.MiddleButton => MDoubleEvent,
				MouseInfo.RightButton => RDoubleEvent,
				MouseInfo.Mouse4 => Mouse4DoubleEvent,
				MouseInfo.Mouse5 => Mouse5DoubleEvent,
				_ => UnhandledEvent
			};

		protected void Invoke(EventHandler<MouseParameter> handler, object sender, MouseParameter param)
		{
			handler?.Invoke(sender, param);
			GlobalEvent?.Invoke(sender, param);
			if(MouseObservable.HasObservers)
				MouseObservable.OnNext(param);
		}
	}
}
