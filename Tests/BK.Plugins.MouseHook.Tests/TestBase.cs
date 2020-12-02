using BK.Plugins.PInvoke;
using BK.Plugins.PInvoke.Core;
using Moq;

namespace BK.Plugins.MouseHook.Tests
{
	public abstract class TestBase
	{
		protected abstract uint DoubleClickTicks { get; }
		protected abstract int DoubleClickDiameter { get; }
		protected int DelayBetweenClicks => (int)DoubleClickTicks + 100;

		protected virtual (MouseHook hook, IMock<IUser32> userMock) Setup()
		{
			var userMock = new Mock<IUser32>();

			userMock.Setup(m => m.GetDoubleClickTime())
				.Returns(DoubleClickTicks);

			userMock.Setup(m => m.GetSystemMetrics(It.IsAny<SystemMetric>()))
				.Returns(DoubleClickDiameter);

			var hook = new MouseHook(userMock.Object);
			return (hook, userMock);
		}
	}
}