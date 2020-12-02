using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using BK.Plugins.PInvoke.Core;

namespace BK.Plugins.PInvoke
{
	public interface IUser32
	{
		bool UnhookWindowsHookEx(IntPtr hhk);
		IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
		bool GetCursorPos(out Point lpPoint);
		uint GetDoubleClickTime();
		IntPtr SetWindowsHookEx(int idHook, User32.HookProc lpfn, IntPtr hInstance, uint threadId);

		/// <summary>
		/// If the function succeeds, the return value is the requested system metric or configuration setting.
		/// If the function fails, the return value is 0. GetLastError does not provide extended error information.
		/// </summary>
		/// <param name="metric">The system metrics. See: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getsystemmetrics </param>
		/// <returns>when fails returns 0</returns>
		int GetSystemMetrics(SystemMetric metric);
	}

	[DebuggerStepThrough]
	public class User32 : IUser32
	{
		public bool UnhookWindowsHookEx(IntPtr hhk) => DllImports.UnhookWindowsHookEx(hhk);

		public IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam) =>
			DllImports.CallNextHookEx(hhk, nCode, wParam, lParam);

		public bool GetCursorPos(out Point lpPoint) => DllImports.GetCursorPos(out lpPoint);

		public uint GetDoubleClickTime() => DllImports.GetDoubleClickTime();

		public IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, uint threadId) =>
			DllImports.SetWindowsHookEx(idHook, lpfn, hInstance, threadId);

		
		public int GetSystemMetrics(SystemMetric metric) => DllImports.GetSystemMetrics(metric);

		#region Delegates

		/// <summary>
		/// When using this delegate make sure you pin it using GCHandle or use as static field to avoid an Exception!
		/// </summary>
		public delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

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
			
			[DllImport("user32.dll")]
			internal static extern int GetSystemMetrics(SystemMetric smIndex);
		}
	}
}
