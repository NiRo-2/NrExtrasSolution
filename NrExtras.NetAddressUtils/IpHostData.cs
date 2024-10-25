using MaxMind.GeoIP2;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace NrExtras.NetAddressUtils
{
    public static class IpHostData
    {
        private static readonly string _dbPath = Path.Combine(AppContext.BaseDirectory, "GeoLite2-Country.mmdb");

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
                using (var reader = new DatabaseReader(_dbPath))
                {
                    // Check if the database is outdated
                    if (reader.Metadata.BuildDate < DateTime.Now.AddMonths(-6))
                        Console.WriteLine("GeoLite2 database is older than 6 months. Please update at https://www.maxmind.com/en/accounts/1027697/geoip/downloads - choose GeoLite2 Country GeoIP2 Binary (.mmdb)");

                    var country = reader.Country(ip);
                    return country?.Country?.Name ?? "N/A";
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
            var ipAddress = httpContext.Connection.RemoteIpAddress;
            return ipAddress?.ToString() ?? string.Empty;
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
            {
                return string.Empty;
            }

            try
            {
                var hostEntry = Dns.GetHostEntry(ipAddress);
                return hostEntry.HostName;
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