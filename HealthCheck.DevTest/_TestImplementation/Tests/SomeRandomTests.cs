﻿using HealthCheck.Core.Attributes;
using HealthCheck.Core.Entities;
using HealthCheck.Core.Enums;
using HealthCheck.WebUI.Extensions;
using HealthCheck.WebUI.Serializers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.DevTest._TestImplementation.Tests
{
    [RuntimeTestClass(
        Name = "Some fancy tests",
        Description = "Some fancy description",
        DefaultRolesWithAccess = RuntimeTestAccessRole.SystemAdmins,
        GroupName = RuntimeTestConstants.Group.AdminStuff,
        UIOrder = 500
    )]
    public class SomeRandomTests
    {
        [RuntimeTest]
        public TestResult TestWithoutDefaultValues(int number, string text, bool toggle, DateTime date)
        {
            return TestResult.CreateSuccess($"Recieved: [{PrettifyValue(number)}, {PrettifyValue(text)}, {PrettifyValue(toggle)}, {PrettifyValue(date)}]");
        }

        [RuntimeTest]
        public TestResult TestWithNullableValues(int? number = null, bool? checkbox = null, DateTime? date = null)
        {
            return TestResult.CreateSuccess($"Recieved: [{PrettifyValue(number)}, {PrettifyValue(checkbox)}, {PrettifyValue(date)}]");
        }

        private string PrettifyValue(object value)
        {
            return (value != null) 
                ? (value.GetType().IsValueType) ? value.ToString() : $"'{value.ToString()}'" 
                : "null";
        }

        [RuntimeTest]
        public TestResult TestAllDataDumpTypes(int imageWidth = 640, int imageHeight = 480, int imageCount = 10, int linkCount = 4)
        {
            var objectToSerialize = TestResult.CreateWarning($"Some random json object");

            return TestResult.CreateSuccess($"Images has been served.")
                .AddHtmlData($"Some <b>html</b> here!<br /><a href='https://www.google.com'>some link</a>", "Some html")
                .AddSerializedData(objectToSerialize, "Serialized object data")
                .AddTextData("Some text data", "Text title")
                .AddXmlData("<test><el test=\"asd\">Some Value</el></test>", "Xml test")
                .AddJsonData(LargeJson, "Big json data")
                .AddUrlsData(Enumerable.Range(1, linkCount).Select(x => new HyperLink($"Link number #{x}", $"{"https://"}www.google.com?q={x}")))
                .AddImageUrlsData(Enumerable.Range(1, imageCount).Select(x => $"{"https://"}loremflickr.com/{imageWidth}/{imageHeight}?v={x}"), $"{imageCount} images from https://loremflickr.com");
        }


        [RuntimeTest(
            Name = "Get data from X",
            Description = "Retrieve some data from some service.",
            ParameterNames = new[] {
                "Data id",
                "Organization name",
                "Only get the latest data"
            },
            ParameterDescriptions = new[] {
                "Id of the thing to get",
                "Name of the organization the data belongs to.",
                "If true only the latest data will be retrieved."
            }
        )]
        public async Task<TestResult> GetDataFromX(int id = 123, string orgName = "Test Organization", bool latestOnly = false, int someNumber = 42)
        {
            await Task.Delay(300);
            return TestResult.CreateSuccess($"Recieved [{id}, {orgName}, {latestOnly}, {someNumber}] and it was a success!");
        }

        [RuntimeTest(
            Name = "Html result test",
            ParameterNames = new[] { "H-number", "Text" },
            ParameterDescriptions = new[] { "What h-tag to return.", "Text in the h-tag" }
        )]
        public TestResult SomeFancyTest(int hNumber = 3, string text = "Title")
        {
            return TestResult.CreateSuccess($"Success")
                .AddHtmlData($"This is the h-tag you ordered: <h{hNumber}>{text}</h{hNumber}>");
        }

        [RuntimeTest(Description = "Throws some stuff depending on other stuff not really just throws stuff.")]
        public async Task<TestResult> TestThatThrowsException()
        {
            await Task.Delay(100);
            throw new ArgumentOutOfRangeException("nonExistentArgument");
        }

        [RuntimeTest(Category = RuntimeTestConstants.Categories.ScheduledHealthCheck, AllowManualExecution = false)]
        public TestResult TestOfAHealthCheckWarning()
        {
            return TestResult.CreateWarning("Some warning here")
                .SetSiteEvent(new SiteEvent(SiteEventSeverity.Warning, "IntegrationXLatency", "Increased latency with X", "Integration with X seems to be a bit slower than usual.", duration: 1));
        }

        [RuntimeTest(Category = RuntimeTestConstants.Categories.ScheduledHealthCheck)]
        public TestResult TestOfAHealthCheckError(bool someBool = true, string someStringParam = "test string")
        {
            return TestResult.CreateError("Some error")
                .SetSiteEvent(
                    new SiteEvent(SiteEventSeverity.Error, "IntegrationDataImport",
                        $"Integration Y instabilities ({someStringParam}, {someBool})",
                        "Failed to retrieve data from integration Y, the service might be temporary down.", duration: 5)
                    .AddRelatedLink("Endpoint", "https://www.google.com/?q=some+endpoint+instead+of+this")
                );
        }

        [RuntimeTest(Category = RuntimeTestConstants.Categories.ScheduledHealthCheck)]
        public TestResult TestOfAHealthCheckError(bool? someBool = true, string someStringParam = null, int? number = 222, int? number2 = null)
        {
            return TestResult.CreateError("Some error")
                .SetSiteEvent(
                    new SiteEvent(SiteEventSeverity.Information, "IntegrationZDataImport",
                        $"Integration Z instabilities ({someStringParam}, {someBool}, {number}, {number2})",
                        "Failed to retrieve data from integration Z, the service might be temporary down.", duration: 5)
                    .AddRelatedLink("Endpoint", "https://www.google.com/?q=some+endpoint+instead+of+this")
                    .AddRelatedLink("Page", "https://www.google.com/?q=page")
                );
        }


        [RuntimeTest]
        public async Task<TestResult> TestThatReturnsErrorWithExceptionStackTrace()
        {
            try
            {
                await Task.Delay(100);
                throw new ArgumentOutOfRangeException("nonExistentArgument");
            }
            catch (Exception ex)
            {
                return TestResult.CreateError("Result with exception included", ex);
            }
        }

        [RuntimeTest(
            Name = "Custom name for this one",
            Description = "Fancy fancy description",
            ParameterNames = new[] { "Id number" },
            ParameterDescriptions = new[] { "The number on this fancy parameter" }
        )]
        public TestResult SomeFancyTest(int number = 123)
        {
            return TestResult.CreateSuccess($"Number {number} is a success!");
        }

        [RuntimeTest]
        public async Task<TestResult> SomeFancyOtherTestAsync(bool someBooleanParameter = true)
        {
            await Task.Delay(100);
            return TestResult.CreateSuccess($"Boolean value is {someBooleanParameter}!");
        }

        [RuntimeTest(
            Name = "Test of warning result with a data dump",
            Description = "This test returns a warning no matter what the input is.",
            ParameterDescriptions = new[] { "Some help text here", "Some help text here", "Some help text here", "Some help text here" }
        )]
        public async Task<TestResult> ThisOneShouldReturnAWarning(int id = 123, string key = "key_asd", int days = 7, bool reverse = false)
        {
            await Task.Delay(400);

            var warningText = string.Join(" ", Enumerable.Range(1, 24).Select(x => $"Some more warnings here."));
            var result = TestResult.CreateWarning($"This is a warning! Input was ({id}, {key}, {days}, {reverse}). {warningText}");
            result.AddSerializedData(result, new NewtonsoftJsonSerializer());
            result.AddData("Some extra detail item here. Etc etc.");
            result.AddData("Has something: true\nHasSomething else: false", "Property with id: 12456426");

            return result;
        }

        [RuntimeTest]
        public async Task<TestResult> ThisOneShouldReturnAnError()
        {
            await Task.Delay(800);
            return TestResult.CreateError($"This is an error!");
        }

        private const string LargeJson = "[\n"
            + "  {\n"
            + "    \"_id\": \"5d2264f60e07a6e91fdc6931\",\n"
            + "    \"index\": 0,\n"
            + "    \"guid\": \"2f7fcc59-059f-4d65-9949-4d30f4959034\",\n"
            + "    \"isActive\": true,\n"
            + "    \"balance\": \"$2,470.87\",\n"
            + "    \"picture\": \"http://placehold.it/32x32\",\n"
            + "    \"age\": 39,\n"
            + "    \"eyeColor\": \"green\",\n"
            + "    \"name\": {\n"
            + "      \"first\": \"Lola\",\n"
            + "      \"last\": \"Frederick\"\n"
            + "    },\n"
            + "    \"company\": \"JAMNATION\",\n"
            + "    \"email\": \"lola.frederick@jamnation.biz\",\n"
            + "    \"phone\": \"+1 (877) 559-3737\",\n"
            + "    \"address\": \"399 Ryder Avenue, Kula, West Virginia, 8068\",\n"
            + "    \"about\": \"Sint ex tempor laborum dolor commodo sunt sunt nulla minim. Aliqua dolore officia in culpa. Lorem pariatur qui ut magna laborum sunt. Nostrud sit incididunt anim ex tempor aute adipisicing sunt. Nostrud sit commodo adipisicing sit incididunt fugiat consequat. Qui consectetur labore dolor quis commodo laborum quis exercitation. Magna elit dolor incididunt dolor Lorem pariatur dolore et exercitation ea.\",\n"
            + "    \"registered\": \"Wednesday, June 25, 2014 11:23 PM\",\n"
            + "    \"latitude\": \"1.123636\",\n"
            + "    \"longitude\": \"-109.599214\",\n"
            + "    \"tags\": [\n"
            + "      \"fugiat\",\n"
            + "      \"culpa\",\n"
            + "      \"consectetur\",\n"
            + "      \"nostrud\",\n"
            + "      \"ex\"\n"
            + "    ],\n"
            + "    \"range\": [\n"
            + "      0,\n"
            + "      1,\n"
            + "      2,\n"
            + "      3,\n"
            + "      4,\n"
            + "      5,\n"
            + "      6,\n"
            + "      7,\n"
            + "      8,\n"
            + "      9\n"
            + "    ],\n"
            + "    \"friends\": [\n"
            + "      {\n"
            + "        \"id\": 0,\n"
            + "        \"name\": \"Paulette Campbell\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 1,\n"
            + "        \"name\": \"Hallie Alston\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 2,\n"
            + "        \"name\": \"Morrison Witt\"\n"
            + "      }\n"
            + "    ],\n"
            + "    \"greeting\": \"Hello, Lola! You have 10 unread messages.\",\n"
            + "    \"favoriteFruit\": \"strawberry\"\n"
            + "  },\n"
            + "  {\n"
            + "    \"_id\": \"5d2264f619151c85023ca9d5\",\n"
            + "    \"index\": 1,\n"
            + "    \"guid\": \"060f02a4-0502-43f1-85ec-9f8139bb3268\",\n"
            + "    \"isActive\": true,\n"
            + "    \"balance\": \"$3,179.95\",\n"
            + "    \"picture\": \"http://placehold.it/32x32\",\n"
            + "    \"age\": 30,\n"
            + "    \"eyeColor\": \"brown\",\n"
            + "    \"name\": {\n"
            + "      \"first\": \"Sparks\",\n"
            + "      \"last\": \"Snider\"\n"
            + "    },\n"
            + "    \"company\": \"ISOPOP\",\n"
            + "    \"email\": \"sparks.snider@isopop.co.uk\",\n"
            + "    \"phone\": \"+1 (833) 550-3562\",\n"
            + "    \"address\": \"365 Bedford Place, Fairlee, South Carolina, 8718\",\n"
            + "    \"about\": \"Nulla nisi in eu elit sit commodo. Voluptate reprehenderit labore elit Lorem excepteur deserunt minim irure. Eiusmod non dolor aliquip sunt adipisicing. Consequat aliqua irure cillum irure anim ut amet minim non qui reprehenderit dolore. Excepteur eu mollit id aute amet consectetur do id anim. Nisi aute veniam commodo fugiat.\",\n"
            + "    \"registered\": \"Monday, August 17, 2015 11:43 PM\",\n"
            + "    \"latitude\": \"59.090596\",\n"
            + "    \"longitude\": \"170.26014\",\n"
            + "    \"tags\": [\n"
            + "      \"anim\",\n"
            + "      \"amet\",\n"
            + "      \"labore\",\n"
            + "      \"occaecat\",\n"
            + "      \"deserunt\"\n"
            + "    ],\n"
            + "    \"range\": [\n"
            + "      0,\n"
            + "      1,\n"
            + "      2,\n"
            + "      3,\n"
            + "      4,\n"
            + "      5,\n"
            + "      6,\n"
            + "      7,\n"
            + "      8,\n"
            + "      9\n"
            + "    ],\n"
            + "    \"friends\": [\n"
            + "      {\n"
            + "        \"id\": 0,\n"
            + "        \"name\": \"Odonnell Sawyer\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 1,\n"
            + "        \"name\": \"Velma Glenn\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 2,\n"
            + "        \"name\": \"Pennington Andrews\"\n"
            + "      }\n"
            + "    ],\n"
            + "    \"greeting\": \"Hello, Sparks! You have 7 unread messages.\",\n"
            + "    \"favoriteFruit\": \"apple\"\n"
            + "  },\n"
            + "  {\n"
            + "    \"_id\": \"5d2264f699edd745c42cdc33\",\n"
            + "    \"index\": 2,\n"
            + "    \"guid\": \"0296bc47-0264-4059-84ce-ba91f4744fed\",\n"
            + "    \"isActive\": true,\n"
            + "    \"balance\": \"$3,654.94\",\n"
            + "    \"picture\": \"http://placehold.it/32x32\",\n"
            + "    \"age\": 24,\n"
            + "    \"eyeColor\": \"green\",\n"
            + "    \"name\": {\n"
            + "      \"first\": \"Georgina\",\n"
            + "      \"last\": \"Trujillo\"\n"
            + "    },\n"
            + "    \"company\": \"ZIDOX\",\n"
            + "    \"email\": \"georgina.trujillo@zidox.us\",\n"
            + "    \"phone\": \"+1 (874) 439-3481\",\n"
            + "    \"address\": \"852 Railroad Avenue, Hackneyville, North Dakota, 1725\",\n"
            + "    \"about\": \"Quis consectetur et aliqua adipisicing amet dolor est proident aliquip do deserunt adipisicing ullamco laboris. Minim aliquip dolor ea minim cupidatat est ipsum aliqua dolore cupidatat. Non consequat aute cillum ea. Nisi dolor adipisicing occaecat cillum. Ut ad labore ex non ex exercitation sint do irure tempor. Sunt ex occaecat occaecat nostrud aute tempor. Ad fugiat magna qui cillum nostrud labore ut aliqua amet reprehenderit voluptate.\",\n"
            + "    \"registered\": \"Saturday, November 15, 2014 5:48 AM\",\n"
            + "    \"latitude\": \"-26.878266\",\n"
            + "    \"longitude\": \"-73.588183\",\n"
            + "    \"tags\": [\n"
            + "      \"consectetur\",\n"
            + "      \"nulla\",\n"
            + "      \"ut\",\n"
            + "      \"exercitation\",\n"
            + "      \"id\"\n"
            + "    ],\n"
            + "    \"range\": [\n"
            + "      0,\n"
            + "      1,\n"
            + "      2,\n"
            + "      3,\n"
            + "      4,\n"
            + "      5,\n"
            + "      6,\n"
            + "      7,\n"
            + "      8,\n"
            + "      9\n"
            + "    ],\n"
            + "    \"friends\": [\n"
            + "      {\n"
            + "        \"id\": 0,\n"
            + "        \"name\": \"Pugh Townsend\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 1,\n"
            + "        \"name\": \"Belinda Sharp\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 2,\n"
            + "        \"name\": \"Giles Guerra\"\n"
            + "      }\n"
            + "    ],\n"
            + "    \"greeting\": \"Hello, Georgina! You have 8 unread messages.\",\n"
            + "    \"favoriteFruit\": \"banana\"\n"
            + "  },\n"
            + "  {\n"
            + "    \"_id\": \"5d2264f66909de14b6562156\",\n"
            + "    \"index\": 3,\n"
            + "    \"guid\": \"39099b1b-0160-4796-8116-456304d92127\",\n"
            + "    \"isActive\": true,\n"
            + "    \"balance\": \"$3,994.04\",\n"
            + "    \"picture\": \"http://placehold.it/32x32\",\n"
            + "    \"age\": 26,\n"
            + "    \"eyeColor\": \"brown\",\n"
            + "    \"name\": {\n"
            + "      \"first\": \"Deleon\",\n"
            + "      \"last\": \"Steele\"\n"
            + "    },\n"
            + "    \"company\": \"VETRON\",\n"
            + "    \"email\": \"deleon.steele@vetron.tv\",\n"
            + "    \"phone\": \"+1 (952) 515-3148\",\n"
            + "    \"address\": \"929 Tapscott Street, Disautel, Oklahoma, 4215\",\n"
            + "    \"about\": \"Officia in ex reprehenderit occaecat pariatur eiusmod nisi officia sint nulla dolor ea ex commodo. Exercitation ad veniam laboris labore Lorem mollit velit non exercitation deserunt duis anim aliqua. Laborum incididunt nostrud tempor nisi voluptate nostrud eiusmod pariatur qui mollit eiusmod sunt enim. In elit adipisicing magna adipisicing sint velit. Cupidatat aliqua occaecat non dolore. Mollit cillum laborum in laborum reprehenderit ut. Amet aliquip magna sint proident do.\",\n"
            + "    \"registered\": \"Saturday, February 13, 2016 11:50 AM\",\n"
            + "    \"latitude\": \"-60.173622\",\n"
            + "    \"longitude\": \"-178.321036\",\n"
            + "    \"tags\": [\n"
            + "      \"irure\",\n"
            + "      \"fugiat\",\n"
            + "      \"nisi\",\n"
            + "      \"ut\",\n"
            + "      \"amet\"\n"
            + "    ],\n"
            + "    \"range\": [\n"
            + "      0,\n"
            + "      1,\n"
            + "      2,\n"
            + "      3,\n"
            + "      4,\n"
            + "      5,\n"
            + "      6,\n"
            + "      7,\n"
            + "      8,\n"
            + "      9\n"
            + "    ],\n"
            + "    \"friends\": [\n"
            + "      {\n"
            + "        \"id\": 0,\n"
            + "        \"name\": \"Ware Pollard\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 1,\n"
            + "        \"name\": \"Murray Harris\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 2,\n"
            + "        \"name\": \"Deirdre Rush\"\n"
            + "      }\n"
            + "    ],\n"
            + "    \"greeting\": \"Hello, Deleon! You have 7 unread messages.\",\n"
            + "    \"favoriteFruit\": \"apple\"\n"
            + "  },\n"
            + "  {\n"
            + "    \"_id\": \"5d2264f6e46648b9e0b51c5f\",\n"
            + "    \"index\": 4,\n"
            + "    \"guid\": \"43b7243f-83d8-4cf2-8147-040e2f1749a9\",\n"
            + "    \"isActive\": true,\n"
            + "    \"balance\": \"$1,865.38\",\n"
            + "    \"picture\": \"http://placehold.it/32x32\",\n"
            + "    \"age\": 23,\n"
            + "    \"eyeColor\": \"green\",\n"
            + "    \"name\": {\n"
            + "      \"first\": \"Rae\",\n"
            + "      \"last\": \"Carey\"\n"
            + "    },\n"
            + "    \"company\": \"FROSNEX\",\n"
            + "    \"email\": \"rae.carey@frosnex.org\",\n"
            + "    \"phone\": \"+1 (869) 458-3160\",\n"
            + "    \"address\": \"262 Gatling Place, Manila, Vermont, 6273\",\n"
            + "    \"about\": \"Reprehenderit sunt consectetur labore do eu in. Duis ipsum cupidatat minim dolore incididunt ullamco duis elit officia. Eiusmod ea excepteur excepteur non magna ad velit ea. Quis id tempor sint enim mollit incididunt qui ex minim deserunt ullamco adipisicing.\",\n"
            + "    \"registered\": \"Tuesday, June 30, 2015 10:11 PM\",\n"
            + "    \"latitude\": \"-22.679271\",\n"
            + "    \"longitude\": \"-105.052508\",\n"
            + "    \"tags\": [\n"
            + "      \"non\",\n"
            + "      \"qui\",\n"
            + "      \"ipsum\",\n"
            + "      \"id\",\n"
            + "      \"do\"\n"
            + "    ],\n"
            + "    \"range\": [\n"
            + "      0,\n"
            + "      1,\n"
            + "      2,\n"
            + "      3,\n"
            + "      4,\n"
            + "      5,\n"
            + "      6,\n"
            + "      7,\n"
            + "      8,\n"
            + "      9\n"
            + "    ],\n"
            + "    \"friends\": [\n"
            + "      {\n"
            + "        \"id\": 0,\n"
            + "        \"name\": \"Walton Zamora\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 1,\n"
            + "        \"name\": \"Wilma Marsh\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 2,\n"
            + "        \"name\": \"Harrell Sherman\"\n"
            + "      }\n"
            + "    ],\n"
            + "    \"greeting\": \"Hello, Rae! You have 6 unread messages.\",\n"
            + "    \"favoriteFruit\": \"banana\"\n"
            + "  },\n"
            + "  {\n"
            + "    \"_id\": \"5d2264f6c0d1324aadd1b500\",\n"
            + "    \"index\": 5,\n"
            + "    \"guid\": \"b9e6b78f-e483-4954-a048-f39b256a1c17\",\n"
            + "    \"isActive\": true,\n"
            + "    \"balance\": \"$1,965.40\",\n"
            + "    \"picture\": \"http://placehold.it/32x32\",\n"
            + "    \"age\": 40,\n"
            + "    \"eyeColor\": \"brown\",\n"
            + "    \"name\": {\n"
            + "      \"first\": \"Pickett\",\n"
            + "      \"last\": \"Hunter\"\n"
            + "    },\n"
            + "    \"company\": \"NORSUP\",\n"
            + "    \"email\": \"pickett.hunter@norsup.name\",\n"
            + "    \"phone\": \"+1 (955) 499-3443\",\n"
            + "    \"address\": \"867 Vanderveer Street, Bethpage, New Mexico, 2248\",\n"
            + "    \"about\": \"Et aliqua eiusmod occaecat nostrud est sit sit anim minim occaecat sit ea est ex. Lorem magna irure nostrud reprehenderit sunt. Laborum consectetur magna id aliquip. Exercitation sint minim velit ut cillum sint cupidatat culpa culpa aliquip consectetur deserunt Lorem occaecat. In est nostrud cupidatat non. Sint adipisicing velit esse laborum reprehenderit anim officia deserunt nostrud quis mollit proident. Laboris tempor sit velit officia amet qui veniam.\",\n"
            + "    \"registered\": \"Friday, March 25, 2016 8:14 PM\",\n"
            + "    \"latitude\": \"86.532366\",\n"
            + "    \"longitude\": \"53.390805\",\n"
            + "    \"tags\": [\n"
            + "      \"aliquip\",\n"
            + "      \"in\",\n"
            + "      \"officia\",\n"
            + "      \"ut\",\n"
            + "      \"officia\"\n"
            + "    ],\n"
            + "    \"range\": [\n"
            + "      0,\n"
            + "      1,\n"
            + "      2,\n"
            + "      3,\n"
            + "      4,\n"
            + "      5,\n"
            + "      6,\n"
            + "      7,\n"
            + "      8,\n"
            + "      9\n"
            + "    ],\n"
            + "    \"friends\": [\n"
            + "      {\n"
            + "        \"id\": 0,\n"
            + "        \"name\": \"Carpenter Lloyd\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 1,\n"
            + "        \"name\": \"Solomon Garrison\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 2,\n"
            + "        \"name\": \"Lisa Watts\"\n"
            + "      }\n"
            + "    ],\n"
            + "    \"greeting\": \"Hello, Pickett! You have 5 unread messages.\",\n"
            + "    \"favoriteFruit\": \"strawberry\"\n"
            + "  },\n"
            + "  {\n"
            + "    \"_id\": \"5d2264f669709ba8cdd57667\",\n"
            + "    \"index\": 6,\n"
            + "    \"guid\": \"7685353b-6dff-4513-906a-0f33df2ce4e8\",\n"
            + "    \"isActive\": false,\n"
            + "    \"balance\": \"$1,239.73\",\n"
            + "    \"picture\": \"http://placehold.it/32x32\",\n"
            + "    \"age\": 24,\n"
            + "    \"eyeColor\": \"blue\",\n"
            + "    \"name\": {\n"
            + "      \"first\": \"Shauna\",\n"
            + "      \"last\": \"Underwood\"\n"
            + "    },\n"
            + "    \"company\": \"SPACEWAX\",\n"
            + "    \"email\": \"shauna.underwood@spacewax.ca\",\n"
            + "    \"phone\": \"+1 (896) 434-3962\",\n"
            + "    \"address\": \"884 Throop Avenue, Selma, Maryland, 8121\",\n"
            + "    \"about\": \"Ullamco in quis minim et. Duis deserunt reprehenderit duis duis anim dolore mollit magna commodo. Laborum in velit irure eu incididunt mollit ea enim ipsum pariatur irure dolore. Ex exercitation magna quis eu ea esse consectetur ea quis magna non voluptate. Voluptate dolor elit consectetur consectetur consequat adipisicing ullamco est occaecat nostrud deserunt. Et cupidatat deserunt do veniam tempor occaecat labore quis Lorem ea eu.\",\n"
            + "    \"registered\": \"Monday, June 17, 2019 6:20 PM\",\n"
            + "    \"latitude\": \"39.566357\",\n"
            + "    \"longitude\": \"-174.185089\",\n"
            + "    \"tags\": [\n"
            + "      \"aute\",\n"
            + "      \"anim\",\n"
            + "      \"officia\",\n"
            + "      \"anim\",\n"
            + "      \"dolore\"\n"
            + "    ],\n"
            + "    \"range\": [\n"
            + "      0,\n"
            + "      1,\n"
            + "      2,\n"
            + "      3,\n"
            + "      4,\n"
            + "      5,\n"
            + "      6,\n"
            + "      7,\n"
            + "      8,\n"
            + "      9\n"
            + "    ],\n"
            + "    \"friends\": [\n"
            + "      {\n"
            + "        \"id\": 0,\n"
            + "        \"name\": \"Blanchard Gilbert\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 1,\n"
            + "        \"name\": \"Mae Simon\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 2,\n"
            + "        \"name\": \"Brittney Sears\"\n"
            + "      }\n"
            + "    ],\n"
            + "    \"greeting\": \"Hello, Shauna! You have 8 unread messages.\",\n"
            + "    \"favoriteFruit\": \"banana\"\n"
            + "  },\n"
            + "  {\n"
            + "    \"_id\": \"5d2264f6d306c0334bc02e7a\",\n"
            + "    \"index\": 7,\n"
            + "    \"guid\": \"f7afaa23-1786-4170-8f67-e731ee3a5421\",\n"
            + "    \"isActive\": false,\n"
            + "    \"balance\": \"$2,130.79\",\n"
            + "    \"picture\": \"http://placehold.it/32x32\",\n"
            + "    \"age\": 25,\n"
            + "    \"eyeColor\": \"green\",\n"
            + "    \"name\": {\n"
            + "      \"first\": \"Bridgette\",\n"
            + "      \"last\": \"Valencia\"\n"
            + "    },\n"
            + "    \"company\": \"NEPTIDE\",\n"
            + "    \"email\": \"bridgette.valencia@neptide.biz\",\n"
            + "    \"phone\": \"+1 (887) 476-3700\",\n"
            + "    \"address\": \"375 Bartlett Place, Finzel, Alaska, 6170\",\n"
            + "    \"about\": \"Laboris velit do culpa reprehenderit duis incididunt nisi proident. Incididunt non velit mollit do ea deserunt ea pariatur ex aliqua. Dolore sit occaecat duis laborum fugiat commodo et minim incididunt amet. Sint amet laboris magna minim. Sit voluptate sint sunt elit deserunt exercitation cillum mollit laboris aliquip. Anim enim ullamco officia est.\",\n"
            + "    \"registered\": \"Friday, March 1, 2019 5:38 AM\",\n"
            + "    \"latitude\": \"-4.849929\",\n"
            + "    \"longitude\": \"-49.427353\",\n"
            + "    \"tags\": [\n"
            + "      \"officia\",\n"
            + "      \"mollit\",\n"
            + "      \"excepteur\",\n"
            + "      \"nostrud\",\n"
            + "      \"dolore\"\n"
            + "    ],\n"
            + "    \"range\": [\n"
            + "      0,\n"
            + "      1,\n"
            + "      2,\n"
            + "      3,\n"
            + "      4,\n"
            + "      5,\n"
            + "      6,\n"
            + "      7,\n"
            + "      8,\n"
            + "      9\n"
            + "    ],\n"
            + "    \"friends\": [\n"
            + "      {\n"
            + "        \"id\": 0,\n"
            + "        \"name\": \"Holmes Daniel\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 1,\n"
            + "        \"name\": \"Christian Page\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 2,\n"
            + "        \"name\": \"Liz Rocha\"\n"
            + "      }\n"
            + "    ],\n"
            + "    \"greeting\": \"Hello, Bridgette! You have 6 unread messages.\",\n"
            + "    \"favoriteFruit\": \"strawberry\"\n"
            + "  }\n"
            + "]\n";
    }
}
