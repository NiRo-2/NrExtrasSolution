using System.Net;

namespace NrExtras.NetAddressUtils
{
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
