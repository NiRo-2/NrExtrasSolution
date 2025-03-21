﻿using MaxMind.GeoIP2;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace NrExtras.NetAddressUtils
{
    public static class IpHostData
    {
        private const string dbFileName = "GeoLite2-Country.mmdb";
        private static readonly string _dbPath = Path.Combine(AppContext.BaseDirectory, dbFileName);

        /// <summary>
        /// Get country from IP address using a local GeoIP database
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns>Country name or "N/A" if not found</returns>
        public static string GetCountryFromIpAddress(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return "N/A";

            if (!IPAddress.TryParse(ipAddress, out var ip))
                return "N/A";

            try
            {
                //if the database is not found, download it from MaxMind
                if (File.Exists(_dbPath) == false)
                {
                    Console.WriteLine("GeoLite2 database not found. Please update at https://www.maxmind.com/en/accounts/1027697/geoip/downloads - choose GeoLite2 Country GeoIP2 Binary (.mmdb)");
                    throw new Exception($"{dbFileName} not found.");
                }
                else
                    using (var reader = new DatabaseReader(_dbPath))
                    {
                        // Check if the database is outdated - auto download and update the database
                        if (reader.Metadata.BuildDate < DateTime.Now.AddMonths(-6))
                            Console.WriteLine("GeoLite2 database is older than 6 months. Please update at https://www.maxmind.com/en/accounts/1027697/geoip/downloads - choose GeoLite2 Country GeoIP2 Binary (.mmdb)");

                        return reader.Country(ip)?.Country?.Name ?? "N/A";
                    }
            }
            catch
            {
                return "N/A";
            }
        }

        /// <summary>
        /// Get client IP address from HttpContext
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns>Found IP address, or an empty string if no IP address found</returns>
        public static string GetIpAddressFromHttpContext(HttpContext httpContext)
        {
            return httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Get hostname from HttpContext
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns>Hostname, or the IP address if DNS resolution fails</returns>
        public static string GetHostnameFromHttpContext(HttpContext httpContext)
        {
            var ipAddress = httpContext.Connection.RemoteIpAddress;

            if (ipAddress == null)
                return string.Empty;

            try
            {
                return Dns.GetHostEntry(ipAddress).HostName;
            }
            catch
            {
                return ipAddress.ToString();
            }
        }

        /// <summary>
        /// Get organized data on host from HttpContext object, including country
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns>String representing the host, including IP address, hostname, and country</returns>
        public static string GetHostDataFromHttpContext(HttpContext httpContext)
        {
            var ipAddress = GetIpAddressFromHttpContext(httpContext);
            var country = GetCountryFromIpAddress(ipAddress);
            var hostname = GetHostnameFromHttpContext(httpContext);

            return $"IP: {ipAddress}, Hostname: {hostname}, Country: {country}";
        }
    }
}