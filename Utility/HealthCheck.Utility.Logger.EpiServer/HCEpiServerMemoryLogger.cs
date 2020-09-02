using EPiServer.Logging;
using HealthCheck.Utility.Logger.Log4Net;
using System;
using System.Text;

namespace HealthCheck.Utility.Logger.EpiServer
{
    /// <summary>
    /// Logs to a <see cref="StringBuilder"/> in memory.
    /// <para>For use during tests to dump the log contents afterwards throught the <c>Contents</c> property.</para>
    /// </summary>
    public class HCEpiServerMemoryLogger : HCLog4NetMemoryLogger, ILogger
	{
		#region EPiServer.Logging.Logger
		/// <summary>Returns true</summary>
		public bool IsEnabled(Level level) => true;

		/// <summary>
		/// Logs to memory using the given data.
		/// </summary>
		public void Log<TState, TException>(Level level, TState state, TException exception, Func<TState, TException, string> messageFormatter, Type boundaryType) 
			where TException : Exception
		{
			var message = messageFormatter?.Invoke(state, exception);
			var messageWithType = $"[{boundaryType.Name}] {message}";

			switch (level)
			{
				case Level.Critical:
				case Level.Error:
					base.Error(messageWithType, exception);
					break;
				case Level.Warning:
					base.Warning(messageWithType, exception);
					break;
				case Level.Debug:
					base.Debug(messageWithType, exception);
					break;
				default:
					base.Information(messageWithType, exception);
					break;
			}
		}
		#endregion
	}
}
