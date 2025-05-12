using System.Net;

namespace NrExtras.NetAddressUtils
{
    public static class NetworkUtils
    {
        #region Check if url works
        /// <summary>
        /// Results helper class
        /// </summary>
        public class UrlCheckResult
        {
            public bool IsSuccess { get; set; }
            public string? ErrorMessage { get; set; }
        }

        // Set a shared HttpClient with a reasonable timeout
        private static readonly HttpClient client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5) // Customize the timeout here
        };

        /// <summary>
        /// Check if a URL works by sending a HEAD request.
        /// </summary>
        /// <param name="url">url to check</param>
        /// <returns>results of success or error message</returns>
        public static async Task<UrlCheckResult> UrlWorksAsync(string url)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Head, url))
                {
                    HttpResponseMessage response = await client.SendAsync(request);
                    return new UrlCheckResult
                    {
                        IsSuccess = response.IsSuccessStatusCode
                    };
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || !ex.CancellationToken.IsCancellationRequested)
            {
                return new UrlCheckResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Request timed out."
                };
            }
            catch (HttpRequestException ex)
            {
                return new UrlCheckResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"HTTP error: {ex.Message}"
                };
            }
            catch (UriFormatException)
            {
                return new UrlCheckResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid URL format."
                };
            }
        }
        #endregion
    }
    public static class IpAndHost_Helper
    {
        #region Get all ips between IP to IP
        /// <summary>
        /// Get list of all ips between ip to ip
        /// </summary>
        /// <param name="startIp">start ip</param>
        /// <param name="endIp">end ip</param>
        /// <returns>list of ips</returns>
        public static List<IPAddress> GetIpRange(IPAddress startIp, IPAddress endIp)
        {
            List<IPAddress> ipRangeList = new List<IPAddress>();

            // Validate IP range format (start <= end)
            if (startIp.GetAddressBytes()[0] > endIp.GetAddressBytes()[0])
                throw new ArgumentException($"Invalid IP range: {startIp} to {endIp} (Start address should be less than or equal to end address)");

            // Convert start and end IP addresses to integer
            uint start = IpToUint(startIp);
            uint end = IpToUint(endIp);

            // Generate the IP addresses from start to end
            for (uint i = start; i <= end; i++)
                ipRangeList.Add(UintToIp(i));

            return ipRangeList;
        }

        // Convert IPAddress to unsigned int
        private static uint IpToUint(IPAddress ip)
        {
            byte[] bytes = ip.GetAddressBytes();
            Array.Reverse(bytes); // Convert to little-endian
            return BitConverter.ToUInt32(bytes, 0);
        }

        // Convert unsigned int to IPAddress
        private static IPAddress UintToIp(uint ipUint)
        {
            byte[] bytes = BitConverter.GetBytes(ipUint);
            Array.Reverse(bytes); // Convert to big-endian
            return new IPAddress(bytes);
        }
        #endregion
    }
}