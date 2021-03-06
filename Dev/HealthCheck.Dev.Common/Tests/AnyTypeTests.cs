using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.WebUI.Extensions;
using System;
using System.Net.Mail;

namespace HealthCheck.Dev.Common.Tests
{
    [RuntimeTestClass(
        Name = "Any type tests",
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
        GroupName = RuntimeTestConstants.Group.AlmostTopGroup,
        UIOrder = 10
    )]
    public class AnyTypeTests
    {
        [RuntimeTest]
        public TestResult TestWithSomeComplexParameters(TestClassA testA, MailMessage testB)
        {
            var result = new
            {
                TestA = testA,
                testB = testB
            };
            return TestResult.CreateSuccess("Success hopefully.")
                .AddSerializedData(result);
        }

        public class TestClassA
        {
#pragma warning disable S1144 // Unused private types or members should be removed
#pragma warning disable S1104 // Fields should not have public accessibility
#pragma warning disable IDE0044 // Add readonly modifier
            public string PublicStringProperty { get; set; }
            public string PublicStringField;
            private string PrivateStringProperty { get; set; }
            private string PrivateStringField;
            public int IntProperty { get; set; }
            public DateTime DateProperty { get; set; }
            public TestClassA RecursiveProperty { get; set; }

            public string Test() 
                => PublicStringField + PublicStringProperty + PrivateStringField + PrivateStringProperty + IntProperty + DateProperty + RecursiveProperty;
#pragma warning restore S1104 // Fields should not have public accessibility
#pragma warning restore S1144 // Unused private types or members should be removed
#pragma warning restore IDE0044 // Add readonly modifier
        }
    }
}
