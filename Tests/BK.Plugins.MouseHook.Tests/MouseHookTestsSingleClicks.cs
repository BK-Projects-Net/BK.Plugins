using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BK.Plugins.MouseHook.Core;
using BK.Plugins.PInvoke;
using BK.Plugins.PInvoke.Core;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Shouldly;
using Shouldly.ShouldlyExtensionMethods;

namespace BK.Plugins.MouseHook.Tests
{
	[TestFixture]
	public class MouseHookTestsSingleClicks : TestBase
	{
		protected override uint DoubleClickTicks => (uint)TimeSpan.FromMilliseconds(500).Milliseconds;
		protected override int DoubleClickDiameter => 0;

		[Test]
		public async Task MouseClickDelegateImpl_LeftMouseButtonSingleClick()
		{
			// Arrange
			using var hook = Setup().hook;
			var eventQueue = new Queue<MouseParameter>();
			hook.GlobalEvent += (sender, parameter) => eventQueue.Enqueue(parameter);
			MSLLHOOKSTRUCT HookStruct() => new MSLLHOOKSTRUCT { time = (int)DateTime.Now.Ticks };

			// Act
			hook.MouseClickDelegateImpl(MouseHookType.WM_LBUTTONDOWN, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_LBUTTONUP, HookStruct());
			await Task.Delay(DelayBetweenClicks);

			// Assert
			eventQueue.Count.ShouldBe(2, string.Join(Environment.NewLine, eventQueue));
			var p1 = eventQueue.Dequeue();
			var p2 = eventQueue.Dequeue();

			p1.MouseInfo.ShouldHaveFlag(MouseInfo.LeftButton | MouseInfo.Down);
			p2.MouseInfo.ShouldHaveFlag(MouseInfo.LeftButton | MouseInfo.Up);
		}

		

	}
}