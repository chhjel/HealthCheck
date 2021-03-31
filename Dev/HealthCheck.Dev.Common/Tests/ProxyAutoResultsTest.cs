﻿using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;
using System;

namespace HealthCheck.Dev.Common.Tests
{
    [RuntimeTestClass(
        Name = "Proxy auto results tests",
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
        GroupName = RuntimeTestConstants.Group.AlmostTopGroup,
        UIOrder = 60
    )]
    public class ProxyAutoResultsTest
    {
        [ProxyRuntimeTests]
        public static ProxyRuntimeTestConfig TestSimpleString()
            => new ProxyRuntimeTestConfig(typeof(ProxyTestSubject));

        public class ProxyTestSubject
        {
#pragma warning disable S3400 // Methods should not return constants
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
            public string TestSimpleString()
                => "String result here";

            public int TestSimpleInt()
                => 1234;

            public DateTime TestSimpleDate()
                => DateTime.Now;

            public string TestUrl()
                => "Some image formats can be found at https://developer.mozilla.org/en-US/docs/Web/Media/Formats/Image_types, a quite nice site.";

            public string TestRelativeUrl()
                => "Some url might be /login/etc or something else.";

            public string TestUrls()
                => "Some image formats can be found at https://developer.mozilla.org/en-US/docs/Web/Media/Formats/Image_types, a quite nice site. https://www.google.com also works.";

            public string TestImageUrls()
                => "A mock image https://via.placeholder.com/150?ext=.jpg and https://via.placeholder.com/550?ext=.png 🤔";
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3400 // Methods should not return constants
        }
    }
}