using HealthCheck.Core.Modules.SecureFileDownload.Abstractions;
using HealthCheck.Core.Modules.SecureFileDownload.Models;
using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;
using System;

namespace HealthCheck.Dev.Common.Tests.Modules
{
    [RuntimeTestClass(
        Name = "Downloads",
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
        GroupName = RuntimeTestConstants.Group.Modules,
        UIOrder = 30
    )]
    public class DownloadsTests
    {
        private readonly ISecureFileDownloadDefinitionStorage _secureFileDownloadDefinitionStorage;

        public DownloadsTests(ISecureFileDownloadDefinitionStorage secureFileDownloadDefinitionStorage)
        {
            _secureFileDownloadDefinitionStorage = secureFileDownloadDefinitionStorage;
        }

        [RuntimeTest]
        public TestResult CreateDownloadFromDisk(int? downloadCountLimit = null, DateTimeOffset? expiresAt = null)
        {
            _secureFileDownloadDefinitionStorage.CreateDefinition(new SecureFileDownloadDefinition
            {
                CreatedAt = DateTimeOffset.Now,
                DownloadCountLimit = downloadCountLimit,
                ExpiresAt = expiresAt,
                FileId = "testA.jpg",
                FileName = "Test File A.jpg",
                Id = Guid.NewGuid(),
                StorageId = "files_test",
                UrlSegmentText = "test"
            });
            return TestResult.CreateSuccess("Created download.");
        }

        [RuntimeTest]
        public TestResult CreateDownloadFromUrl(int? downloadCountLimit = null, DateTimeOffset? expiresAt = null)
        {
            _secureFileDownloadDefinitionStorage.CreateDefinition(new SecureFileDownloadDefinition
            {
                CreatedAt = DateTimeOffset.Now,
                DownloadCountLimit = downloadCountLimit,
                ExpiresAt = expiresAt,
                FileId = "https://via.placeholder.com/500x400",
                FileName = "Test File B.jpg",
                Id = Guid.NewGuid(),
                StorageId = "urls_test",
                UrlSegmentText = "test_url"
            });
            return TestResult.CreateSuccess("Created download.");
        }

        public static TimeSpan TimingDefault() => TimeSpan.FromSeconds(1.2);
    }
}
