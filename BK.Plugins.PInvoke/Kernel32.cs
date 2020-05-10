using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using BK.Plugins.CreationalHelpers;

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

		public static class DllImports
		{
			[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			public static extern IntPtr GetModuleHandle(string lpModuleName);

			[DllImport("kernel32.dll")]
			public static extern uint GetCurrentThreadId();
		}

	}
}

