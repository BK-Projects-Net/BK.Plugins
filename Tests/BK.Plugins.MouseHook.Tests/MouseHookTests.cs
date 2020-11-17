using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BK.Plugins.PInvoke.Core;
using NUnit.Framework;
using System.Reactive;
using System.Reactive.Concurrency;
using BK.Plugins.MouseHook.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;

namespace BK.Plugins.MouseHook.Tests
{
	[TestFixture]
	public class MouseHookTests
	{

		[Test]
		[NonParallelizable]
		public void MouseClickDelegateImpl_LeftDoubleClick()
		{
			// Arrange
			var hook = Logic.MouseHook.Instance;
			hook.SubscribeOnScheduler = new HistoricalScheduler();
			var scheduler = (HistoricalScheduler)hook.SubscribeOnScheduler;
			hook.SetHook();
			MouseParameter? result = null;
			using var _ = hook.MouseObservable.Subscribe(r => result = r);

			// act
			hook.MouseClickDelegateImpl(MouseHookType.WM_LBUTTONDOWN, new MSLLHOOKSTRUCT());
			hook.MouseClickDelegateImpl(MouseHookType.WM_LBUTTONUP, new MSLLHOOKSTRUCT());
			hook.MouseClickDelegateImpl(MouseHookType.WM_LBUTTONDOWN, new MSLLHOOKSTRUCT());
			hook.MouseClickDelegateImpl(MouseHookType.WM_LBUTTONDOWN, new MSLLHOOKSTRUCT());

			// assert
			scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500));
			result.ShouldNotBeNull();
			var expected = MouseInfo.LeftButton | MouseInfo.Double | MouseInfo.Down;
			result.ShouldBeMouseInfo(expected);

		}
	}

	internal static class MouseHookTestExtensions
	{
		internal static void ShouldBeMouseInfo(this MouseParameter? source, MouseInfo info)
		{
			if (source is null) throw new ArgumentNullException();
			if (!source.Value.Is(info))
				throw new InternalTestFailureException(
					$"should be '{info.ToString()}' \nbut was \n{source.Value.MouseInfo.ToString()}!");
		}

	}
}
