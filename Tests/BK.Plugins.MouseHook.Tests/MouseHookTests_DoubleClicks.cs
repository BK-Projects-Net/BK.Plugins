using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BK.Plugins.PInvoke.Core;
using NUnit.Framework;
using BK.Plugins.MouseHook.Core;
using BK.Plugins.PInvoke;
using Moq;
using Shouldly;
using Shouldly.ShouldlyExtensionMethods;

namespace BK.Plugins.MouseHook.Tests
{
	[TestFixture]
	public class MouseHookTests_DoubleClicks : TestBase
	{
		protected override uint DoubleClickTicks => (uint)TimeSpan.FromMilliseconds(500).Milliseconds;
		protected override int DoubleClickDiameter => 50;

		[Test]
		public async Task MouseClickDelegateImpl_LeftDoubleClick()
		{
			// Arrange
			using var hook = Setup().hook;
			var eventQueue = new Queue<MouseParameter>();
			hook.GlobalEvent += (sender, parameter) => eventQueue.Enqueue(parameter);
			MSLLHOOKSTRUCT HookStruct() => new MSLLHOOKSTRUCT {time = (int) DateTime.Now.Ticks};

			// Act
			hook.MouseClickDelegateImpl(MouseHookType.WM_LBUTTONDOWN, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_LBUTTONUP, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_LBUTTONDOWN, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_LBUTTONUP, HookStruct());

			await Task.Delay(DelayBetweenClicks);

			// Assert
			eventQueue.Count.ShouldBe(1, string.Join(Environment.NewLine, eventQueue));
			var mouseParameter = eventQueue.Dequeue();
			mouseParameter.MouseInfo.ShouldHaveFlag(MouseInfo.Double | MouseInfo.LeftButton);
		}

		[Test]
		public async Task MouseClickDelegateImpl_MiddleDoubleClick()
		{
			// Arrange
			using var hook = Setup().hook;
			var eventQueue = new Queue<MouseParameter>();
			hook.GlobalEvent += (sender, parameter) => eventQueue.Enqueue(parameter);
			MSLLHOOKSTRUCT HookStruct() => new MSLLHOOKSTRUCT { time = (int)DateTime.Now.Ticks };

			// Act
			hook.MouseClickDelegateImpl(MouseHookType.WM_MBUTTONDOWN, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_MBUTTONUP, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_MBUTTONDOWN, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_MBUTTONUP, HookStruct());

			await Task.Delay(DelayBetweenClicks);

			// Assert
			eventQueue.Count.ShouldBe(1, string.Join(Environment.NewLine, eventQueue));
			var mouseParameter = eventQueue.Dequeue();
			mouseParameter.MouseInfo.ShouldHaveFlag(MouseInfo.Double | MouseInfo.MiddleButton);
		}

		[Test]
		public async Task MouseClickDelegateImpl_RightDoubleClick()
		{
			// Arrange
			using var hook = Setup().hook;
			var eventQueue = new Queue<MouseParameter>();
			hook.GlobalEvent += (sender, parameter) => eventQueue.Enqueue(parameter);
			MSLLHOOKSTRUCT HookStruct() => new MSLLHOOKSTRUCT { time = (int)DateTime.Now.Ticks };

			// Act
			hook.MouseClickDelegateImpl(MouseHookType.WM_RBUTTONDOWN, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_RBUTTONUP, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_RBUTTONDOWN, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_RBUTTONUP, HookStruct());

			await Task.Delay(DelayBetweenClicks);

			// Assert
			eventQueue.Count.ShouldBe(1, string.Join(Environment.NewLine, eventQueue));
			var mouseParameter = eventQueue.Dequeue();
			mouseParameter.MouseInfo.ShouldHaveFlag(MouseInfo.Double | MouseInfo.RightButton);
		}

		[Test]
		public async Task MouseClickDelegateImpl_Mouse4DoubleClick()
		{
			// Arrange
			using var hook = Setup().hook;
			var eventQueue = new Queue<MouseParameter>();
			hook.GlobalEvent += (sender, parameter) => eventQueue.Enqueue(parameter);
			MSLLHOOKSTRUCT HookStruct() => new MSLLHOOKSTRUCT { time = (int)DateTime.Now.Ticks, mouseData = 0x010000};

			// Act
			hook.MouseClickDelegateImpl(MouseHookType.WM_XBUTTONDOWN, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_XBUTTONUP, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_XBUTTONDOWN, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_XBUTTONUP, HookStruct());

			await Task.Delay(DelayBetweenClicks);

			// Assert
			eventQueue.Count.ShouldBe(1, string.Join(Environment.NewLine, eventQueue));
			var mouseParameter = eventQueue.Dequeue();
			mouseParameter.MouseInfo.ShouldHaveFlag(MouseInfo.Double | MouseInfo.Mouse4);
		}

		[Test]
		public async Task MouseClickDelegateImpl_Mouse5DoubleClick()
		{
			// Arrange
			using var hook = Setup().hook;
			var eventQueue = new Queue<MouseParameter>();
			hook.GlobalEvent += (sender, parameter) => eventQueue.Enqueue(parameter);
			MSLLHOOKSTRUCT HookStruct() => new MSLLHOOKSTRUCT { time = (int)DateTime.Now.Ticks, mouseData = 0x020000 };

			// Act
			hook.MouseClickDelegateImpl(MouseHookType.WM_XBUTTONDOWN, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_XBUTTONUP, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_XBUTTONDOWN, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_XBUTTONUP, HookStruct());

			await Task.Delay(DelayBetweenClicks);

			// Assert
			eventQueue.Count.ShouldBe(1, string.Join(Environment.NewLine, eventQueue));
			var mouseParameter = eventQueue.Dequeue();
			mouseParameter.MouseInfo.ShouldHaveFlag(MouseInfo.Double | MouseInfo.Mouse5);
		}

	}
}
