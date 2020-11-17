using System;
using System.Linq.Expressions;

namespace BK.Plugins.CreationalHelpers
{
	public abstract class Singleton<T> where T : class
	{
		public static T Instance => _instance.Value;

		private static readonly Lazy<T> _instance = new Lazy<T>(CreateInstance);
		private static T CreateInstance() => 
			Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile()();
	}
}
