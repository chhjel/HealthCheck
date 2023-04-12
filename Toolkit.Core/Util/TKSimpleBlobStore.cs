using QoDL.Toolkit.Core.Modules.Metrics.Context;
using System;
using System.IO;

namespace QoDL.Toolkit.Core.Util;

/// <summary>
/// Simple large string data storage where each item is stored in a separate file.
/// </summary>
public class TKSimpleBlobStore
{
    /// <summary>
    /// Options for automatic storage cleanup.
    /// <para>Cleanup affects all files in the <see cref="BlobFolderPath"/> that has the .SimpleBlob file extension.</para>
    /// </summary>
    public TKSimpleBlobStoreRetentionOptions RetentionOptions { get; set; }

    /// <summary>
    /// Path to the storage folder.
    /// </summary>
    public string BlobFolderPath { get; private set; }

    private DateTimeOffset? _lastCleanupPerformedAt;
    private const string BLOB_FILE_EXTENSION = ".SimpleBlob";

    /// <summary>
    /// Simple data storage where each item is stored in a separate file.
    /// </summary>
    /// <param name="blobFolder">Path to the file where data will be stored.</param>
    public TKSimpleBlobStore(string blobFolder)
    {
        BlobFolderPath = blobFolder;

        EnsureFolderExists();
    }

    /// <summary>
    /// Store a string blob and returns a generated id for it.
    /// </summary>
    public Guid StoreBlob(string data)
    {
        var id = Guid.NewGuid();
        var path = GetBlobFilePath(id);
        var timer = new TKMetricsTimer($"SimpleBlob({Path.GetFileNameWithoutExtension(path)}).SaveData()");
        File.WriteAllText(path, data ?? "");
        TKMetricsContext.AddGlobalTimingValue(timer);
        return id;
    }

    /// <summary>
    /// Get an string blob object with the given id.
    /// </summary>
    public string GetBlob(Guid id)
    {
        var path = GetBlobFilePath(id);
        var timer = new TKMetricsTimer($"SimpleBlob({Path.GetFileNameWithoutExtension(path)}).LoadData()");
        var contents = File.ReadAllText(path);
        TKMetricsContext.AddGlobalTimingValue(timer);
        return contents;
    }

    /// <summary>
    /// Returns true if a blob for the given id is found.
    /// </summary>
    public bool HasBlob(Guid id)
    {
        var path = GetBlobFilePath(id);
        return File.Exists(path);
    }

    /// <summary>
    /// Perform cleanup if retention options allow it.
    /// </summary>
    protected void CheckCleanup()
    {
        if (_lastCleanupPerformedAt == null && RetentionOptions?.DelayFirstCleanupByMinimumCleanupInterval == true)
        {
            _lastCleanupPerformedAt = DateTimeOffset.Now;
        }

        if (!CanCleanup())
        {
            return;
        }

        PerformCleanup();
    }

    private bool CanCleanup()
    {
        // Cleanup not enabled => abort
        if (RetentionOptions == null)
        {
            return false;
        }

        // Less than min time since last cleanup => abort
        if (_lastCleanupPerformedAt != null && (DateTimeOffset.Now.ToUniversalTime() - _lastCleanupPerformedAt?.ToUniversalTime()) < RetentionOptions.MinimumCleanupInterval)
        {
            return false;
        }

        return true;
    }

    private void PerformCleanup()
    {
        _lastCleanupPerformedAt = DateTimeOffset.Now;
        if (RetentionOptions?.MaxItemAge != null)
        {
            var threshold = DateTimeOffset.Now.ToUniversalTime() - RetentionOptions.MaxItemAge;
            try
            {
                var blobs = Directory.GetFiles(BlobFolderPath, $"*{BLOB_FILE_EXTENSION}");
                foreach (var blob in blobs)
                {
                    var blobInfo = new FileInfo(blob);
                    if (blobInfo.LastWriteTimeUtc <= threshold)
                    {
                        blobInfo.Delete();
                    }
                }
            }
            catch (Exception) { /* Ignore any errors here */ }
        }
    }

    private string GetBlobFilePath(Guid id)
    {
        var filename = $"{id}{BLOB_FILE_EXTENSION}";
        return Path.Combine(BlobFolderPath, filename);
    }

    private void EnsureFolderExists()
    {
        if (!Directory.Exists(BlobFolderPath))
        {
            Directory.CreateDirectory(BlobFolderPath);
        }
    }
}
