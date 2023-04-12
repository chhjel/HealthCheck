using EPiServer.Framework.Blobs;
using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Util.Storage;
using QoDL.Toolkit.Episerver.Utils;
using QoDL.Toolkit.Module.DynamicCodeExecution.Abstractions;
using QoDL.Toolkit.Module.DynamicCodeExecution.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Episerver.DynamicCodeExecution;

/// <summary>
/// Stores and retrieves <see cref="DynamicCodeScript"/>s using episerver blobstorage.
/// </summary>
public class TKEpiserverBlobDynamicCodeScriptStorage
    : TKSingleBlobStorageBase<TKEpiserverBlobDynamicCodeScriptStorage.TKDynamicCodeExecutionBlobData>, IDynamicCodeScriptStorage
{
    /// <summary>
    /// Container id used if not overridden.
    /// </summary>
    protected virtual Guid DefaultContainerId => Guid.Parse("81b6186e-5909-4148-b527-f6e2be64c759");

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

    private readonly TKEpiserverBlobHelper<TKDynamicCodeExecutionBlobData> _blobHelper;

    /// <summary>
    /// Create a new <see cref="TKEpiserverBlobDynamicCodeScriptStorage"/>.
    /// </summary>
    public TKEpiserverBlobDynamicCodeScriptStorage(IBlobFactory blobFactory, ITKCache cache)
        : base(cache)
    {
        _blobHelper = new TKEpiserverBlobHelper<TKDynamicCodeExecutionBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
    }

    /// <summary>
    /// Get all stored scripts.
    /// </summary>
    public async Task<List<DynamicCodeScript>> GetAllScripts()
        => await Task.FromResult(GetBlobData().Scripts);

    /// <summary>
    /// Get a single stored script.
    /// </summary>
    public async Task<DynamicCodeScript> GetScript(Guid id)
        => await Task.FromResult(GetBlobData().Scripts.FirstOrDefault(x => x.Id == id));

    /// <summary>
    /// Deletes a single stored script.
    /// </summary>
    public async Task<bool> DeleteScript(Guid id)
    {
        var data = GetBlobData();
        if (data?.Scripts?.Any(x => x.Id == id) == true)
        {
            data.Scripts.RemoveAll(x => x.Id == id);
            SaveBlobData(data);
        }
        return await Task.FromResult(true);
    }

    /// <summary>
    /// Save or create the given script and return the script with any changes.
    /// </summary>
    public async Task<DynamicCodeScript> SaveScript(DynamicCodeScript script)
    {
        var data = GetBlobData();

        var scriptToSave = (script.Id == Guid.Empty)
            ? default
            : data.Scripts?.FirstOrDefault(x => x.Id == script.Id);

        if (scriptToSave == null)
        {
            scriptToSave = script;
            scriptToSave.Id = (scriptToSave.Id == Guid.Empty) ? Guid.NewGuid() : scriptToSave.Id;
            data.Scripts.Add(scriptToSave);
        }

        scriptToSave.Title = script.Title;
        scriptToSave.Code = script.Code;

        SaveBlobData(data);
        return await Task.FromResult(scriptToSave);
    }

    /// <inheritdoc />
    protected override TKDynamicCodeExecutionBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

    /// <inheritdoc />
    protected override void StoreBlobData(TKDynamicCodeExecutionBlobData data) => _blobHelper.StoreBlobData(data);

    /// <summary>
    /// Model stored in blob storage.
    /// </summary>
    public class TKDynamicCodeExecutionBlobData
    {
        /// <summary>
        /// Stored scripts.
        /// </summary>
        public List<DynamicCodeScript> Scripts { get; set; } = new List<DynamicCodeScript>();
    }
}
