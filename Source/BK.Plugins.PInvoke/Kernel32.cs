using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BK.Plugins.PInvoke
{
	public interface IKernel32
	{
		IntPtr GetModuleHandle(string lpModuleName);
		uint GetCurrentThreadId();
	}

	public class Kernel32 : IKernel32
	{
		public IntPtr GetModuleHandle(string lpModuleName) => DllImports.GetModuleHandle(lpModuleName);
		public uint GetCurrentThreadId() => DllImports.GetCurrentThreadId();

		internal static class DllImports
		{
			[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			internal static extern IntPtr GetModuleHandle(string lpModuleName);

			[DllImport("kernel32.dll")]
			internal static extern uint GetCurrentThreadId();
		}

	}
}

