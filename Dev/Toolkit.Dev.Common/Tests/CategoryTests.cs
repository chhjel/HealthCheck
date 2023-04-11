using QoDL.Toolkit.Core.Modules.Tests.Attributes;
using QoDL.Toolkit.Core.Modules.Tests.Models;

namespace QoDL.Toolkit.Dev.Common.Tests
{
    [RuntimeTestClass(
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
        UIOrder = 700,
        DefaultAllowParallelExecution = true
    )]
    public class CategoryTests
    {
        [RuntimeTest]
        public TestResult TestWithoutAnyTags()
            => TestResult.CreateSuccess($"Ok");

        [RuntimeTest(Category = TestCategoryConstants.Public)]
        public TestResult TestWithPublicTag()
            => TestResult.CreateSuccess($"Ok");

        [RuntimeTest(Categories = new[] { TestCategoryConstants.Public, TestCategoryConstants.API_A })]
        public TestResult TestWithPublicAndApiATags()
            => TestResult.CreateSuccess($"Ok");

        [RuntimeTest(Categories = new[] { TestCategoryConstants.Internal, TestCategoryConstants.API_A, TestCategoryConstants.API_B })]
        public TestResult TestWithInternalApiAAndApiBTags()
            => TestResult.CreateSuccess($"Ok");

        public static class TestCategoryConstants
        {
            public const string Common = "Common";
            public const string Internal = "Internal";
            public const string API_A = "API_A";
            public const string API_B = "API_B";
            public const string Public = "Public";
        }
    }
}
