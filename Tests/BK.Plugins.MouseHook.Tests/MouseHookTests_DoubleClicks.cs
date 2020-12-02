using System;
using System.Collections.Generic;
using BK.Plugins.PInvoke.Core;
using NUnit.Framework;
using BK.Plugins.MouseHook.Core;
using BK.Plugins.PInvoke;
using Moq;

namespace BK.Plugins.MouseHook.Tests
{
	[TestFixture]
	public class MouseHookTests_DoubleClicks
	{
		private readonly uint _doubleClickTicks = (uint)TimeSpan.FromMilliseconds(500).Ticks;
		private readonly int _doubleClickDiameter = 50;

		[Test]
		public void MouseClickDelegateImpl_LeftDoubleClick()
		{
			// Arrange
			var hook = Setup().hook;
			var eventQueue = new Queue<MouseParameter>();
			hook.GlobalEvent += (sender, parameter) => eventQueue.Enqueue(parameter);
			MSLLHOOKSTRUCT HookStruct() => new MSLLHOOKSTRUCT {time = (int) DateTime.Now.Ticks};

			// Act
			hook.MouseClickDelegateImpl(MouseHookType.WM_LBUTTONDOWN, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_LBUTTONUP, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_LBUTTONDOWN, HookStruct());
			hook.MouseClickDelegateImpl(MouseHookType.WM_LBUTTONUP, HookStruct());

			// Assert
		}


		private (MouseHook hook, IMock<IUser32> userMock) Setup()
		{
			var userMock = new Mock<IUser32>();

			userMock.Setup(m => m.GetDoubleClickTime())
				.Returns(_doubleClickTicks);

			userMock.Setup(m => m.GetSystemMetrics(It.IsAny<SystemMetric>()))
				.Returns(_doubleClickDiameter);

			var hook = new MouseHook(userMock.Object);
			return (hook, userMock);
		}

	}

}
