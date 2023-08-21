namespace NrExtras.PassHash_Helper
{
    public class PassHash_Helper
    {
        /// <summary>
        /// Hash password
        /// </summary>
        /// <param name="pass">plain pass</param>
        /// <returns>hashed pass</returns>
        public static string HashPassword(string pass) => BCrypt.Net.BCrypt.HashPassword(pass);
        /// <summary>
        /// verify hash vs pass
        /// </summary>
        /// <param name="pass">plain pass</param>
        /// <param name="passwordHash">hashed pass</param>
        /// <returns>true if pass is the same, false otherwise</returns>
        public static bool VerifyHashVsPass(string pass, string passwordHash) => BCrypt.Net.BCrypt.Verify(pass, passwordHash);
    }
}