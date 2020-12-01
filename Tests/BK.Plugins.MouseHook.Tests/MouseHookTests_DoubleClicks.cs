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
using BK.Plugins.PInvoke;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;

namespace BK.Plugins.MouseHook.Tests
{

	[TestFixture]
	public class MouseHookTests_DoubleClicks
	{
		[Test]
		public void MouseClickDelegateImpl_LeftDoubleClick()
		{
			// Arrange
			var setup = Setup();
			var hook = setup.hook;
			var userMock = setup.userMock;
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

		private readonly int _doubleClickTicks = (int)TimeSpan.FromMilliseconds(500).Ticks;

		private (MouseHook hook, IMock<IUser32> userMock) Setup()
		{
			var userMock = new Mock<IUser32>();
			
			userMock.Setup(m => m.GetSystemMetrics(It.IsAny<SystemMetric>()))
				.Returns(5);

			userMock.Setup(m => m.GetDoubleClickTime())
				.Returns((uint) TimeSpan.FromMilliseconds(_doubleClickTicks).Ticks);

			var hook = new MouseHook(userMock.Object);
			return (hook, userMock);
		}

	}

}
