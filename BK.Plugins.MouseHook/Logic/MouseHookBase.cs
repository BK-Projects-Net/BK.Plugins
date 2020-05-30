using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BK.Plugins.PInvoke;
using BK.Plugins.PInvoke.Core;
using BK.Plugins.MouseHook.Common;
using BK.Plugins.MouseHook.Core;

namespace BK.Plugins.MouseHook.Logic
{
	public abstract class MouseHookBase<T> : Singleton<T> where T : class
	{
		private readonly IUser32 _user32 = new User32();
		private readonly IKernel32 _kernel32 = new Kernel32();
		internal readonly MouseDictionaryMapper _dictionaryMapper = new MouseDictionaryMapper();

		private static User32.HookProc _mouseHookProc;
		private static IntPtr _mouseHook = IntPtr.Zero;
		private TimeSpan? _doubleClickTime;
		
		protected MouseHookBase()
		{
			_mouseHookProc = MouseClickDelegate;
		}

		public bool IsTest { get; set; }


		public bool IsHooked { get; protected set; }
		public TimeSpan DoubleClickTime => _doubleClickTime ??= TimeSpan.FromMilliseconds(_user32.GetDoubleClickTime());

	
		public event EventHandler<MouseParameter> MoveEvent;
		public event EventHandler<MouseParameter> LDownEvent;
		public event EventHandler<MouseParameter> LUpEvent;
		public event EventHandler<MouseParameter> MDownEvent;
		public event EventHandler<MouseParameter> MUpEvent;
		public event EventHandler<MouseParameter> RDownEvent;
		public event EventHandler<MouseParameter> RUpEvent;
		public event EventHandler<MouseParameter> WheelEvent;

		public event EventHandler<MouseParameter> LDoubleEvent;
		public event EventHandler<MouseParameter> MDoubleEvent;
		public event EventHandler<MouseParameter> RDoubleEvent;

		public event EventHandler<MouseParameter> GlobalEvent;
		public event EventHandler<MouseParameter> UnhandledEvent;

		public virtual void UnHook() => _user32.UnhookWindowsHookEx(_mouseHook);

		public virtual void SetHook()
		{
			using var process = Process.GetCurrentProcess();
			using var module = process.MainModule;

			var mouseHook = HookType.WH_MOUSE_LL;
			var handle = _kernel32.GetModuleHandle(module.ModuleName);
			_mouseHook = _user32.SetWindowsHookEx((int)mouseHook, _mouseHookProc,
				handle, 0);
			if ((IntPtr) _mouseHook == IntPtr.Zero)
			{
				var error = Marshal.GetLastWin32Error(); 
				throw new InvalidComObjectException($"Cannot set the mouse hook! error-code: {error}");
			}
		}

		private IntPtr MouseClickDelegate(int code, IntPtr wparam, IntPtr lparam)
		{
			var hookType = (int) wparam;
			var mouseHookStruct = (MouseHookStruct) Marshal.PtrToStructure(lparam, typeof(MouseHookStruct));

			var point = new MousePoint(mouseHookStruct.Point.X, mouseHookStruct.Point.Y);
			var type = (MouseHookType) hookType;

			if(type == MouseHookType.WM_MOUSEMOVE)
				MoveEvent?.Invoke(this, MouseParameter.Factory.Create(_dictionaryMapper.Map(type), point));
			else
				MouseClickDelegateTemplateMethod(in type, in point);

			return (IntPtr)_user32.CallNextHookEx(_mouseHook, code, wparam, lparam);
		}

		internal abstract void MouseClickDelegateTemplateMethod(in MouseHookType type, in MousePoint point);


		internal ref EventHandler<MouseParameter> GetHandler(MouseHookType key)
		{
			switch (key)
			{
				case MouseHookType.WM_LBUTTONDOWN: return ref LDownEvent;
				case MouseHookType.WM_LBUTTONUP: return ref LUpEvent;
				case MouseHookType.WM_MOUSEMOVE: return ref MoveEvent;
				case MouseHookType.WM_MOUSEWHEEL: return ref WheelEvent;
				case MouseHookType.WM_RBUTTONDOWN: return ref RDownEvent;
				case MouseHookType.WM_RBUTTONUP: return ref RUpEvent;
				case MouseHookType.WM_MBUTTONDOWN: return ref MDownEvent;
				case MouseHookType.WM_MBUTTONUP: return ref MUpEvent;
				default: return ref UnhandledEvent;
			}
		}

		internal ref EventHandler<MouseParameter> GetDoubleClickHandler(MouseInfo info)
		{
			switch (info)
			{
				case MouseInfo.LeftButton: return ref LDownEvent;
				case MouseInfo.MiddleButton: return ref MDoubleEvent;
				case MouseInfo.RightButton: return ref RDoubleEvent;
				default: return ref UnhandledEvent;
			}
		}

		protected virtual void OnGlobalEvent(MouseParameter e) => 
			GlobalEvent?.Invoke(this, e);
	}
}
