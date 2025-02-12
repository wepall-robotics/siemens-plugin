using Godot;
using System;
using System.Net;

[GlobalClass]
public partial class NetworkUtils : Resource
{
    /// <summary>
    /// Validates an IP address. The validation is done by parsing the IP as an IPv4 address and
    /// verifying that the parsed address matches the given string exactly.
    /// </summary>
    /// <param name="ip">The IP address to validate.</param>
    /// <returns><c>true</c> if the IP is valid, <c>false</c> otherwise.</returns>
    public static bool ValidateIP(string ip)
    {
        if (IPAddress.TryParse(ip, out var address))
        {
            // Verify that the IP is IPv4 and matches the given string exactly
            bool isValid = address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                        && address.ToString() == ip;

            return isValid;
        }
        return false;
    }
}
