using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BK.Plugins.MouseHook.Core;
using BK.Plugins.PInvoke;
using BK.Plugins.PInvoke.Core;

namespace BK.Plugins.MouseHook.Logic
{
	internal class MouseHookLogic
	{
		public bool IsHooked { get; private set; }
		private TimeSpan _cursorUpdateInterval = TimeSpan.FromMilliseconds(30);
		private static PInvoke.User32.HookProc _mouseHookProc;
		private static IntPtr _mouseHook = IntPtr.Zero;
		private readonly Lazy<Dictionary<MouseInfo, EventHandler<MouseParameter>>> _eventMap;
		private readonly IUser32 _user32 = new User32();
		private readonly IKernel32 _kernel32 = new Kernel32();

		private TimeSpan? _doubleClickTime;
		public TimeSpan DoubleClickTime => _doubleClickTime ??= TimeSpan.FromMilliseconds(_user32.GetDoubleClickTime());

		public MouseHookLogic()
		{
			_mouseHookProc = MouseClickDelegate;
		}

		public event EventHandler<MouseParameter> MoveEvent;
		public event EventHandler<MouseParameter> LDownEvent;
		public event EventHandler<MouseParameter> LUpEvent;
		public event EventHandler<MouseParameter> MDownEvent;
		public event EventHandler<MouseParameter> MUpEvent;
		public event EventHandler<MouseParameter> RDownEvent;
		public event EventHandler<MouseParameter> RUpEvent;
		public event EventHandler<MouseParameter> WheelEvent;
		internal event EventHandler<MouseParameter> MouseHookInternal;

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


		public TimeSpan CursorUpdateInterval
		{
			get => _cursorUpdateInterval;
			set
			{
				if(value < TimeSpan.FromMilliseconds(1))
					throw new ArgumentException($"'value' of {nameof(CursorUpdateInterval)} must not be smaller then 1 ms!");
				_cursorUpdateInterval = value;
			}
		}

		private IntPtr MouseClickDelegate(int code, IntPtr wparam, IntPtr lparam)
		{
			var now = DateTime.Now;	
			var hookType = (int) wparam;
			var mouseHookStruct = (MouseHookStruct) Marshal.PtrToStructure(lparam, typeof(MouseHookStruct));
			
			var point = new MousePoint(mouseHookStruct.Point.X, mouseHookStruct.Point.Y);
			var type = (MouseInfo) hookType;
			var parameter = new MouseParameter(type, point);

			//_eventMap.Value[type]?.Invoke(type, parameter);
			GetHandler(type)?.Invoke(type, parameter);
			MouseHookInternal?.Invoke(type, parameter);

			return (IntPtr)_user32.CallNextHookEx(_mouseHook, code, wparam, lparam);
		}

		private ref EventHandler<MouseParameter> GetHandler(MouseInfo key)
		{
			switch (key)
			{
				//case MouseInfo.LDown: return ref LDownEvent;
				//case MouseInfo.LUp:	  return ref LUpEvent;
				//case MouseInfo.Move:  return ref MoveEvent;
				//case MouseInfo.Wheel: return ref WheelEvent;
				//case MouseInfo.RDown: return ref RDownEvent;
				//case MouseInfo.RUp:   return ref RUpEvent;
				//case MouseInfo.MDown: return ref MDownEvent;
				//case MouseInfo.MUp:   return ref MUpEvent;
				//default: throw new ArgumentOutOfRangeException(nameof(key), key, "is not implemented!");
			}

			throw new NotFiniteNumberException();
		}
	}
}
