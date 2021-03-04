﻿using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;
using System;

namespace HealthCheck.Dev.Common.Tests
{
    [RuntimeTestClass(
        Name = "Auto results tests",
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
        GroupName = RuntimeTestConstants.Group.AlmostTopGroup,
        UIOrder = 5
    )]
    public class AutoResultsTest
    {
        [RuntimeTest]
        public TestResult TestSimpleString()
            => TestResult.CreateSuccess("Success.").AddAutoCreatedResultData("String result here");

        [RuntimeTest]
        public TestResult TestSimpleInt()
            => TestResult.CreateSuccess("Success.").AddAutoCreatedResultData(1234);

        [RuntimeTest]
        public TestResult TestSimpleDate()
            => TestResult.CreateSuccess("Success.").AddAutoCreatedResultData(DateTime.Now);

        [RuntimeTest]
        public TestResult TestUrl()
            => TestResult.CreateSuccess("Success.").AddAutoCreatedResultData("Some image formats can be found at https://developer.mozilla.org/en-US/docs/Web/Media/Formats/Image_types, a quite nice site.");

        [RuntimeTest]
        public TestResult TestRelativeUrl()
            => TestResult.CreateSuccess("Success.").AddAutoCreatedResultData("Some url might be /login/etc or something else.");

        [RuntimeTest]
        public TestResult TestUrls()
            => TestResult.CreateSuccess("Success.").AddAutoCreatedResultData("Some image formats can be found at https://developer.mozilla.org/en-US/docs/Web/Media/Formats/Image_types, a quite nice site. https://www.google.com also works.");

        [RuntimeTest]
        public TestResult TestImageUrls()
            => TestResult.CreateSuccess("Success.").AddAutoCreatedResultData("A mock image https://via.placeholder.com/150?ext=.jpg and https://via.placeholder.com/550?ext=.png 🤔");
    }
}
