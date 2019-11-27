﻿using HealthCheck.Core.Modules.ActionsTestLog.Enums;
using System;
using System.Reflection;

namespace HealthCheck.Core.Modules.ActionsTestLog.Models
{
    /// <summary>
    /// Log event data sent from action filters.
    /// </summary>
    public class LogFilterEvent
    {
        /// <summary>
        /// What type of method event was stored from.
        /// </summary>
        public LogFilterMethod FilterMethod { get; set; }

        /// <summary>
        /// Controller type the event was stored from.
        /// </summary>
        public Type ControllerType { get; set; }

        /// <summary>
        /// Controller name the event was stored from.
        /// </summary>
        public string Controller { get; set; }

        /// <summary>
        /// Action type the event was stored from.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Action method info the event was stored from.
        /// </summary>
        public MethodInfo ActionMethod { get; set; }

        /// <summary>
        /// HTTP verb.
        /// </summary>
        public string RequestMethod { get; set; }

        /// <summary>
        /// Requested URL.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Returned status code if any.
        /// </summary>
        public string StatusCode { get; set; }

        /// <summary>
        /// Result data.
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// Exception if any.
        /// </summary>
        public Exception Exception { get; set; }
    }
}
