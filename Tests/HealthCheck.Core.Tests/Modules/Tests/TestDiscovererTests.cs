using HealthCheck.Core.Exceptions;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Core.Modules.Tests.Services;
using HealthCheck.Core.Tests.Helpers;
using HealthCheck.Core.Tests.Modules.Tests.TestClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace HealthCheck.Core.Tests.Modules.Tests
{
    public class TestDiscovererTests
    {
        public ITestOutputHelper Output { get; }

        public TestDiscovererTests(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void DiscoverTestDefinitions_WithCustomParameterNames_ResolvesCustomParameterNames()
        {
            var discoverer = CreateTestDiscoveryService();
            var tests = discoverer.DiscoverTestDefinitions().SelectMany(x => x.Tests);
            var test = tests.Single(x => x.Name == "TestMethodWithCustomNames");
            Assert.Equal("First name", test.Parameters[0].Name);
            Assert.Equal("Second name", test.Parameters[1].Name);
            Assert.Equal("Third name", test.Parameters[2].Name);
        }
        //
        [Fact]
        public void DiscoverTestDefinitions_WithCategorisedTest_ResolvesCustomCategory()
        {
            var discoverer = CreateTestDiscoveryService();
            var tests = discoverer.DiscoverTestDefinitions().SelectMany(x => x.Tests);
            var test = tests.Single(x => x.Name == nameof(TestClass.TestMethodA));
            Assert.Single(test.Categories);
            Assert.Equal("TestMethodACategory", test.Categories[0]);
        }

        [Fact]
        public void DiscoverTestDefinitions_WithCategorisedClassAndTest_ResolvesCustomCategories()
        {
            var discoverer = CreateTestDiscoveryService();
            var tests = discoverer.DiscoverTestDefinitions().SelectMany(x => x.Tests);

            var testA2 = tests.Single(x => x.Name == nameof(TestClass2.TestMethodA2));
            Assert.Single(testA2.Categories);
            Assert.Equal("TestSetId2Category", testA2.Categories[0]);

            var testB2 = tests.Single(x => x.Name == nameof(TestClass2.TestMethodB2));
            Assert.Single(testB2.Categories);
            Assert.Contains("TestMethodB2Category", testB2.Categories);
        }

        [Fact]
        public void DiscoverTestDefinitions_WithCustomParameterDescriptions_ResolvesCustomParameterDescriptions()
        {
            var discoverer = CreateTestDiscoveryService();
            var tests = discoverer.DiscoverTestDefinitions().SelectMany(x => x.Tests);
            var test = tests.Single(x => x.Name == "TestMethodWithCustomNames");
            Assert.Equal("First desc.", test.Parameters[0].Description);
            Assert.Equal("Second desc.", test.Parameters[1].Description);
            Assert.Equal("Third desc.", test.Parameters[2].Description);
        }

        [Fact]
        public void DiscoverTestDefinitions_WithDefaultRoleThatUserHas_HasAccess()
        {
            var discoverer = CreateTestDiscoveryService();
            var userRoles = AccessRoles.WebAdmins;
            var tests = discoverer.DiscoverTestDefinitions(userRoles,
                testFilter: (test) => test.Name == nameof(TestClass.TestMethodA),
                defaultTestAccessLevel: AccessRoles.WebAdmins)
                .SelectMany(x => x.Tests);
            Assert.Single(tests);
            Assert.Single(tests, x => x.Name == nameof(TestClass.TestMethodA));
        }

        [Fact]
        public void DiscoverTestDefinitions_WithDefaultRoleThatUserHasNot_NoAccess()
        {
            var discoverer = CreateTestDiscoveryService();
            var userRoles = AccessRoles.WebAdmins;
            var tests = discoverer.DiscoverTestDefinitions(userRoles,
                testFilter: (test) => test.Name == nameof(TestClass.TestMethodA),
                defaultTestAccessLevel: AccessRoles.SystemAdmins)
                .SelectMany(x => x.Tests);
            Assert.Empty(tests);
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.TestMethodA));
        }

        [Fact]
        public void DiscoverTestDefinitions_WithFilter_ReturnsOnlyWantedTests()
        {
            var discoverer = CreateTestDiscoveryService();
            var userRoles = AccessRoles.WebAdmins;
            var tests = discoverer.DiscoverTestDefinitions(userRoles, testFilter: (test) => test.Name == nameof(TestClass.TestMethodA))
                .SelectMany(x => x.Tests);
            Assert.Single(tests);
            Assert.Single(tests, x => x.Name == nameof(TestClass.TestMethodA));
        }

        [Fact]
        public void DiscoverTestDefinitions_WithRoles_ReturnsOnlyTestsWithGivenRoles()
        {
            var discoverer = CreateTestDiscoveryService();
            var userRoles = AccessRoles.WebAdmins;
            var tests = discoverer.DiscoverTestDefinitions(userRoles)
                .SelectMany(x => x.Tests);
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.TestMethodForSysAdmins));
            Assert.Contains(tests, x => x.Name == nameof(TestClass.TestMethodA));
            Assert.Contains(tests, x => x.Name == nameof(TestClass.TestMethodB));
        }

        [Fact]
        public void DiscoverTestDefinitions_WithDifferentRolesObject_GivesException()
        {
            var discoverer = CreateTestDiscoveryService();
            var userRoles = WrongEnumA.Something;
            Assert.Throws<HCInvalidAccessRolesDefinitionException>(() =>
            {
                try
                {
                    discoverer.DiscoverTestDefinitions(userRoles);
                }
                catch (Exception ex)
                {
                    Output.WriteLine(ex.Message);
                    throw;
                }
            });
        }
        private enum WrongEnumA { Something = 1, Etc = 2 }

        [Fact]
        public void DiscoverTestDefinitions_WithTwoTests_FindsTests()
        {
            var discoverer = CreateTestDiscoveryService();
            var tests = discoverer.DiscoverTestDefinitions().SelectMany(x => x.Tests);
            Assert.Contains(tests, x => x.Name == nameof(TestClass.TestMethodA));
            Assert.Contains(tests, x => x.Name == nameof(TestClass.TestMethodB));
        }

        [Fact]
        public void DiscoverTestDefinitions_WithTwoTestsAndANonTest__DoesNotContainNonTests()
        {
            var discoverer = CreateTestDiscoveryService();
            var tests = discoverer.DiscoverTestDefinitions().SelectMany(x => x.Tests);
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.NotATestMethod));
        }

        [Fact]
        public void DiscoverTestDefinitions_WithCancellableTests__DoesNotIncludeParameterDefinitionForCancellationToken()
        {
            var discoverer = CreateTestDiscoveryService();
            var tests = discoverer.DiscoverTestDefinitions().SelectMany(x => x.Tests);
            var test = tests.FirstOrDefault(x => x.Categories.Contains("TestSetId3Category"));
            Assert.NotNull(test);
            Assert.True(test.IsCancellable);
            Assert.DoesNotContain(test.Parameters, x => x.ParameterType == typeof(CancellationToken));
        }

        [Fact]
        public void GetInvalidTests_WithInvalidTests_ReturnsInvalidTests()
        {
            var discoverer = CreateTestDiscoveryService();
            var invalidTests = discoverer.GetInvalidTests();
            Assert.Contains(invalidTests, x => x.Test.Name == nameof(TestClass.InvalidMethodA));
            Assert.Contains(invalidTests, x => x.Test.Name == nameof(TestClass.InvalidMethodB));
            Assert.Contains(invalidTests, x => x.Test.Name == nameof(TestClass.InvalidMethodC));

            Output.WriteLine(string.Join("\n", invalidTests.Select(x => x.Error)));
        }

        [Fact]
        public void GetInvalidTests_WithInvalidTests_ThrowsException()
        {
            var discoverer = CreateTestDiscoveryService();
            Assert.Throws<InvalidTestDefinitionException>(() => discoverer.ValidateTests());
        }

        [Fact]
        public void DiscoverTestDefinitions_ProxyTestWithoutRoles()
        {
            var discoverer = CreateTestDiscoveryService();
            var userRoles = AccessRoles.None;
            var tests = discoverer.DiscoverTestDefinitions(userRoles)
                .SelectMany(x => x.Tests)
                .Where(x => x.ParentClass.Name.StartsWith("Proxy Test Class With"));
            Assert.Single(tests, x => x.ParentClass.Name == "Proxy Test Class Without Access");
        }

        [Fact]
        public void DiscoverTestDefinitions_ProxyTestWithRole1()
        {
            var discoverer = CreateTestDiscoveryService();
            var userRoles = AccessRoles.WebAdmins;
            var tests = discoverer.DiscoverTestDefinitions(userRoles)
                .SelectMany(x => x.Tests)
                .Where(x => x.ParentClass.Name.StartsWith("Proxy Test Class With"));
            Assert.Equal(3, tests.Count());
        }

        [Fact]
        public void DiscoverTestDefinitions_ProxyTestWithRole2()
        {
            var discoverer = CreateTestDiscoveryService();
            var userRoles = AccessRoles.SystemAdmins | AccessRoles.WebAdmins;
            var tests = discoverer.DiscoverTestDefinitions(userRoles)
                .SelectMany(x => x.Tests)
                .Where(x => x.ParentClass.Name.StartsWith("Proxy Test Class With"));
            Assert.Equal(4, tests.Count());
        }

        [Fact]
        public void DiscoverTestDefinitions_ProxyTestWithRole3()
        {
            var discoverer = CreateTestDiscoveryService();
            var userRoles = AccessRoles.SystemAdmins;
            var tests = discoverer.DiscoverTestDefinitions(userRoles)
                .SelectMany(x => x.Tests)
                .Where(x => x.ParentClass.Name.StartsWith("Proxy Test Class With"));
            Assert.Equal(2, tests.Count());
        }

        [Fact]
        public void DiscoverTestDefinitions_WithRolesAndCategoriesAccess_ReturnsOnlyAllowedCategorizedTests()
        {
            var discoverer = CreateTestDiscoveryService();
            var userRoles = AccessRoles.SystemAdmins;
            var categories = new List<string> { "XYZ" };
            var tests = discoverer.DiscoverTestDefinitions(userRoles, userCategoryAccess: categories)
                .SelectMany(x => x.Tests);
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.TestMethodForSysAdmins));
            Assert.Contains(tests, x => x.Name == nameof(TestClass.TestMethodForCategoryXYZ));
            Assert.Contains(tests, x => x.Name == nameof(TestClass.TestMethodForCategoryXYZAndASysdmin));
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.TestMethodForCategoryXYZAndWebAdmin));
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.TestMethodA));
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.TestMethodB));
        }

        [Fact]
        public void DiscoverTestDefinitions_WithRolesAndEmptyCategoriesAccess_ReturnsOnlyAllowedTests()
        {
            var discoverer = CreateTestDiscoveryService();
            var userRoles = AccessRoles.SystemAdmins;
            var categories = new List<string>();
            var tests = discoverer.DiscoverTestDefinitions(userRoles, userCategoryAccess: categories)
                .SelectMany(x => x.Tests);
            Assert.Contains(tests, x => x.Name == nameof(TestClass.TestMethodForSysAdmins));
            Assert.Contains(tests, x => x.Name == nameof(TestClass.TestMethodForCategoryXYZ));
            Assert.Contains(tests, x => x.Name == nameof(TestClass.TestMethodForCategoryXYZAndASysdmin));
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.TestMethodForCategoryXYZAndWebAdmin));
            Assert.Contains(tests, x => x.Name == nameof(TestClass.TestMethodA));
            Assert.Contains(tests, x => x.Name == nameof(TestClass.TestMethodB));
        }

        [Fact]
        public void DiscoverTestDefinitions_WithRolesAndNullCategoriesAccess_ReturnsOnlyAllowedTests()
        {
            var discoverer = CreateTestDiscoveryService();
            var userRoles = AccessRoles.SystemAdmins;
            List<string> categories = null;
            var tests = discoverer.DiscoverTestDefinitions(userRoles, userCategoryAccess: categories)
                .SelectMany(x => x.Tests);
            Assert.Contains(tests, x => x.Name == nameof(TestClass.TestMethodForSysAdmins));
            Assert.Contains(tests, x => x.Name == nameof(TestClass.TestMethodForCategoryXYZ));
            Assert.Contains(tests, x => x.Name == nameof(TestClass.TestMethodForCategoryXYZAndASysdmin));
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.TestMethodForCategoryXYZAndWebAdmin));
            Assert.Contains(tests, x => x.Name == nameof(TestClass.TestMethodA));
            Assert.Contains(tests, x => x.Name == nameof(TestClass.TestMethodB));
        }

        [Fact]
        public void DiscoverTestDefinitions_WithoutRolesAndHasCategoryAAccess_ReturnsOnlyAllowedTests()
        {
            var discoverer = CreateTestDiscoveryService();
            var userRoles = AccessRoles.None;
            var categories = new List<string> { "XYZ" };
            var tests = discoverer.DiscoverTestDefinitions(userRoles, userCategoryAccess: categories)
                .SelectMany(x => x.Tests);
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.TestMethodForSysAdmins));
            Assert.Contains(tests, x => x.Name == nameof(TestClass.TestMethodForCategoryXYZ));
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.TestMethodForCategoryXYZAndASysdmin));
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.TestMethodForCategoryXYZAndWebAdmin));
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.TestMethodA));
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.TestMethodB));
        }

        [Fact]
        public void DiscoverTestDefinitions_WithoutRolesAndHasCategoryBAccess_ReturnsOnlyAllowedTests()
        {
            var discoverer = CreateTestDiscoveryService();
            var userRoles = AccessRoles.None;
            var categories = new List<string> { "TestMethodACategory" };
            var tests = discoverer.DiscoverTestDefinitions(userRoles, userCategoryAccess: categories)
                .SelectMany(x => x.Tests);
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.TestMethodForSysAdmins));
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.TestMethodForCategoryXYZ));
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.TestMethodForCategoryXYZAndASysdmin));
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.TestMethodForCategoryXYZAndWebAdmin));
            Assert.Contains(tests, x => x.Name == nameof(TestClass.TestMethodA));
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.TestMethodB));
        }

        [Fact]
        public void DiscoverTestDefinitions_WithoutRolesAndHasMultipleCategoriesAccess_ReturnsOnlyAllowedTests()
        {
            var discoverer = CreateTestDiscoveryService();
            var userRoles = AccessRoles.None;
            var categories = new List<string> { "TestMethodACategory", "XYZ" };
            var tests = discoverer.DiscoverTestDefinitions(userRoles, userCategoryAccess: categories)
                .SelectMany(x => x.Tests);
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.TestMethodForSysAdmins));
            Assert.Contains(tests, x => x.Name == nameof(TestClass.TestMethodForCategoryXYZ));
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.TestMethodForCategoryXYZAndASysdmin));
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.TestMethodForCategoryXYZAndWebAdmin));
            Assert.Contains(tests, x => x.Name == nameof(TestClass.TestMethodA));
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.TestMethodB));
        }

        private TestDiscoveryService CreateTestDiscoveryService()
        {
            return new TestDiscoveryService()
            {
                AssembliesContainingTests = new[] { GetType().Assembly }
            };
        }
    }
}
