using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace QoDL.Toolkit.Core.Util
{
    /// <summary>
    /// Logs to a <see cref="StringBuilder"/> in memory.
    /// <para>For use during tests to dump the log contents afterwards, use .ToString() to get the logged contents.</para>
    /// <para>Auto-create an implementation using TKLogTypeBuilder from the QoDL.Toolkit.Utility.Reflection nuget package.</para>
    /// </summary>
    public abstract class TKMemoryLoggerBase
	{
		/// <summary>
		/// Max number of times the log method can be called before it's ignored.
		/// </summary>
		public int? LogCountLimit { get; set; } = 100000;

		/// <summary>
		/// Contains all logged data.
		/// </summary>
		private readonly StringBuilder _builder = new();

		/// <summary>
		/// Clear the currently logged contents.
		/// </summary>
		public void ClearLoggedData() => _builder.Clear();

		/// <summary>
		/// Returns the logged lines.
		/// </summary>
		public override string ToString() => _builder.ToString();

		private int _logCount = 0;
		private object _forwardTo;

		/// <summary>
		/// Invoked from util.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "A bit obscure naming on purpose to prevent collisions.")]
		public void ___InitLogger(Type interfce, object forwardTo = null)
		{
			if (interfce != null && forwardTo != null && interfce.IsAssignableFrom(forwardTo.GetType()))
            {
				_forwardTo = forwardTo;
			}
		}

		/// <summary>
		/// Invoked from IL.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "A bit obscure naming on purpose to prevent collisions.")]
        protected void ___LogToMemory(string methodName, object[] parameters)
		{
			if (LogCountLimit != null)
            {
				if (_logCount >= LogCountLimit.Value)
                {
					return;
                }
				_logCount++;
            }

			var parameterStrings = parameters.Select(x =>
			{
				if (x == null) return "null";
				else if (x is Exception ex) return ex.GetType().Name;

				var value = x.ToString();
				var shouldQuote = x?.GetType()?.IsValueType != true;
				if (shouldQuote)
				{
					value = $"\"{value}\"";
				}
				return value;
			});

			var summary = string.Join(", ", parameterStrings);
			var line = $"{methodName}({summary})";
			_builder.AppendLine(line);

			var exceptions = parameters.OfType<Exception>();
			foreach (var exception in exceptions)
            {
				_builder.AppendLine(TKExceptionUtils.GetFullExceptionDetails(exception));
            }

			if (_forwardTo != null)
            {
				try
                {
					___TryForwardEvent(methodName, parameters);
                }
                catch (Exception) { /* Ignored */ }
            }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "A bit obscure naming on purpose to prevent collisions.")]
		private void ___TryForwardEvent(string methodName, object[] parameters)
        {
			var forwardType = _forwardTo.GetType();
			var method = forwardType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
				.FirstOrDefault(x => {
					if (x.Name != methodName) return false;
					var p = x.GetParameters();
					if (p.Length != parameters.Length) return false;
					return true;
				});
			method.Invoke(_forwardTo, parameters);
        }
	}
}
