using System;
using System.Reactive.Disposables;

namespace BK.Plugins.MouseHookRx
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
