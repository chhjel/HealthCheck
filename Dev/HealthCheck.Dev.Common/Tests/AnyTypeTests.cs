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
            public string PublicStringProperty { get; set; }
#pragma warning disable S1104 // Fields should not have public accessibility
            public string PublicStringField;
#pragma warning restore S1104 // Fields should not have public accessibility
            private string PrivateStringProperty { get; set; }
            private string PrivateStringField;
            public int IntProperty { get; set; }
            public DateTime DateProperty { get; set; }
            // todo: [HCPropertyDescription]?
            public TestClassA RecursiveProperty { get; set; }
        }
    }
}
