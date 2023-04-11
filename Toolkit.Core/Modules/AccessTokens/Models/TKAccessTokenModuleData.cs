using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.AccessTokens.Models
{
    /// <summary>
    /// Describes what module access options a token has access to.
    /// </summary>
    public class TKAccessTokenModuleData
    {
        /// <summary>
        /// Target module.
        /// </summary>
        public string ModuleId { get; set; }

        /// <summary>
        /// Access options for the module.
        /// </summary>
        public List<string> Options { get; set; }

        /// <summary>
        /// Categories within the module if any that the token will be limited to.
        /// <para>If null or empty all categories will be available including things without categories.</para>
        /// </summary>
        public List<string> Categories { get; set; }

        /// <summary>
        /// Ids within the module if any that the token will be limited to.
        /// <para>If null or empty all ids will be available.</para>
        /// </summary>
        public List<string> Ids { get; set; }
    }
}
