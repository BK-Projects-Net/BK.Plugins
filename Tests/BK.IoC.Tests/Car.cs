namespace BK.IoC.Tests
{
	public class Car
	{
		public class Factory
		{
			public Car Create(Engine e, Tank t, Security s) => new Car(e,t,s);
		}

		public Car(Engine engine, Tank tank, Security type)
		{
			Engine = engine;
			Tank = tank;
			Type = type;
		}

		public Engine Engine { get; set; }
		public Tank Tank { get; set; }
		public Security Type { get; set; }
	}
}