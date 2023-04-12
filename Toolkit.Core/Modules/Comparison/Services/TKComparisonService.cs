using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Modules.Comparison.Abstractions;
using QoDL.Toolkit.Core.Modules.Comparison.Models;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace QoDL.Toolkit.Core.Modules.Comparison.Services;

/// <summary>
/// Handles comparison of data.
/// </summary>
public class TKComparisonService : ITKComparisonService
{
    private readonly IEnumerable<ITKComparisonTypeHandler> _typeHandlers;
    private readonly IEnumerable<ITKComparisonDiffer> _differs;

    /// <summary>
    /// Handles comparison of data.
    /// </summary>
    public TKComparisonService(IEnumerable<ITKComparisonTypeHandler> typeHandlers, IEnumerable<ITKComparisonDiffer> differs)
    {
        _typeHandlers = typeHandlers;
        _differs = differs;
    }

    /// <inheritdoc />
    public List<TKComparisonTypeDefinition> GetComparisonTypeDefinitions()
    {
        return _typeHandlers
            .Select(x => new TKComparisonTypeDefinition
            {
                Id = x.GetType().Name,
                Name = x.Name,
                Description = x.Description,
                FindInstanceDescription = x.FindInstanceDescription,
                FindInstanceSearchPlaceholder = x.FindInstanceSearchPlaceholder
            })
            .ToList();
    }

    /// <inheritdoc />
    public Dictionary<string, List<TKComparisonDifferDefinition>> GetDifferDefinitionsByHandlerId()
    {
        return _typeHandlers.ToDictionary(
            x => x.GetType().Name,
            x => GetCompatibleDiffersFor(x)
                .OrderByDescending(x => x.UIOrder)
                .Select(d => new TKComparisonDifferDefinition
                {
                    Id = d.GetType().Name,
                    Name = d.Name,
                    Description = d.Description,
                    EnabledByDefault = d.DefaultEnabledFor(x)
                }).ToList()
        );
    }

    /// <inheritdoc />
    public async Task<List<TKComparisonInstanceSelection>> GetFilteredOptionsAsync(string handlerId, TKComparisonTypeFilter filter)
    {
        var handler = GetHandlerById(handlerId);
        if (handler == null) return new List<TKComparisonInstanceSelection>();
        var results = await handler.GetFilteredOptionsAsync(filter);
        return results.ToList();
    }

    /// <inheritdoc />
    public async Task<TKComparisonMultiDifferOutput> ExecuteDiffAsync(string handlerId, string[] differIds, string leftId, string rightId)
    {
        var handler = GetHandlerById(handlerId);
        if (handler == null) return new TKComparisonMultiDifferOutput();
        var differs = GetCompatibleDiffersFor(handler)
            .Where(x => differIds.Contains(x.GetType().Name));
        if (!differs.Any()) return new TKComparisonMultiDifferOutput();

        var left = await handler.GetInstanceWithIdAsync(leftId);
        var right = await handler.GetInstanceWithIdAsync(rightId);
        var leftName = left == null ? "<null>" : handler.GetInstanceDisplayName(left);
        var rightName = right == null ? "<null>" : handler.GetInstanceDisplayName(right);

        var result = new TKComparisonMultiDifferOutput();
        foreach (var differ in differs)
        {
            try
            {
                var diffResult = await differ.CompareInstancesAsync(left, right, leftName, rightName);
                result.Data.Add(new TKComparisonMultiDifferSingleOutput
                {
                    DifferId = differ.GetType().Name,
                    Data = diffResult.Data
                });
            } catch(Exception ex)
            {
                result.Data.Add(new TKComparisonMultiDifferSingleOutput
                {
                    DifferId = differ.GetType().Name,
                    Data = new List<TKComparisonDifferOutputData>
                    {
                        new TKComparisonDifferOutputData
                        {
                            DataType = TKComparisonDiffOutputType.Html,
                            Title = "Exception during attempted diff",
                            Data = TKGlobalConfig.Serializer.Serialize(new {
                                Html = $"Failed to compare data using the diff implementation '{HttpUtility.HtmlEncode(differ.GetType().GetFriendlyTypeName())}' with the error:<br />" +
                                       $"<code>{HttpUtility.HtmlEncode(TKExceptionUtils.GetFullExceptionDetails(ex))}</code>"
                            })
                        }
                    }
                });
            }
        }
        return result;
    }

    private ITKComparisonTypeHandler GetHandlerById(string id)
        => _typeHandlers.FirstOrDefault(x => x.GetType().Name == id);

    private List<ITKComparisonDiffer> GetCompatibleDiffersFor(ITKComparisonTypeHandler handler)
        => _differs
            .Where(x => x.CanHandle(handler))
            .OrderByDescending(x => x.UIOrder)
            .ToList();

}
