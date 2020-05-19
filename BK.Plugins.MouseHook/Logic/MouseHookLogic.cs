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
	internal class MouseHookLogic
	{
		private readonly IUser32 _user32 = new User32();
		private readonly IKernel32 _kernel32 = new Kernel32();
		private readonly MouseEnumMapper _enumMapper = new MouseEnumMapper();

		private static User32.HookProc _mouseHookProc;
		private static IntPtr _mouseHook = IntPtr.Zero;
		private TimeSpan? _doubleClickTime;
		
		public MouseHookLogic()
		{
			_mouseHookProc = MouseClickDelegate;
		}


		public bool IsHooked { get; private set; }
		public TimeSpan DoubleClickTime => _doubleClickTime ??= TimeSpan.FromMilliseconds(_user32.GetDoubleClickTime());

		public event EventHandler<MouseParameter> MoveEvent;
		public event EventHandler<MouseParameter> LDownEvent;
		public event EventHandler<MouseParameter> LUpEvent;
		public event EventHandler<MouseParameter> MDownEvent;
		public event EventHandler<MouseParameter> MUpEvent;
		public event EventHandler<MouseParameter> RDownEvent;
		public event EventHandler<MouseParameter> RUpEvent;
		public event EventHandler<MouseParameter> WheelEvent;
		public event EventHandler<MouseParameter> MouseHookEvent;
		public event EventHandler<MouseParameter> UnhandledEvent;

		public virtual void UnHook()
		{
			if(!IsHooked) return;

			_user32.UnhookWindowsHookEx(_mouseHook);

			IsHooked = false;
		}

		public virtual void SetHook()
		{
			if (IsHooked) return;

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

			IsHooked = true;
		}

		private IntPtr MouseClickDelegate(int code, IntPtr wparam, IntPtr lparam)
		{
			var hookType = (int) wparam;
			var mouseHookStruct = (MouseHookStruct) Marshal.PtrToStructure(lparam, typeof(MouseHookStruct));
			
			var point = new MousePoint(mouseHookStruct.Point.X, mouseHookStruct.Point.Y);
			var type = (MouseHookType) hookType;

			var mappedType = _enumMapper.Map(type);
			var parameter = new MouseParameter(mappedType, point, DateTime.Now, Guid.NewGuid());

			GetHandler(type)?.Invoke(type, parameter);
			MouseHookEvent?.Invoke(type, parameter);

			return (IntPtr)_user32.CallNextHookEx(_mouseHook, code, wparam, lparam);
		}

		private ref EventHandler<MouseParameter> GetHandler(MouseHookType key)
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
	}
}
