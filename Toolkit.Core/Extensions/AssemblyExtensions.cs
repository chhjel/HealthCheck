using System.Linq;
using System.Reflection;

namespace QoDL.Toolkit.Core.Extensions
{
    /// <summary>
    /// Extensions related to <see cref="Assembly"/>s.
    /// </summary>
    public static class AssemblyExtensions
    {
        /// <summary>
        /// The first part of <see cref="Assembly.FullName"/>.
        /// </summary>
        public static string ShortName(this Assembly assembly)
            => assembly?.FullName?.Split(',')?.FirstOrDefault();
    }
}
