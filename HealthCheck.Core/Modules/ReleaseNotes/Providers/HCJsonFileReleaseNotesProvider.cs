﻿using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.ReleaseNotes.Abstractions;
using HealthCheck.Core.Modules.ReleaseNotes.Models;
using HealthCheck.Core.Modules.Tests.Services;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.ReleaseNotes.Providers
{
    /// <summary>
    /// Provides data from a json file path.
    /// <para>Caches data statically.</para>
    /// </summary>
    public class HCJsonFileReleaseNotesProvider : IHCReleaseNotesProvider
    {
        /// <summary>
        /// Path to the json file containing the releasenotes.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// If set, changes with issue ids will get a link generated by the given function.
        /// <para>Input is issue id.</para>
        /// </summary>
        public Func<string, string> IssueUrlFactory { get; set; }

        /// <summary>
        /// If set, issue links will use a title generated by the given function.
        /// <para>Input is issue id.</para>
        /// </summary>
        public Func<string, string> IssueLinkTitleFactory { get; set; }

        /// <summary>
        /// If set, issue links will use this icon.
        /// <para>Value should be a constant from <see cref="MaterialIcons"/>.</para>
        /// </summary>
        public string IssueLinkIcon { get; set; }

        /// <summary>
        /// If set, changes with a pull-request number will get a link generated by the given function.
        /// <para>Input is pull-request number.</para>
        /// </summary>
        public Func<string, string> PullRequestUrlFactory { get; set; }

        /// <summary>
        /// If set, pull-request links will use a title generated by the given function.
        /// <para>Input is pull-request number.</para>
        /// </summary>
        public Func<string, string> PullRequestLinkTitleFactory { get; set; }

        /// <summary>
        /// If set, pull-request links will use this icon.
        /// <para>Value should be a constant from <see cref="MaterialIcons"/>.</para>
        /// </summary>
        public string PullRequestLinkIcon { get; set; }

        /// <summary>
        /// If set, the default title will be "Latest release notes" for production and for non-prod "Latest changes"
        /// </summary>
        public Func<bool> IsProduction { get; set; }

        /// <summary>
        /// Use to set title based on e.g. environment.
        /// </summary>
        public Func<string> Title { get; set; }

        private HCReleaseNotesViewModels _cachedModel;

        /// <summary>
        /// Provides data from a json file path.
        /// </summary>
        public HCJsonFileReleaseNotesProvider(string filePath)
        {
            FilePath = filePath;
        }

        /// <inheritdoc />
        public virtual Task<HCReleaseNotesViewModels> GetViewModelAsync()
        {
            if (_cachedModel != null)
            {
                return Task.FromResult(_cachedModel);
            }
            return Task.FromResult(CreateViewModel());
        }

        private HCReleaseNotesViewModels CreateViewModel()
        {
            if (string.IsNullOrWhiteSpace(FilePath) || !File.Exists(FilePath))
            {
                return CreateFileNotFoundResult();
            }

            try
            {
                var json = IOUtils.ReadFile(FilePath);
                var model = BuildViewModel(json);
                _cachedModel = model;
                return _cachedModel;
            }
            catch (Exception ex)
            {
                return HCReleaseNotesViewModels.CreateError($"Failed to read release notes.", $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create model when no json file was found.
        /// </summary>
        protected virtual HCReleaseNotesViewModels CreateFileNotFoundResult()
            => HCReleaseNotesViewModels.CreateError("No release notes data was found.");

        /// <summary>
        /// Parses the given json to a view model.
        /// </summary>
        protected virtual HCReleaseNotesViewModels BuildViewModel(string json)
        {
            try
            {
                var fileModel = DeserializeJsonData(json);
                if (fileModel == null)
                {
                    return HCReleaseNotesViewModels.CreateError($"Failed to deserialize release notes, model result was null.");
                }

                return new HCReleaseNotesViewModels
                {
                    WithDevDetails = BuildViewModel(fileModel, true),
                    WithoutDevDetails = BuildViewModel(fileModel, false)
                };
            }
            catch (Exception ex)
            {
                return HCReleaseNotesViewModels.CreateError($"Failed to deserialize release notes.", $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Build the view model from the given json file data.
        /// </summary>
        protected virtual HCReleaseNotesViewModel BuildViewModel(HCDefaultReleaseNotesJsonModel data, bool includeDevDetails)
        {
            var title = Title?.Invoke() ?? "Latest release notes";
            if (Title == null && IsProduction?.Invoke() != true)
            {
                title = "Latest changes";
            }

            var changes = data.changes
                    ?.Select(x => BuildChangeViewModel(x, includeDevDetails))
                    ?.Where(x => x != null)
                    ?.ToList()
                    ?? new List<HCReleaseNoteChangeViewModel>();

            changes = PostProcessChanges(changes, includeDevDetails);

            var model = new HCReleaseNotesViewModel
            {
                Version = data.version,
                //DeployedAt = DateTime.Now,
                BuiltAt = data.builtAt,

                Title = title,
                Description = "Auto-generated release notes from changes since the previous production deploy.",
                BuiltCommitHash = includeDevDetails ? data.builtCommitHash : null,
                Changes = changes
            };

            return model;
        }

        /// <summary>
        /// Removes duplicates etc when no dev details are shown.
        /// </summary>
        protected virtual List<HCReleaseNoteChangeViewModel> PostProcessChanges(List<HCReleaseNoteChangeViewModel> changes, bool includeDevDetails)
        {
            if (!includeDevDetails)
            {
                changes = changes
                    .GroupBy(x => x.Title).Select(x => x.First())
                    .ToList();
            }
            return changes;
        }

        /// <summary>
        /// Build a change view model from the given json file data.
        /// </summary>
        protected virtual HCReleaseNoteChangeViewModel BuildChangeViewModel(HCDefaultReleaseNotesChangeJsonModel data, bool includeDevDetails)
        {
            if (string.IsNullOrWhiteSpace(data.issueId) && !includeDevDetails)
            {
                return null;
            }

            List<HCReleaseNoteLinkViewModel> links = new();
            var hasIssueLink = false;
            var hasPrLink = false;

            if (data.issueIds?.Any() == true && IssueUrlFactory != null)
            {
                hasIssueLink = true;
                foreach (var issueId in data.issueIds)
                {
                    links.Add(new HCReleaseNoteLinkViewModel
                    {
                        Title = IssueLinkTitleFactory?.Invoke(issueId) ?? $"Issue {issueId}",
                        Url = IssueUrlFactory.Invoke(issueId)
                    });
                }
            }

            if (includeDevDetails && !string.IsNullOrWhiteSpace(data.pullRequestNumber) && PullRequestUrlFactory != null)
            {
                hasPrLink = true;
                links.Add(new HCReleaseNoteLinkViewModel
                {
                    Title = PullRequestLinkTitleFactory?.Invoke(data.pullRequestNumber) ?? $"Pull request #{data.pullRequestNumber}",
                    Url = PullRequestUrlFactory.Invoke(data.pullRequestNumber)
                });
            }

            var descriptionBuilder = new StringBuilder();
            if (includeDevDetails && !string.IsNullOrWhiteSpace(data.body))
            {
                descriptionBuilder.Append(data.body?.Trim());
            }
            if (includeDevDetails && !string.IsNullOrWhiteSpace(data.pullRequestNumber) && PullRequestUrlFactory == null)
            {
                descriptionBuilder.Append($"\n\nPull-request #{data.pullRequestNumber}");
            }
            if (data.issueIds?.Count() > 1 && IssueUrlFactory == null)
            {
                descriptionBuilder.Append($"\n\nAlso related to:");
                foreach (var issueId in data.issueIds.Skip(1))
                {
                    descriptionBuilder.Append($"\n-{issueId}");
                }
            }

            string title = null;
            if (includeDevDetails)
            {
                title = $"{data.issueId} {data.cleanMessage?.CapitalizeFirst()}".Trim();
            }
            else if (!string.IsNullOrWhiteSpace(data.issueId))
            {
                title = data.issueId;
            }

            return new HCReleaseNoteChangeViewModel
            {
                CommitHash = includeDevDetails ? data.hash : null,
                AuthorName = includeDevDetails ? data.authorName : null,
                Timestamp = data.timestamp,

                Title = title,
                Description = descriptionBuilder.ToString(),

                Links = links,
                MainLink = links.FirstOrDefault()?.Url,

                HasIssueLink = hasIssueLink,
                HasPullRequestLink = hasPrLink
            };
        }

        /// <summary>
        /// Deserializes the json into the default model.
        /// </summary>
        protected virtual HCDefaultReleaseNotesJsonModel DeserializeJsonData(string json)
            => TestRunnerService.Serializer?.Deserialize<HCDefaultReleaseNotesJsonModel>(json);
    }
}
