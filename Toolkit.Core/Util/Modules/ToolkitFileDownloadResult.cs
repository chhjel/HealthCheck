using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QoDL.Toolkit.Core.Util.Modules;

/// <summary>
/// Can be returned from module actions to download a file.
/// </summary>
public class ToolkitFileDownloadResult
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
    /// Encoding to use.
    /// </summary>
    public Encoding Encoding { get; set; }

    /// <summary>
    /// Optionally cookies to set.
    /// </summary>
    public Dictionary<string, string> CookiesToSet { get; set; } = new();

    /// <summary>
    /// Optionally cookies to delete.
    /// </summary>
    public List<string> CookiesToDelete { get; set; } = new();

    /// <summary>
    /// Create a new file result from a stream.
    /// </summary>
    public static ToolkitFileDownloadResult CreateFromStream(string filename, Stream stream, string contentType = null)
        => new() { FileName = filename, Stream = stream, ContentType = contentType ?? "text/plain" };

    /// <summary>
    /// Create a new file result from a byte array.
    /// </summary>
    public static ToolkitFileDownloadResult CreateFromBytes(string filename, byte[] bytes, string contentType = null)
        => new() { FileName = filename, Bytes = bytes, ContentType = contentType ?? "text/plain" };

    /// <summary>
    /// Create a new file result from a string.
    /// <para>Defaults encoding to <see cref="Encoding.UTF8"/></para>
    /// </summary>
    public static ToolkitFileDownloadResult CreateFromString(string filename, string content, string contentType = null, Encoding encoding = null)
        => new() { FileName = filename, Content = content, ContentType = contentType ?? "text/plain", Encoding = encoding ?? Encoding.UTF8 };
}
