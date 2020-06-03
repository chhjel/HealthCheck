using HealthCheck.Core.Modules.AccessTokens.Abstractions;
using System;

namespace HealthCheck.Core.Modules.AccessTokens
{
    /// <summary>
    /// Options for <see cref="HCAccessTokensModule"/>.
    /// </summary>
    public class HCAccessTokensModuleOptions
    {
        /// <summary>
        /// Provides storage for generated access tokens.
        /// </summary>
        public IAccessManagerTokenStorage TokenStorage { get; set; }
    }
}
