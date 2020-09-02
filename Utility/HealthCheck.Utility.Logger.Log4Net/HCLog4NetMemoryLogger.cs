using HealthCheck.Core.Util;
using log4net;
using log4net.Core;
using log4net.Repository;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace HealthCheck.Utility.Logger.Log4Net
{
	/// <summary>
	/// Logs to a <see cref="StringBuilder"/> in memory.
	/// <para>For use during tests to dump the log contents afterwards throught the <c>Contents</c> property.</para>
	/// </summary>
	public class HCLog4NetMemoryLogger : HCMemoryLogger, ILog, ILogger
	{
		#region log4net.Core.ILogger
		/// <summary>
		/// Name of the logger - "HC Log4Net Memory Logger".
		/// </summary>
		public string Name => "HC Log4Net Memory Logger";

		/// <summary>
		/// Always returns null.
		/// </summary>
		public ILoggerRepository Repository => null;
		#endregion

		#region log4net.ILog
		/// <summary>
		/// Creates a new <see cref="HCLog4NetMemoryLogger"/>.
		/// </summary>
		public ILogger Logger => new HCLog4NetMemoryLogger();

		/// <summary>Returns true.</summary>
		public bool IsEnabledFor(log4net.Core.Level level) => true;

		/// <summary>Returns true.</summary>
		public bool IsDebugEnabled => true;
		/// <summary>Returns true.</summary>
		public bool IsInfoEnabled => true;
		/// <summary>Returns true.</summary>
		public bool IsWarnEnabled => true;
		/// <summary>Returns true.</summary>
		public bool IsErrorEnabled => true;
		/// <summary>Returns true.</summary>
		public bool IsFatalEnabled => true;

		/// <summary>
		/// Log an event with debug severity.
		/// </summary>
		[SuppressMessage("Critical Code Smell", "S4019:Base class methods should not be hidden", Justification = "Handled.")]
		public void Debug(object message) => base.Debug(message?.ToString());

		/// <summary>
		/// Log an event with debug severity.
		/// </summary>
		[SuppressMessage("Critical Code Smell", "S4019:Base class methods should not be hidden", Justification = "Handled.")]
		public void Debug(object message, Exception exception) => base.Debug(message?.ToString(), exception);

		/// <summary>Does nothing.</summary>
		public void DebugFormat(string format, params object[] args) { /* does nothing */ }
		/// <summary>Does nothing.</summary>
		public void DebugFormat(string format, object arg0) { /* does nothing */ }
		/// <summary>Does nothing.</summary>
		public void DebugFormat(string format, object arg0, object arg1) { /* does nothing */ }
		/// <summary>Does nothing.</summary>
		public void DebugFormat(string format, object arg0, object arg1, object arg2) { /* does nothing */ }
		/// <summary>Does nothing.</summary>
		public void DebugFormat(IFormatProvider provider, string format, params object[] args) { /* does nothing */ }

		/// <summary>
		/// Log an event with error severity.
		/// </summary>
		[SuppressMessage("Critical Code Smell", "S4019:Base class methods should not be hidden", Justification = "Handled.")]
		public void Error(object message) => base.Error(message?.ToString());

		/// <summary>
		/// Log an event with error severity.
		/// </summary>
		[SuppressMessage("Critical Code Smell", "S4019:Base class methods should not be hidden", Justification = "Handled.")]
		public void Error(object message, Exception exception) => base.Error(message?.ToString(), exception);

		/// <summary>Does nothing.</summary>
		public void ErrorFormat(string format, params object[] args) { /* does nothing */ }
		/// <summary>Does nothing.</summary>
		public void ErrorFormat(string format, object arg0) { /* does nothing */ }
		/// <summary>Does nothing.</summary>
		public void ErrorFormat(string format, object arg0, object arg1) { /* does nothing */ }
		/// <summary>Does nothing.</summary>
		public void ErrorFormat(string format, object arg0, object arg1, object arg2) { /* does nothing */ }
		/// <summary>Does nothing.</summary>
		public void ErrorFormat(IFormatProvider provider, string format, params object[] args) { /* does nothing */ }

		/// <summary>
		/// Log an event with fatal severity.
		/// </summary>
		public void Fatal(object message) => base.Error(message?.ToString());

		/// <summary>
		/// Log an event with fatal severity.
		/// </summary>
		public void Fatal(object message, Exception exception) => base.Error(message?.ToString(), exception);

		/// <summary>Does nothing.</summary>
		public void FatalFormat(string format, params object[] args) { /* does nothing */ }
		/// <summary>Does nothing.</summary>
		public void FatalFormat(string format, object arg0) { /* does nothing */ }
		/// <summary>Does nothing.</summary>
		public void FatalFormat(string format, object arg0, object arg1) { /* does nothing */ }
		/// <summary>Does nothing.</summary>
		public void FatalFormat(string format, object arg0, object arg1, object arg2) { /* does nothing */ }
		/// <summary>Does nothing.</summary>
		public void FatalFormat(IFormatProvider provider, string format, params object[] args) { /* does nothing */ }

		/// <summary>
		/// Log an event with info severity.
		/// </summary>
		public void Info(object message) => base.Information(message?.ToString());

		/// <summary>
		/// Log an event with info severity.
		/// </summary>
		public void Info(object message, Exception exception) => base.Information(message?.ToString());

		/// <summary>Does nothing.</summary>
		public void InfoFormat(string format, params object[] args) { /* does nothing */ }
		/// <summary>Does nothing.</summary>
		public void InfoFormat(string format, object arg0) { /* does nothing */ }
		/// <summary>Does nothing.</summary>
		public void InfoFormat(string format, object arg0, object arg1) { /* does nothing */ }
		/// <summary>Does nothing.</summary>
		public void InfoFormat(string format, object arg0, object arg1, object arg2) { /* does nothing */ }
		/// <summary>Does nothing.</summary>
		public void InfoFormat(IFormatProvider provider, string format, params object[] args) { /* does nothing */ }

		/// <summary>
		/// Log an event.
		/// </summary>
		public void Log(Type callerStackBoundaryDeclaringType, log4net.Core.Level level, object message, Exception exception)
		{
			if (level == Level.Fatal || level == Level.Error)
			{
				Error(message?.ToString(), exception);
			}
			else if (level == Level.Warn)
			{
				Warning(message?.ToString(), exception);
			}
			else if (level == Level.Info)
			{
				Information(message?.ToString());
			}
			else
			{
				Warning(message?.ToString(), exception);
			}
		}

		/// <summary>
		/// Log an event.
		/// </summary>
		public void Log(LoggingEvent logEvent) => Log(null, logEvent.Level, logEvent.MessageObject?.ToString(), logEvent.ExceptionObject);

		/// <summary>
		/// Log an event with warning severity.
		/// </summary>
		public void Warn(object message) => base.Warning(message?.ToString());

		/// <summary>
		/// Log an event with warning severity.
		/// </summary>
		public void Warn(object message, Exception exception) => base.Warning(message?.ToString(), exception);

		/// <summary>Does nothing.</summary>
		public void WarnFormat(string format, params object[] args) { /* does nothing */ }
		/// <summary>Does nothing.</summary>
		public void WarnFormat(string format, object arg0) { /* does nothing */ }
		/// <summary>Does nothing.</summary>
		public void WarnFormat(string format, object arg0, object arg1) { /* does nothing */ }
		/// <summary>Does nothing.</summary>
		public void WarnFormat(string format, object arg0, object arg1, object arg2) { /* does nothing */ }
		/// <summary>Does nothing.</summary>
		public void WarnFormat(IFormatProvider provider, string format, params object[] args) { /* does nothing */ }
		#endregion
	}
}
