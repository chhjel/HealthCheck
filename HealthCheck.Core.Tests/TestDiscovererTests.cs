using HealthCheck.Core.Attributes;
using HealthCheck.Core.Entities;
using HealthCheck.Core.Exceptions;
using System.Linq;
using Xunit;

namespace HealthCheck.Core.TestManagers
{
    public class TestDiscovererTests
    {
        [Fact]
        public void DiscoverTestDefinitions_WithTwoTests_FindsTests()
        {
            var discoverer = new TestDiscoverer()
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
            var discoverer = new TestDiscoverer()
            {
                AssemblyContainingTests = GetType().Assembly
            };
            var tests = discoverer.DiscoverTestDefinitions().SelectMany(x => x.Tests);
            Assert.DoesNotContain(tests, x => x.Name == nameof(TestClass.NotATestMethod));
        }

        [Fact]
        public void GetInvalidTests_WithInvalidTests_ReturnsInvalidTests()
        {
            var discoverer = new TestDiscoverer()
            {
                AssemblyContainingTests = GetType().Assembly
            };

            var invalidTests = discoverer.GetInvalidTests();
            Assert.Contains(invalidTests, x => x.Test.Name == nameof(TestClass.InvalidMethodA));
            Assert.Contains(invalidTests, x => x.Test.Name == nameof(TestClass.InvalidMethodB));
            Assert.Contains(invalidTests, x => x.Test.Name == nameof(TestClass.InvalidMethodC));
        }

        [Fact]
        public void GetInvalidTests_WithInvalideTests_ThrowsException()
        {
            var discoverer = new TestDiscoverer()
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

            [RuntimeTest(ParameterDescriptions = new[] { "a desc", "b", "c" })]
            public TestResult TestMethodA(string stringArg = "wut", bool boolArg = true, int intArg = 123)
            {
                return TestResult.CreateSuccess($"Success! [{stringArg}, {boolArg}, {intArg}]");
            }

            [RuntimeTest]
            public TestResult TestMethodB()
            {
                return new TestResult();
            }

            [RuntimeTest]
            public bool InvalidMethodA() => true;

            [RuntimeTest]
            public void InvalidMethodB() { }

            [RuntimeTest]
            public TestResult InvalidMethodC(string a, string b) => TestResult.CreateSuccess($"{a}, {b}");

            public TestResult NotATestMethod() => new TestResult();
        }
    }
}
