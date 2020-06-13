using NUnit.Framework;
using Shouldly;

namespace BK.IoC.Tests
{
	public class IoCTests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void Resolve_CanResolve_Class_WhichHas_Dependencies()
		{
			var ioc = new IoC();
			ioc.Register<Engine>();
			ioc.Register<Tank>();
			ioc.Register<Security>();
			ioc.Register<Car>();

			Car car = null;
			Assert.DoesNotThrow(() => car = ioc.Resolve<Car>());
			car.ShouldNotBeNull();
			
		}
	}
}