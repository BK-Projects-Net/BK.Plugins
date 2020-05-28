using System;
using System.Collections.Generic;

namespace BK.Plugins.MouseHook.Common
{
	internal abstract class DictionaryMapper<TKey, TValue> 
	{
		protected Dictionary<TKey, TValue> Mapping { get; }
		public abstract TValue FallBack { get; } 

		protected DictionaryMapper(Dictionary<TKey, TValue> mappings)
		{
			Mapping = mappings;
		}

		protected TValue Map(TKey a) => !Mapping.TryGetValue(a, out var result) ? EvaluateFallBackOverride() : result;

		/// <summary>
		/// You can override this and throw an exception instead
		/// </summary>
		protected virtual TValue EvaluateFallBackOverride() => FallBack;
	}
}