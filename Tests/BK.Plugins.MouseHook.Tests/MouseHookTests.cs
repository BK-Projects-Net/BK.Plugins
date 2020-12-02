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
	public class MouseHookTests
	{
		private readonly uint _doubleClickTicks = (uint)TimeSpan.FromMilliseconds(500).Ticks;
		private readonly int _doubleClickDiameter = 50;

		[Test]
		[NonParallelizable]
		public void MouseClickDelegateImpl_LeftDoubleClick()
		{
			// arrange
			var hook = Setup();
			var eventQueue = new Queue<MouseParameter>();
			hook.GlobalEvent += (sender, parameter) => eventQueue.Enqueue(parameter);

			MSLLHOOKSTRUCT hookStructFactory() => new MSLLHOOKSTRUCT() {time = (int) DateTime.Now.Ticks};

			// act
			hook.MouseClickDelegateImpl(MouseHookType.WM_LBUTTONDOWN, hookStructFactory());
			hook.MouseClickDelegateImpl(MouseHookType.WM_LBUTTONUP, hookStructFactory());
			hook.MouseClickDelegateImpl(MouseHookType.WM_LBUTTONDOWN, hookStructFactory());
			hook.MouseClickDelegateImpl(MouseHookType.WM_LBUTTONUP, hookStructFactory());

			// assert
		

		}

		private MouseHook Setup()
		{
			var userMock = new Mock<IUser32>();
			userMock.Setup(m => m.GetDoubleClickTime())
				.Returns(_doubleClickTicks);

			userMock.Setup(m => m.GetSystemMetrics(It.IsAny<SystemMetric>()))
				.Returns(_doubleClickDiameter);

			var hook = new MouseHook(userMock.Object);
			return hook;
		}

	}

}
