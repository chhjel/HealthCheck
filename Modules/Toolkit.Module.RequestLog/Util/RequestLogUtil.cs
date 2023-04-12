#if NETFULL
using QoDL.Toolkit.RequestLog.Abstractions;
using QoDL.Toolkit.RequestLog.Attributes;
using QoDL.Toolkit.RequestLog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web.Http;
using System.Web.Mvc;

namespace QoDL.Toolkit.Module.RequestLog.Util;

/// <summary>
/// Utils related to the <see cref="IRequestLogService"/>.
/// </summary>
public static class RequestLogUtil
{
    /// <summary>
    /// Find all MVC and WebApi controllers in the given assemblies and create entries from any not already existing.
    /// </summary>
    /// <param name="service">Store in this service.</param>
    /// <param name="assembliesWithControllers">Assemblies that contain controllers.</param>
    /// <param name="actionUrlResolver">Resolve url from the given controller type and action name.</param>
    /// <param name="includeMvc">Include MVC controllers</param>
    /// <param name="includeWebApi">Include WebApi controllers</param>
    /// <param name="includeWebForms">Include WebForms pages</param>
    /// <param name="webFormsTypeFilter">Optionally filter webforms pages</param>
    public static void EnsureDefinitionsFromTypes(
        IRequestLogService service,
        IEnumerable<Assembly> assembliesWithControllers,
        Func<Type, string, string> actionUrlResolver = null,
        bool includeMvc = true, bool includeWebApi = true, bool includeWebForms = true,
        Func<Type, bool> webFormsTypeFilter = null)
    {
        var controllerTypes = assembliesWithControllers
           .SelectMany(x => x.GetTypes().Where(t =>
                (includeMvc && typeof(Controller).IsAssignableFrom(t)) || (includeWebApi && typeof(ApiController).IsAssignableFrom(t)))
           )
           .Where(x => !x.IsAbstract && GetRequestLogInfoAttribute(x)?.Hidden != true);

        var existingActionEntries = service.GetRequests();
        foreach (var actionEntry in controllerTypes.SelectMany(x => GetControllerActions(service, x, actionUrlResolver)))
        {
            if (existingActionEntries.Any(x => x.EndpointId == actionEntry.EndpointId))
            {
                continue;
            }

            service.StoreRequest(actionEntry);
        }

        if (includeWebForms)
        {
            var webformsTemplates = assembliesWithControllers
                .SelectMany(x => x.ExportedTypes)
                .Where(x => typeof(System.Web.UI.Page).IsAssignableFrom(x) && !x.IsAbstract)
                .Where(x => webFormsTypeFilter?.Invoke(x) != false)
                .Select(x =>
                    new LoggedEndpointDefinition()
                    {
                        EndpointId = service.CreateEndpointId(x, null, "PageLoad"),
                        Name = x.Name.Split('_').Last(),
                        Description = null,
                        Group = service.GetControllerGroupNameFactory()?.Invoke(x),
                        ControllerType = "WebForms",
                        Controller = x.Name,
                        FullControllerName = x.FullName,
                        Action = "PageLoad",
                        Url = null,
                        HttpVerb = "GET"
                    }
                );

            foreach(var template in webformsTemplates)
            {
                if (existingActionEntries.Any(x => x.EndpointId == template.EndpointId))
                {
                    continue;
                }

                service.StoreRequest(template);
            }
        }
    }

    private static List<LoggedEndpointDefinition> GetControllerActions(IRequestLogService service, Type controllerType, Func<Type, string, string> actionUrlResolver = null)
    {
        var actionMethods = GetActionMethods(controllerType)
            .Select(x => new
            {
                ControllerType = controllerType,
                Controller = controllerType.Name,
                //DeclaringControllerType = x.DeclaringType,
                Action = x.Name,
                ActionMethod = x,
                ReturnType = x.ReturnType.Name
            })
            .OrderBy(x => x.Controller)
            .ThenBy(x => x.Action)
            .ToList();

        var isMvc = typeof(Controller).IsAssignableFrom(controllerType);
        return actionMethods
            .Select(x =>
            {
                var info = CreateActionInfo(x.ControllerType, x.Action, x.ActionMethod);
                return new LoggedEndpointDefinition()
                {
                    EndpointId = service.CreateEndpointId(controllerType, x.ActionMethod, x.Action),
                    Name = info.Name,
                    Description = info.Description,
                    Group = service.GetControllerGroupNameFactory()?.Invoke(controllerType),
                    ControllerType = isMvc ? "MVC" : "Web API",
                    Controller = x.Controller,
                    FullControllerName = controllerType.FullName,
                    Action = x.Action,
                    Url = info?.Url ?? actionUrlResolver?.Invoke(controllerType, x.Action),
                    HttpVerb = GetMethodHttpVerb(x.ActionMethod, !isMvc)
                };
            })
            .ToList();
    }

    private static ActionInfo CreateActionInfo(Type controller, string action, MethodInfo actionMethod)
    {
        string fallbackNameGen()
        {
            var prettifiedControllerName = controller?.Name?.Replace("Controller", "");
            return $"{prettifiedControllerName} - {action}";
        }

        actionMethod ??= controller.GetMethods().FirstOrDefault(x => x.Name == action);
        var infoAttribute = GetRequestLogInfoAttribute(actionMethod);
        return new ActionInfo()
        {
            Name = !string.IsNullOrWhiteSpace(infoAttribute?.Name)
                ? infoAttribute?.Name
                : fallbackNameGen(),
            Description = infoAttribute?.Description,
            Url = infoAttribute?.Url
        };
    }

    private static string GetMethodHttpVerb(MethodInfo method, bool isWebApi)
    {
        var mapping = new Dictionary<string, string>
        {
            { nameof(System.Web.Http.HttpGetAttribute), "GET" },
            { nameof(System.Web.Http.HttpDeleteAttribute), "DELETE" },
            { nameof(System.Web.Http.HttpHeadAttribute), "HEAD" },
            { nameof(System.Web.Http.HttpOptionsAttribute), "OPTIONS" },
            { nameof(System.Web.Http.HttpPatchAttribute), "PATCH" },
            { nameof(System.Web.Http.HttpPostAttribute), "POST" },
            { nameof(System.Web.Http.HttpPutAttribute), "PUT" }
        };

        var attributeNames = method.GetCustomAttributes().Select(x => x.GetType().Name);
        var matchingAttributeName = attributeNames.FirstOrDefault(x => mapping.ContainsKey(x));
        if (matchingAttributeName != null)
        {
            return mapping[matchingAttributeName];
        }
        else if (!isWebApi)
        {
            return "GET";
        }

        var methodNames = new[] { "Get", "Delete", "Head", "Options", "Patch", "Post", "Put" };
        var matchingMethodName = methodNames.FirstOrDefault(x => x.ToLower() == method.Name.ToLower());
        if (matchingMethodName != null)
        {
            return matchingMethodName.ToUpper();
        }

        return "GET";
    }

    private static List<MethodInfo> GetActionMethods(Type controllerType)
    {
        List<MethodInfo> list;

        // Web API controllers
        if (typeof(ApiController).IsAssignableFrom(controllerType))
        {
            list = controllerType.GetMethods()
                .Where(t => t.Name != "Dispose" && !t.IsSpecialName && t.DeclaringType.IsSubclassOf(typeof(ApiController)) && t.IsPublic && !t.IsStatic)
                .Where(m => !m.GetCustomAttributes(typeof(CompilerGeneratedAttribute), true).Any())
                .ToList();
        }
        // MVC controllers
        else
        {
            list = new ReflectedControllerDescriptor(controllerType)
                .GetCanonicalActions()
                .Select(x => new
                {
                    x.ActionName,
                    ActionMethod = (x is ReflectedActionDescriptor reflectedAction) ? reflectedAction.MethodInfo : null,
                })
                .Select(x => x.ActionMethod ?? controllerType.GetMethods().FirstOrDefault(m => m.Name == x.ActionName))
                .ToList();
        }

        // Filter out hidden ones
        list = list
            .Where(x => GetRequestLogInfoAttribute(x)?.Hidden != true)
            .ToList();

        return list;
    }

    internal static RequestLogInfoAttribute GetRequestLogInfoAttribute(MethodInfo method)
    {
        if (method == null) return null;

        var attr = method.GetCustomAttribute<RequestLogInfoAttribute>(true);
        if (attr != null) return attr;

        var hasNamedAttr = method.GetCustomAttributes().Any(x => x.GetType().Name == "HideFromRequestLogAttribute");
        if (hasNamedAttr) return new RequestLogInfoAttribute() { Hidden = true };

        return null;
    }

    internal static RequestLogInfoAttribute GetRequestLogInfoAttribute(Type type)
    {
        if (type == null) return null;

        var attr = type.GetCustomAttribute<RequestLogInfoAttribute>(true);
        if (attr != null) return attr;

        var hasNamedAttr = type.GetCustomAttributes().Any(x => x.GetType().Name == "HideFromRequestLogAttribute");
        if (hasNamedAttr) return new RequestLogInfoAttribute() { Hidden = true };

        return null;
    }
}
#endif
