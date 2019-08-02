# HealthCheck
![Nuget](https://img.shields.io/nuget/v/HealthCheck.WebUI?label=HealthCheck.WebUI&logo=nuget)
![npm](https://img.shields.io/npm/v/christianh-healthcheck?label=christianh-healthcheck&logo=npm)

## What is it
Provides an abstract stand-alone controller that generates a web interface where given backend methods can be executed to check the status of integrations, run utility methods, dump some data and other things.

Also exposes an optional service for registering events that can be shown in a generated interface inspired by http://status.github.com.

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

<details><summary>Example controller</summary>
<p>

```csharp
public class MyController : HealthCheckControllerBase<AccessRoles>
{
    // Optionally provide site event and audit services and set them.
    // See the Services section further down in readme.
    public MyController(ISiteEventService siteEventService, AuditEventService auditEventService)
        : base(assemblyContainingTests: typeof(MyController).Assembly)
    {
        SiteEventService = siteEventService;
        AuditEventService = auditEventService;
    }

    // Set any options that will be passed to the front-end here, including the path to this controller.
    protected override FrontEndOptionsViewModel GetFrontEndOptions()
        => new FrontEndOptionsViewModel("/HealthCheck")
        {
            ApplicationTitle = "My Title"
            //...
        };

    // Set any options for the razor view here.
    protected override PageOptions GetPageOptions()
        => new PageOptions()
        {
            PageTitle = "My Title | My Site"
            //...
        };

    // Access options and other configs here.
    // CurrentRequestAccessRoles is returned from GetRequestInformation method below.
    protected override void Configure(HttpRequestBase request)
    {
        TestRunner.IncludeExceptionStackTraces = CurrentRequestAccessRoles.HasValue && CurrentRequestAccessRoles.Value.HasFlag(AccessRoles.SystemAdmins);
        
        AccessOptions.OverviewPageAccess = new Maybe<AccessRoles>(AccessRoles.Guest);
        AccessOptions.TestsPageAccess = new Maybe<AccessRoles>(AccessRoles.WebAdmins);
        AccessOptions.AuditLogAccess = new Maybe<AccessRoles>(AccessRoles.SystemAdmins);
        AccessOptions.InvalidTestsAccess = new Maybe<AccessRoles>(AccessRoles.SystemAdmins);
        AccessOptions.SiteEventDeveloperDetailsAccess = new Maybe<AccessRoles>(AccessRoles.SystemAdmins);
        
        // If the current request does not have access to any of the pages they will be redirected to the given url. If not set they will get a 404.
        // AccessOptions.RedirectTargetOnNoAccess = "/login?redirect=..";
    }

    // Optionally set group configs here. Only order in the UI for now.
    // Groups are defined by setting the GroupName property on RuntimeTestClass attributes. 
    protected override void SetTestSetGroupsOptions(TestSetGroupsOptions options)
    {
        options
            .SetOptionsFor(MyHealthCheckConstants.Group.GroupA, uiOrder: 100)
            .SetOptionsFor(MyHealthCheckConstants.Group.GroupB, uiOrder: 90);
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

### Method parameters

Executable methods can have parameter with or without default values. Default values will be included in the generated interface.

Supported parameter types:
* `int`, `int?`
* `string`
* `DateTime`, `DateTime?`
* `Enum` (=> select)
* `Enum with `[Flags]` (=> multiselect)
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
|AddHtmlData|Will be rendered as HTML|
|AddTextData|Just plain text|
|AddSerializedData|Two variants of this method exists. Use the extension method variant unless you want to provide your own serializer implementation. The method simply serializes the given object to json and includes it.|
|AddData|Adds string data and optionally define the type yourself.|

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

### IAuditEventStorage
If a IAuditEventStorage is provided in the controller any test executions/cancellations will be logged to it. This also allows for the audit log interface to be shown.

```csharp
IAuditEventStorage auditEventStorage = new FlatFileAuditEventStorage(HostingEnvironment.MapPath("~/App_Data/AuditEventStorage.json"), maxEventAge: TimeSpan.FromDays(30));
```

### ISiteEventService
If a ISiteEventService is provided in the controller any events will be retrieved from it and can be shown in a overview page. Call this service from other places in the code to register new events.

Test methods can register events if executed through `<TestRunnerService>.ExecuteTests(..)`, a site event service is given, and the `TestResult` from a method includes a `SiteEvent`.

Site events are grouped on `SiteEvent.EventTypeId` and extend their duration when multiple events are registered after each other.

```csharp
ISiteEventService siteEventService = new SiteEventService(new FlatFileSiteEventStorage(HostingEnvironment.MapPath("~/App_Data/SiteEventStorage.json"), maxEventAge: TimeSpan.FromDays(5)));
```