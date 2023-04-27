using QoDL.Toolkit.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace QoDL.Toolkit.Core.Util;

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

    /// <summary>
    /// Checks if the given IP matches another value or is within its given CIDR range. Throws an exception if the IP is invalid.
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="ipWithOptionalCidr"></param>
    /// <returns></returns>
    public static bool IpMatchesOrIsWithinCidrRange(string ip, string ipWithOptionalCidr)
    {
        if (ipWithOptionalCidr.Contains("/"))
        {
            return IpAddressIsInRange(ip, ipWithOptionalCidr);
        }
        else return ip?.ToLower() == ipWithOptionalCidr?.ToLower();
    }

    /// <summary>
    /// Checks if the given IP is within the given CIDR, throwing an exception if the IP is invalid.
    /// </summary>
    /// <param name="checkIp"></param>
    /// <param name="cidrIp"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static bool IpAddressIsInRange(string checkIp, string cidrIp)
    {
        if (string.IsNullOrEmpty(checkIp))
        {
            throw new ArgumentException("Input string must not be null", checkIp);
        }

        var ipAddress = ParseIPv4Addresses(checkIp)[0];

        return IpAddressIsInRange(ipAddress, cidrIp);
    }

    /// <summary>
    /// Checks if the given IP is within the given CIDR, throwing an exception if the IP is invalid.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static bool IpAddressIsInRange(IPAddress checkIp, string cidrIp)
    {
        if (string.IsNullOrEmpty(cidrIp))
        {
            throw new ArgumentException("Input string must not be null", cidrIp);
        }

        var cidrAddress = ParseIPv4Addresses(cidrIp)[0];

        var parts = cidrIp.Split('/');
        if (parts.Length != 2)
        {
            throw new FormatException($"cidrMask was not in the correct format:\nExpected: a.b.c.d/n\nActual: {cidrIp}");
        }

        if (!int.TryParse(parts[1], out var netmaskBitCount))
        {
            throw new FormatException($"Unable to parse netmask bit count from {cidrIp}");
        }

        if (0 > netmaskBitCount || netmaskBitCount > 32)
        {
            throw new ArgumentOutOfRangeException($"Netmask bit count value of {netmaskBitCount} is invalid, must be in range 0-32");
        }

        var ipAddressBytes = BitConverter.ToInt32(checkIp.GetAddressBytes(), 0);
        var cidrAddressBytes = BitConverter.ToInt32(cidrAddress.GetAddressBytes(), 0);
        var cidrMaskBytes = IPAddress.HostToNetworkOrder(-1 << (32 - netmaskBitCount));

        var ipIsInRange = (ipAddressBytes & cidrMaskBytes) == (cidrAddressBytes & cidrMaskBytes);
        return ipIsInRange;
    }

    /// <summary>
    /// Parse IP addresses from the given input string.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    public static List<IPAddress> ParseIPv4Addresses(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Input string must not be null", input);
        }

        var ips = new List<IPAddress>();
        try
        {
            foreach (Match match in _ipV4Regex.Matches(input))
            {
                var ip = ParseSingleIPv4Address(match.Value);
                ips.Add(ip);
            }
        }
        catch (RegexMatchTimeoutException) { }

        return ips;
    }
    private static readonly Regex _ipV4Regex = new(@"(?:(?:1\d\d|2[0-5][0-5]|2[0-4]\d|0?[1-9]\d|0?0?\d)\.){3}(?:1\d\d|2[0-5][0-5]|2[0-4]\d|0?[1-9]\d|0?0?\d)");

    /// <summary>
    /// Parses a single IPv4 address string, throwing exceptions if it's not valid.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static IPAddress ParseSingleIPv4Address(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Input string must not be null", input);
        }

        var addressBytesSplit = input.Trim().Split('.').ToList();
        if (addressBytesSplit.Count != 4)
        {
            throw new ArgumentException("Input string was not in valid IPV4 format \"a.b.c.d\"", input);
        }

        var addressBytes = new byte[4];
        foreach (var i in Enumerable.Range(0, addressBytesSplit.Count))
        {
            if (!int.TryParse(addressBytesSplit[i], out var parsedInt))
            {
                throw new FormatException($"Unable to parse integer from {addressBytesSplit[i]}");
            }

            if (0 > parsedInt || parsedInt > 255)
            {
                throw new ArgumentOutOfRangeException($"{parsedInt} not within required IP address range [0,255]");
            }
            addressBytes[i] = (byte)parsedInt;
        }
        return new IPAddress(addressBytes);
    }
}
