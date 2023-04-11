using QoDL.Toolkit.Core.Modules.AccessTokens.Abstractions;
using QoDL.Toolkit.Core.Modules.AccessTokens.Models;
using QoDL.Toolkit.Core.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.WebUI.Services;

/// <summary>
/// Stores and retrieves <see cref="TKAccessToken"/>s.
/// </summary>
public class FlatFileAccessManagerTokenStorage : IAccessManagerTokenStorage
{
    private TKSimpleDataStoreWithId<TKAccessToken, Guid> Store { get; set; }

    /// <summary>
    /// Create a new <see cref="FlatFileAccessManagerTokenStorage"/> with the given file path.
    /// </summary>
    /// <param name="filepath">Filepath to where the data will be stored.</param>
    public FlatFileAccessManagerTokenStorage(string filepath)
    {
        Store = new TKSimpleDataStoreWithId<TKAccessToken, Guid>(
            filepath,
            serializer: new Func<TKAccessToken, string>((e) => JsonConvert.SerializeObject(e)),
            deserializer: new Func<string, TKAccessToken>((row) => JsonConvert.DeserializeObject<TKAccessToken>(row)),
            idSelector: (e) => e.Id,
            idSetter: (e, id) => e.Id = id,
            nextIdFactory: (events, e) => (e.Id == Guid.Empty ? Guid.NewGuid() : e.Id)
        );
    }

    /// <summary>
    /// Get all tokens.
    /// </summary>
    public IEnumerable<TKAccessToken> GetTokens()
        => Store.GetEnumerable();

    /// <summary>
    /// Get a single token by id.
    /// </summary>
    public TKAccessToken GetToken(Guid id)
        => Store.GetItem(id);

    /// <summary>
    /// Save a new token.
    /// </summary>
    public TKAccessToken SaveNewToken(TKAccessToken token)
        => Store.InsertItem(token);

    /// <summary>
    /// Update token last used at datetime.
    /// </summary>
    public TKAccessToken UpdateTokenLastUsedAtTime(Guid id, DateTimeOffset time)
    {
        var token = GetToken(id);
        if (token != null)
        {
            token.LastUsedAt = time;
            Store.InsertOrUpdateItem(token);
        }
        return token;
    }

    /// <summary>
    /// Delete a token.
    /// </summary>
    public void DeleteToken(Guid tokenId)
        => Store.DeleteItem(tokenId);
}
