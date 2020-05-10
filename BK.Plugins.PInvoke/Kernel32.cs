using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using BK.Plugins.CreationalHelpers;

namespace BK.Plugins.PInvoke
{
	internal interface IKernel32
	{
		IntPtr GetModuleHandle(string lpModuleName);
		uint GetCurrentThreadId();
	}

	internal class Kernel32 : IKernel32
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

