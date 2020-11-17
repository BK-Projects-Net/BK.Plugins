using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		public override readonly string ToString() => 
			$"Guid: {Guid:D}, Time: {DateTime:G}, Action: {MouseInfo:G}, {Position.ToString()}";

		public bool Is(MouseInfo info) => (MouseInfo & info) != 0;

		internal MouseParameter ToDoubleClick() => 
			new MouseParameter(MouseInfo | MouseInfo.Double, Position, DateTime, Guid);

		internal readonly struct Factory
		{
			internal static MouseParameter Create(MouseInfo info, MousePoint point, int time) => new MouseParameter(info, point, DateTime.Today + TimeSpan.FromTicks(time), System.Guid.NewGuid());
		}
	}
}
