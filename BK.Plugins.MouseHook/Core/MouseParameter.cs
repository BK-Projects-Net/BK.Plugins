using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BK.Plugins.PInvoke.Core;

namespace BK.Plugins.MouseHook.Core
{
	public readonly struct MouseParameter
	{
		public readonly PrimaryMouseInfo Primary;
		public readonly MousePoint Position;
		public readonly DateTime DateTime;
		public readonly Guid Guid;

		public MouseParameter(PrimaryMouseInfo info, MousePoint position, DateTime dateTime, Guid guid)
		{
			Primary = info;
			Position = position;
			DateTime = dateTime;
			Guid = guid;
		}

		public override readonly string ToString() => 
			$"Guid: {Guid:D}, Time: {DateTime:G}, Action: {Primary:G}, {Position.ToString()}";

	}
}
