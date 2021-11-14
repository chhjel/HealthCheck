using System;
using System.Linq;
using System.Text;

namespace HealthCheck.Core.Util
{
    /// <summary>
    /// Logs to a <see cref="StringBuilder"/> in memory.
    /// <para>For use during tests to dump the log contents afterwards, use .ToString() to get the logged contents.</para>
    /// <para>Auto-create an implementation using HCLogTypeBuilder from the HealthCheck.Utility.Reflection nuget package.</para>
    /// </summary>
    public abstract class HCMemoryLoggerBase
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
				_builder.AppendLine(ExceptionUtils.GetFullExceptionDetails(exception));
            }
		}
	}
}
