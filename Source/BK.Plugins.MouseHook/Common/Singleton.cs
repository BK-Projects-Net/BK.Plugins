using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BK.Plugins.MouseHook.Common
{
	public abstract class Singleton<T> where T : class
	{
		public static T Instance => _instance.Value;

		private static readonly Lazy<T> _instance = new Lazy<T>(CreateInstance);
		private static T CreateInstance() => 
			Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile()();
	}
}
