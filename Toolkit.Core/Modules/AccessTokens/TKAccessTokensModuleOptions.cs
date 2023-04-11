using QoDL.Toolkit.Core.Modules.AccessTokens.Abstractions;

namespace QoDL.Toolkit.Core.Modules.AccessTokens
{
    /// <summary>
    /// Options for <see cref="TKAccessTokensModule"/>.
    /// </summary>
    public class TKAccessTokensModuleOptions
    {
        /// <summary>
        /// Provides storage for generated access tokens.
        /// </summary>
        public IAccessManagerTokenStorage TokenStorage { get; set; }
    }
}
