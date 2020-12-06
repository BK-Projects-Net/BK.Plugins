using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BK.Plugins.MouseHook.Core;
using BK.Plugins.PInvoke.Core;
using NUnit.Framework;
using Shouldly;
using Shouldly.ShouldlyExtensionMethods;

namespace BK.Plugins.MouseHook.Tests
{
	public class MouseHookTests_Wheel : TestBase
	{
		protected override uint DoubleClickTicks { get; } = 0;
		protected override int DoubleClickDiameter { get; } = 0;

		[Test]
		public async Task MouseClickDelegateImpl_WheelScrollUp()
		{
			// Arrange
			using var hook = Setup().hook;
			var eventQueue = new Queue<MouseParameter>();
			hook.GlobalEvent += (sender, parameter) => eventQueue.Enqueue(parameter);
			MSLLHOOKSTRUCT HookStruct() => TestData.TestData.GetWheelUp();

			// Act
			hook.MouseClickDelegateImpl(MouseHookType.WM_MOUSEWHEEL, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_MOUSEWHEEL, HookStruct());
			await Task.Delay(DelayBetweenClicks);

			// Assert
			eventQueue.Count.ShouldBe(2, string.Join(Environment.NewLine, eventQueue));
			var p1 = eventQueue.Dequeue();
			var p2 = eventQueue.Dequeue();

			p1.MouseInfo.ShouldHaveFlag(MouseInfo.Wheel | MouseInfo.Up);
			p2.MouseInfo.ShouldHaveFlag(MouseInfo.Wheel | MouseInfo.Up);
		}

		[Test]
		public async Task MouseClickDelegateImpl_WheelScrollDown()
		{
			// Arrange
			using var hook = Setup().hook;
			var eventQueue = new Queue<MouseParameter>();
			hook.GlobalEvent += (sender, parameter) => eventQueue.Enqueue(parameter);
			MSLLHOOKSTRUCT HookStruct() => TestData.TestData.GetWheelDown();

			// Act
			hook.MouseClickDelegateImpl(MouseHookType.WM_MOUSEWHEEL, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_MOUSEWHEEL, HookStruct());
			await Task.Delay(DelayBetweenClicks);

			// Assert
			eventQueue.Count.ShouldBe(2, string.Join(Environment.NewLine, eventQueue));
			var p1 = eventQueue.Dequeue();
			var p2 = eventQueue.Dequeue();

			p1.MouseInfo.ShouldHaveFlag(MouseInfo.Wheel | MouseInfo.Down);
			p2.MouseInfo.ShouldHaveFlag(MouseInfo.Wheel | MouseInfo.Down);
		}

	}
}