using System.IO;

namespace HealthCheck.Core.Util.Modules
{
	/// <summary>
	/// Can be returned from module actions to download a file.
	/// </summary>
	public class HealthCheckFileStreamResult
	{
		/// <summary>
		/// Name of the file to be downloaded.
		/// </summary>
		public string FileName { get; set; } = "file";

		/// <summary>
		/// Content of the file to be downloaded.
		/// </summary>
		public Stream ContentStream { get; set; }
	}
}
