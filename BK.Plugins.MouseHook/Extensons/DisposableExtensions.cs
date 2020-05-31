using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using BK.Plugins.MouseHook.Core;
using BK.Plugins.PInvoke.Core;

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

	internal static class MSLLHOOKSTRUCTExtensions
	{
		public static MousePoint GetMousePoint(this MSLLHOOKSTRUCT s) => new MousePoint(s.pt.X, s.pt.Y);
	}
}
