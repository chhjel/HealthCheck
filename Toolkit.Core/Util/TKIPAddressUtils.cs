using QoDL.Toolkit.Core.Models;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace QoDL.Toolkit.Core.Util
{
	/// <summary>
	/// IP address related utils.
	/// </summary>
	public static class TKIPAddressUtils
	{
		/// <summary>
		/// Parse the given ip address and strips away any port number.
		/// <para>Supports IPv4 and IPv6 with or without port numbers.</para>
		/// </summary>
		public static string StripAnyPortNumber(string ip)
			=> ParseIP(ip)?.IP;

		/// <summary>
		/// Parse the given ip address.
		/// <para>Supports IPv4 and IPv6 with or without port numbers.</para>
		/// </summary>
		public static TKIPData ParseIP(string ip, bool acceptLocalhostString = false)
		{
			var data = new TKIPData()
			{
				AddressFamily = AddressFamily.Unknown
			};
			if (ip == null) return data;
			else if (ip?.ToLower() == "localhost" && acceptLocalhostString)
			{
				data.IP = ip;
				return data;
			}

            if (IPAddress.TryParse(ip, out IPAddress address))
			{
				data.AddressFamily = address.AddressFamily;
			}

            static int? parsePort(string rawPort)
			{
				if (!int.TryParse(rawPort, out var num)
					|| num < IPEndPoint.MinPort || num > IPEndPoint.MaxPort)
				{
					return null;
				}
				return num;
			}

			var ipWithoutPort = ip;
			if (ip.Contains(":"))
			{
				// IPv4
				if (data.AddressFamily == AddressFamily.Unknown)
				{
					var rawPort = ip.Split(':').LastOrDefault();
					var port = parsePort(rawPort);
					if (port != null)
					{
						data.PortNumber = port;
						ipWithoutPort = ipWithoutPort.Substring(0, ipWithoutPort.Length - rawPort.Length - 1);
					}
				}
				// IPv6
				else if (data.AddressFamily == AddressFamily.InterNetworkV6
					&& ip.Contains("]:"))
				{
					var rawPort = ip.Split(':').LastOrDefault();
					var port = parsePort(rawPort);
					if (port != null)
					{
						data.PortNumber = port;
						ipWithoutPort = ipWithoutPort.Substring(1);
						ipWithoutPort = ipWithoutPort.Substring(0, ipWithoutPort.Length - rawPort.Length - 2);
					}
				}
			}

			if (IPAddress.TryParse(ipWithoutPort, out address))
			{
				data.AddressFamily = address.AddressFamily;
				data.IP = address.ToString();
			}

			return data;
		}
	}
}
