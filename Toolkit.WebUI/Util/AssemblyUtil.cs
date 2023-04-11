using System.Reflection;

namespace QoDL.Toolkit.WebUI.Util
{
    /// <summary>
    /// Utilities related to the <see cref="Assembly"/>.
    /// </summary>
    public static class AssemblyUtil
    {
        /// <summary>
        /// Attempts to retrieve the entry assembly in ASP.net applications.
        /// </summary>
        public static Assembly GetWebEntryAssembly()
        {
#if NETFULL
            if (System.Web.HttpContext.Current == null ||
                System.Web.HttpContext.Current.ApplicationInstance == null)
            {
                return null;
            }

            var type = System.Web.HttpContext.Current.ApplicationInstance.GetType();
            while (type != null && type.Namespace == "ASP")
            {
                type = type.BaseType;
            }

            return type?.Assembly;
#elif NETCORE
            return Assembly.GetEntryAssembly();
#else
            return null;
#endif
        }
    }
}
