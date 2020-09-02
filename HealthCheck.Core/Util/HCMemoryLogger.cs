using System;
using System.Text;

namespace HealthCheck.Core.Util
{
	/// <summary>
	/// Logs to a <see cref="StringBuilder"/> in memory.
	/// <para>For use during tests to dump the log contents afterwards throught the <c>Contents</c> property.</para>
	/// <para>Separate nuget packages are available for e.g. log4net and Episerver logging implementations.</para>
	/// </summary>
	public class HCMemoryLogger
	{
        #region Constants
		/// <summary>
		/// Log prefix for debug level events.
		/// </summary>
        protected const string Prefix_Debug = "[DEBUG]";

		/// <summary>
		/// Log prefix for info level events.
		/// </summary>
		protected const string Prefix_Info = "[INFO]";

		/// <summary>
		/// Log prefix for warning level events.
		/// </summary>
		protected const string Prefix_Warning = "[WARNING]";

		/// <summary>
		/// Log prefix for error level events.
		/// </summary>
		protected const string Prefix_Error = "[ERROR]";
        #endregion

        #region Fields & Properties
        /// <summary>
        /// True if there's any errors logged.
        /// </summary>
        public virtual bool HasAnyError => ErrorCount > 0;

		/// <summary>
		/// Number of logged errors.
		/// </summary>
		public virtual int ErrorCount { get; private set; }

		/// <summary>
		/// True if there's any warnings logged.
		/// </summary>
		public virtual bool HasAnyWarning => WarningCount > 0;

		/// <summary>
		/// Number of logged warnings.
		/// </summary>
		public virtual int WarningCount { get; private set; }

		/// <summary>
		/// Current contents of the stringbuilder - all the logged data.
		/// </summary>
		public virtual string Contents => Builder.ToString();

		/// <summary>
		/// Contains all logged data.
		/// </summary>
		protected virtual StringBuilder Builder { get; set; } = new StringBuilder();
		#endregion

		/// <summary>
		/// Clear the currently logged contents.
		/// </summary>
		public virtual void Clear() => Builder.Clear();

		/// <summary>
		/// Log a message with information level severity.
		/// </summary>
		public virtual void Information(string message)
			=> Builder.AppendLine($"{Prefix_Info} {message}");

		/// <summary>
		/// Log a message with information level severity.
		/// </summary>
		public virtual void Information(string message, Exception exception)
		{
			Builder.AppendLine($"{Prefix_Info} {message}");
			if (exception != null)
			{
				Builder.AppendLine(exception.ToString());
			}
		}

		/// <summary>
		/// Log a message with debug level severity.
		/// </summary>
		public virtual void Debug(string message)
			=> Builder.AppendLine($"{Prefix_Debug} {message}");

		/// <summary>
		/// Log a message with debug level severity.
		/// </summary>
		public virtual void Debug(string message, Exception exception)
		{
			Builder.AppendLine($"{Prefix_Debug} {message}");
			if (exception != null)
            {
				Builder.AppendLine(exception.ToString());
            }
		}

		/// <summary>
		/// Log a message with warning level severity.
		/// </summary>
		public virtual void Warning(string message)
		{
			Builder.AppendLine($"{Prefix_Warning} {message}");
			WarningCount++;
		}

		/// <summary>
		/// Log a message and exception with warning level severity.
		/// </summary>
		public virtual void Warning(string message, Exception exception)
		{
			Builder.AppendLine($"{Prefix_Warning} {message}");
			if (exception != null)
            {
				Builder.AppendLine(exception.ToString());
            }
			WarningCount++;
		}

		/// <summary>
		/// Log a message with error level severity.
		/// </summary>
		public virtual void Error(string message)
		{
			Builder.AppendLine($"{Prefix_Error} {message}");
			ErrorCount++;
		}

		/// <summary>
		/// Log a message and exception with error level severity.
		/// </summary>
		public virtual void Error(string message, Exception exception)
		{
			Builder.AppendLine($"{Prefix_Error} {message}");
			if (exception != null)
			{
				Builder.AppendLine(exception.ToString());
			}
			ErrorCount++;
		}
	}
}
