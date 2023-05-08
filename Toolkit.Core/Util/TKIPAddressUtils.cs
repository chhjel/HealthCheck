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
    /// Checks if the given IPv4 or IPv6 matches another value or is within its given CIDR range. Throws an exception if the IP is invalid.
    /// </summary>
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
    /// <para>Supports IPv4 and IPv6.</para>
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static bool IpAddressIsInRange(string ipAddress, string cidrNotation)
    {
        if (string.IsNullOrEmpty(ipAddress))
        {
            throw new ArgumentException("Input string must not be null", ipAddress);
        }

        var parsedIPv4Addresses = ParseIPv4Addresses(ipAddress);
        if (parsedIPv4Addresses.Any())
        {
            return IPv4AddressIsInRange(parsedIPv4Addresses[0], cidrNotation);
        }
        else
        {
            return IPv6AddressIsInRange(ipAddress, cidrNotation);
        }
    }

    /// <summary>
    /// Checks if the given IP is within the given CIDR, throwing an exception if the IP is invalid.
    /// <para>Supports IPv4 and IPv6.</para>
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static bool IpAddressIsInRange(IPAddress ipAddress, string cidrNotation)
    {
        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            return IPv6AddressIsInRange(ipAddress.ToString(), cidrNotation);
        }
        else
        {
            return IPv4AddressIsInRange(ipAddress, cidrNotation);
        }
    }

    /// <summary>
    /// Checks if the given IPv4 is within the given CIDR, throwing an exception if the IP is invalid.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static bool IPv4AddressIsInRange(IPAddress ipAddress, string cidrNotation)
    {
        if (string.IsNullOrEmpty(cidrNotation))
        {
            throw new ArgumentException("Input string must not be null", cidrNotation);
        }

        var cidrAddress = ParseIPv4Addresses(cidrNotation)[0];

        var parts = cidrNotation.Split('/');
        if (parts.Length != 2)
        {
            throw new FormatException($"cidrMask was not in the correct format:\nExpected: a.b.c.d/n\nActual: {cidrNotation}");
        }

        if (!int.TryParse(parts[1], out var netmaskBitCount))
        {
            throw new FormatException($"Unable to parse netmask bit count from {cidrNotation}");
        }

        if (0 > netmaskBitCount || netmaskBitCount > 32)
        {
            throw new ArgumentOutOfRangeException($"Netmask bit count value of {netmaskBitCount} is invalid, must be in range 0-32");
        }

        var ipAddressBytes = BitConverter.ToInt32(ipAddress.GetAddressBytes(), 0);
        var cidrAddressBytes = BitConverter.ToInt32(cidrAddress.GetAddressBytes(), 0);
        var cidrMaskBytes = IPAddress.HostToNetworkOrder(-1 << (32 - netmaskBitCount));

        var ipIsInRange = (ipAddressBytes & cidrMaskBytes) == (cidrAddressBytes & cidrMaskBytes);
        return ipIsInRange;
    }

    /// <summary>
    /// Checks if the given IPv6 is within the given CIDR, throwing an exception if the IP is invalid.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static bool IPv6AddressIsInRange(string ipAddress, string cidrNotation)
    {
        // Parse the CIDR notation to get the network prefix and subnet mask length
        string[] parts = cidrNotation.Split('/');
        if (parts.Length != 2)
        {
            throw new ArgumentException("Invalid CIDR notation: " + cidrNotation);
        }

        IPAddress networkPrefix = IPAddress.Parse(parts[0]);
        int subnetMaskLength = int.Parse(parts[1]);

        // Make sure the network prefix is an IPv6 address
        if (networkPrefix.AddressFamily != AddressFamily.InterNetworkV6)
        {
            throw new ArgumentException("Invalid IPv6 address: " + networkPrefix);
        }

        // Parse the input IP address
        IPAddress ip = IPAddress.Parse(ipAddress);

        // Make sure the input IP address is an IPv6 address
        if (ip.AddressFamily != AddressFamily.InterNetworkV6)
        {
            throw new ArgumentException("Invalid IPv6 address: " + ipAddress);
        }

        // Convert the IPv6 addresses to byte arrays
        byte[] networkPrefixBytes = networkPrefix.GetAddressBytes();
        byte[] ipBytes = ip.GetAddressBytes();

        // Make sure the byte arrays are the same length
        if (networkPrefixBytes.Length != ipBytes.Length)
        {
            throw new ArgumentException("Invalid CIDR notation or IPv6 address: " + cidrNotation + ", " + ipAddress);
        }

        // Calculate the number of bytes and bits in the subnet mask
        int bytesInSubnetMask = subnetMaskLength / 8;
        int bitsInSubnetMask = subnetMaskLength % 8;

        // Check if the IP address matches the network prefix up to the last byte in the subnet mask
        for (int i = 0; i < bytesInSubnetMask; i++)
        {
            if (networkPrefixBytes[i] != ipBytes[i])
            {
                return false;
            }
        }

        // Check if the last byte in the subnet mask matches up to the specified number of bits
        if (bitsInSubnetMask > 0)
        {
            byte mask = (byte)(0xFF << (8 - bitsInSubnetMask));
            if ((networkPrefixBytes[bytesInSubnetMask] & mask) != (ipBytes[bytesInSubnetMask] & mask))
            {
                return false;
            }
        }

        // If all checks pass, the IP address matches the CIDR notation
        return true;
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
