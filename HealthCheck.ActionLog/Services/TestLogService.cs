#if NETFULL
using HealthCheck.ActionLog.Abstractions;
using HealthCheck.ActionLog.Enums;
using HealthCheck.ActionLog.Util;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Attributes;
using HealthCheck.Core.Modules.ActionsTestLog.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HealthCheck.ActionLog.Services
{
    /// <summary>
    /// Handles the request testlog.
    /// </summary>
    public class TestLogService : ITestLogService
    {
        private TestLogServiceOptions Options { get; set; }
        private ITestLogStorage Store { get; }

        /// <summary>
        /// Handles the request testlog.
        /// </summary>
        public TestLogService(ITestLogStorage storage, TestLogServiceOptions options)
        {
            Options = options;
            Store = storage;
        }

        /// <summary>
        /// Handle a new event.
        /// </summary>
        public void HandleActionEvent(LogFilterEvent e)
        {
            if (e.ActionMethod != null)
            {
                var attr = e.ActionMethod.GetCustomAttribute<ActionsTestLogInfoAttribute>(true);
                if (attr?.Hidden == true)
                {
                    return;
                }
            }

            var endpointId = CreateEndpointId(e.ControllerType, e.ActionMethod, e.Action);
            var entry = Store.GetEntryWithEndpointId(endpointId);

            if (entry == null)
            {
                entry = CreateNewEntry(e, endpointId);
                Store.InsertOrUpdate(entry);
                return;
            }

            var entryEvent = CreateNewRequest(e);
            var targetList = entryEvent.IsSuccess ? entry.Calls : entry.Errors;
            var targetListLimit = entryEvent.IsSuccess ? Options.MaxCallCount : Options.MaxErrorCount;
            var limitBehaviour = entryEvent.IsSuccess ? Options.CallStoragePolicy : Options.ErrorStoragePolicy;
            var updateEntry = false;

            // Ensure data is up to date
            if (string.IsNullOrWhiteSpace(entry.Url))
            {
                entry.Url = e.Url?.Split('?')?[0]?.Split('#')?[0];
                updateEntry = true;
            }
            if (string.IsNullOrWhiteSpace(entry.HttpVerb))
            {
                entry.HttpVerb = e.RequestMethod;
                updateEntry = true;
            }

            // Remove oldest item if full and behaviour is RemoveOldest
            if (targetList.Count >= targetListLimit && limitBehaviour == TestLogCallStoragePolicy.RemoveOldest)
            {
                updateEntry = true;
                targetList.RemoveAt(0);
            }

            // Add new item if not full
            if (targetList.Count < targetListLimit)
            {
                updateEntry = true;
                targetList.Add(entryEvent);
            }

            if (updateEntry)
            {
                Store.InsertOrUpdate(entry);
            }
        }

        /// <summary>
        /// Returns factory defined in <see cref="Options"/>.
        /// </summary>
        public Func<Type, string> GetControllerGroupNameFactory()
            => Options.ControllerGroupNameFactory;

        /// <summary>
        /// Store the given entry.
        /// </summary>
        public void StoreAction(LoggedEndpointDefinition entry)
            => Store.Insert(entry);

        /// <summary>
        /// Get all stored actions.
        /// </summary>
        public List<LoggedEndpointDefinition> GetActions()
            => Store.GetAll();

        /// <summary>
        /// Clear all stored actions.
        /// </summary>
        public async Task ClearActions()
            => await Store.ClearAll();

        /// <summary>
        /// Get the current application version.
        /// </summary>
        public string GetCurrentVersion()
        {
            if (!string.IsNullOrWhiteSpace(Options.ApplicationVersion))
            {
                return Options.ApplicationVersion;
            }

            var assembly = AssemblyUtil.GetWebEntryAssembly()
                ?? Assembly.GetExecutingAssembly();
            var fvi = (assembly == null) ? "1.0.0.0" : FileVersionInfo.GetVersionInfo(assembly.Location)?.FileVersion;
            return fvi ?? assembly.GetName().Version.ToString(); ;
        }

        /// <summary>
        /// Create an id for the given endpoint.
        /// </summary>
        public string CreateEndpointId(Type controllerType, MethodInfo actionMethod, string actionName)
            => $"{controllerType?.FullName ?? "nullController"}=>{(actionMethod?.ToString() ?? actionName)?.Replace(" ", "_") ?? "nullAction"}".ToLower();

        private LoggedEndpointDefinition CreateNewEntry(LogFilterEvent e, string endpointId)
        {
            var info = CreateActionInfo(e.ControllerType, e.Action, e.ActionMethod);
            var entry = new LoggedEndpointDefinition()
            {
                EndpointId = endpointId,
                Name = info.Name,
                Description = info.Description,
                Group = Options.ControllerGroupNameFactory?.Invoke(e.ControllerType),
                ControllerType = typeof(Controller).IsAssignableFrom(e.ControllerType) ? "MVC" : "Web API",
                Controller = e.Controller,
                FullControllerName = e.ControllerType.FullName,
                Action = e.Action,
                Url = e.Url,
                HttpVerb = e.RequestMethod
            };

            var entryEvent = CreateNewRequest(e);
            if (entryEvent.IsSuccess)
            {
                entry.Calls.Add(entryEvent);
            }
            else
            {
                entry.Errors.Add(entryEvent);
            }

            return entry;
        }

        private LoggedEndpointRequest CreateNewRequest(LogFilterEvent e)
        {
            var request = new LoggedEndpointRequest()
            {
                Url = e.Url,
                Timestamp = DateTime.Now,
                Version = GetCurrentVersion(),
                StatusCode = e.StatusCode,
                ErrorDetails = e.Exception?.ToString(),
                SourceIP = e.SourceIP
            };
            return request;
        }

        private ActionInfo CreateActionInfo(Type controller, string action, MethodInfo actionMethod)
        {
            actionMethod = actionMethod ?? controller.GetMethods().FirstOrDefault(x => x.Name == action);
            var infoAttribute = actionMethod?.GetCustomAttribute<ActionsTestLogInfoAttribute>();
            return new ActionInfo()
            {
                Name = !string.IsNullOrWhiteSpace(infoAttribute?.Name)
                    ? infoAttribute?.Name
                    : (action ?? "Unknown action"),
                Description = infoAttribute?.Description,
                Url = infoAttribute?.Url
            };
        }
    }
}
#endif
