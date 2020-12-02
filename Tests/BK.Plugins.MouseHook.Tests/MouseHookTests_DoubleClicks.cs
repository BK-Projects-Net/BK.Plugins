﻿using System;
using System.Collections.Generic;
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
	public class MouseHookTests_DoubleClicks
	{
		private readonly uint _doubleClickTicks = (uint)TimeSpan.FromMilliseconds(500).Ticks;
		private readonly int _doubleClickDiameter = 50;

		[Test]
		public void MouseClickDelegateImpl_LeftDoubleClick()
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

			// Assert
			eventQueue.Count.ShouldBe(1, string.Join(Environment.NewLine, eventQueue));
			var mouseParameter = eventQueue.Dequeue();
			mouseParameter.MouseInfo.ShouldHaveFlag(MouseInfo.Double | MouseInfo.LeftButton | MouseInfo.Down);
		}

		[Test]
		public void MouseClickDelegateImpl_MiddleDoubleClick()
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

			// Assert
			eventQueue.Count.ShouldBe(1, string.Join(Environment.NewLine, eventQueue));
			var mouseParameter = eventQueue.Dequeue();
			mouseParameter.MouseInfo.ShouldHaveFlag(MouseInfo.Double | MouseInfo.MiddleButton | MouseInfo.Down);
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