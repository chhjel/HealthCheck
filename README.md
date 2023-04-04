# HealthCheck

[![Nuget](https://img.shields.io/nuget/v/HealthCheck.WebUI?label=HealthCheck.WebUI&logo=nuget)](https://www.nuget.org/packages/HealthCheck.WebUI)
[![npm](https://img.shields.io/npm/v/christianh-healthcheck?label=christianh-healthcheck&logo=npm)](https://www.npmjs.com/package/christianh-healthcheck)

## What is it

Provides an almost plug and play web interface with a few different utility modules that can enabled as needed and access to each module can be restricted.

Available modules:

* Tests module that allows given backend methods to be executed in a UI to check the status of integrations, run utility methods and other things.
* Messages module where latest sent messages from the system can be viewed, optionally along with any error message. Can be used for e.g. outgoing mail and sms.
* Endpoint Control module to set request limits for decorated endpoints, as well as viewing some request statistics.
* Overview module where registed events that can be shown in a status interface, e.g. showing the stability of integrations.
* Audit module where actions from other modules are logged.
* Data repeater module that can store and retry sending/recieving data with modifications.
* Data flow module that can show filtered custom data. For e.g. previewing the latest imported/exported data.
* Data exporter module that can filter and export data.
* Content permutations module to help find permutations of site content.
* Comparison module where content can be compared in a bit more simplified interface.
* GoTo module where content can be located in a bit more simplified interface.
* Mapped Data module to show how models are mapped.
* Event notifications module for notifying through custom implementations when custom events occur.
* Settings module for custom settings related to healthcheck.
* IDE where C# scripts can be stored and executed in the context of the web application.
* Access token module where tokens with limited access and lifespan can be created to access other modules.
* Downloads module where files can be made available for download, optionally protected by password, expiration date and download count limit.
* Metrics module that outputs some simple metrics you can track manually.
* Request log module that lists controllers and actions with their latest requests and errors.
* Release notes module that can show release notes.
* *[Not styled yet in 4.x+]* Documentation module that shows generated sequence diagrams from code decorated with attributes.
* *[Deprecated in 4.x+]* Log searcher module for searching through logfiles on disk.

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

4. Invoke `UseModule(..)` to enable any desired modules.

5. For .NET Core configure HttpContextAccessor and your instance resolver:

* `services.AddHttpContextAccessor()`
* `HCIoCSetup.ConfigureForServiceProvider(app.Services);`
* or `HCGlobalConfig.DefaultInstanceResolver = (type) => app.ApplicationServices.GetService(type);`
* or `HCGlobalConfig.DefaultInstanceResolver = (type) => app.Services.GetService(type);`

<details><summary>Example controller</summary>
<p>

```csharp
public class MyController : HealthCheckControllerBase<AccessRoles>
{
    // Enable any modules by invoking the UseModule(..) method.
    public MyController()
    {
        // UseModule(<module>, <optionally override name>)
        UseModule(new HCTestsModule(new HCTestsModuleOptions() {
            AssembliesContainingTests = new[] { typeof(MyController).Assembly }
        }));
    }

    // Set any options that will be passed to the front-end here,
    // including the path to this controller.
    protected override HCFrontEndOptions GetFrontEndOptions()
        => new HCFrontEndOptions("/HealthCheck")
        {
            ApplicationTitle = "HealthCheck",
            // See own section below on how to avoid using cdn for js assets if needed.
            EditorConfig = new HCFrontEndOptions.EditorWorkerConfig
            {
                EditorWorkerUrl = "/scripts/editor.worker.js",
                JsonWorkerUrl = "/scripts/json.worker.js"
            }
            //...
        };

    // Set any options for the view here.
    protected override HCPageOptions GetPageOptions()
        => new HCPageOptions()
        {
            PageTitle = "HealthCheck",
            // See own section below on how to avoid using cdn for js assets if needed.
            JavaScriptUrls = new List<string> {
                "/scripts/healthcheck.js",
                "/scripts/healthcheck.vendor.js"
            },
            //...
        };

    // Return the user id/name and any roles the the current request have here.
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

        // The user id/name provided are used for the audit module, "changed by" texts etc.
        return new RequestInformation<AccessRoles>(
            roles, request.UserId(), request.UserName());
    }

    // Access options and other configs here.
    protected override void ConfigureAccess(HttpRequestBase request, AccessConfig<AccessRoles> config)
    {
        // There's 3 methods available to grant the request access to modules:

        // #1: Give a given role access to a given module,
        // without setting any module access options:
        config.GiveRolesAccessToModule<HCTestsModule>(AccessRoles.SystemAdmins);

        // #2: Give a given role access to a given module,
        // with the given access options:
        config.GiveRolesAccessToModule(AccessRoles.SystemAdmins, HCTestsModule.AccessOption.ViewInvalidTests);
        // Optionally limit access to the given categories
        config.GiveRolesAccessToModule(AccessRoles.SystemAdmins, HCTestsModule.AccessOption.ViewInvalidTests, new[] { "CategoryX" });

        // #3: Give a given role full access to a given module,
        // including all module access options:
        config.GiveRolesAccessToModuleWithFullAccess<HCTestsModule>(AccessRoles.WebAdmins);

        // Other access options are available on the config object:
        config.ShowFailedModuleLoadStackTrace = new Maybe<AccessRole>(AccessRoles.WebAdmins);
        config.PingAccess = new Maybe<AccessRole>(AccessRoles.WebAdmins);
        config.RedirectTargetOnNoAccess = "/no-access";
        // To redirect after login and persist state something like this can be used:
        config.RedirectTargetOnNoAccessUsingRequest = (r, q) => $"/login?returnUrl={HttpUtility.UrlEncode($"/healthcheck?{q}")}";
        //..

        // Properties CurrentRequestAccessRoles and CurrentRequestInformation
        // are available to use here as well if needed.
    }
}
```

</p>
</details>

<details><summary>(Optional) How to bundle frontend instead of using CDN</summary>
<p>

By default frontend scripts with versions matching the nuget package version are fetched from unpkg.com. Alternatively use one of the following methods to bundle the frontend with the project:

#### Using the HealthCheck.WebUI.Assets nuget package

The fastest and easiest way is to add the `HealthCheck.WebUI.Assets` nuget package. The package contains all frontend assets, will load them into memory and configure the ui to use them. Requires a few extra mb of memory but makes it easy to update. Does not include the summary scripts for metrics and release notes (see below).

#### Manual configuration

Optionally manually download the frontend files from https://www.npmjs.com/package/christianh-healthcheck and include in project. Then configure `JavaScriptUrls` to include healthecheck.js, and `EditorWorkerUrl` + `JsonWorkerUrl` to include their scripts in the frontend and page option models. Requires the files to be manually updated when updating to a new version of the nuget package.

#### NB: Summary scripts

If metrics or release notes summary is to be bundled with the project, they will have to be configured manually. See example below.

```csharp
// Example using HealthCheck.WebUI.Assets nuget package that enables the GetAsset endpoint.
var hcController = "/url_to_your_hc_controller";
var assemblyVersion = "your_version";
HCAssetGlobalConfig.DefaultMetricsSummaryJavascriptUrl = $"{hcController}/GetAsset?n=metrics.js&v={assemblyVersion}";
HCAssetGlobalConfig.DefaultReleaseNotesSummaryJavascriptUrl = $"{hcController}/GetAsset?n=releaseNotesSummary.js&v={assemblyVersion}";
```

</p>
</details>

---------

# Modules

## Module: Tests

Allows given backend methods to be executed in a UI to check the status of integrations, run utility methods and other things. Any exception thrown from a test will be included in full detail in the UI for easy debugging.

Hold ctrl-shift to view any test categories and show links to open tests in single-mode.

By default test definitions are cached statically, if this is not desired call `TestDiscoveryService.UseCache = false` on project startup.

### Setup

```csharp
UseModule(new HCTestsModule(new HCTestsModuleOptions() {
        AssembliesContainingTests = new[] { typeof(MyController).Assembly },
        // Optionally support custom reference parameter types
        // ReferenceParameterFactories = ...
    }))
    // Optionally configure group order
    .ConfigureGroups((options) => options
        .ConfigureGroup(MyHCConstants.Group.StatusChecks, uiOrder: 100)
        .ConfigureGroup(...)
    );;
```

### Executable methods

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
[RuntimeTestParameter(target: "orgName", name: "Organization name", description: "Name of the organization the data belongs to", uIHints: HCUIHint.NotNull)]
public async Task<TestResult> GetDataFromServiceX(int id = 42, string orgName = "Test Organization")
{
    var data = await dataService.GetData(id, orgName);
    return TestResult.CreateSuccess("Recieved data successfully")
        .AddSerializedData(data, data.Name);
}
```

</p>
</details>

#### Method parameters

Executable methods can have parameter with or without default values. Default values will be included in the generated interface.

Supported parameter types:

* `string`
* `int`, `int?`
* `long`, `long?`
* `float/single`, `float/single?`
* `double`, `double?`
* `decimal`, `decimal?`
* `bool`, `bool?`
* `DateTime`, `DateTime?`, `DateTimeOffset`, `DateTimeOffset?`
* `DateTime[]`, `DateTime?[]`, `DateTimeOffset[]`, `DateTimeOffset?[]` (-> date range selection)
* `TimeSpan`, `TimeSpan?`
* `Enum`, `Enum?` (-> select)
* `Enum` with `[Flags]` (-> multiselect)
* `Guid`, `Guid?` (combine with HCUIHint.AllowRandom to allow new guid generation)
* `byte[]`, `HttpPostedFileBase` (.NET Framework), `IFormFile` (.NET Core) (-> file upload)
* `List<T>` where `<T>` is any of the above types (w/ option for readable list for setting order only)
* `CancellationToken` to make the method cancellable, see below.
* Search and filter for any custom type when custom factory methods are implemented, see below.
* Any other serializable type can be inputted as json.

#### Cancellable methods

If the first parameter is of the type `CancellationToken` a cancel button will be shown in the UI while the method is running, and only one instance of the method will be able to execute at a time.

#### Custom types

Custom parameter types for `[RuntimeTest]`-methods can be used by providing parameter factories to `ReferenceParameterFactories` in `HCTestsModuleOptions`.

<details><summary>Example</summary>
<p>

```csharp

// The first factory with a matching parameter type will be used if any.
private List<RuntimeTestReferenceParameterFactory> CreateReferenceParameterFactories()
{
    return new List<RuntimeTestReferenceParameterFactory>()
    {
        new RuntimeTestReferenceParameterFactory(
            parameterType: typeof(CustomReferenceType),
            
            // `choicesFactory` has to return all the available options for the user to pick
            choicesFactory: (filter) => GetUserChoices()
                .Where(x => string.IsNullOrWhiteSpace(filter) || x.Title.Contains(filter) || x.Id.ToString().Contains(filter))
                .Select(x => new RuntimeTestReferenceParameterChoice(x.Id.ToString(), x.Title)),
            // `getInstanceByIdFactory` has to return one selected instance by id.
            getInstanceByIdFactory: (id) => GetUserChoices().FirstOrDefault(x => x.Id.ToString() == id)
        )
        // Optionally use overload that takes derived types: (type, filter) => ...
        // Can be used to easily support base types in e.g. a cms.
    };
}
```

</p>
</details>

The global parameter factory provided in the options can be overridden per test through the `ReferenceParameterFactoryProviderMethodName` attribute option if needed: `RuntimeTest(ReferenceParameterFactoryProviderMethodName = nameof(GetReferenceFactories))`.

#### Proxy tests

To automatically create tests for all public methods of another class you can use the `[ProxyRuntimeTests]` instead of `[RuntimeTest]`.
The method has to be static, take zero parameters and return a `ProxyRuntimeTestConfig` where you define what type to create tests from.

<details><summary>Example</summary>
<p>

```csharp
[ProxyRuntimeTests]
public static ProxyRuntimeTestConfig SomeServiceProxyTest()
{
    // This will result in one test per public method on the SomeService class.
    return new ProxyRuntimeTestConfig(typeof(SomeService));
}
```

</p>
</details>

<details><summary>Example with custom result action</summary>
<p>

```csharp
[ProxyRuntimeTests]
public static ProxyRuntimeTestConfig SomeServiceProxyTest()
{
    return new ProxyRuntimeTestConfig(typeof(SomeService))
        // After test is executed this callback is invoked where you can e.g. add any extra data to results
        .SetCustomResultAction((result) => result.AddTextData(result.ProxyTestResultObject?.GetType()?.Name, "Result type");
}
```

</p>
</details>

<details><summary>Example with context and custom result action</summary>
<p>

```csharp
[ProxyRuntimeTests]
public static ProxyRuntimeTestConfig SomeServiceProxyTest()
{
    return new ProxyRuntimeTestConfig(typeof(SomeService))
        // Optionally add a custom context for more flexibility
        .SetCustomContext(
            // Create any object as a context object that will be used in the resultAction below
            // Using logger auto-creation logic from the HealthCheck.Utility.Reflection nuget package here.
            contextFactory: () => new { MemoryLogger = HCLogTypeBuilder.CreateMemoryLoggerFor<ISomeLogger>() },
            
            // Optionally override service activation to inject e.g. a memory logger and dump the log along with the test result.
            // instanceFactory: (context) => new SomeService(context.MemoryLogger),

            // After test is executed this callback is invoked where you can e.g. add any extra data to results
            resultAction: (result, context) =>
            {
                result
                    // For proxy tests, the raw return value from the executed method will be placed in result.ProxyTestResultObject
                    .AddTextData(result.ProxyTestResultObject?.GetType()?.Name, "Result type")
                    // Shortcut for executing the given action if the method result is of the given type.
                    .ForProxyResult<OrderLinks>((value) => result.AddUrlsData(value.Select(x => x.Url)))
                    // E.g. include data logged during execution
                    .AddCodeData(context.MemoryLogger.ToString());
            }
        );
}
```

</p>
</details>

#### The TestResult

The `TestResult` class has a few static factory methods for quick creation of a result object, and can contain extra data in various formats.

|Data methods||
|-|-|
|AddImageUrlsData|Will be shown as a image gallery|
|AddUrlsData|Will be shown as a list of links|
|AddJsonData|Will be formatted as Json|
|AddXmlData|Will be formatted as XML|
|AddCodeData|Text shown in a monaco-editor|
|AddDiff|Show a diff of two strings or objects to be serialized in a monaco diff-editor.|
|AddTextData|Just plain text|
|AddData|Adds string data and optionally define the type yourself.|
|AddSerializedData|Two variants of this method exists. Use the extension method variant unless you want to provide your own serializer implementation. The method simply serializes the given object to json and includes it.|
|AddHtmlData|Two variants of this method exists. Use the extension method variant for html presets using `new HtmlPresetBuilder()` or the non-extension method for raw html.|
|AddDataTable|Creates a sortable, filterable datatable from the given list of objects. Top-level properties will be used.|
|AddTimingData|Creates timing metric display.|
|AddTimelineData|Creates a timeline from the given steps. Each step can show a dialog with more info/links.|
|AddFileDownload|Creates a download button that can download e.g. larger files by id. Requires `HCTestsModuleOptions.FileDownloadHandler` to be implemented, see further below.|
|AddExceptionData|Creates a summary of a given exception to display.|

##### Cosmetics

The following methods can be called on the testresult instance to tweak the output look.

|Method|Effect|
|-|-|
|`SetCleanMode()`|Removes expansion panel and copy/fullscreeen/download buttons. Always shows any dump data.|
|`DisallowDataExpansion()`|Always shows any dump data.|
|`SetDataExpandedByDefault()`|Expands any dump data by default.|

##### Validation

If you want to display validation errors on input fields, you can use the following methods on the testresult instance.

|Method|Effect|
|-|-|
|`SetParameterFeedback(..)`| Sets parameter feedback for a single parameter.|
|`SetParametersFeedback(..)`|Sets parameter feedback conditionally for all parameters.|

##### FileDownloadHandler

Example:

```csharp
UseModule(new HCTestsModule(new HCTestsModuleOptions()
{
    AssembliesContainingTests = assemblies,
    FileDownloadHandler = (type, id) =>
    {
        if (type == "blob") return HealthCheckFileDownloadResult.CreateFromStream("myfile.pdf", CreateFileDownloadBlobStream(id));
        else return null;
    }
    ...
```

### Attributes

Methods are configured through the `RuntimeTestClass`, `RuntimeTest` and `RuntimeTestParameter` attributes.

#### [RuntimeTestClass]

Must be applied to the class that contains methods to include. Constructor parameter injection is supported for test classes.

|Property Name|Function|
|-|-|
|Name|Name of the test set that is shown in the UI.|
|Description|Description of the test set that is shown in the UI. Can include html.|
|DefaultAllowParallelExecution|Default value for `AllowParallelExecution` for all methods within this class.|
|DefaultAllowManualExecution|Default value for `AllowManualExecution` for all methods within this class.|
|DefaultRolesWithAccess|Default value for `RolesWithAccess` for all methods within this class. Defaults to controller access options value.|
|DefaultCategory/DefaultCategories|Default value for `Category/Categories` for all methods within this class. Categories can be viewed in the UI by holding ctrl+shift|
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
|ReferenceParameterFactoryProviderMethodName|Optional name of a static method that provides factory methods for reference parameters. See example above.|

#### [RuntimeTestParameter]

Can be applied to either the method itself using the `Target` property or the parameters directly.

|Property Name|Function|
|-|-|
|Target|If the attribute is placed on a method this needs to be the name of the target property.|
|Name|Name of the property. Defaults to a prettified name.|
|Description|Description of the property. Shown as a help text and can contain html.|
|UIHint|Options for parameter display can be set here. Read only lists, prevent null-values, text areas etc.|
|NullName|Override "null"-placeholder values for nullable types if desired.|
|TextPattern|Can be used on text inputs to require the input to match the given regex pattern. Input is validated on blur.|
|DefaultValueFactoryMethod|For property types that cannot have default values (e.g. lists), use this to specify the name of a public static method in the same class as the method. The method should have the same return type as this parameter, and have zero parameters or one string parameter. If the method has one string parameter, the name of the parameter will be its value.|

#### [ProxyRuntimeTests]

Can be used to automatically create tests from all public methods on a type. See own section above.
|Property Name|Function|
|-|-|
|RolesWithAccess|Roles allowed to view/execute the generated methods. Uses roles from the parent `RuntimeTestClass` by default.|

### Scheduled executions

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

#### Log output from tests

Inject a memory logger into the instances being tested and include the output in the result.

```csharp
    // Optionally include the nuget package HealthCheck.Utility.Reflection to create a memory logger for any interface at runtime e.g:
    var memoryLogger = HCLogTypeBuilder.CreateMemoryLoggerFor<ILogger>();

    // GetInstance<T> attempts to create a new instance of the given type by calling the
    // types' constructor with parameters retrieved from the IoC container, except for the values given to the GetInstance method.
    // When passing only the memoryLogger instance below all the other parameters will be retrieved from IoC.
    // By default the parameters passed here is forced through the whole IoC chain for the created instance.
    var myService = IoCUtils.GetInstance<MyService>(memoryLogger);

    // Invoke something to test.
    myService.DoSomething();

    // Include log data in the result
    result.AddCodeData(memoryLogger.ToString(), "Log");
```

#### Test context

When a test is executed a context object is created for the current request that can be accessed through static methods on `HCTestContext`. This can be used in e.g. proxy tests to include some extra logging or timings. The context methods only have any effect when the request executed a test.

* `HCTestContext.Log("Start of test")` Add some log data to the result.
* `HCTestContext.StartTiming("Parsing data")` Start timing with the given description. Can be stopped with `HCTestContext.EndTiming` or continues until the end of the test method is reached.
* `HCTestContext.WithCurrentResult(x => x.AddTextData("Something")); Access the `TestResult` object for the running test if any.

---------

## Module: Audit Log

If the audit log module is used, actions by other modules will be logged and can be viewed in the audit log module interface.

### Setup

```csharp
UseModule(new HCAuditLogModule(new HCAuditLogModuleOptions() {
    AuditEventService = IAuditEventStorage implementation,
    // Optional strip sensitive information in parts of audit event data
    SensitiveDataStripper = (value) => {
        value = HCSensitiveDataUtils.MaskNorwegianNINs(value);
        // MaskAllEmails defaults to masking partially, e.g: ***my@****in.com
        value = HCSensitiveDataUtils.MaskAllEmails(value);
        return value;
    }
}));
```

```csharp
// Built in implementation example

// Optionally include blob storage for larger data (e.g. copy of executed code if enabled)
var blobFolder = HostingEnvironment.MapPath("~/App_Data/AuditEventBlobs");
var blobService = new FlatFileAuditBlobStorage(blobFolder, maxEventAge: TimeSpan.FromDays(1));

IAuditEventStorage auditEventStorage = new FlatFileAuditEventStorage(HostingEnvironment.MapPath("~/App_Data/AuditEventStorage.json"), maxEventAge: TimeSpan.FromDays(30), blobStorage: blobService);
```

---------

## Module: Log Viewer *[Deprecated in 4.x+]*

UI for searching through logfiles.

### Setup

```csharp
UseModule(new HCLogViewerModule(new HCLogViewerModuleOptions() { LogSearcherService = ILogSearcherService implementation() }));
```

```csharp
// Built in implementation example
var logSearcherOptions = new FlatFileLogSearcherServiceOptions()
    .IncludeLogFilesInDirectory(HostingEnvironment.MapPath("~/App_Data/TestLogs/"), filter: "*.log", recursive: true);
ILogSearcherService logSearcherService = new FlatFileLogSearcherService(logSearcherOptions);
```

### Log search query language

When not using regex the search supports the following syntax:

* Or: (a|b|c)
* And: a b c
* Exact: "a b c"

E.g. the query `(Exception|Error) "XR 442" order details` means that the resulting contents must contain either `Exception` or `Error`, and contain both `order`, `details` and `XR 442`.

---------

## Module: Site Events

If an ISiteEventService is provided any events will be retrieved from it and can be shown in a UI. Call `StoreEvent(..)` on this service from other places in the code to register new events.

Test methods can register events if executed through `<TestRunnerService>.ExecuteTests(..)`, a site event service is given, and the `TestResult` from a method includes a `SiteEvent`. When executing a method from the UI the site event data will be ignored. 

Site events are grouped on `SiteEvent.EventTypeId` and extend their duration when multiple events are registered after each other.

### Setup

```csharp
UseModule(new HCSiteEventsModule(new HCSiteEventsModuleOptions() { SiteEventService = ISiteEventService implementation }));
```

```csharp
// Built in implementation example
// Flatfile storages should be injected as singletons, for epi storage implementation see further below
ISiteEventStorage flatfileStorage = new FlatFileSiteEventStorage(HostingEnvironment.MapPath("~/App_Data/SiteEventStorage.json"), maxEventAge: TimeSpan.FromDays(30));
ISiteEventService siteEventService = new SiteEventService(flatfileStorage);
```

#### Example usage from tests module

<details><summary>Example</summary>
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

#### Example usage from static utility or service directly

The included class `HCSiteEventUtils` can optionally be used to quickly register events. (If nothing happens when calling the methods, verify that `HCGlobalConfig.DefaultInstanceResolver` is configured to your resolver.)

<details><summary>Example</summary>
<p>

```csharp
// When something fails you can register an event 
HCSiteEventUtils.TryRegisterNewEvent(SiteEventSeverity.Error, "api_x_error", "Oh no! API X is broken!", "How could this happen to us!?",
    developerDetails: "Error code X, reason Y etc.",
    config: x => x.AddRelatedLink("Status page", "https://status.otherapi.com"));
}

// When the event has been resolved you can mark it as resolved using the same id:
HCSiteEventUtils.TryRegisterResolvedEvent("api_x_error", "Seems it fixed itself somehow.");

// The following could be executed from a scheduled job to resolve events you deem no longer failing based on some criteria.
var unresolvedEvents = HCSiteEventUtils.TryGetAllUnresolvedEvents();
foreach (var unresolvedEvent in unresolvedEvents)
{
    // Basic check, it would probably be better to store somewhere statically when the event ids last worked,
    // and compare against that to check if the issue should be marked as resolved.
    var timeSince = DateTimeOffset.Now - (unresolvedEvent.Timestamp + TimeSpan.FromMinutes(unresolvedEvent.Duration));
    if (timeSince > TimeSpan.FromMinutes(15))
    {
        HCSiteEventUtils.TryMarkEventAsResolved(unresolvedEvent.Id, "Seems to be fixed now.");
    }
}
```

</p>
</details>


---------

## Module: Request Log

Shows the last n requests per endpoint, including stack trace of any unhandled exceptions, statuscodes etc.

For requests to be logged and viewable a few things needs to be configured:

* [![Nuget](https://img.shields.io/nuget/v/HealthCheck.Module.RequestLog?label=HealthCheck.Module.RequestLog&logo=nuget)](https://www.nuget.org/packages/HealthCheck.Module.RequestLog) nuget package must be added.
* A set of action filters will need to be registered.
* Optionally run a utility method on startup to generate definitions from all controller actions.

### Setup

```csharp
UseModule(new HCRequestLogModule(new HCRequestLogModuleOptions() { RequestLogService = IRequestLogStorage implementation }));
```

<details><summary>View full setup details</summary>
<p>

```csharp
// Built in implementation example
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

Optionally decorate methods or classes with the `RequestLogInfoAttribute` attribute to hide endpoints/classes from the log, or to provide additional details. Any method/class decorated with any attribute named `HideFromRequestLogAttribute` will also hide it from the log.

</p>
</details>

---------

## Module: Dynamic Code Execution

Provides a monaco-editor IDE where C# scripts can be stored and executed in the context of the web application to extract data, debug issues or other things. Requires an additional nuget package installed [![Nuget](https://img.shields.io/nuget/v/HealthCheck.Module.DynamicCodeExecution?label=HealthCheck.Module.DynamicCodeExecution&logo=nuget)](https://www.nuget.org/packages/HealthCheck.Module.DynamicCodeExecution)

Should be heavily locked down if used other places than localhost, optimally behind MFA.

### Setup

```csharp
UseModule(new HCDynamicCodeExecutionModule(new HCDynamicCodeExecutionModuleOptions() {
    // Provide the entry assembly of the web application
    TargetAssembly = typeof(MyType).Assembly,

    // Optionally provide a IDynamicCodeScriptStorage to allow online script storage.
    // The provided FlatFileDynamicCodeScriptStorage can be used:
    ScriptStorage = new FlatFileDynamicCodeScriptStorage(@"D:\Server\DCE_Script_Storage.data"),

    // PreProcessors = ...
    // Pre-processors modify code before exectution, and return the modified code back to the frontend.
    // * Included types: BasicAutoCreateUsingsPreProcessor, WrapUsingsInRegionPreProcessor, FuncPreProcessor

    // Validators = ..
    // Validators check the code that is about to be executed and can halt execution with a message.
    // * Included types: FuncCodeValidator

    // StaticSnippets = ..
    // Snippets can be inserted by entering @@@.
}));
```


---------

## Module: DataRepeater

The module allows for storing e.g. incoming/outgoing api requests that failed. The data is listed with simple filtering, can be repaired and be retried processed again.

A default implementation `HCDataRepeaterService` is provided that picks up any registered `IHCDataRepeaterStream` streams.

### Setup

```csharp
// Register your streams and service
services.AddSingleton<IHCDataRepeaterStream, MyStreamA>();
services.AddSingleton<IHCDataRepeaterStream, MyStreamB>();
services.AddSingleton<IHCDataRepeaterService, HCDataRepeaterService>();
```

```csharp
// Use module in hc controller
UseModule(new HCDataRepeaterModule(new HCDataRepeaterModuleOptions
{
    Service = IHCDataRepeaterService implementation
}));
```

```csharp
// Example usage, store data when something fails:
var streamItem = TestOrderStreamItem.CreateFrom(myModel, myModel.ExternalId, "From \"Jimmy Smithy\" - 1234$")
    .AddTags("Capture failed")
    .SetError("Capture failed because of server downtime.", exception);
await myStream.AddItemAsync(streamItem);

// Alternatively using the static util:
var streamItem = TestOrderStreamItem.CreateFrom(myModel, myModel.ExternalId, "From \"Jimmy Smithy\" - 1234$")
    .AddTags("Capture failed")
    .SetError("Capture failed because of server downtime.", exception);
HCDataRepeaterUtils.AddStreamItem<ExampleDataRepeaterStream>(item); // or AddStreamItemAsync<T>

// HCDataRepeaterUtils contains various other shortcuts for setting item properties by the custom id used. E.g. external id above.
// Modify stored items when their statuses changes, e.g. something that failed now works again.
HCDataRepeaterUtils.SetAllowItemRetryAsync<ExampleDataRepeaterStream>(itemId, true);
HCDataRepeaterUtils.AddItemTagAsync<ExampleDataRepeaterStream>(itemId, "Tag X");

// Extension methods exist for streams with shortcuts to item modification methods with only item id and not the guid id. E.g:
await myStream.AddItemTagAsync(itemId, "Tag X");
```

<details><summary>Example stream</summary>
<p>

```csharp
public class MyModelFromEgApi
{
    public string ExternalId { get; set; }
    public decimal Amount { get; set; }
}
public class MyStreamItem : HCDefaultDataRepeaterStreamItem<MyModelFromEgApi, MyStreamItem> { }
public class ExampleDataRepeaterStream : HCDataRepeaterStreamBase<MyStreamItem>
{
    public override string StreamDisplayName => "Order Captures";
    public override string StreamGroupName => "Orders";
    public override string StreamItemsName => "Orders";
    public override string ItemIdDisplayName => "Order number";
    public override string RetryActionName => "Retry capture";
    public override string RetryDescription => "Attempts to perform the capture action again.";
    public override List<string> InitiallySelectedTags => new List<string> { "Failed" };
    public override List<string> FilterableTags => new List<string> { "Failed", "Retried", "Fixed" };
    public override List<IHCDataRepeaterStreamItemAction> Actions => new List<IHCDataRepeaterStreamItemAction>
    {
        new ExampleDataRepeaterStreamItemActionToggleAllow()
    };
    public override List<IHCDataRepeaterStreamItemBatchAction> BatchActions => new List<IHCDataRepeaterStreamItemBatchAction>()
    {
        new ExampleDataRepeaterStreamBatchActionRenameTag()
    };
    // override AllowedAccessRoles or Categories for more granular access control.

    public TestOrderDataRepeaterStream()
        : base(/* IHCDataRepeaterStreamItemStorage implementation here, HCFlatFileDataRepeaterStreamItemStorage<TItem> exists for flatfile or check below for epi */)
    {
    }

    // Resolve optional extra details for the a given item.
    protected override Task<HCDataRepeaterStreamItemDetails> GetItemDetailsAsync(MyStreamItem item)
    {
        var details = new HCDataRepeaterStreamItemDetails
        {
            DescriptionHtml = "<p>Description here with support for <a href=\"#etc\">html.</a></p>",
            Links = new List<HCDataRepeaterStreamItemHyperLink>
            {
                new HCDataRepeaterStreamItemHyperLink("Some link", "/etc1"),
                new HCDataRepeaterStreamItemHyperLink("Details page", "/etc2")
            }
        };
        return Task.FromResult(details);
    }

    // Analyze is called when adding items through the default service and base stream, and optionally manually from the interface.
    // Use to categorize using tags, skip inserting if not needed etc.
    protected override Task<HCDataRepeaterItemAnalysisResult> AnalyzeItemAsync(MyStreamItem item, bool isManualAnalysis = false)
    {
        var result = new HCDataRepeaterItemAnalysisResult();
        // item.AllowRetry = false;
        // result.TagsThatShouldExist.Add("etc");
        // result.TagsThatShouldNotExist.Add("etc");
        result.Message = $"Result from analysis here.";
        return Task.FromResult(result);
    }

    protected override Task<HCDataRepeaterRetryResult> RetryItemAsync(MyStreamItem item)
    {
        // Retry whatever failed initially here.
        // ...

        // And return the result of the attempted retry.
        var result = new HCDataRepeaterRetryResult
        {
            Success = true,
            Message = $"Success! New {item.Data.ExternalId} amount is ${item.Data.Amount}",

            AllowRetry = false,
            Delete = false,
            RemoveAllTags = true,
            TagsThatShouldExist = new List<string> { "Processed" }
        };
        return Task.FromResult(result);
    }
}
```

</p>
</details>

<details><summary>Example stream item action</summary>
<p>

```csharp
// Simple example action that forces AllowRetry on or off.
public class ExampleDataRepeaterStreamItemActionToggleAllow : HCDataRepeaterStreamItemActionBase<ExampleDataRepeaterStreamItemActionToggleAllow.Parameters>
{
    public override string DisplayName => "Set allow retry";
    public override string Description => "Forces AllowRetry property to the given value.";
    public override string ExecuteButtonLabel => "Set";
    // override AllowedAccessRoles or Categories for more granular access control.

    // Optionally override to disable disallowed actions
    // public override Task<HCDataRepeaterStreamItemActionAllowedResult> ActionIsAllowedForAsync(IHCDataRepeaterStreamItem item)

    protected override Task<HCDataRepeaterStreamItemActionResult> PerformActionAsync(IHCDataRepeaterStream stream, IHCDataRepeaterStreamItem item, Parameters parameters)
    {
        var result = new HCDataRepeaterStreamItemActionResult
        {
            Success = true,
            AllowRetry = parameters.Allowed,
            Message = $"AllowRetry was set to {parameters.Allowed}."
        };
        return Task.FromResult(result);
    }

    public class Parameters
    {
        [HCCustomProperty]
        public bool Allowed { get; set; }
    }
}
```

</p>
</details>

<details><summary>Example stream item action that modifies data</summary>
<p>

```csharp
// Simple example action that modifies stream item data
public class ExampleDataRepeaterStreamItemActionModify : HCDataRepeaterStreamItemActionBase<ExampleDataRepeaterStreamItemActionModify.Parameters>
{
    public override string DisplayName => "Modify data example";
    public override string Description => "Example that modifies item data";
    public override string ExecuteButtonLabel => "Update";

    protected override Task<HCDataRepeaterStreamItemActionResult> PerformActionAsync(IHCDataRepeaterStream stream, IHCDataRepeaterStreamItem item, Parameters parameters)
    {
        var result = HCDataRepeaterStreamItemActionResult.CreateSuccess("Data updated.");

        // To perform item modifications from an action, use the SetStreamItemModification<TStreamItem>:
        result.SetStreamItemModification<MyStreamItem>(streamItem =>
        {
            streamItem.ForcedStatus = HCDataRepeaterStreamItemStatus.Error;

            // To update the Data property use ModifyData:
            streamItem.ModifyData(d => d.Something = "Updated");
        });

        return Task.FromResult(result);
    }

    public class Parameters { }
}
```

</p>
</details>

<details><summary>Example stream batch action</summary>
<p>

```csharp
// Example action that renames tags.
public class ExampleDataRepeaterStreamBatchActionRenameTag : HCDataRepeaterStreamItemBatchActionBase<ExampleDataRepeaterStreamBatchActionRenameTag.Parameters>
{
    public override string DisplayName => "Rename tag";
    public override string Description => "Renames a tag on all items.";
    public override string ExecuteButtonLabel => "Rename";

    protected override Task<HCDataRepeaterStreamItemBatchActionResult> PerformBatchActionAsync(IHCDataRepeaterStreamItem item, Parameters parameters, HCDataRepeaterStreamBatchActionResult batchResult)
    {
        if (!item.Tags.Contains(parameters.TagToRename))
        {
            return Task.FromResult(HCDataRepeaterStreamItemBatchActionResult.CreateNotAttemptedUpdated());
        }

        item.Tags.Remove(parameters.TagToRename);
        item.Tags.Add(parameters.NewTagName);

        var shouldStopJob = batchResult.AttemptedUpdatedCount + 1 >= parameters.MaxItemsToUpdate;
        return Task.FromResult(HCDataRepeaterStreamItemBatchActionResult.CreateSuccess(shouldStopJob));
    }

    public class Parameters
    {
        [HCCustomProperty(UIHints = HCUIHint.NotNull)]
        public string TagToRename { get; set; }

        [HCCustomProperty(UIHints = HCUIHint.NotNull)]
        public string NewTagName { get; set; }

        [HCCustomProperty(UIHints = HCUIHint.NotNull)]
        public int MaxItemsToUpdate { get; set; }
    }
}
```

</p>
</details>

---------

## Module: DataExport

Requires an additional nuget package installed [![Nuget](https://img.shields.io/nuget/v/HealthCheck.Module.DataExport?label=HealthCheck.Module.DataExport&logo=nuget)](https://www.nuget.org/packages/HealthCheck.Module.DataExport).

The module allows for filtering and exporting data. The type of data source you have available determines how to filter it.

* IQueryable: Lets the user enter a linq query to filter on.
* IEnumerable&lt;T&gt;: Lets the user filter the data either using an entered linq query or custom parameter inputs depending on your stream implementation.

A default implementation `HCDataExportService` is provided that picks up any registered `IHCDataExportStream` streams.

If you dare allow raw SQL queries, you can inherit a stream from `HCSqlExportStreamBase<HCSqlExportStreamParameters>`. The stream requires a registered `IHCSqlExportStreamQueryExecutor`, `HCDataExportExportSqlQueryExecutor` in the [![Nuget](https://img.shields.io/nuget/v/HealthCheck.Module.DataExport.SQLExecutor?label=HealthCheck.Module.DataExport.SQLExecutor&logo=nuget)](https://www.nuget.org/packages/HealthCheck.Module.DataExport.SQLExecutor) nuget package can be used unless you want to create your own implementation.

If the request only has access to load presets + export, a simplified version of the interface will be displayed where the only actions available is to select a stream, preset and export format.

### Setup

```csharp
// Register your streams and service
services.AddSingleton<IHCDataExportStream, MyDataExportStreamA>();
services.AddSingleton<IHCDataExportStream, MyDataExportStreamB>();
services.AddSingleton<IHCDataExportService, HCDataExportService>();
// Optionally register a preset storage if you want preset save/load functionality enabled
services.AddSingleton<IHCDataExportPresetStorage>(x => new HCFlatFileDataExportPresetStorage(@"your\location\HCDataExportPresets.json"));
```

```csharp
// Use module in hc controller
UseModule(new HCDataExportModule(new HCDataExportModuleOptions
    {
        Service = dataExportService,
        // Optionally provide preset storage if needed
        PresetStorage = dataExportPresetStorage
        // Exporters = ..
    })
    // By default CSV (semicolon + comma), TSV, XML and JSON exporters are configured.
    // Excel exporter can be found in the nuget package HealthCheck.Module.DataExport.Exporter.Excel
    .AddExporter(new HCDataExportExporterXlsx())
);
```

<details><summary>Example stream</summary>
<p>

```csharp
public class MyDataExportStreamA : HCDataExportStreamBase<MyModel>
{
    public override string StreamDisplayName => "My stream A";
    public override string StreamDescription => "Some optional description of the stream.";
    // Number of items to export fetch per batch during export
    public override int ExportBatchSize => 500;
    // The Method parameter decides what method will be used to retrieve data.
    // - Queryable uses GetQueryableItemsAsync()
    // - Enumerable uses GetEnumerableItemsAsync(int pageIndex, int pageSize, Func<MyModel, bool> predicate)
    // - EnumerableWithCustomFilter GetEnumerableWithCustomFilterAsync(..)
    public override IHCDataExportStream.QueryMethod Method => IHCDataExportStream.QueryMethod.Queryable;
    // Optionally set any allowed column formatters. Defaults to allowing all built-in implementations.
    // public override IEnumerable<IHCDataExportValueFormatter> ValueFormatters => new[] { new HCDataExportDateTimeValueFormatter() };
    
    // Optional stream group name
    // public override string StreamGroupName => null;
    // Optional stream access
    // public override object AllowedAccessRoles => RuntimeTestAccessRole.WebAdmins;
    // Optional stream categories
    // public override List<string> Categories => null;
    // Optionally ignore members on model:
    // public override HCMemberFilterRecursive IncludedMemberFilter { get; } = new HCMemberFilterRecursive { ... }

    // Get queryable
    protected override Task<IQueryable<MyModel>> GetQueryableItemsAsync()
        => await _someService.GetItems().AsQueryable();
}
```

</p>
</details>

<details><summary>Example stream with custom parameters</summary>
<p>

```csharp
public class MyDataExportStreamB : HCDataExportStreamBase<MyModel, MyDataExportStreamB.Parameters>
{
    public override string StreamDisplayName => "My stream B";
    public override string StreamDescription => "Some optional description of the stream.";
    public override int ExportBatchSize => 500;
    // Optionally override SupportsQuery to true if you want a predicate available in addition to custom inputs.
    // public override bool SupportsQuery() => true;
    
    protected override Task<TypedEnumerableResult> GetEnumerableItemsAsync(HCDataExportFilterDataTyped<MyModel, MyDataExportStreamB.Parameters> filter)
    {
        var matches = await _something.GetDataAsync(filter.Parameters.StringParameter, filter.Parameters.SomeValue, filter.Parameters.AnotherValue);

        var pageItems = matches
            .Skip(filter.PageIndex * filter.PageSize)
            .Take(filter.PageSize);

        return new TypedEnumerableResult
        {
            PageItems = pageItems,
            TotalCount = matches.Count()
        };
    }
}
    // Add any properties to filter on here.
    public class Parameters
    {
        public string StringParameter { get; set; }
        public int? SomeValue { get; set; }
        // Optionally configure inputs using the HCCustomProperty attribute.
        [HCCustomProperty]
        public DateTime AnotherValue { get; set; }
    }
}
```

</p>
</details>

---------

## Module: Content Permutations

The Content Permutation module helps find different content to e.g. test. Create a class, and a set of instances will be generated with permuted values, allowing you to quickly find example contents in different states using implemented handlers.

### Setup

```csharp
// Register your handlers and service
services.AddSingleton<IHCContentPermutationContentHandler, MyExampleAPermutationHandler>();
services.AddSingleton<IHCContentPermutationContentHandler, MyExampleBPermutationHandler>();
services.AddSingleton<IHCContentPermutationContentDiscoveryService, HCContentPermutationContentDiscoveryService>();
```

```csharp
// Use module in hc controller
UseModule(new HCContentPermutationModule(new HCContentPermutationModuleOptions
{
    AssembliesContainingPermutationTypes = new[] { /* your assembly here */ },
    Service = permutationContentDiscoveryService
}));
```

<details><summary>Example implementation</summary>
<p>

```csharp
// Define your model to generate permutations from.
// Be carefull not to use too many properties or you will be stuck for a while :-)
// Currently only bool and enum types are supported.
[HCContentPermutationType(Name = "Example", Description = "Example description here.")]
public class ExampleAPermutations
{
    public ExampleStatusEnum Status { get; set; }

    // Optionally decorate properties with HCCustomProperty to override name and add descriptions.
    [HCCustomProperty(Name = "Is exported", Description = "Some description here.")]
    public bool IsExported { get; set; }
}

// Then create a handler to fetch content by inheriting from HCContentPermutationContentHandlerBase<YourModelClass>
public class MyExampleAPermutationHandler : HCContentPermutationContentHandlerBase<ExampleAPermutations>
{
    public override Task<List<HCPermutatedContentItemViewModel>> GetContentForAsync(HCGetContentPermutationContentOptions<ExampleAPermutations> options)
    {
        // options.Permutations is an instance of the selected permutation in the UI.
        var permutation = options.Permutation;

        // Get your content enumerable/query..
        var content = yourContentSource.GetEnumerable();

        // ..filter it based on the input permutation
        content = content
            .Where(x => x.Status == permutation.Status
                     && x.IsExported == permutation.IsExported)

        // ..and limit count by options.MaxCount
        var matchingContent = content.Take(options.MaxCount);

        var models = matchingContent
            // Convert to viewmodels, optionally include urls, image url etc.
            .Select(x => new HCPermutatedContentItemViewModel(x.Details, x.PublicUrl))
            .ToList();
        return Task.FromResult(models);
    }
}
```

</p>
</details>

---------

## Module: Comparison

The Comparison module is a simplified interface where content can be searched and compared against each other for debugging purposes.

The built in differ `HCComparisonDifferSerializedJson` can be used to compare serialized versions of content.

### Setup

```csharp
// Register your handlers, differs and service
// - Handlers allow comparing new types
services.AddSingleton<IHCComparisonTypeHandler, MyExampleAComparisonTypeHandler>();
services.AddSingleton<IHCComparisonTypeHandler, MyExampleBComparisonTypeHandler>();
// - Differs compare instances of content in different ways
services.AddSingleton<IHCComparisonDiffer, MyCustomDiffer>();
services.AddSingleton<IHCComparisonDiffer, HCComparisonDifferSerializedJson>();
// - The service handles the boring parts
services.AddSingleton<IHCComparisonService, HCComparisonService>();

```

```csharp
// Use module in hc controller
UseModule(new HCComparisonModule(new HCComparisonModuleOptions
{
    Service = comparisonService
}));
```

<details><summary>Example content handler implementation</summary>
<p>

```csharp
public class MyExampleAComparisonTypeHandler  : HCComparisonTypeHandlerBase<MyContentType>
{
    public override string Description => "Some description for this type.";

    // Find instances to select in the UI based in input search string
    public override Task<List<HCComparisonInstanceSelection>> GetFilteredOptionsAsync(HCComparisonTypeFilter filter)
    {
        var items = MyEnumerable()
            .Where(x => x.Id.ToString().Contains(filter.Input))
            .Take(10)
            .Select(x => new HCComparisonInstanceSelection
            {
                Id = x.Id.ToString(),
                Name = x.Name,
                Description = x.Description
            })
            .ToList();
        return Task.FromResult(items);
    }

    // Get a single instance from its id to compare
    public override Task<DummyThing> GetInstanceWithIdOfAsync(string id)
        => Task.FromResult(_items.FirstOrDefault(x => x.Id.ToString() == id));

    // Get a suitable name displayed in some places
    public override string GetInstanceDisplayNameOf(DummyThing instance) => instance.Name;
}
```

</p>
</details>

<details><summary>Example differ implementation</summary>
<p>

```csharp
// Either extend HCComparisonDifferBase with your content type to allow the differ to be used on, or implement IHCComparisonDiffer directly for more control.
public class MyCustomDiffer : HCComparisonDifferBase<MyContentType>
{
    public override string Name => "Investigate possible conflicts";

    public override Task<HCComparisonDifferOutput> CompareInstancesAsync(MyContentType left, MyContentType right, string leftName, string rightName)
    {
        // Use the methods on HCComparisonDifferOutput to create the output to display for the diff.
        return Task.FromResult(
            new HCComparisonDifferOutput()
                .AddNote("A note", "Note title")
                .AddSideNotes("Left side note", "Right side note", "Side notes title")
                .AddHtml($"Some custom <b>HTML</b>.", "Html title")
                .AddSideHtml($"This ones name is <b>'{leftName}'</b>", $"And this ones name is <b>'{rightName}'</b>", "Side html title")
        );
    }
}
```

</p>
</details>

---------

## Module: Mapped Data

Simple module to display mapping of data.

### Setup

```csharp
// Register service
services.AddSingleton<IHCMappedDataService, HCMappedDataService>();

```

```csharp
// Use module in hc controller
UseModule(new HCMappedDataModule(new HCMappedDataModuleOptions
{
    Service = mappeddataService,
    IncludedAssemblies = new[] { typeof(YourModel).Assembly }
}));
```

<details><summary>Example mapping</summary>
<p>

* Use <=> to indicate a mapping of values.
* Wrap mapped values in [] to indicate that they are mapped from multiple other values.
* Mapped values within quotes (") indicates hardcoded values.
* Lines starting with // will be included as comments.
* To map complex properties, do like in the address example below.
* Override names etc using available attribute properties.

```csharp
[HCMappedClass(@"
ExternalId <=> MyRemoteModel.Id
// Name is joined from first and last name.
FullName <=> [MyRemoteModel.FirstName, ""Middle"", MyRemoteModel.LastName]
Address {
    StreetName <=> MyRemoteModel.HomeAddress.Street,
    StreetNo <=> MyRemoteModel.HomeAddress.StreetNo,
    City <=> MyRemoteModel.HomeAddress.City,
    Zip <=> MyRemoteModel.HomeAddress.ZipCode
}
Something <=> MyRemoteModel.SomeIndexableThing[1].Etc
Another <=> MyRemoteModel.IndexerCanContainAnything[last].Etc
")]
public class MyLocalModel
{
    public string ExternalId { get; set; }
    public string FullName { get; set; }

    public MyAddressModel Address { get; set; }
}

[HCMappedReferencedType]
public class MyRemoteModel { ... }
```

</p>
</details>

### Utils

* Optionally use `HCMappedDataUtils.SetExampleFor(myInstance);` to display example values in the UI. Only supported for classes decorated with `HCMappedClass`.

---------

## Module: GoTo

A very simplified search that allows only a single result per type. Use to quickly find something by e.g. an id that is not normally searchable other places.

### Setup

```csharp
// Register your resolvers
services.AddSingleton<IHCGoToResolver, MyAGotoResolver>();
services.AddSingleton<IHCGoToResolver, MyBGotoResolver>();
// And the built in service
services.AddSingleton<IHCGoToService, HCGoToService>();

```

```csharp
// Use module in hc controller
UseModule(new HCGoToModule(new HCGoToModuleOptions
{
    Service = goToService
}));
```

<details><summary>Example goto resolver implementation</summary>
<p>

```csharp
public class CustomerGotoResolver : IHCGoToResolver
{
    public string Name => "Customer";

    public async Task<HCGoToResolvedData> TryResolveAsync(string input)
    {
        var match = await _myCustomerService.GetCustomerById(input);
        if (match == null) return null;
        return new HCGoToResolvedData
        {
            Name = match.Name,
            Description = match.Description,
            ResolvedFrom = nameof(MyCustomer.Id),
            Urls = new List<HCGoToResolvedUrl> {
                new HCGoToResolvedUrl("Customer Profile", $"/some-url")
            }
        };
    }
}
```

</p>
</details>

### Querystrings

Some special querystrings are supported on the goto page.

|Querystrings||
|-|-|
|query=MyQuery|Prefill the input with the given value.|
|auto=true|Automatically search on page load.|
|autoNav=true|Automatically navigate to the first result if theres only one.|

Combine them all to e.g. make a browser search to quickly goto any content directly.

---------

## Module: Dataflow

If the Dataflow module is enabled the dataflow tab will become available where custom data can be shown. The module can show a filtered list of any data and was made for showing latest imported data per id to quickly verify that incoming data was correct.

A default implementation `DefaultDataflowService` is provided where custom data streams can be registered. Data can be fetched in the ui for each registered stream, optionally filtered on and each property given a hint for how to be displayed. Only `Raw` and `HTML` types have any effect when not expanded.

### Setup

```csharp

UseModule(new HCDataflowModule<RuntimeTestAccessRole>(new HCDataflowModuleOptions<RuntimeTestAccessRole>() {
    DataflowService = IDataflowService implementation
}));
```

```csharp
// Built in implementation example
var options = new DefaultDataflowServiceOptions() {
    Streams = ..your streams,
    // UnifiedSearches = ..any searches if needed
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


<details><summary>Example search across streams</summary>
<p>

```csharp
    public class ExampleSearch : IHCDataflowUnifiedSearch<YourAccessRolesEnum>
    {
        public Maybe<YourAccessRolesEnum> RolesWithAccess => null;
        public string Name => "Example Search";
        public string Description => "Searches some streams.";
        public string QueryPlaceholder => "Search..";
        public string GroupName => "Searches";
        public string GroupByLabel { get; }
        public Dictionary<Type, string> StreamNamesOverrides { get; }
        public Dictionary<Type, string> GroupByStreamNamesOverrides { get; }
        public Func<bool> IsVisible { get; } = () => true;
        public IEnumerable<Type> StreamTypesToSearch { get; } = new[] { typeof(MyStreamA), typeof(MyStreamB), typeof(MyStreamC) };

        public Dictionary<string, string> CreateStreamPropertyFilter(IDataflowStream<YourAccessRolesEnum> stream, string query)
        {
            var filter = new Dictionary<string, string>();
            
            // Create property filter per stream
            if (stream.GetType() == typeof(TestStreamA)) filter[nameof(MyStreamItemA.Title)] = query;
            else if (stream.GetType() == typeof(TestStreamB)) filter[nameof(MyStreamItemB.Text)] = query;
            else if (stream.GetType() == typeof(TestStreamC)) filter[nameof(MyStreamItemC.Name)] = query;

            return filter;
        }

        public HCDataflowUnifiedSearchResultItem CreateResultItem(Type streamType, IDataflowEntry entry)
        {
            var item = entry as MyStreamItem;
            var result = new HCDataflowUnifiedSearchResultItem
            {
                Title = item.Title,
                Body = item.Text
            };
            // Optionally try to include all item data
            result.TryCreatePopupBodyFrom(item);
            return result;
        }
    }
```

</p>
</details>

---------

## Module: Settings

Allows custom settings to be configured.

### Setup

```csharp
UseModule(new HCSettingsModule(new HCSettingsModuleOptions() {
    SettingsService = IHCSettingsService implementation,
    ModelType = typeof(YourSettingsModel)
}));
```

```csharp
// Built in implementation examples
SettingsService = new HCDefaultSettingsService(new HCFlatFileStringDictionaryStorage(@"D:\settings.json"));
```

<details><summary>Example</summary>
<p>

```csharp
// Create a custom model for your settings
public class YourSettingsModel
{
    public string PropertyX { get; set; }

    [HCSetting(GroupName = "Service X")]
    public bool Enabled { get; set; }

    [HCSetting(GroupName = "Service X")]
    public int ThreadLimit { get; set; } = 2;

    [HCSetting(GroupName = "Service X", Description = "Some description here")]
    public int NumberOfThings { get; set; } = 321;

    [HCSetting(GroupName = "Service X", Description = "When to start")]
    public DateTime StartAt { get; set; };
}
```

```csharp
// Retrieve settings model using the GetSettings<T> method.
service.GetSettings<YourSettingsModel>().Enabled
```

</p>
</details>

---------

## Module: Access Tokens

Allows access tokens to be generated with limited access and duration. Tokens are stored hashed and salted in the given `IAccessManagerTokenStorage` implementation. The data being hashed includes given roles, module options, categories and expiration to prevent tampering. Tokens can be used in e.g. querystring to share quick and easy access to limited parts of the healthcheck functionality.

### Setup

```csharp
UseModule(new HCAccessTokensModule(new HCAccessTokensModuleOptions()
{
    TokenStorage = IAccessManagerTokenStorage implementation
}));
```

```csharp
// Built in implementation example
new FlatFileAccessManagerTokenStorage(@"e:\config\access-tokens.json")
```

</p>
</details>

---------

## Module: Event Notifications

Enables notifications of custom events. Rules for notifications can be edited in a UI and events are easily triggered from code. Notifications are delivered through implementations of `IEventNotifier`. Built-in implementations: `DefaultEventDataSink`, `HCWebHookEventNotifier`, `HCMailEventNotifierBase`.

Events can be filtered on their id, stringified payload or properties on their payload, and limits and distinctions can be set.

### Setup

```csharp
UseModule(new HCEventNotificationsModule(new HCEventNotificationsModuleOptions() {
    EventSink = IEventDataSink implementation
}));
```

```csharp
// Built in implementation examples
var notificationConfigStorage = new FlatFileEventSinkNotificationConfigStorage(@"e:\config\eventconfigs.json");
var notificationDefinitionStorage = new FlatFileEventSinkKnownEventDefinitionsStorage(@"e:\config\eventconfig_defs.json");

var eventSink = new DefaultEventDataSink(notificationConfigStorage, notificationDefinitionStorage)
    // Setup any notifiers that should be available
    .AddNotifier(new MyNotifier())
    .AddNotifier(new WebHookEventNotifier())
    // Add any custom placeholders
    .AddPlaceholder("NOW", () => DateTime.Now.ToString())
    .AddPlaceholder("ServerName", () => Environment.MachineName);
```

<details><summary>Example</summary>
<p>

```csharp
// Implement any custom notifiers
public class MyNotifier : IEventNotifier
{
    public string Id => "my_notifier";
    public string Name => "My Notifier";
    public string Description => "Does nothing, just an example.";
    public Func<bool> IsEnabled { get; set; } = () => true;
    public HashSet<string> PlaceholdersWithOnlyNames => null;
    public Dictionary<string, Func<string>> Placeholders { get; } = new Dictionary<string, Func<string>>
    {
        { "Custom_Placeholder", () => "Custom placeholder replaced successfully." }
    };

    public Type OptionsModelType => typeof(MyNotifierOptions);

    public async Task<string> NotifyEvent(NotifierConfig notifierConfig, string eventId, Dictionary<string, string> payloadValues)
    {
        var options = optionsObject as MyNotifierOptions;
        var message = options.Message;

        try
        {
            Console.WriteLine(message);

            // The latest 10 returned strings will be stored and displayed in the UI.
            return await Task.FromResult($"Message '{message}' was outputted.");
        }
        catch (Exception ex)
        {
            return $"Failed to create message '{message}'. {ex.Message}";
        }
    }

    public class MyNotifierOptions
    {
        [EventNotifierOption(description: "Text that will be outputted")]
        public string Message { get; set; }
    }
}
```

```csharp
// Register events from interesting places..

// ..without any additional details
eventSink.RegisterEvent("new_order");

// ..with a payload that can be stringified
eventSink.RegisterEvent("order_exception", errorMessage);

// ..with a payload with stringifiable properties
eventSink.RegisterEvent("new_order", new { PaymentType = 'Invoice', Warnings = 0 });

// The static TryRegisterEvent method can be used for quick and easy access.
EventSinkUtil.TryRegisterEvent("thing_imported", () => new { Type = "etc", Value = 321 })

```

</p>
</details>

---------

## Module: Messages

Store sent messages and view the latest ones sent from the system, optionally along with any error message. Can be used for e.g. outgoing mail and sms.

The following storage implementations are included, both contains options for max counts and time to live and should not be used with more than max a few hundred items per inbox max:

* `HCMemoryMessageStore`: keeps the latest messages in memory without storing anything.
* `HCFlatFileMessageStore`: keeps the latest messages in memory and saves data delayed to a flatfile.

### Setup

```csharp
UseModule(new HCMessagesModule(new HCMessagesModuleOptions()
    { MessageStorage = IHCMessageStorage implementation }
    // Define any inboxes you want to be visible in the UI
    .DefineInbox("mail", "Mail", "All outgoing mail.")
    .DefineInbox("sms", "SMS", "All outgoing sms.")
));
```

```csharp
// Built in implementation examples:
... new HCMemoryMessageStore();
// Flatfile storages should be registered as singletons
... new HCFlatFileMessageStore(@"e:\etc\hc_messages");
```

### Usage in code

```csharp
// Create message item to store
var message = new HCDefaultMessageItem("RE: Hi there",
    "from@somwhere.com", "to@somewhere.com",
    "<b>Some mail</b> content here.",
    isHtml: true);

// Optionally add any error to the message
message.SetError("Failed to send because of invalid email.");

// Send to storage implementation
_messageStore.StoreMessage(inboxId: "mail", message);
```

---------

## Module: Endpoint Control

Requires an additional nuget package installed [![Nuget](https://img.shields.io/nuget/v/HealthCheck.Module.EndpointControl?label=HealthCheck.Module.EndpointControl&logo=nuget)](https://www.nuget.org/packages/HealthCheck.Module.EndpointControl).

Decorate mvc and webapi actions with `HCControlledEndpointAttribute` or `HCControlledApiEndpointAttribute` to allow for a bit of spam control by setting conditional rules at runtime using the interface. The module can also show the latest requests sent to decorated endpoints, including a few graphs.

Also allows for optionally handling blocked requests in code manually, and only count requests that reach a certain step in the code. See usage example below.

Adding attributes to actions will not block anything until you add some rules in the interface.

The default response when request is blocked is a 409 with either a text for GET requests and a json for any other method.

### Setup

```csharp
UseModule(new HCEndpointControlModule(new HCEndpointControlModuleOptions()
{
    EndpointControlService = IEndpointControlService implementation,
    RuleStorage = IEndpointControlRuleStorage implementation,
    DefinitionStorage = IEndpointControlEndpointDefinitionStorage implementation,
    HistoryStorage = IEndpointControlRequestHistoryStorage implementation
}));
```

```csharp
// Built in implementation examples

// Flatfile storages should be registered as singletons
... new FlatFileEndpointControlRuleStorage("e:\etc\ec_rules.json");
... new FlatFileEndpointControlEndpointDefinitionStorage(@"e:\etc\ec_definitions.json");
... new FlatFileEndpointControlRequestHistoryStorage(@"e:\etc\ec_history.json");

// DefaultEndpointControlService can be scoped or singleton depending on your DI framework
...RegisterSingleton<IEndpointControlService, DefaultEndpointControlService>();
```

### Usage in code

<details><summary>Simple usage</summary>
<p>

```csharp
[HttpPost]
// Just decorate with this attribute.
[HCControlledEndpoint]
public ActionResult Submit(MyModel model)
{
    // ...
}
```

</p>
</details>

<details><summary>Custom handling</summary>
<p>

```csharp
[HttpPost]
// Set CustomBlockedHandling to not block the request automatically,
// check on EndpointControlUtils.CurrentRequestWasDecidedBlocked() manually.
[HCControlledEndpoint(CustomBlockedHandling = true)]
public ActionResult Submit(MyModel model)
{
    if (EndpointControlUtils.CurrentRequestWasDecidedBlocked())
    {
        return HttpNotFound();
    }
    // ...
}
```

</p>
</details>

<details><summary>Conditional request counting</summary>
<p>

```csharp
[HttpPost]
// Set ManuallyCounted to not store/count the request automatically,
// invoke EndpointControlUtils.CountCurrentRequest() manually to store it.
[HCControlledEndpoint(ManuallyCounted = true)]
public ActionResult Submit(MyModel model)
{
    if (!ModelState.IsValid)
    {
        return ..;
    }

    // E.g. store after validation to only count valid requests.
    // This way you can set more logical request count limits.
    EndpointControlUtils.CountCurrentRequest();
    // ...
}
```

</p>
</details>

### Custom result types

To override the default blocked result when not using `CustomBlockedHandling`, create your own attributes inheriting from the provided ones and override `CreateBlockedResult`.

To create new types of results that can be selected in the UI, create custom implementations of `IEndpointControlRequestResult` and add them to the endpoint control service through `AddCustomBlockedResult(..)`.

Built in custom types:

* `EndpointControlForwardedRequestResult`: Forwards request to a given url without blocking them. (Currently only for .Net Framework)
* `EndpointControlContentResult`: Allows custom content on block, e.g. some json.
* `EndpointControlRedirectResult`: Redirects to a given url on block.

---------

## Module: Downloads

The downloads module allow files to be made available for download, optionally protected by password, expiration date and download count limit. Downloads are tracked in the audit log. Built-in implementations: `FlatFileSecureFileDownloadDefinitionStorage` for download definition storage, and 3 file storage implementations: `FolderFileStorage`, `UrlFileStorage` and `HCEpiserverBlobFileStorage` (in epi package).

### Setup

```csharp
UseModule(new HCSecureFileDownloadModule(new HCSecureFileDownloadModuleOptions()
{
    DefinitionStorage = ISecureFileDownloadDefinitionStorage implementation,
    FileStorages = new ISecureFileDownloadFileStorage[]
    {
        // By default FolderFileStorage only allows uploading new files.
        // Optionally configure it to allow selecting existing files etc. Uploaded files can't manually be selected later.
        new FolderFileStorage("files_testA", "Disk storage (upload only)", @"e:\files\folderA") { SupportsSelectingFile = false, SupportsUpload = true },
        new FolderFileStorage("files_testB", "Disk storage (download only)", @"e:\files\folderB") { SupportsSelectingFile = true, SupportsUpload = false },
        new FolderFileStorage("files_testC", "Disk storage (upload and download)", @"e:\files\folderC") { SupportsSelectingFile = true, SupportsUpload = true },
        new UrlFileStorage("url", "External url")
    }
}));
```

```csharp
// Built in implementation examples
var downloadDefinitionStorage = new FlatFileSecureFileDownloadDefinitionStorage(@"e:\config\download_definitions.json");;
```

---------

## Module: Metrics

Very simple module that outputs some metrics you can track manually through `HCMetricsContext` statically, to e.g. verify that some methods are not called too often, or to include extra details on every page (timings, errors, notes, etc).

### Setup

`HCMetricsUtil.AllowTrackRequestMetrics` must be configured on startup to select what when tracking is allowed. By default `false` is returned and no context will be created, causing any attempt to track metrics to be ignored.

```csharp
HCMetricsUtil.AllowTrackRequestMetrics = (r) =>  r.Url?.Contains("some=key") || !r.HasRequestContext;
```

To enable the module to view globally tracked metrics register `<IHCMetricsStorage, HCMemoryMetricsStorage>` as a singleton to store data in memory and enable the module:

```csharp
UseModule(new HCMetricsModule(new HCMetricsModuleOptions()
{
    // Register HCMemoryMetricsStorage as a singleton and pass to storage here
    Storage = IHCMetricsStorage instance
}));
```

To include metrics data on every page when any metrics are available use `CreateContextSummaryHtml()` in a view to create the html:

```csharp
@if (allowMetrics)
{
    // If no data has been logged through `HCMetricsContext` for the current request null will be returned.
    @Html.Raw(HealthCheck.Core.Modules.Metrics.Context.HCMetricsUtil.CreateContextSummaryHtml())
}
```

### Usage

Call static shortcuts on `HCMetricsContext` to track details for the current request.

* Global methods `IncrementGlobalCounter` and `AddGlobalValue` stores data for display in the module, non-global methods only stores data temporarily to be shown using `CreateContextSummaryHtml`.
* Data is stored to the registered `IHCMetricsStorage` instance when the context object for the request is disposed, so expect some delays before data shows up in the module interface if used.

```csharp
HCMetricsContext.StartTiming("LoadData()");
// .. do something to be timed here
HCMetricsContext.EndTiming();

// Count something
HCMetricsContext.IncrementGlobalCounter("GetRequestInformation()", 1);

// Add a value that will be stored with counter, min, max and average values
HCMetricsContext.AddGlobalValue("Some value", 42);

// Include some error details
HCMetricsContext.AddError("etc", ex);
```

---------

## Module: Documentation *[Not styled yet in 4.x+]*

Work in progress. At the moment sequence diagrams and flowcharts generated from decorated code will be shown.

The default implementations searches through any given assemblies for methods decorated with `SequenceDiagramStepAttribute` and `FlowChartStepAttribute` and generate diagrams using them.

### Setup

```csharp
UseModule(new HCDocumentationModule(new HCDocumentationModuleOptions()
{
    SequenceDiagramService = ISequenceDiagramService implementation,
    FlowChartsService = IFlowChartsService implementation
}));
```

```csharp
// Built in implementation examples
SequenceDiagramService = new DefaultSequenceDiagramService(new DefaultSequenceDiagramServiceOptions()
{
    DefaultSourceAssemblies = new[] { typeof(MyController).Assembly }
}),
FlowChartsService = new DefaultFlowChartService(new DefaultFlowChartServiceOptions()
{
    DefaultSourceAssemblies = new[] { typeof(MyController).Assembly }
})
```

---------

## Module: Release notes

Simple module that shows release notes e.g. generated during the build process. Supports dev-mode where developers can see any extra details.

### Setup

```csharp
UseModule(new HCReleaseNotesModule(new HCReleaseNotesModuleOptions {
    // Note: inject HCJsonFileReleaseNotesProvider as a singleton
    ReleaseNotesProvider = new HCJsonFileReleaseNotesProvider(HostingEnvironment.MapPath(@"~\App_Data\ReleaseNotes.json"))
    {
        IssueUrlFactory = (id) => $"https://www.yourjira.com/etc/{id}",
        IssueLinkTitleFactory = (id) => $"Jira {id}",
        PullRequestUrlFactory = (number) => $"https://github.com/yourOrg/yourProject/pull/{number}",
    }
}));
```

To include a floating release notes button on every page when any notes are available use `CreateReleaseNotesSummaryHtml()` in a view to create the html. The button pulses when new notes are available.

```csharp
@if (showReleaseNotes)
{
    // If there's nothing to display it outputs a html comment with the reason why.
    @Html.Raw(HealthCheck.Core.Modules.ReleaseNotes.Util.HCReleaseNotesUtil.CreateReleaseNotesSummaryHtml(/* optionally pass true here to include dev details */))
}
```


---------

## Integrated login dialog

An integrated login dialog is included, but custom authentication logic must be provided. To enable the dialog two steps are required.

1. The main controller uses readonly session behaviour that can cause some login logic dependent on sessions to fail, so a new controller is required that handles the login request. Inherit from `HealthCheckLoginControllerBase` and implement `HandleLoginRequest`.

    ```csharp
    public class HCLoginController : HealthCheckLoginControllerBase
    {
        protected override HCIntegratedLoginResult HandleLoginRequest(HCIntegratedLoginRequest request)
        {
            var success = _myAccessService.AuthenticateUser(request.Username, request.Password);
            // also validate request.TwoFactorCode if enabled
            return HCIntegratedLoginResult.CreateResult(success, "Wrong username or password, try again or give up.");
        }
    }
    ```

2. Enable the dialog by setting the `IntegratedLoginEndpoint` property to the url of the `Login` action on the controller in step 1.

    ```csharp
    protected override void ConfigureAccess(HttpRequestBase request,AccessConfig<RuntimeTestAccessRole> config) {
        ...
        config.IntegratedLoginConfig = new HCIntegratedLoginConfig("/hclogin/login")
            // Optionally require 2FA input using OTP, TOTP or WebAuthn
            .EnableOneTimePasswordWithCodeRequest("/hclogin/Request2FACode")
            .EnableTOTP() // <-- requires separate nuget package below
            .EnableWebAuthn();
    }
    ```

Any requests to the index action of the main controller that does not have access to any of the content will now be shown the login dialog. On a successfull login the page will refresh and the user will have access to any content you granted the request.

### MFA: TOTP

To add TOTP MFA you can add the [![Nuget](https://img.shields.io/nuget/v/HealthCheck.WebUI.MFA.TOTP?label=HealthCheck.WebUI.MFA.TOTP&logo=nuget)](https://www.nuget.org/packages/HealthCheck.WebUI.MFA.TOTP) package. If you already have code for validation of TOTP codes in your project this package is not needed.

* For it to work you need to store a 2FA secret per user to validate the codes against. The secret must be a base32 string and can be generated using e.g. `HCMfaTotpUtil.GenerateOTPSecret()`.
* Validate codes using the `HCMfaTotpUtil.ValidateTotpCode(userSecret, code)` method.
* Enable on IntegratedLoginConfig easily by using the extension method `.EnableTOTP()`
* Bitwarden and most authenticator apps supports TOTP and can be used to generate codes from the generated secret.

### MFA: WebAuthn/FIDO2

To add WebAuthn MFA you can add the [![Nuget](https://img.shields.io/nuget/v/HealthCheck.WebUI.MFA.WebAuthn?label=HealthCheck.WebUI.MFA.WebAuthn&logo=nuget)](https://www.nuget.org/packages/HealthCheck.WebUI.MFA.WebAuthn) package.

You can use the included `HCWebAuthnHelper` to register FIDO2 keys and create data secrets to store on your user objects.

* Enable on IntegratedLoginConfig easily by using the method `.EnableWebAuthn()`

<details><summary>Example setup</summary>
<p>

1. Create your implementation of `IHCWebAuthnCredentialManager` that will store and retrieve WebAuthn credential data.

2. In the healthcheck controller specify desired WebAuthn mode for the login page.

    ```csharp
        config.IntegratedLoginConfig = new HCIntegratedLoginConfig
        {
            // ...
            WebAuthnMode = HCIntegratedLoginConfig.HCLoginWebAuthnMode.Required
        };
    ```

3. In the login controller add a factory method to create the `HCWebAuthnHelper`.

    ```csharp
    private HCWebAuthnHelper CreateWebAuthnHelper()
        => new HCWebAuthnHelper(new HCWebAuthnHelperOptions
        {
            ServerDomain = "localhost",
            ServerName = "My fancy site",
            Origin = Request.Headers["Origin"]
        }, new HCMemoryWebAuthnCredentialManager() /* Add your own implementation here that actually stores data */ );
    private HCWebAuthnHelper GetWebAuthnHelper()
    ```

4. Override `CreateWebAuthnAssertionOptionsJson` in the login controller with e.g. the following:

    ```csharp
    protected override string CreateWebAuthnAssertionOptionsJson(HCIntegratedLoginCreateWebAuthnAssertionOptionsRequest request)
    {
        var webauthn = GetWebAuthnHelper();
        var options = webauthn.CreateAssertionOptions(request.Username);
        return options?.ToJson();
    }
    ```

5. Verify the new data in `HandleLoginRequest`.

    ```csharp
    protected override HCIntegratedLoginResult HandleLoginRequest(HCIntegratedLoginRequest request)
    {
        //... username/pass validation etc

        // Verify WebAuthn payload
        if (request.WebAuthnPayload?.Id == null)
        {
            return HCIntegratedLoginResult.CreateError("Invalid FIDO key assertion data.");
        }

        var webauthn = GetWebAuthnHelper();
        var jsonOptions = GetWebAuthnAssertionOptionsJsonForSession();
        var options = AssertionOptions.FromJson(jsonOptions);
        var webAuthnResult = AsyncUtils.RunSync(() => webauthn.VerifyAssertion(options, request.WebAuthnPayload));
        if (!webAuthnResult.Success)
        {
            return HCIntegratedLoginResult.CreateError(webAuthnResult.Error);
        }

        return HCIntegratedLoginResult.CreateSuccess();
    }
    ```

</p>
</details>

### MFA: Sending one time use codes to user

To send a one-time-use code to the user instead of using TOTP you can set the `Send2FACodeEndpoint` option to target the `Request2FACode` action on the login controller. A button to send a code to the user will be shown in the login form, and you can override `Handle2FACodeRequest` to handle what happens when the button is clicked.

Example logic using built in helper methods for creating 2FA codes in session:


```csharp
    protected override HCIntegratedLogin2FACodeRequestResult Handle2FACodeRequest(HCIntegratedLoginRequest2FACodeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return HCIntegratedLogin2FACodeRequestResult.CreateError("You must enter your username first.");
        }

        var code = CreateSession2FACode(request.Username);
        // E.g. send code by mail or sms to user here

        return HCIntegratedLogin2FACodeRequestResult.CreateSuccess($"Code has been sent.", codeExpiresIn: TimeSpan.FromMinutes(5));
    }
```

```csharp
    protected override HCIntegratedLoginResult HandleLoginRequest(HCIntegratedLoginRequest request)
    {
        if (!_myAccessService.AuthenticateUser(request.Username, request.Password))
        {
            return HCIntegratedLoginResult.CreateError("Wrong username or password.");
        }
        else if (!ValidateSession2FACode(request.Username, request.TwoFactorCode))
        {
            return HCIntegratedLoginResult.CreateError("Two-factor code was wrong, try again.");
        }

        return HCIntegratedLogin2FACodeRequestResult.CreateSuccess(
            message: "<b>Success!</b> Your code has been sent.",
            showAsHtml: true,
            codeExpiresIn: TimeSpan.FromMinutes(5)
        );

        // Or the simple way without any extra details
        // return HCIntegratedLoginResult.CreateSuccess();
    }
```

---------

## Integrated Profile

Set `IntegratedProfileConfig` to show a profile button that displays the username, resolved healthcheck roles, and optionally add/remove/elevate access for TOTP and WebAuthn.

```csharp
    config.IntegratedProfileConfig = new HCIntegratedProfileConfig
    {
        Username = CurrentRequestInformation.UserName,
        // BodyHtml = "Here is some custom content.<ul><li><a href=\"https://www.google.com\">A link here</a></li></ul>",
        // ShowTotpElevation = ..
        // TotpElevationLogic = (code) => ..
        // ...
    };
```

---------

## Data storage

### Flatfile storage implementations

The built in flatfile storage classes should work fine for most use cases when a persistent folder is available. If used make sure they are registered as singletons, they are thread safe but only within their own instances.

### Episerver / Optimizely

For Episerver/Optimizely projects storage implementations can optionally be used from [![Nuget](https://img.shields.io/nuget/v/HealthCheck.Episerver?label=HealthCheck.Episerver&logo=nuget)](https://www.nuget.org/packages/HealthCheck.Episerver) and the other episerver packages for specific modules. If used they should be registered as singletons for optimal performance.

Cache can optionally be set to null in constructor if not wanted, or the included memory cache `HCSimpleMemoryCache` can be used as a singleton.  For load balanced environments `HCSimpleMemoryCacheForEpiLoadBalanced` can optionally be used (not much tested yet).

#### Load balanced environments

The storage implementations are not optimized for load balanced environments, if desired the `HCSimpleMemoryCacheForEpiLoadBalanced` cache can be used.


<details><summary>Example IoC setup</summary>
<p>

```csharp
    // Cache required by most of the epi blob implementations below
    context.Services.AddSingleton<IHCCache, HCSimpleMemoryCache>();
    // Alternative (not much tested yet): context.Services.AddSingleton<IHCCache, HCSimpleMemoryCacheForEpiLoadBalanced>();

    // Audit log (defaults to storing the last 10000 events/30 days)
    context.Services.AddSingleton<IAuditEventStorage, HCEpiserverBlobAuditEventStorage>();
    // Messages
    context.Services.AddSingleton<IHCMessageStorage, HCEpiserverBlobMessagesStore<HCDefaultMessageItem>>();
    // AccessTokens
    context.Services.AddSingleton<IAccessManagerTokenStorage, HCEpiserverBlobAccessTokenStorage>();
    // Settings
    context.Services.AddSingleton<IHCStringDictionaryStorage, HCEpiserverBlobStringDictionaryStorage>();
    context.Services.AddSingleton<IHCSettingsService, HCDefaultSettingsService>();
    // DynamicCodeExecution
    context.Services.AddSingleton<IDynamicCodeScriptStorage, HCEpiserverBlobDynamicCodeScriptStorage>();
    // Endpoint control
    context.Services.AddSingleton<IEndpointControlRuleStorage, HCEpiserverBlobEndpointControlRuleStorage>();
    context.Services.AddSingleton<IEndpointControlEndpointDefinitionStorage, HCEpiserverBlobEndpointControlEndpointDefinitionStorage>();
    context.Services.AddSingleton<IEndpointControlRequestHistoryStorage, HCEpiserverBlobEndpointControlRequestHistoryStorage>();
    context.Services.AddSingleton<IEndpointControlService, DefaultEndpointControlService>();
    // Dataflow
    context.Services.AddSingleton<TestDataStream>();
    context.Services.AddSingleton((s) => new DefaultDataflowServiceOptions<AccessRoles>
    {
        Streams = new[] {
            s.GetInstance<TestDataStream>()
        }
    });
    context.Services.AddSingleton<IDataflowService<AccessRoles>, DefaultDataflowService<AccessRoles>>();
    // Site events (defaults to storing the last 1000 events/30 days)
    context.Services.AddSingleton<ISiteEventStorage, HCEpiserverBlobSiteEventStorage>();

    // DataExport
    context.Services.AddSingleton<IHCDataExportPresetStorage, HCEpiserverBlobDataExportPresetStorage>();

    // DataRepeater
    // Example setup:
    /// public class SomeExistingModel {}
    /// public class MyStreamItemA : HCDefaultDataRepeaterStreamItem<SomeExistingModel, MyStreamItemA> {}
    /// public class MyStreamStorageA : HCEpiserverBlobDataRepeaterStreamItemStorage<MyStreamItemA>, IMyStreamStorageA
    // {
    //     protected override Guid ContainerId => Guid.Parse("c0254918-1234-1234-1234-062ed6a11aaa"); // <-- set a unique guid per stream
    //     public MyStreamStorageA(IBlobFactory blobFactory, Core.Abstractions.IHCCache cache) : base(blobFactory, cache) {}
    // }
    // public class MyStreamA : HCDataRepeaterStreamBase<MyStreamItemA> {
    //     public MyStreamA(MyStreamStorageA storage) : base(storage) { }
    // }
    context.Services.AddSingleton<MyStreamStorageA>();
    context.Services.AddSingleton<IHCDataRepeaterStream, MyStreamA>();
    // services.AddSingleton<IHCDataRepeaterStream, MyStreamB>(); etc

    // File download
    context.Services.AddSingleton<ISecureFileDownloadFileStorage, HCEpiserverBlobFileStorage>();
```

</p>
</details>

---------

## Utils

Various utility classes can be found below the `HealthCheck.Core.Util` namespace.

* `HCSensitiveDataUtils` - Util methods for stripping numbers of given lengths, emails etc from texts.
* `HCIPAddressUtils` - Parse strings to IP address models.
* `HCExceptionUtils` - Get a summary of exceptions to include in test results.
* `HCConnectivityUtils` - Ping or send webrequests to check if a host is alive and return `TestResult` objects.
* `HCTimeUtils` - Prettify durations.
* `HCIoCUtils` -  Get instances of types with partial IoC etc.
* `HCAsyncUtils` - Invoke async through reflection, run async synchronous.
* `HCRequestData` - Quickly get/set some data in request items.
* Memory loggers for any interface can be created at runtime by using `HCLogTypeBuilder.CreateMemoryLoggerFor<TInterface>` included in the nuget package `HealthCheck.Utility.Reflection`.
* `HealthCheck.Core.Config.HCGlobalConfig` contains some global static options that can be configured at startup:
  * Dependency resolver override (must be configured for .NET Core).
  * Types and namespaces ignored in data serialization.
  * Current request IP resolver logic override.
* `HCSelfUptimeChecker` - Can be used to trigger actions when the site is back up after a certain amount of downtime.
* `HCEpiserverUtils` - Some utils using epi implementations, e.g. uptime checking using dds storage.

## Troubleshooting errors

If something doesn't work as expected it might be a silenced internal exception. To handle such exceptions subscribe to `HCGlobalConfig.OnExceptionEvent` and handle as needed.
