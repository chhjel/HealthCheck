using HealthCheck.Core.Util;
using System.Collections.Generic;

namespace HealthCheck.Core.Models
{
    /// <summary>
    /// Information about the current request.
    /// </summary>
    public class RequestInformation<TAccessRole>
    {
        /// <summary>
        /// Access roles of the current request.
        /// </summary>
        public Maybe<TAccessRole> AccessRole { get; set; }

        /// <summary>
        /// Optional for auditing if enabled.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Optional for auditing if enabled.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Is set automatically to the full url of the request.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Is set automatically to all header keys and values.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Client ip address.
        /// </summary>
        public string ClientIP { get; set; }

        /// <summary>
        /// Create a new <see cref="RequestInformation{TAccessRole}"/> object.
        /// </summary>
        /// <param name="accessRole">Roles the current request has.</param>
        /// <param name="userId">Optional user id reference if any. Optionally used for auditing.</param>
        /// <param name="userName">Optional user name reference if any. Optionally used for auditing.</param>
        public RequestInformation(Maybe<TAccessRole> accessRole, string userId = null, string userName = null)
        {
            AccessRole = accessRole;
            UserId = userId;
            UserName = userName;
        }

        /// <summary>
        /// Create a new <see cref="RequestInformation{TAccessRole}"/> object.
        /// </summary>
        /// <param name="accessRole">Roles the current request has.</param>
        /// <param name="userId">Optional user id reference if any. Optionally used for auditing.</param>
        /// <param name="userName">Optional user name reference if any. Optionally used for auditing.</param>
        public RequestInformation(TAccessRole accessRole, string userId = null, string userName = null)
            : this(new Maybe<TAccessRole>(accessRole), userId, userName) {}
    }
}
