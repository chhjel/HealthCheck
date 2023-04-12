using System;
using System.IO;

namespace QoDL.Toolkit.Core.Models;

/// <summary></summary>
internal class RequestFormFile
{
    /// <summary></summary>
    public string FileName { get; set; }

    /// <summary></summary>
    public long Length { get; set; }

    /// <summary></summary>
    public Func<Stream> GetStream { get; set; }
}
