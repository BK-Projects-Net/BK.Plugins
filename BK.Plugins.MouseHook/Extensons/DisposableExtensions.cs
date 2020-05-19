﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace BK.Plugins.MouseHook.Extensons
{
	internal static class DisposableExtensions
	{
		internal static CompositeDisposable DisposeWith(this IDisposable d, CompositeDisposable composite)
		{
			composite.Add(d);
			return composite;
		}

	}
}
