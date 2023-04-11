using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Tests.Models;

/// <summary>
/// Result from a test.
/// </summary>
public class TestResultDataDump
{
    /// <summary>
    /// Name of the data.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Data contents.
    /// </summary>
    public string Data { get; set; }

    /// <summary>
    /// Type of data.
    /// </summary>
    public TestResultDataDumpType Type { get; set; }

    /// <summary>
    /// Removes copy/fullscreeen buttons.
    /// </summary>
    public bool? DisplayClean { get; set; }

    /// <summary>
    /// Optional filename for download.
    /// <para>If set, a download button will be displayed where the data can be downloaded.</para>
    /// </summary>
    public string DownloadFileName { get; set; }

    /// <summary>
    /// Any extra flags.
    /// </summary>
    public List<string> Flags { get; set; }
}
