using QoDL.Toolkit.Core.Modules.SecureFileDownload.Models;
using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.SecureFileDownload.Abstractions;

/// <summary>
/// Provides <see cref="SecureFileDownloadDefinition"/>s.
/// </summary>
public interface ISecureFileDownloadDefinitionStorage
{
    /// <summary>
    /// Retrieve all stored definitions to display.
    /// </summary>
    IEnumerable<SecureFileDownloadDefinition> GetDefinitions();

    /// <summary>
    /// Retrieve a stored definition by id.
    /// </summary>
    SecureFileDownloadDefinition GetDefinition(Guid id);

    /// <summary>
    /// Delete a stored definition by id.
    /// </summary>
    void DeleteDefinition(Guid id);

    /// <summary>
    /// Retrieve a stored definition by <see cref="SecureFileDownloadDefinition.UrlSegmentText"/>.
    /// </summary>
    SecureFileDownloadDefinition GetDefinitionByUrlSegmentText(string urlSegmentText);

    /// <summary>
    /// Create a new definition.
    /// </summary>
    SecureFileDownloadDefinition CreateDefinition(SecureFileDownloadDefinition definition);

    /// <summary>
    /// Edit an already stored definition.
    /// </summary>
    SecureFileDownloadDefinition UpdateDefinition(SecureFileDownloadDefinition definition);
}
