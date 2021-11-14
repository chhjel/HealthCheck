using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace HealthCheck.Utility.Reflection.Logging
{
    /// <summary>
    /// Generates logger implementations at runtime for given logging interfaces.
    /// </summary>
    public static class HCLogTypeBuilder
	{
		private static readonly Dictionary<Type, Type> _typeCache = new();

		/// <summary>
		/// Generates logger implementations at runtime for the given logging interface.
		/// </summary>
		/// <param name="logCountLimit">Optional max number of times the log method can be called before it's ignored.</param>
		public static TInterface CreateMemoryLoggerFor<TInterface>(int logCountLimit = 100000)
			where TInterface : class
			=> CreateMemoryLoggerFor(typeof(TInterface), logCountLimit) as TInterface;

		/// <summary>
		/// Generates logger implementations at runtime for the given logging interface.
		/// </summary>
		public static object CreateMemoryLoggerFor(Type interfce, int logCountLimit = 100000)
		{
			lock (_typeCache)
			{
				if (!_typeCache.ContainsKey(interfce))
				{
					var myTypeInfo = CompileResultTypeInfo(typeof(HCMemoryLoggerBase), interfce);
					_typeCache[interfce] = myTypeInfo.AsType();
				}

				var instance = Activator.CreateInstance(_typeCache[interfce]);
				if (instance is HCMemoryLoggerBase logger)
				{
					logger.LogCountLimit = logCountLimit;
				}

				return instance;
			}
		}

		private static TypeInfo CompileResultTypeInfo(Type baseClass, Type interfce)
		{
			var typeBuilder = GetTypeBuilder(baseClass, interfce);

			typeBuilder.AddInterfaceImplementation(interfce);
			foreach (var methodInfo in interfce.GetMethods())
			{
				CreateMethodImplementation(typeBuilder, methodInfo);
			}

			return typeBuilder.CreateTypeInfo();
		}

		private static TypeBuilder GetTypeBuilder(Type baseClass, Type interfce)
		{
			var typeSignature = $"{interfce.Name}_{Guid.NewGuid().ToString().Replace("-", "")}";
			var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
			ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
			TypeBuilder tb = moduleBuilder.DefineType(typeSignature,
					TypeAttributes.Public | TypeAttributes.Class |
					TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
					TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout,
					parent: baseClass);
			return tb;
		}

		private static void CreateMethodImplementation(TypeBuilder typeBuilder, MethodInfo methodInfo)
		{
			var parameters = methodInfo.GetParameters();
			var parameterTypes = parameters.Select(x => x.ParameterType).ToArray();

			MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual,
				methodInfo.ReturnType, parameterTypes);
			typeBuilder.DefineMethodOverride(methodBuilder, methodInfo);

			var ilGen = methodBuilder.GetILGenerator();

			#region Call method on DynamicBaseLogger
			var baseMethod = typeof(HCMemoryLoggerBase).GetMethod("___LogToMemory", BindingFlags.NonPublic | BindingFlags.Instance);

			// this
			ilGen.Emit(OpCodes.Ldarg_0);

			// MethodName
			ilGen.Emit(OpCodes.Ldstr, methodInfo.Name);

			// Load all parameters as object[]
			ilGen.DeclareLocal(typeof(object[]));
			ilGen.Emit(OpCodes.Ldc_I4, parameters.Length);
			ilGen.Emit(OpCodes.Newarr, typeof(object));

			for (int i = 0; i < parameters.Length; i++)
			{
				ilGen.Emit(OpCodes.Dup);
				ilGen.Emit(OpCodes.Ldc_I4, i);
				ilGen.Emit(OpCodes.Ldarg, i + 1);

				var type = parameters[i].ParameterType;
				if (type.IsValueType) { ilGen.Emit(OpCodes.Box, type); }

				ilGen.Emit(OpCodes.Stelem_Ref);
			}

			ilGen.Emit(OpCodes.Call, baseMethod);
			#endregion

			ilGen.Emit(OpCodes.Ret);
		}
	}
}
