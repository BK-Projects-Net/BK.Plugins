using System;

namespace BK.Plugins.MouseHook.Core
{
	public readonly struct MouseParameter
	{
		public readonly MouseInfo MouseInfo;
		public readonly MousePoint Position;
		public readonly DateTime DateTime;
		public readonly Guid Guid;

		public MouseParameter(MouseInfo info, MousePoint position, DateTime dateTime, Guid guid)
		{
			MouseInfo = info;
			Position = position;
			DateTime = dateTime;
			Guid = guid;
		}

		internal MouseParameter ToDoubleClick() => new MouseParameter(MouseInfo | MouseInfo.Double, Position, DateTime, Guid);

		public override string ToString() => 
			$"Guid: {Guid:D}, Time: {DateTime:G}, Action: {MouseInfo:G}, {Position.ToString()}";

		public bool Is(MouseInfo info) => (MouseInfo & info) != 0;

		public readonly struct Factory
		{
			public static MouseParameter Create(MouseInfo info, MousePoint point, int ticks) => 
				new MouseParameter(info, point, DateTime.Today + TimeSpan.FromTicks(ticks), Guid.NewGuid());
			
			public static MouseParameter CreateHasDoubleClick(MouseInfo info, MousePoint point, int ticks) =>
				new MouseParameter(info | MouseInfo.Double, point, DateTime.Today + TimeSpan.FromTicks(ticks), Guid.NewGuid());
		}
	}
}
