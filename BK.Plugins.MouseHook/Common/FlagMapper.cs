using System;
using System.Collections.Generic;

namespace BK.Plugins.MouseHook.Common
{
	internal abstract class FlagMapper
	{
		protected Dictionary<int, int> ForwardMapping { get; }

		protected FlagMapper(Dictionary<int, int> mappings)
		{
			ForwardMapping = mappings;
		}

		protected int Map(int a)
		{
			int result = 0;

			foreach (KeyValuePair<int, int> mapping in ForwardMapping)
			{
				if ((a & mapping.Key) == mapping.Key)
				{
					if (mapping.Value < 0)
					{
						throw new Exception("Cannot map");
					}

					result |= mapping.Value;
				}
			}

			return result;
		}
	}
}