using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using BK.Plugins.PInvoke.Core;

namespace BK.Plugins.PInvoke
{
	internal interface IUser32
	{
		bool UnhookWindowsHookEx(IntPtr hhk);
		IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
		bool GetCursorPos(out Point lpPoint);
		uint GetDoubleClickTime();
		IntPtr SetWindowsHookEx(int idHook, User32.HookProc lpfn, IntPtr hInstance, uint threadId);
	}

	[DebuggerStepThrough]
	internal class User32 : IUser32
	{
		public bool UnhookWindowsHookEx(IntPtr hhk) => DllImports.UnhookWindowsHookEx(hhk);
		public IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam) =>
			DllImports.CallNextHookEx(hhk, nCode, wParam, lParam);
		public bool GetCursorPos(out Point lpPoint) => DllImports.GetCursorPos(out lpPoint);
		public uint GetDoubleClickTime() => DllImports.GetDoubleClickTime();
		public IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, uint threadId) =>
			DllImports.SetWindowsHookEx(idHook, lpfn, hInstance, threadId);

		#region Delegates

		/// <summary>
		/// When using this delegate make sure you pin it as static field to avoid a StackOverflow Exception!
		/// </summary>
		internal delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

		#endregion

		internal static class DllImports
		{
			[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

			[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

			[DllImport("user32.dll", SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			internal static extern bool GetCursorPos(out Point lpPoint);

			[DllImport("user32.dll")]
			internal static extern uint GetDoubleClickTime();

			[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			internal static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, uint threadId);
		}
	}
}
