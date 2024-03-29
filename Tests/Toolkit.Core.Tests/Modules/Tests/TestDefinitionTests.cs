using QoDL.Toolkit.Core.Modules.Tests.Attributes;
using QoDL.Toolkit.Core.Modules.Tests.Models;
using QoDL.Toolkit.Core.Modules.Tests.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace QoDL.Toolkit.Core.Tests.Modules.Tests;

public class TestDefinitionTests
{
    public ITestOutputHelper Output { get; }

    public TestDefinitionTests(ITestOutputHelper output)
    {
        Output = output;
    }

    [Fact]
    public void Id_OfAllTests_AreUnique()
    {
        var discoverer = CreateTestDiscoveryService();
        var tests = discoverer.DiscoverTestDefinitions().SelectMany(x => x.Tests);

        foreach (var test in tests)
        {
            Output.WriteLine(test.Id);
        }
        Assert.True(!tests.GroupBy(x => x.Id).Any(x => x.Count() > 1), "Duplicate ids detected");
    }

    [Fact]
    public void Id_OfValidTest_IsGenerated()
    {
        var discoverer = CreateTestDiscoveryService();
        var tests = discoverer.DiscoverTestDefinitions().SelectMany(x => x.Tests);
        var test = tests.First(x => x.Validate().IsValid);
        Assert.NotNull(test.Id);
        Assert.NotEqual(0, test.Id.Length);
    }

    [Fact]
    public void Id_OfInvalidTest_IsGenerated()
    {
        var discoverer = CreateTestDiscoveryService();
        var tests = discoverer.DiscoverTestDefinitions(includeInvalidTests: true).SelectMany(x => x.Tests);
        var test = tests.First(x => !x.Validate().IsValid);
        Assert.NotNull(test.Id);
        Assert.NotEqual(0, test.Id.Length);
    }

    [Fact]
    public async Task ExecuteTest_WithoutMethodParametersAndNullParam_Works()
    {
        var discoverer = CreateTestDiscoveryService();
        var tests = discoverer.DiscoverTestDefinitions().SelectMany(x => x.Tests);
        var test = tests.First(x => x.Name == nameof(TestClass.TestMethodWithoutParameters));

        var instance = Activator.CreateInstance(test.ParentClass.ClassType);
        await test.ExecuteTest(instance, null);
        Assert.True(true);
    }

    [Fact]
    public async Task ExecuteTest_WithMethodParametersAndNullParam_UsesDefaultParams()
    {
        var discoverer = CreateTestDiscoveryService();
        var tests = discoverer.DiscoverTestDefinitions().SelectMany(x => x.Tests);
        var test = tests.First(x => x.Name == nameof(TestClass.TestMethodWithParameters));

        var instance = Activator.CreateInstance(test.ParentClass.ClassType);
        var result = await test.ExecuteTest(instance, null);
        var tagArray = (object[])result.Tag;
        Assert.Equal("wut", tagArray[0]);
        Assert.Equal(true, tagArray[1]);
        Assert.Equal(123, tagArray[2]);
    }

    private TestDiscoveryService CreateTestDiscoveryService()
    {
        return new TestDiscoveryService()
        {
            AssembliesContainingTests = new[] { GetType().Assembly }
        };
    }

    [RuntimeTestClass(Id = "TestSetId", Description = "Some test set", Name = "Dev test set")]
    public class TestClass
    {
        [RuntimeTest(Name = "TestMethodWithoutParameters")]
        public TestResult TestMethodWithoutParameters()
        {
            return new TestResult();
        }

        [RuntimeTest(Name = "TestMethodWithParameters")]
        public TestResult TestMethodWithParameters(string stringArg = "wut", bool boolArg = true, int intArg = 123)
        {
            return new TestResult()
            {
                Tag = new object[] { stringArg, boolArg, intArg }
            };
        }

        [RuntimeTest]
        public void InvalidTestMethod()
        {
            // Method intentionally left empty.
        }
    }
}
