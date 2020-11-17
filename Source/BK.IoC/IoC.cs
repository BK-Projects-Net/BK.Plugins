using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

// TODO: allow multiple ctors
// TODO: allow to resolve instances where the dependency has also dependencies
// TODO: proper Exception handling

namespace BK.IoC
{
	public class IoC
	{
		private IDictionary<Type, Type> _map = new Dictionary<Type, Type>();

		public void Register<T>() => _map[typeof(T)] = typeof(T);

		public T Resolve<T>()
		{
			var type = _map[typeof(T)];
			var constructors = type.GetConstructors();

			var ctor = constructors.First();
			var parameters = ctor.GetParameters().Select(p => (Type) p.ParameterType);
			var dependencies = parameters.Select(t => _map[t]).ToList();
			if (dependencies.Count <= 0)
				return CreateGenericInstance<T>();

			var expression = BuildFactoryExpression<T>(ctor);
			var instance = expression.Invoke(dependencies.Select(CreateInstance).ToArray());
			return instance;

		}

		private T CreateGenericInstance<T>() => Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile()();

		private T CreateGenericInstance<T>(List<object> dependencies) =>
			Expression.Lambda<Func<List<object>, T>>(Expression.New(typeof(T))).Compile()(dependencies);

		private object CreateInstance(Type type) => Expression.Lambda(Expression.New(type)).Compile().DynamicInvoke();

		private static Func<object[], T> BuildFactoryExpression<T>(ConstructorInfo ctor)
		{
			ParameterInfo[] par = ctor.GetParameters(); // Get the parameters of the constructor
			Expression[] args = new Expression[par.Length];
			ParameterExpression param = Expression.Parameter(typeof(object[])); // The object[] paramter to the Func
			for (int i = 0; i != par.Length; ++i)
			{
				// get the item from the array in the parameter and cast it to the correct type for the constructor
				args[i] = Expression.Convert(Expression.ArrayIndex(param, Expression.Constant(i)),
					par[i].ParameterType);
			}

			return Expression.Lambda<Func<object[], T>>(
				// call the constructor and cast to IMyInterface.
				Expression.Convert(
					Expression.New(ctor, args)
					, typeof(T)
				), param
			).Compile();
		}
	}
}
