# HealthCheck
[![Nuget](https://img.shields.io/nuget/v/HealthCheck.WebUI?label=HealthCheck.WebUI&logo=nuget)](https://www.nuget.org/packages/HealthCheck.WebUI)
[![Nuget](https://img.shields.io/nuget/v/HealthCheck.RequestLog?label=HealthCheck.RequestLog&logo=nuget)](https://www.nuget.org/packages/HealthCheck.RequestLog)
[![npm](https://img.shields.io/npm/v/christianh-healthcheck?label=christianh-healthcheck&logo=npm)](https://www.npmjs.com/package/christianh-healthcheck)

## What is it
Provides an almost plug and play web interface with a few different utility modules that can enabled as needed and access to each module can be restricted.

Modules:

* Overview module where registed events that can be shown in a status interface, e.g. showing the stability of integrations.
* Audit module where actions from other modules are logged.
* Log searcher module for searching through logfiles on disk.
* Request log module that lists controllers and actions with their latest requests and errors.
* Documentation module that shows generated sequence diagrams from code decorated with attributes.
* Dataflow module that can show filtered custom data. For e.g. previewing the latest imported/exported data.
* Event notifications module for notifying through custom implementations when custom events occur.
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

4. Invoke `UseModule(..)` to enable any desired modules.

<details><summary>Example controller</summary>
<p>

```csharp
public class MyController : HealthCheckControllerBase<AccessRoles>
{
    // Enable any modules by invoking the UseModule(..) method.
    public MyController()
    {
        UseModule(new HCTestsModule(new HCTestsModuleOptions() {
            AssemblyContainingTests = typeof(MyController).Assembly
        }));
        // UseModule(..)
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
            PageTitle = "My Title | My Site",
            // In order to not use a cdn for the main scripts
            // you can override them using the 'JavaScriptUrls' property.
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
        // There's 3 methods to grant the request access to modules:

        // #1: Give a given role access to a given module,
        // without setting any module access options:
        config.GiveRolesAccessToModule<HCTestsModule>(AccessRoles.SystemAdmins);
        
        // #2: Give a given role access to a given module,
        // with the given access options:
        config.GiveRolesAccessToModule(AccessRoles.SystemAdmins, HCTestsModule.AccessOption.ViewInvalidTests);
        
        // #3: Give a given role full access to a given module,
        // including all module access options:
        config.GiveRolesAccessToModuleWithFullAccess<HCTestsModule>(AccessRoles.WebAdmins);
        
        // Other access options are available on the config object:
        config.ShowFailedModuleLoadStackTrace = new Maybe<AccessRole>(AccessRoles.WebAdmins);
        config.PingAccess = new Maybe<AccessRole>(AccessRoles.WebAdmins);
        config.RedirectTargetOnNoAccess = "/no-access";
        //..
        
        // Properties CurrentRequestAccessRoles and CurrentRequestInformation
        // are available to use here as well.
    }
}
```
</p>
</details>

---------
# Modules

## Module: Tests

Allows given backend methods to be executed in a UI to check the status of integrations, run utility methods and other things.

### Setup

```csharp
UseModule(new HCTestsModule(new HCTestsModuleOptions() {
        AssemblyContainingTests = typeof(MyController).Assembly
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
[RuntimeTestParameter(target: "orgName", name: "Organization name", description: "Name of the organization the data belongs to", uIHints: RuntimeTestParameterAttribute.UIHint.NotNull)]
public async Task<TestResult> GetDataFromServiceX(int id = 42, string orgName = "Test Organization")
{
    var data = await dataService.GetData(id, orgName, allowCache, maxResults);
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
* `float/single`, `float/single?`
* `double`, `double?`
* `decimal`, `decimal?`
* `bool`, `bool?`
* `DateTime`, `DateTime?`
* `Enum` (-> select)
* `Enum` with `[Flags]` (-> multiselect)
* `HttpPostedFileBase` (.net framework only for now)
* `List<T>` where `<T>` is any of the above types (w/ option for readable list for setting order only)
* `CancellationToken` to make the method cancellable, see below.

#### Cancellable methods

If the first parameter is of the type `CancellationToken` a cancel button will be shown in the UI while the method is running, and only one instance of the method will be able to execute at a time.

#### The TestResult

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

##### Cosmetics
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
|UIHint|Options for parameter display can be set here. Read only lists, prevent null-values, text areas etc.|
|DefaultValueFactoryMethod|For property types that cannot have default values (e.g. lists), use this to specify the name of a public static method in the same class as the method. The method should have the same return type as this parameter, and have zero parameters.|

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

----------

## Module: Audit Log

If the audit log module is used, actions by other modules will be logged and can be viewed in the audit log module interface.

### Setup

```csharp
UseModule(new HCAuditLogModule(new HCAuditLogModuleOptions() { AuditEventService = IAuditEventStorage implementation }));
```

```csharp
// Built in implementation example
IAuditEventStorage auditEventStorage = new FlatFileAuditEventStorage(HostingEnvironment.MapPath("~/App_Data/AuditEventStorage.json"), maxEventAge: TimeSpan.FromDays(30));
```

----------

## Module: Log Viewer

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

----------

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
ISiteEventService siteEventService = new SiteEventService(new FlatFileSiteEventStorage(HostingEnvironment.MapPath("~/App_Data/SiteEventStorage.json"), maxEventAge: TimeSpan.FromDays(5)));
```

#### Example method
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

---------

## Module: Request Log

Shows the last n requests per endpoint, including stack trace of any unhandled exceptions, statuscodes etc.

For requests to be logged and viewable a few things needs to be configured:
* [![Nuget](https://img.shields.io/nuget/v/HealthCheck.RequestLog?label=HealthCheck.RequestLog&logo=nuget)](https://www.nuget.org/packages/HealthCheck.RequestLog) nuget package must be added.
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

</p>
</details>

----------

## Module: Documentation

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

----------

## Module: Dataflow


### Setup

```csharp
```

```csharp
// Built in implementation examples
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

----------

## Module: Settings

Allows custom settings to be configured. Only string, int and boolean properties are supported.

### Setup

```csharp
UseModule(new HCSettingsModule(new HCSettingsModuleOptions() { SettingsService = IHealthCheckSettingsService implementation }));
```

```csharp
// Built in implementation examples
SettingsService = new FlatFileHealthCheckSettingsService<TestSettings>(@"D:\settings.json");
```

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
// Retrieve settings using the GetValue method.
service.GetValue<bool>(nameof(TestSettings.Enabled))
```

</p>
</details>


----------

## Module: Event Notifications

Enables notifications of custom events. Rules for notifications can be edited in a UI and events are easily triggered from code. Notifications are delivered through implementations of `IEventNotifier`. Built-in implementations: `DefaultEventDataSink`, `WebHookEventNotifier`.

Events can be filtered on their id, stringified payload or properties on their payload. Limits can also be set to restrict number of notifications and between dates.

### Setup

```csharp
UseModule(new HCEventNotificationsModule(new HCEventNotificationsModuleOptions() { EventSink = IEventDataSink implementation }));
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
```

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

----------

## Utils

A few utility classes are included below `HealthCheck.Core.Util`:
* `ExceptionUtils` - Get a summary of exceptions to include in results.
* `ConnectivityUtils` - Ping or send webrequests to check if a host is alive and return `TestResult` objects.
* `TimeUtils` - Prettify durations.

## Built in services
Some flatfile storage classes are included and should work fine as the amount of data should not be too large. If used make sure they are registered as singletons, they are thread safe but only within their own instances.
