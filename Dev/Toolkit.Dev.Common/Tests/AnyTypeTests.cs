using QoDL.Toolkit.Core.Modules.Tests.Attributes;
using QoDL.Toolkit.Core.Modules.Tests.Models;
using QoDL.Toolkit.WebUI.Extensions;
using System;
using System.Net.Mail;

namespace QoDL.Toolkit.Dev.Common.Tests;

[RuntimeTestClass(
    Name = "Any type tests",
    DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
    GroupName = RuntimeTestConstants.Group.AlmostTopGroup,
    UIOrder = 100
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
        public string PublicStringField;
        private string PrivateStringProperty { get; set; }
        private string PrivateStringField;
        public int IntProperty { get; set; }
        public DateTime DateProperty { get; set; }
        public TestClassA RecursiveProperty { get; set; }

        public string Test()
            => PublicStringField + PublicStringProperty + PrivateStringField + PrivateStringProperty + IntProperty + DateProperty + RecursiveProperty;
    }
}
