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
	public class MouseHookTests_SingleClicks : TestBase
	{
		protected override uint DoubleClickTicks => (uint)TimeSpan.FromMilliseconds(500).Milliseconds;
		protected override int DoubleClickDiameter => 0;

		[Test]
		public async Task MouseClickDelegateImpl_LeftSingleClick()
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

		[Test]
		public async Task MouseClickDelegateImpl_MiddleSingleClick()
		{
			// Arrange
			using var hook = Setup().hook;
			var eventQueue = new Queue<MouseParameter>();
			hook.GlobalEvent += (sender, parameter) => eventQueue.Enqueue(parameter);
			MSLLHOOKSTRUCT HookStruct() => new MSLLHOOKSTRUCT { time = (int)DateTime.Now.Ticks };

			// Act
			hook.MouseClickDelegateImpl(MouseHookType.WM_MBUTTONDOWN, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_MBUTTONUP, HookStruct());
			await Task.Delay(DelayBetweenClicks);

			// Assert
			eventQueue.Count.ShouldBe(2, string.Join(Environment.NewLine, eventQueue));
			var p1 = eventQueue.Dequeue();
			var p2 = eventQueue.Dequeue();

			p1.MouseInfo.ShouldHaveFlag(MouseInfo.MiddleButton | MouseInfo.Down);
			p2.MouseInfo.ShouldHaveFlag(MouseInfo.MiddleButton | MouseInfo.Up);
		}

		[Test]
		public async Task MouseClickDelegateImpl_RightSingleClick()
		{
			// Arrange
			using var hook = Setup().hook;
			var eventQueue = new Queue<MouseParameter>();
			hook.GlobalEvent += (sender, parameter) => eventQueue.Enqueue(parameter);
			MSLLHOOKSTRUCT HookStruct() => new MSLLHOOKSTRUCT { time = (int)DateTime.Now.Ticks };

			// Act
			hook.MouseClickDelegateImpl(MouseHookType.WM_RBUTTONDOWN, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_RBUTTONUP, HookStruct());
			await Task.Delay(DelayBetweenClicks);

			// Assert
			eventQueue.Count.ShouldBe(2, string.Join(Environment.NewLine, eventQueue));
			var p1 = eventQueue.Dequeue();
			var p2 = eventQueue.Dequeue();

			p1.MouseInfo.ShouldHaveFlag(MouseInfo.RightButton | MouseInfo.Down);
			p2.MouseInfo.ShouldHaveFlag(MouseInfo.RightButton | MouseInfo.Up);
		}


		[Test]
		public async Task MouseClickDelegateImpl_Mouse4SingleClick()
		{
			// Arrange
			using var hook = Setup().hook;
			var eventQueue = new Queue<MouseParameter>();
			hook.GlobalEvent += (sender, parameter) => eventQueue.Enqueue(parameter);
			MSLLHOOKSTRUCT HookStruct() => TestData.TestData.GetMouse4();

			// Act
			hook.MouseClickDelegateImpl(MouseHookType.WM_XBUTTONDOWN, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_XBUTTONUP, HookStruct());
			await Task.Delay(DelayBetweenClicks);

			// Assert
			eventQueue.Count.ShouldBe(2, string.Join(Environment.NewLine, eventQueue));
			var p1 = eventQueue.Dequeue();
			var p2 = eventQueue.Dequeue();

			p1.MouseInfo.ShouldHaveFlag(MouseInfo.Mouse4 | MouseInfo.Down);
			p2.MouseInfo.ShouldHaveFlag(MouseInfo.Mouse4 | MouseInfo.Up);
		}

		[Test]
		public async Task MouseClickDelegateImpl_Mouse5SingleClick()
		{
			// Arrange
			using var hook = Setup().hook;
			var eventQueue = new Queue<MouseParameter>();
			hook.GlobalEvent += (sender, parameter) => eventQueue.Enqueue(parameter);
			MSLLHOOKSTRUCT HookStruct() => TestData.TestData.GetMouse5();

			// Act
			hook.MouseClickDelegateImpl(MouseHookType.WM_XBUTTONDOWN, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_XBUTTONUP, HookStruct());
			await Task.Delay(DelayBetweenClicks);

			// Assert
			eventQueue.Count.ShouldBe(2, string.Join(Environment.NewLine, eventQueue));
			var p1 = eventQueue.Dequeue();
			var p2 = eventQueue.Dequeue();

			p1.MouseInfo.ShouldHaveFlag(MouseInfo.Mouse5 | MouseInfo.Down);
			p2.MouseInfo.ShouldHaveFlag(MouseInfo.Mouse5 | MouseInfo.Up);
		}
	}
}