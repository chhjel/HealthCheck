using System.IO;

namespace HealthCheck.Core.Util.Modules
{
    /// <summary>
    /// Can be returned from module actions to download a file.
    /// </summary>
    public class HealthCheckFileDownloadResult
	{
		/// <summary>
		/// Name of the file to be downloaded.
		/// </summary>
		public string FileName { get; set; } = "file";

		/// <summary>
		/// Defaults to "text/plain"
		/// </summary>
		public string ContentType { get; set; }

		/// <summary>
		/// Content of the file to be downloaded.
		/// </summary>
		public Stream Stream { get; private set; }

		/// <summary>
		/// Content of the file to be downloaded.
		/// </summary>
		public string Content { get; private set; }

		/// <summary>
		/// Content of the file to be downloaded.
		/// </summary>
		public byte[] Bytes { get; private set; }

		/// <summary>
		/// Create a new file result from a stream.
		/// </summary>
		public static HealthCheckFileDownloadResult CreateFromStream(string filename, Stream stream, string contentType = null)
			=> new() { FileName = filename, Stream = stream, ContentType = contentType ?? "text/plain" };

		/// <summary>
		/// Create a new file result from a byte array.
		/// </summary>
		public static HealthCheckFileDownloadResult CreateFromBytes(string filename, byte[] bytes, string contentType = null)
			=> new() { FileName = filename, Bytes = bytes, ContentType = contentType ?? "text/plain" };

		/// <summary>
		/// Create a new file result from a string.
		/// </summary>
		public static HealthCheckFileDownloadResult CreateFromString(string filename, string content, string contentType = null)
			=> new() { FileName = filename, Content = content, ContentType = contentType ?? "text/plain" };
	}
}
