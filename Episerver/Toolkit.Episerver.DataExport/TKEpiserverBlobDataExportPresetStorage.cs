using EPiServer.Framework.Blobs;
using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Util.Storage;
using QoDL.Toolkit.Episerver.Utils;
using QoDL.Toolkit.Module.DataExport.Abstractions;
using QoDL.Toolkit.Module.DataExport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Episerver.DataExport;

/// <summary>
/// Stores and retrieves presets using episerver blobstorage.
/// </summary>
public class TKEpiserverBlobDataExportPresetStorage
    : TKSingleBlobStorageBase<TKEpiserverBlobDataExportPresetStorage.TKDataExportPresetBlobData>, ITKDataExportPresetStorage
{
    /// <summary>
    /// Container id used if not overridden.
    /// </summary>
    protected virtual Guid DefaultContainerId => Guid.Parse("8849c8de-4278-423f-bfc4-3264ea3170a2");

    /// <summary>
    /// Defaults to the default provider if null.
    /// </summary>
    public string ProviderName { get; set; }

    /// <summary>
    /// Defaults to a hardcoded guid if null
    /// </summary>
    public Guid? ContainerId { get; set; }

    /// <summary>
    /// Shortcut to <c>ContainerId ?? DefaultContainerId</c>
    /// </summary>
    protected Guid ContainerIdWithFallback => ContainerId ?? DefaultContainerId;

    /// <inheritdoc />
    protected override string CacheKey => $"__tk_{ContainerIdWithFallback}";

    private readonly TKEpiserverBlobHelper<TKDataExportPresetBlobData> _blobHelper;

    /// <summary>
    /// Create a new <see cref="TKEpiserverBlobDataExportPresetStorage"/>.
    /// </summary>
    public TKEpiserverBlobDataExportPresetStorage(IBlobFactory blobFactory, ITKCache cache)
        : base(cache)
    {
        _blobHelper = new TKEpiserverBlobHelper<TKDataExportPresetBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
    }

    #region ITKDataExportPresetStorage Implementation
    /// <inheritdoc />
    public Task<IEnumerable<TKDataExportStreamQueryPreset>> GetStreamQueryPresetsAsync(string streamId)
        => Task.FromResult(GetBlobData().Presets.Where(x => x.StreamId == streamId));

    /// <inheritdoc />
    public Task<TKDataExportStreamQueryPreset> GetStreamQueryPresetAsync(Guid id)
        => Task.FromResult(GetBlobData().Presets.FirstOrDefault(x => x.Id == id));

    /// <inheritdoc />
    public Task DeleteStreamQueryPresetAsync(Guid id)
    {
        var data = GetBlobData();
        if (data?.Presets?.Any(x => x.Id == id) == true)
        {
            data.Presets.RemoveAll(x => x.Id == id);
            SaveBlobData(data);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<TKDataExportStreamQueryPreset> SaveStreamQueryPresetAsync(TKDataExportStreamQueryPreset preset)
    {
        var data = GetBlobData();

        if (preset.Id == Guid.Empty)
        {
            preset.Id = Guid.NewGuid();
        }
        
        var existingIndex = data.Presets?.FindIndex(x => x.Id == preset.Id) ?? -1;
        if (existingIndex == -1)
        {
            data.Presets.Add(preset);
        }
        else
        {
            data.Presets[existingIndex] = preset;
        }

        SaveBlobData(data);
        return Task.FromResult(preset);
    }
    #endregion

    /// <inheritdoc />
    protected override TKDataExportPresetBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

    /// <inheritdoc />
    protected override void StoreBlobData(TKDataExportPresetBlobData data) => _blobHelper.StoreBlobData(data);

    /// <summary>
    /// Model stored in blob storage.
    /// </summary>
    public class TKDataExportPresetBlobData
    {
        /// <summary>
        /// Stored presets.
        /// </summary>
        public List<TKDataExportStreamQueryPreset> Presets { get; set; } = new List<TKDataExportStreamQueryPreset>();
    }
}
