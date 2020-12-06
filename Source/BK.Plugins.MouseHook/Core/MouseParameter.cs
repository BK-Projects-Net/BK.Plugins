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

		public MouseParameter(MouseInfo info, MousePoint point, int ticks)
		{
			MouseInfo = info;
			Position = point;
			DateTime = DateTime.Today + TimeSpan.FromTicks(ticks);
			Guid = Guid.NewGuid();
		}

		public MouseParameter(MouseInfo info, MousePoint point, int ticks, bool hasDoubleClick)
		{
			MouseInfo = info;
			Position = point;
			DateTime = DateTime.Today + TimeSpan.FromTicks(ticks);
			Guid = Guid.NewGuid();
			if (hasDoubleClick)
			{
				MouseInfo |= MouseInfo.Double;
			}
		}

		internal MouseParameter ToDoubleClick()
		{
			var info = MouseInfo;
			if (info.HasFlag(MouseInfo.Up)) info -= MouseInfo.Up;
			if (info.HasFlag(MouseInfo.Down)) info -= MouseInfo.Down;

			var result = new MouseParameter(info | MouseInfo.Double, Position, DateTime, Guid);
			return result;
		}

		public override string ToString() => 
			$"Guid: {Guid:D}, Time: {DateTime:G}, Action: {MouseInfo:G}, {Position.ToString()}";

		public bool Is(MouseInfo info) => (MouseInfo & info) != 0;

	}
}
