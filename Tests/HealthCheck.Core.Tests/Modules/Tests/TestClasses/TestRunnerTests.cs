using HealthCheck.Core.Modules.SiteEvents.Enums;
using HealthCheck.Core.Modules.SiteEvents.Models;
using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Core.Tests.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthCheck.Core.Tests.Modules.Tests.TestClasses
{
    public class TestClasses
    {
        [RuntimeTestClass(Id = "TestRunnerTestsSetA", Description = "Some test set", Name = "Dev test set")]
        public class TestClassA
        {
            [RuntimeTest]
            public TestResult TestMethodWithoutParameters()
            {
                return new TestResult();
            }

            [RuntimeTest()]
            public TestResult TestMethodWithParameters(string stringArg = "wut", bool boolArg = true, int intArg = 123)
            {
                return new TestResult()
                {
                    Tag = new object[] { stringArg, boolArg, intArg }
                };
            }
        }

        [RuntimeTestClass(Id = "TestRunnerTestsSetB", Description = "Some test set", Name = "Dev test set")]
        public class TestClassB
        {
            [RuntimeTest(Category = "CategoryB")]
            public async Task<TestResult> AsyncTestMethod()
            {
                await Task.Delay(100);
                return new TestResult() { Tag = "AsyncTestMethod" };
            }

            [RuntimeTest(Category = "CategoryA")]
            public Task<TestResult> TaskTestMethod()
            {
                var e = new SiteEvent(SiteEventSeverity.Error, "typeId", "EventA", "description");
                return Task.FromResult(new TestResult() { Tag = "TaskTestMethod" }
                    .SetSiteEvent(e));
            }
        }

        [RuntimeTestClass(Id = "TestRunnerTestsSetC", Description = "Some test set", Name = "Dev test set")]
        public class TestClassC
        {
            [RuntimeTest(Category = "CategoryC")]
            public TestResult TestWithDefaultValues(string text = "defaultValue")
            {
                return new TestResult() { Tag = text };
            }

            [RuntimeTest(Category = "CategoryD")]
            public TestResult TestWithoutDefaultValues(string text, DateTimeOffset date)
            {
                return new TestResult() { Tag = new object[] { text, date } };
            }
        }

        [RuntimeTestClass(Id = "TestRunnerTestsSetD", Description = "Some test set D", Name = "Dev test set D")]
        public class TestClassD
        {
            [RuntimeTest(Category = "DCategoryA")]
            public TestResult TestThatResolvesEvent()
            {
                return TestResult.CreateSuccess("Ok")
                    .SetSiteEvent(new SiteEvent("DCategoryA-eventIdA", "Resolved message!"));
            }
        }

        [RuntimeTestClass(Id = "TestRunnerTestsSetE", Description = "Some test set E", Name = "Dev test set E", DefaultCategory = "DCategoryE")]
        public class TestClassE
        {
            private const string EventTypeId = "DCategoryE-eventIdE";

            [RuntimeTest]
            public TestResult Success()
            {
                return TestResult.CreateResolvedSiteEvent("Ok", EventTypeId, "Resolved message!");
            }

            [RuntimeTest]
            public TestResult ErrorA()
            {
                return TestResult.CreateError("Opsie!")
                    .SetSiteEvent(new SiteEvent(SiteEventSeverity.Error, EventTypeId, "Oh no!", "Desc!"));
            }
        }


        [RuntimeTestClass(Id = "TestRunnerTestsSetG", Description = "Some test set G", Name = "Dev test set G", DefaultCategory = "DCategoryG")]
        public class TestClassG
        {
            private const string EventTypeId = "DCategoryG-eventIdG";

            [RuntimeTest]
            public TestResult Success()
            {
                return TestResult.CreateResolvedSiteEvent("Ok", EventTypeId, "Resolved message!");
            }

            [RuntimeTest]
            public TestResult WarningA()
            {
                return TestResult.CreateWarning("Opsie!")
                    .SetSiteEvent(new SiteEvent(SiteEventSeverity.Warning, EventTypeId, "Oh no!", "Desc!"));
            }
        }

        [RuntimeTestClass(Id = "TestRunnerTestsSetF", Description = "Some test set F", Name = "Dev test set F", DefaultCategory = "DCategoryF")]
        public class TestClassF
        {
            private const string EventTypeId = "DCategoryF-eventIdF";

            [RuntimeTest]
            public TestResult ErrorA()
            {
                return TestResult.CreateError("Opsie Warning!")
                    .SetSiteEvent(new SiteEvent(SiteEventSeverity.Warning, EventTypeId, "Oh no Warning!", "Desc Warning!"));
            }

            [RuntimeTest]
            public TestResult ErrorB()
            {
                return TestResult.CreateError("Opsie Information!")
                    .SetSiteEvent(new SiteEvent(SiteEventSeverity.Information, EventTypeId, "Oh no Information!", "Desc Information!"));
            }

            [RuntimeTest]
            public TestResult ErrorC()
            {
                return TestResult.CreateError("Opsie Fatal!")
                    .SetSiteEvent(new SiteEvent(SiteEventSeverity.Fatal, EventTypeId, "Oh no Fatal!", "Desc Fatal!"));
            }

            [RuntimeTest]
            public TestResult ErrorD()
            {
                return TestResult.CreateError("Opsie Error!")
                    .SetSiteEvent(new SiteEvent(SiteEventSeverity.Error, EventTypeId + "2", "Oh no Error2!", "Other Error!"));
            }
        }

        [RuntimeTestClass(Id = "TestClass_Cancellable", DefaultCategory = "DCategory_Cancellable")]
        public class TestClass_Cancellable
        {
            [RuntimeTest(name: "CancellableTest1")]
            public async Task<TestResult> CancellableTest1(CancellationToken cancellationToken)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                return TestResult.CreateSuccess("Completed!");
            }

            [RuntimeTest(name: "CancellableTest2")]
            public async Task<TestResult> CancellableTest2(CancellationToken cancellationToken)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(20), cancellationToken);
                return TestResult.CreateSuccess("Completed!");
            }

            [RuntimeTest(name: "CancellableTest3")]
            public async Task<TestResult> CancellableTest2(CancellationToken cancellationToken, string param1)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(20), cancellationToken);
                return TestResult.CreateSuccess($"Completed! {param1}");
            }
        }

        [RuntimeTestClass(Id = "TestClass_IoCExceptionA", DefaultCategory = "DCategory_IoCException")]
        public class TestClass_IoCExceptionA
        {
            private readonly IDummyInterfaceA _notRegisteredInterface;

            public TestClass_IoCExceptionA(IDummyInterfaceA notRegisteredInterface, int number, object obj)
            {
                _notRegisteredInterface = notRegisteredInterface;
            }

            [RuntimeTest(name: "ExceptionTest1")]
            public TestResult ExceptionTest1()
            {
                return TestResult.CreateSuccess($"Completed! Value is {_notRegisteredInterface.PropA}");
            }
        }

        public class DummyClassA : IDummyInterfaceA { public string PropA { get; set; } = "Value"; }
        public interface IDummyInterfaceA { string PropA { get; } }
    }

    public class TestClassUsedInProxyTests
    {
        public bool ProxyTestedMethod(int id) => (id == 1);
    }

    [RuntimeTestClass]
    public class ProxyTestClassWithMethodAccess
    {
        [ProxyRuntimeTests(RolesWithAccess = AccessRoles.WebAdmins)]
        public static ProxyRuntimeTestConfig ProxyTest() => new ProxyRuntimeTestConfig(typeof(TestClassUsedInProxyTests));
    }

    [RuntimeTestClass(DefaultRolesWithAccess = AccessRoles.WebAdmins)]
    public class ProxyTestClassWithClassAccess
    {
        [ProxyRuntimeTests]
        public static ProxyRuntimeTestConfig ProxyTest() => new ProxyRuntimeTestConfig(typeof(TestClassUsedInProxyTests));
    }

    [RuntimeTestClass(DefaultRolesWithAccess = AccessRoles.WebAdmins)]
    public class ProxyTestClassWithClassAndMethodAccess
    {
        [ProxyRuntimeTests(RolesWithAccess = AccessRoles.SystemAdmins)]
        public static ProxyRuntimeTestConfig ProxyTest() => new ProxyRuntimeTestConfig(typeof(TestClassUsedInProxyTests));
    }

    [RuntimeTestClass]
    public class ProxyTestClassWithoutAccess
    {
        [ProxyRuntimeTests]
        public static ProxyRuntimeTestConfig ProxyTest() => new ProxyRuntimeTestConfig(typeof(TestClassUsedInProxyTests));
    }

    [RuntimeTestClass(Id = "TestSetId", Description = "Some test set", Name = "Dev test set")]
    public class TestClass
    {
        public TestClass()
        {
        }

        [RuntimeTest(Name = "TestMethodForCategoryXYZ", Category = "XYZ")]
        public TestResult TestMethodForCategoryXYZ()
        {
            return TestResult.CreateSuccess($"Success!");
        }

        [RuntimeTest(Name = "TestMethodForCategoryXYZAndASysdmin", Category = "XYZ", RolesWithAccess = AccessRoles.SystemAdmins)]
        public TestResult TestMethodForCategoryXYZAndASysdmin()
        {
            return TestResult.CreateSuccess($"Success!");
        }

        [RuntimeTest(Name = "TestMethodForCategoryXYZAndWebAdmin", Categories = new[] { "XYZ", "Another one" }, RolesWithAccess = AccessRoles.WebAdmins)]
        public TestResult TestMethodForCategoryXYZAndWebAdmin()
        {
            return TestResult.CreateSuccess($"Success!");
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
        public void InvalidMethodB()
        {
            // Method intentionally left empty.
        }

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
