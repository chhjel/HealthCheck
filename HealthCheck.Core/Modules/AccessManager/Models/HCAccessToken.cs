﻿using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.AccessManager.Models
{
    /// <summary>
    /// A generated access token.
    /// </summary>
    public class HCAccessToken
    {
        /// <summary>
        /// Unique id for this token.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name that was given to the token.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Hash of the token value.
        /// </summary>
        public string HashedToken { get; set; }

        /// <summary>
        /// Salt for the hashed token.
        /// </summary>
        public string TokenSalt { get; set; }

        /// <summary>
        /// Expiration date if any.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Roles the token will be granted.
        /// </summary>
        public List<string> Roles { get; set; }

        /// <summary>
        /// Modules the token has access to.
        /// </summary>
        public List<HCAccessTokenModuleData> Modules { get; set; }
    }

    /// <summary>
    /// Describes what module access options a token has access to.
    /// </summary>
    public class HCAccessTokenModuleData
    {
        /// <summary>
        /// Target module.
        /// </summary>
        public string ModuleId { get; set; }

        /// <summary>
        /// Access options for the module.
        /// </summary>
        public List<string> Options { get; set; }
    }
}
