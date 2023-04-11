using EPiServer.Data;
using EPiServer.Data.Dynamic;

namespace QoDL.Toolkit.Episerver.Models
{
    /// <summary>
    /// Model used for storing last checked deployed version.
    /// </summary>
    [EPiServerDataStore(AutomaticallyCreateStore = false, AutomaticallyRemapStore = true)]
    public class TKLastCheckedVersionData
    {
        /// <summary>
        /// Epi generated id.
        /// </summary>
        public Identity Id { get; set; }

        /// <summary>
        /// Last version number that was checked.
        /// </summary>
        public string LastCheckedVersion { get; set; }
    }
}
