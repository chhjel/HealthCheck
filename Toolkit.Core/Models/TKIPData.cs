using QoDL.Toolkit.Core.Util;
using System.Net.Sockets;

namespace QoDL.Toolkit.Core.Models
{
    /// <summary>
    /// Contains parsed IP information using <see cref="TKIPAddressUtils.ParseIP"/>.
    /// </summary>
    public class TKIPData
	{
		/// <summary>
		/// Type of address.
		/// </summary>
		public AddressFamily AddressFamily { get; set; }

		/// <summary>
		/// IP address without any port number.
		/// </summary>
		public string IP { get; set; }

		/// <summary>
		/// Port number if any.
		/// </summary>
		public int? PortNumber { get; set; }

		/// <summary>
		/// True if the ip is 127.0.0.1, localhost or ::1.
		/// </summary>
		public bool IsLocalHost
			=> IP == "127.0.0.1"
			|| IP == "localhost"
			|| IP == "::1";

		/// <summary>
		/// IP including any port number.
		/// </summary>
		public override string ToString()
		{
			var portPart = PortNumber != null ? $":{PortNumber}" : "";
			return $"{IP}{portPart}";
		}
	}
}
