# HealthCheck
[![Nuget](https://img.shields.io/nuget/v/HealthCheck.WebUI?label=HealthCheck.WebUI&logo=nuget)](https://www.nuget.org/packages/HealthCheck.WebUI)
[![Nuget](https://img.shields.io/nuget/v/HealthCheck.RequestLog?label=HealthCheck.RequestLog&logo=nuget)](https://www.nuget.org/packages/HealthCheck.RequestLog)
[![npm](https://img.shields.io/npm/v/christianh-healthcheck?label=christianh-healthcheck&logo=npm)](https://www.npmjs.com/package/christianh-healthcheck)

## What is it
Provides an almost plug and play web interface with a few different utility modules that can enabled as needed and access to each module can be restricted.

Modules:

* Test module where given backend methods can be executed to check the status of integrations, run utility methods and other things.
* Overview module where registed events that can be shown in a status interface, e.g. showing the stability of integrations.
* Audit module where actions from other modules are logged.
* Log searcher module for searching through logfiles on disk.
* Request log module that lists controllers and actions with their latest requests and errors.
* Documentation module that shows generated sequence diagrams from code decorated with attributes.
* Dataflow module that can show filtered custom data. For e.g. previewing the latest imported/exported data.
* Settings module for custom settings related to healthcheck.

## Getting started

1. Install the HealthCheck.WebUI nuget package.
2. Create a custom flags enum with any desired access roles, e.g:
    ```csharp
    [Flags]
    public enum AccessRoles
    {
        None = 0,
        Guest = 1,
        WebAdmins = 2,
        SystemAdmins = 4
    }
    ```
3. Create a controller and inherit `HealthCheckControllerBase<AccessRoles>`, where AccessRoles is your enum from the step above.

4. Optionally set any extra services on the `Services` property to enable their modules.

<details><summary>Example controller</summary>
<p>

```csharp
public class MyController : HealthCheckControllerBase<AccessRoles>
{
    // Optionally provide any extra services and set on the 'Service' property.
    // See the Services section further down in readme.
    public MyController(
        ISiteEventService siteEventService,
        IAuditEventStorage auditEventService)
        : base(assemblyContainingTests: typeof(MyController).Assembly)
    {
        Services.SiteEventService = siteEventService;
        Services.AuditEventService = auditEventService;
        // ...
    }

    // Set any options that will be passed to the front-end here,
    // including the path to this controller.
    protected override FrontEndOptionsViewModel GetFrontEndOptions()
        => new FrontEndOptionsViewModel("/HealthCheck")
        {
            ApplicationTitle = "My Title"
            //...
        };

    // Set any options for the view here.
    protected override PageOptions GetPageOptions()
        => new PageOptions()
        {
            PageTitle = "My Title | My Site"
            //...
        };

    // Access options and other configs here.
    // CurrentRequestAccessRoles is returned from your implementation
    // of the GetRequestInformation method below.
    protected override void Configure(HttpRequestBase request)
    {
        TestRunner.IncludeExceptionStackTraces = CurrentRequestAccessRoles.HasValue && CurrentRequestAccessRoles.Value.HasFlag(AccessRoles.SystemAdmins);
        
        AccessOptions.OverviewPageAccess = new Maybe<AccessRoles>(AccessRoles.Guest);
        AccessOptions.TestsPageAccess = new Maybe<AccessRoles>(AccessRoles.WebAdmins);
        AccessOptions.AuditLogAccess = new Maybe<AccessRoles>(AccessRoles.SystemAdmins);
        AccessOptions.LogViewerPageAccess = new Maybe<AccessRoles>(AccessRoles.SystemAdmins);
        AccessOptions.InvalidTestsAccess = new Maybe<AccessRoles>(AccessRoles.SystemAdmins);
        AccessOptions.SiteEventDeveloperDetailsAccess = new Maybe<AccessRoles>(AccessRoles.SystemAdmins);
        AccessOptions.RequestLogPageAccess = new Maybe<AccessRoles>(AccessRoles.SystemAdmins);
        AccessOptions.ClearRequestLogAccess = new Maybe<AccessRoles>(AccessRoles.SystemAdmins);
        
        // If the current request does not have access to any of the pages they will be redirected to the given url. If not set they will get a 404.
        // AccessOptions.RedirectTargetOnNoAccess = "/login?redirect=..";
    }

    // Optionally set group configs here. Only order in the UI for now.
    // Groups are defined by setting the GroupName property on RuntimeTestClass attributes. 
    protected override void SetTestSetGroupsOptions(TestSetGroupsOptions options)
    {
        options
            .SetOptionsFor(MyHealthCheckConstants.Groups.GroupA, uiOrder: 100)
            .SetOptionsFor(MyHealthCheckConstants.Groups.GroupB, uiOrder: 90);
    }

    // Return the user id/name and any roles the the current request have here.
    // The return value is used for any access options and audit logging if enabled.
    protected override RequestInformation<AccessRoles> GetRequestInformation(HttpRequestBase request)
    {
        var roles = AccessRoles.Guest;

        if (request.IsWebAdmin())
        {
            roles |= AccessRoles.WebAdmins;
        }
        if (request.IsSysAdmin())
        {
            roles |= AccessRoles.SystemAdmins;
        }

        return new RequestInformation<AccessRoles>(
            roles, request.UserId(), request.UserName());
    }
}
```
</p>
</details>

## Executable methods

For a method to be discovered it needs to..
* ..be public.
* ..be in a class with a `[RuntimeTestClass]` attribute.
* ..be decorated with a `[RuntimeTest]` attribute.
* ..return a `TestResult` or be async and return a `Task<TestResult>`.

```csharp
[RuntimeTestClass]
public class MyClass
{
    [RuntimeTest]
    public TestResult MyMethod()
        => TestResult.CreateSuccess("Executed successfully");
}
```

<details><summary>Another method example</summary>
<p>

```csharp

[RuntimeTest("Get data from somewhere", "Retrieves data from service X and shows the response data.")]
[RuntimeTestParameter(target: "id", name: "Data id", description: "Id of the thing to get")]
[RuntimeTestParameter(target: "orgName", name: "Organization name", description: "Name of the organization the data belongs to", notNull: true)]
public async Task<TestResult> GetDataFromServiceX(int id = 42, string orgName = "Test Organization")
{
    var data = await dataService.GetData(id, orgName, allowCache, maxResults);
    return TestResult.CreateSuccess("Recieved data successfully")
        .AddSerializedData(data, data.Name);
}
```
</p>
</details>

### Method parameters

Executable methods can have parameter with or without default values. Default values will be included in the generated interface.

Supported parameter types:
* `string`
* `int`, `int?`
* `bool`, `bool?`
* `DateTime`, `DateTime?`
* `Enum` (-> select)
* `Enum` with `[Flags]` (-> multiselect)
* `HttpPostedFileBase` (.net framework only for now)
* `List<T>` where `<T>` is any of the above types (w/ option for readable list for setting order only)
* `CancellationToken` to make the method cancellable, see below.

### Cancellable methods

If the first parameter is of the type `CancellationToken` a cancel button will be shown in the UI while the method is running, and only one instance of the method will be able to execute at a time.

### The TestResult

The `TestResult` class has a few static factory methods for quick creation of a result object, and can contain extra data in various formats.

|Data methods||
|-|-|
|AddImageUrlsData|Will be shown as a image gallery|
|AddUrlsData|Will be shown as a list of links|
|AddJsonData|Will be formatted as Json|
|AddXmlData|Will be formatted as XML|
|AddCodeData|Text shown in a monaco-editor|
|AddTextData|Just plain text|
|AddData|Adds string data and optionally define the type yourself.|
|AddSerializedData|Two variants of this method exists. Use the extension method variant unless you want to provide your own serializer implementation. The method simply serializes the given object to json and includes it.|
|AddHtmlData|Two variants of this method exists. Use the extension method variant for html presets using `new HtmlPresetBuilder()` or the non-extension method for raw html.|
|AddTimelineData|Creates a timeline from the given steps. Each step can show a dialog with more info/links.|

#### Cosmetics
The following methods can be called on the testresult instance to tweak the output look.

|Method|Effect|
|-|-|
|`SetCleanMode()`|Removes expansion panel and copy/fullscreeen buttons. Always shows any dump data.|
|`DisallowDataExpansion()`|Always shows any dump data.|
|`SetDataExpandedByDefault()`|Expands any dump data by default.|

### Attributes

Methods are configured through the `RuntimeTestClass`, `RuntimeTest` and `RuntimeTestParameter` attributes.

#### [RuntimeTestClass]
Must be applied to the class that contains methods to include.

|Property Name|Function|
|-|-|
|Name|Name of the test set that is shown in the UI.|
|Description|Description of the test set that is shown in the UI. Can include html.|
|DefaultAllowParallelExecution|Default value for `AllowParallelExecution` for all methods within this class.|
|DefaultAllowManualExecution|Default value for `AllowManualExecution` for all methods within this class.|
|DefaultRolesWithAccess|Default value for `RolesWithAccess` for all methods within this class. Defaults to controller access options value.|
|DefaultCategory/DefaultCategories|Default value for `Category/Categories` for all methods within this class.|
|GroupName|Optional group name in the UI.|
|UIOrder|Order of the set in the UI, higher value = higher up.|

#### [RuntimeTest]
Must be applied to the method that should be executed.

|Property Name|Function|
|-|-|
|Name|Name of the test that is shown in the UI. Defaults to prettified method name.|
|Description|Description of the test that is hown in the UI. Can include HTML.|
|Category/Categories|Optional categories that can be filtered upon.|
|RolesWithAccess|Roles allowed to view/execute this method. Uses roles from the parent `RuntimeTestClass` by default.|
|RunButtonText/RunningButtonText|Optional custom texts for the button that executes the method.|
|AllowManualExecution|True by default, can be set to false to hide the method from the interface.|
|AllowParallelExecution|True by default, can be overridden for single methods. Does not have any effect when running methods from the UI, only when executing multiple methods via code.|

#### [RuntimeTestParameter]
Can be applied to either the method itself using the `Target` property or the parameters directly.

|Property Name|Function|
|-|-|
|Target|If the attribute is placed on a method this needs to be the name of the target property.|
|Name|Name of the property. Defaults to a prettified name.|
|Description|Description of the property. Shown as a help text and can contain html.|
|NotNull|Set to true to not allow null values to be entered in the interface.|
|ReadOnlyList|Only affects generic lists. Does not allow new entries to be added, or existing entries to be changed. Only the order of the items can be changed.|
|DefaultValueFactoryMethod|For property types that cannot have default values (e.g. lists), use this to specify the name of a public static method in the same class as the method. The method should have the same return type as this parameter, and have zero parameters.|

## Services
A few flatfile storage classes are included and should work fine as the amount of data should not be too large. If used make sure they are registered as singletons, they are thread safe but only within their own instances.

## API
An `/ExecuteTests` endpoint exists to execute all tests within a given category and return the results. Only tests the request has access to will be executed. The request must also have the `TestsPageAccess`. There is also a `/Ping` endpoint that can be used that just returns 'OK' and 200 status code.

Example request:
```
Invoke-WebRequest -Uri "https://server/ExecuteTests?key=something" -Method "POST" -Headers @{ -ContentType "application/x-www-form-urlencoded" -Body "TestCategory=IntegrationTests"
``` 

<details><summary>Example response:</summary>
<p>

```json
{
    "TotalResult": "Error",
    "SuccessCount": 2,
    "WarningCount": 1,
    "ErrorCount": 1,
    "ErrorMessage": null,
    "Results": [
        {
            "TestId": "HealthCheckTests.SomeAPITests.TestServiceX",
            "TestName": "Check integration X",
            "Result": "Error",
            "Message": "Failed to execute test with the exception: Input string was not in a correct format.",
            "StackTrace": "System.FormatException: Input string was not in a correct format.\r\n   at System.Number.StringToNumber(String str, NumberStyles options, NumberBuffer& number, NumberFormatInfo info, Boolean parseDecimal)\r\n   at System.Number.ParseInt32(String s, NumberStyles style, NumberFormatInfo info)\r\n   at System.Int32.Parse(String s)\r\n   at HealthCheckTests.SomeAPITests.<TestSomething>d__4.MoveNext() in D:\\...\\SomeAPITests.cs:line 47\r\n--- End of stack trace from previous location where exception was thrown ---\r\n   at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)\r\n   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)\r\n   at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()\r\n   at HealthCheck.Core.Entities.TestDefinition.<ExecuteTest>d__58.MoveNext()\r\n--- End of stack trace from previous location where exception was thrown ---\r\n   at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)\r\n   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)\r\n   at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()\r\n   at HealthCheck.Core.Services.TestRunnerService.<ExecuteTest>d__9.MoveNext()"
        },
        {
            "TestId": "HealthCheckTests.SomeAPITests.TestServiceY",
            "TestName": "Check integration Y",
            "Result": "Success",
            "Message": "Success, it took about 3 seconds.",
            "StackTrace": null
        },
        {
            "TestId": "HealthCheckTests.SomeAPITests.TestServiceZ",
            "TestName": "Check integration Z",
            "Result": "Warning",
            "Message": "Success, it took about a second.",
            "StackTrace": null
        },
        {
            "TestId": "HealthCheckTests.SomeAPITests.TestServiceF.Int32-String-Boolean-Int32",
            "TestName": "Test Service A",
            "Result": "Success",
            "Message": "Retrieved id ('1234') successfully.",
            "StackTrace": null
        }
    ]
}
```
</p>
</details>

### IAuditEventStorage
If an IAuditEventStorage is provided in the controller any test executions/cancellations will be logged to it. This also allows for the audit log interface to be shown.

```csharp
IAuditEventStorage auditEventStorage = new FlatFileAuditEventStorage(HostingEnvironment.MapPath("~/App_Data/AuditEventStorage.json"), maxEventAge: TimeSpan.FromDays(30));
```

### ILogSearcherService
Specify a ILogSearcherService implementation to use to enable the log searcher tab. The provided FlatFileLogSearcherServiceOptions works for flatfile logs where entries start with a timestamp.

```csharp
var logSearcherOptions = new FlatFileLogSearcherServiceOptions()
    .IncludeLogFilesInDirectory(HostingEnvironment.MapPath("~/App_Data/TestLogs/"), filter: "*.log", recursive: true);
ILogSearcherService logSearcherService = new FlatFileLogSearcherService(logSearcherOptions);
```

#### Log search query language
When not using regex the search supports the following syntax:
* Or: (a|b|c)
* And: a b c
* Exact: "a b c"

E.g. the query `(Exception|Error) "XR 442" order details` means that the resulting contents must contain either `Exception` or `Error`, and contain both `order`, `details` and `XR 442`.

### ISiteEventService
If an ISiteEventService is provided in the controller any events will be retrieved from it and can be shown in a overview page. Call this service from other places in the code to register new events.

Test methods can register events if executed through `<TestRunnerService>.ExecuteTests(..)`, a site event service is given, and the `TestResult` from a method includes a `SiteEvent`. When executing a method from the UI the site event data will be ignored. 

Site events are grouped on `SiteEvent.EventTypeId` and extend their duration when multiple events are registered after each other.

```csharp
ISiteEventService siteEventService = new SiteEventService(new FlatFileSiteEventStorage(HostingEnvironment.MapPath("~/App_Data/SiteEventStorage.json"), maxEventAge: TimeSpan.FromDays(5)));
```

<details><summary>Example method</summary>
<p>

```csharp

[RuntimeTest]
public TestResult CheckIntegrationX()
{
    // Use the same event type id when reporting and resolving the event.
    var eventTypeId = "IntegrationXAvailability";

    try {
        ...
        
        // Methods that include site events should always include a resolved event
        // when the method runs successfully. The event will then be marked as resolved.
        return TestResult.CreateResolvedSiteEvent(
            testResultMessage: "Integration X seems to be alive.",
            eventTypeid: eventTypeId,
            resolvedMessage: "Integration X seems to be working again.");
    }
    catch(Exception ex)
    {
        // On error include a site event in the result
        return TestResult.CreateError(ex.Message)
            .SetSiteEvent(new SiteEvent(SiteEventSeverity.Error, eventTypeId,
                title: "Integration X availability reduced",
                description: "There seems to be some instabilities at the moment " +
                "and feature Y and Z might temporarily experience reduced functionality."));
    }
}
```
</p>
</details>

### IRequestLogService
For requests to be logged and viewable 2-3 things needs to be configured:
* [![Nuget](https://img.shields.io/nuget/v/HealthCheck.RequestLog?label=HealthCheck.RequestLog&logo=nuget)](https://www.nuget.org/packages/HealthCheck.RequestLog) nuget package must be added.
* An IRequestLogService has to be provided in the healthcheck controller. The default one `RequestLogService` can be used.
* A set of action filters will need to be registered.
* Optionally run a utility method on startup to generate definitions from all controller actions.

To clear the requestlog use the button at the bottom of the requestlog page. It will be visible for selected roles when the access option `ClearRequestLogAccess` is set.

`AccessOptions.ClearRequestLogAccess = new Maybe<AccessRoles>(AccessRoles.SystemAdmins);`

<details><summary>Example setup</summary>
<p>

```csharp
// Register the service with desired options
IRequestLogStorage storage = new FlatFileRequestLogStorage(HostingEnvironment.MapPath("~/App_Data/RequestLog.json");
var options = new RequestLogServiceOptions
{
    MaxCallCount = 3,
    MaxErrorCount = 5,
    CallStoragePolicy = RequestLogCallStoragePolicy.RemoveOldest,
    ErrorStoragePolicy = RequestLogCallStoragePolicy.RemoveOldest
};
IRequestLogService service = new RequestLogService(storage, options);
```

```csharp
// Register MVC action filters.
public static void RegisterGlobalFilters(GlobalFilterCollection filters)
{
    filters.Add(new RequestLogActionFilter());
    filters.Add(new RequestLogErrorFilter());
    ..
}

// Register WebAPI action filter.
public static void RegisterWebApiFilters(HttpFilterCollection filters)
{
    filters.Add(new RequestLogWebApiActionFilter());
    ..
}
```

```csharp
// Optionally call this method on startup to generate endpoint definitions
Task.Run(() => RequestLogUtil.EnsureDefinitionsFromTypes(RequestLogServiceAccessor.Current, new[] { <your assemblies that contain controllers> }));
```

```csharp
// The following utility method can be called to register requests that the filters can't capture
RequestLogUtils.HandleRequest(RequestLogServiceAccessor.Current, GetType(), Request);
```

```csharp
// In some cases or if IoC is not used the static
// RequestLogServiceAccessor.Current property must be set to an instance of the service.
RequestLogServiceAccessor.Current = .. service instance
```

</p>
</details>

### ISequenceDiagramService
If an `ISequenceDiagramService` is provided in the controller the documentation tab will become available where generated diagrams are shown.

A default implementation `DefaultSequenceDiagramService` is provided. It will search through any assemblies provided through its options property for any methods decorated with `SequenceDiagramStepAttribute` and generate diagrams using them.

```csharp
var options = new DefaultSequenceDiagramServiceOptions()
{
    DefaultSourceAssemblies = new[] { <your assemblies> }
};
ISequenceDiagramService service = new DefaultSequenceDiagramService(options);
```


### IDataflowService
If an `IDataflowService` is provided in the controller the dataflow tab will become available where custom data can be shown.

A default implementation `DefaultDataflowService` is provided where custom data streams can be registered. Data can be fetched in the ui for each registered stream, optionally filtered on and each property given a hint for how to be displayed. Only `Raw` and `HTML` types have any effect when not expanded.

```csharp
var options = new DefaultDataflowServiceOptions() {
    Streams = <your streams>
};
IDataflowService service = new DefaultDataflowService(options);
```

A default abstract stream `FlatFileStoredDataflowStream<TEntry, TEntryId>` is provided and can be used to store and retrieve latest entries per id to a flatfile + optionally limit the age of entries.
* Use `.InsertEntries(..)` method to insert new entries.
* Use `IsVisible` property to set stream visibility in the UI.
* Use `AllowInsert` property to optionally ignore any new data attempted to be inserted.
* Override `RolesWithAccess` property to set who has access to view the stream data.
* If used make sure the services are registered as singletons, they are thread safe but only within their own instances.
* `GenericDataflowStreamObject.Create` can optionally be used to include a subset of an existing types properties instead of creating a new model.

<details><summary>Simple example stream</summary>
<p>

```csharp
    public class MySimpleStream : FlatFileStoredDataflowStream<YourAccessRolesEnum, YourDataModel, string>
    {
        public override Maybe<YourAccessRolesEnum> RolesWithAccess =>new Maybe<YourAccessRolesEnum>(YourAccessRolesEnum.SystemAdmins);
        public override string Name => $"My Simple Stream";
        public override string Description => $"The simplest of streams.";

        public MySimpleStream()
            : base(
                @"e:\storage\path\my_simple_stream.json",
                idSelector: (e) => e.Code,
                idSetter: (e, id) => e.Code = id
            ) {
            // To attempt auto-creation of filters for some suitable
            // property types the AutoCreateFilters method can be used.
            AutoCreateFilters<YourDataModel>();
        }
    }
```
</p>
</details>

<details><summary>Example stream using a few more options</summary>
<p>

```csharp
    public class MyStream : FlatFileStoredDataflowStream<YourAccessRolesEnum, YourDataModel, string>
    {
        public override Maybe<YourAccessRolesEnum> RolesWithAccess =>new Maybe<YourAccessRolesEnum>(YourAccessRolesEnum.SystemAdmins);
        public override string Name => $"My Stream";
        public override string Description => $"A stream using a few more options.";

        public MyStream(IConfig yourOptionalConfigService)
            : base(
                @"e:\storage\path\my_stream.json",
                idSelector: (e) => e.Code,
                idSetter: (e, id) => e.Code = id,
                maxEntryAge: TimeSpan.FromDays(7)
            )
        {
            // Optionally toggle some options at runtime
            IsVisible = () => yourOptionalConfigService.ShowMyStream;
            AllowInsert = () => yourOptionalConfigService.EnableMyStreamInserts;

            // Optionally customize object property data
            ConfigureProperty(nameof(YourDataModel.Code), "Product Code").SetFilterable();
            ConfigureProperty(nameof(YourDataModel.Details))
                .SetUIHint(DataFlowPropertyDisplayInfo.DataFlowPropertyUIHint.Dictionary);
                .SetVisibility(DataFlowPropertyDisplayInfo.DataFlowPropertyUIVisibilityOption.OnlyWhenExpanded);
        }
        
        // Override FilterEntries method to implement any custom filtering.
        // To show a filter in frontend IsFilterable must be set to true in ConfigureProperty above.
        protected override Task<IEnumerable<YourDataModel>> FilterEntries(DataflowStreamFilter filter, IEnumerable<YourDataModel> entries)
        {
            // Get user input for Code property
            var codeFilter = filter.GetPropertyFilterInput(nameof(YourDataModel.Code));
            // Filter on property
            entries = entries.Where(x => codeFilter == null || x.Code.ToLower().Contains(codeFilter));
            
            // Or use the FilterContains shortcut for the same effect
            entries = filter.FilterContains(entries, nameof(YourDataModel.Code), x => x.Code);

            return Task.FromResult(entries);
        }
    }
```
</p>
</details>

### IHealthCheckSettingsService

If an `IHealthCheckSettingsService` is provided in the controller the settings tab will become available where custom settings can be configured. Only string, int and boolean properties are supported. A `FlatFileHealthCheckSettingsService` is provided for simple use cases.

<details><summary>Example</summary>
<p>

```csharp
// Create a custom model for your settings
public class TestSettings
{
    public string PropertyX { get; set; }

    [HealthCheckSetting(GroupName = "Service X")]
    public bool Enabled { get; set; }

    [HealthCheckSetting(GroupName = "Service X")]
    public int ThreadLimit { get; set; } = 2;

    [HealthCheckSetting(GroupName = "Service X", description: "Some description here")]
    public int NumberOfThings { get; set; } = 321;
}
```

```csharp
// Register the service in controller
// IoC a singleton of the default FlatFileHealthCheckSettingsService if used.
Services.SettingsService = new FlatFileHealthCheckSettingsService<TestSettings>(@"e:\config\settings.json");
```

```csharp
// Retrieve settings using the GetValue method.
service.GetValue<bool>(nameof(TestSettings.Enabled))
```

</p>
</details>


## Scheduled health checks

There is no built in scheduler but the `TestRunnerService` can be used to easily execute a subset of the methods from e.g. a scheduled job and report the results to the given site `ISiteEventService`.

```csharp
TestDiscoveryService testDiscovererService = ..;
ISiteEventService siteEventService = ..;

var runner = new TestRunnerService();
var results = await runner.ExecuteTests(testDiscovererService,
    // Only include methods belonging to the custom "Scheduled Checks"-category
    (m) => m.Categories.Contains("Scheduled Checks"),
    // Provide an event service to automatically report to it
    siteEventService);
```

## Utils

A few utility classes are included below `HealthCheck.Core.Util`:
* `ExceptionUtils` - Get a summary of exceptions to include in results.
* `ConnectivityUtils` - Ping or send webrequests to check if a host is alive and return `TestResult` objects.
* `TimeUtils` - Prettify durations.
