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

			// act
			hook.MouseClickDelegateImpl(MouseHookType.WM_LBUTTONDOWN, new MSLLHOOKSTRUCT());
			hook.MouseClickDelegateImpl(MouseHookType.WM_LBUTTONUP, new MSLLHOOKSTRUCT());
			hook.MouseClickDelegateImpl(MouseHookType.WM_LBUTTONDOWN, new MSLLHOOKSTRUCT());
			hook.MouseClickDelegateImpl(MouseHookType.WM_LBUTTONDOWN, new MSLLHOOKSTRUCT());

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
