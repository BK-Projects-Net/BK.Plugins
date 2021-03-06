﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BK.Plugins.PInvoke.Core
{
	// https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-msllhookstruct?redirectedfrom=MSDN
	// https://www.pinvoke.net/default.aspx/Structures/MSLLHOOKSTRUCT.html
	[StructLayout(LayoutKind.Sequential)]
	public struct MSLLHOOKSTRUCT
	{
		public Point pt;
		public int mouseData; // used do detect up/down mouse wheel scroll // also used to distinct between mouse4 and mouse5
		public int flags;
		public int time;
		public UIntPtr dwExtraInfo;
	}

}