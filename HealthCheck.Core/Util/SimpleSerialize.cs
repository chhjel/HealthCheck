using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HealthCheck.Core.Util
{
	internal static class SimpleStringifier
	{
		public static string Stringify(object data, int maxLevels = 10)
		{
			try
			{
				return Stringify(null, data, maxLevels, new StringBuilder()).ToString();
			}
			catch (Exception)
			{
				return "";
			}
		}

		private static readonly Type[] SimpleTypes = new[] {
			typeof(string), typeof(DateTime), typeof(DateTimeOffset)
		};
		private static StringBuilder Stringify(string name, object data, int maxLevels, StringBuilder builder, int currentLevel = 0)
		{
			if (currentLevel >= maxLevels)
			{
				return builder;
			}

			if (data == null)
				return builder;

			var indent = new string(' ', currentLevel);
			try
			{
				if (name != null)
				{
					builder.Append($"{indent}{name}: ");
				}

				if (data.GetType().IsPrimitive || SimpleTypes.Contains(data.GetType()))
				{
					builder.Append(data?.ToString()?.Replace("\n", "\\n"));
				}
				else
				{
					builder.Append($"{indent}{{");
					builder.AppendLine();
					var props = data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
							.Where(x => !x.IsSpecialName)
							.Where(x => x.GetIndexParameters().Length == 0);
					foreach (var prop in props)
					{
						var value = prop.GetValue(data);
						Stringify(prop.Name, value, maxLevels, builder, currentLevel + 1);
					}
					builder.Append($"{indent}}}");
				}

				builder.AppendLine();
			}
			catch (Exception) { /* Ignore errors here */  }
			return builder;
		}
	}
}
