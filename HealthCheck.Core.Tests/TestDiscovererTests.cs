using HealthCheck.Core.Exceptions;
using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Core.Modules.Tests.Services;
using HealthCheck.Core.Tests.Helpers;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HealthCheck.Core.Services
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
            var discoverer = new TestDiscoveryService()
            {
                AssemblyContainingTests = GetType().Assembly
            };
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
            var discoverer = new TestDiscoveryService()
            {
                AssemblyContainingTests = GetType().Assembly
            };
            var tests = discoverer.DiscoverTestDefinitions().SelectMany(x => x.Tests);
            var test = tests.Single(x => x.Name == nameof(TestClass.TestMethodA));
            Assert.Single(test.Categories);
            Assert.Equal("TestMethodACategory", test.Categories[0]);
        }

        [Fact]
        public void DiscoverTestDefinitions_WithCategorisedClassAndTest_ResolvesCustomCategories()
        {
            var discoverer = new TestDiscoveryService()
            {
                AssemblyContainingTests = GetType().Assembly
            };
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
            var discoverer = new TestDiscoveryService()
            {
                AssemblyContainingTests = GetType().Assembly
            };
            var tests = discoverer.DiscoverTestDefinitions().SelectMany(x => x.Tests);
            var test = tests.Single(x => x.Name == "TestMethodWithCustomNames");
            Assert.Equal("First desc.", test.Parameters[0].Description);
            Assert.Equal("Second desc.", test.Parameters[1].Description);
            Assert.Equal("Third desc.", test.Parameters[2].Description);
        }

        [Fact]
        public void DiscoverTestDefinitions_WithFilter_ReturnsOnlyWantedTests()
        {
            var discoverer = new TestDiscoveryService()
            {
                AssemblyContainingTests = GetType().Assembly
            };

            var userRoles = AccessRoles.WebAdmins;
            var tests = discoverer.DiscoverTestDefinitions(userRoles, testFilter: (test) => test.Name == nameof(TestClass.TestMethodA))
                .SelectMany(x => x.Tests);
            Assert.Single(tests);
            Assert.Single(tests, x => x.Name == nameof(TestClass.TestMethodA));
        }

        [Fact]
        public void DiscoverTestDefinitions_WithRoles_ReturnsOnlyTestsWithGivenRoles()
        {
            var discoverer = new TestDiscoveryService()
            {
                AssemblyContainingTests = GetType().Assembly
            };

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
            var discoverer = new TestDiscoveryService()
            {
                AssemblyContainingTests = GetType().Assembly
            };

            var userRoles = WrongEnumA.Something;
            Assert.Throws<InvalidAccessRolesDefinitionException>(() =>
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
            var discoverer = new TestDiscoveryService()
            {
                AssemblyContainingTests = GetType().Assembly
            };
            var tests = discoverer.DiscoverTestDefinitions().SelectMany(x => x.Tests);
            Assert.Contains(tests, x => x.Name == nameof(TestClass.TestMethodA));
            Assert.Contains(tests, x => x.Name == nameof(TestClass.TestMethodB));
        }

        [Fact]
        public void DiscoverTestDefinitions_WithTwoTestsAndANonTest__DoesNotContainNonTests()
        {
            var discoverer = new TestDiscoveryService()
            {
                AssemblyContainingTests = GetType().Assembly
            };
            var tests = discoverer.DiscoverTestDefinitions().SelectMany(x => x.Tests);
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.NotATestMethod));
        }

        [Fact]
        public void DiscoverTestDefinitions_WithCancellableTests__DoesNotIncludeParameterDefinitionForCancellationToken()
        {
            var discoverer = new TestDiscoveryService()
            {
                AssemblyContainingTests = GetType().Assembly
            };
            var tests = discoverer.DiscoverTestDefinitions().SelectMany(x => x.Tests);
            var test = tests.FirstOrDefault(x => x.Categories.Contains("TestSetId3Category"));
            Assert.NotNull(test);
            Assert.True(test.IsCancellable);
            Assert.DoesNotContain(test.Parameters, x => x.ParameterType == typeof(CancellationToken));
        }

        [Fact]
        public void GetInvalidTests_WithInvalidTests_ReturnsInvalidTests()
        {
            var discoverer = new TestDiscoveryService()
            {
                AssemblyContainingTests = GetType().Assembly
            };

            var invalidTests = discoverer.GetInvalidTests();
            Assert.Contains(invalidTests, x => x.Test.Name == nameof(TestClass.InvalidMethodA));
            Assert.Contains(invalidTests, x => x.Test.Name == nameof(TestClass.InvalidMethodB));
            Assert.Contains(invalidTests, x => x.Test.Name == nameof(TestClass.InvalidMethodC));

            Output.WriteLine(string.Join("\n", invalidTests.Select(x => x.Error)));
        }

        [Fact]
        public void GetInvalidTests_WithInvalidTests_ThrowsException()
        {
            var discoverer = new TestDiscoveryService()
            {
                AssemblyContainingTests = GetType().Assembly
            };

            Assert.Throws<InvalidTestDefinitionException>(() => discoverer.ValidateTests());
        }

        [RuntimeTestClass(Id = "TestSetId", Description = "Some test set", Name = "Dev test set")]
        public class TestClass
        {
            public TestClass()
            {
            }

            [RuntimeTest(Name = "TestMethodForSysAdmins", RolesWithAccess = AccessRoles.SystemAdmins)]
            public TestResult TestMethodForSysAdmins()
            {
                return TestResult.CreateSuccess($"Success!");
            }

            [RuntimeTest(Name = "TestMethodA", Category = "TestMethodACategory")]
            public TestResult TestMethodA(string stringArg = "wut", bool boolArg = true, int intArg = 123)
            {
                return TestResult.CreateSuccess($"Success! [{stringArg}, {boolArg}, {intArg}]");
            }

            [RuntimeTest(Name = "TestMethodB")]
            public TestResult TestMethodB()
            {
                return new TestResult();
            }

            [RuntimeTest(Name = "InvalidMethodA")]
            public bool InvalidMethodA() => true;

            [RuntimeTest(Name = "InvalidMethodB")]
            public void InvalidMethodB() { }

            [RuntimeTest(Name = "InvalidMethodC")]
            [RuntimeTestParameter("Name", "Description")]
            [RuntimeTestParameter("c", "Name", "Description")]
            public TestResult InvalidMethodC(string a, string b) => TestResult.CreateSuccess($"{a}, {b}");

            public TestResult NotATestMethod() => new TestResult();

            [RuntimeTest("TestMethodWithCustomNames")]
            [RuntimeTestParameter("stringArg", "First name", "First desc")]
            public TestResult TestMethodWithCustomNames(
                string stringArg = "wut",
                [RuntimeTestParameter("Second name", "Second desc")] bool boolArg = true,
                [RuntimeTestParameter("Third name", "Third desc")] int intArg = 123)
            {
                return TestResult.CreateSuccess($"Success! [{stringArg}, {boolArg}, {intArg}]");
            }
        }

        [RuntimeTestClass(Id = "TestSetId2", Description = "Some test set #2", Name = "Dev test set #2", DefaultCategory = "TestSetId2Category")]
        public class TestClass2
        {
            [RuntimeTest(Name = "TestMethodA2")]
            public async Task<TestResult> TestMethodA2(string stringArg = "wut", bool boolArg = true, int intArg = 123)
            {
                await Task.Delay(1);
                return TestResult.CreateSuccess($"Success! [{stringArg}, {boolArg}, {intArg}]");
            }

            [RuntimeTest(Name = "TestMethodB2", Category = "TestMethodB2Category")]
            public async Task<TestResult> TestMethodB2()
            {
                await Task.Delay(1);
                return TestResult.CreateSuccess($"Success!");
            }
        }

        [RuntimeTestClass(Id = "TestSetId3", Description = "Some test set #3", Name = "Dev test set #3", DefaultCategory = "TestSetId3Category")]
        public class TestClass3
        {
            [RuntimeTest(name: "TestMethodA3")]
            public async Task<TestResult> CancellableTest1(CancellationToken cancellationToken, string param1)
            {
                await Task.Delay(TimeSpan.FromSeconds(0.1), cancellationToken);
                return TestResult.CreateSuccess("Completed! " + param1);
            }
        }
    }
}
