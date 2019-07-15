using HealthCheck.Core.Attributes;
using HealthCheck.Core.Entities;
using HealthCheck.Core.Exceptions;
using HealthCheck.Core.Tests.Helpers;
using System;
using System.Linq;
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

            [RuntimeTest(Name = "TestMethodA")]
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
    }
}
