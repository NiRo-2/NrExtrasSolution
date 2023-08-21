using System.Security.Cryptography;
using static NrExtras.Logger.Logger;

namespace NrExtras.RandomPasswordGenerator
{
    public static class RandomPasswordGenerator
    {
        private static readonly Random random = new Random();

        // Define default min and max password lengths.
        private static int DEFAULT_MIN_PASSWORD_LENGTH = 8;
        private static int DEFAULT_MAX_PASSWORD_LENGTH = 10;

        /// <summary>
        /// Generate 256 bit pass
        /// </summary>
        /// <returns>random password</returns>
        public static string Generate256BitPassword()
        {
            return Generate(32);
        }
        /// <summary>
        /// Generate 128 bit pass
        /// </summary>
        /// <returns>random password</returns>
        public static string Generate128BitPassword()
        {
            return Generate(16);
        }
        /// <summary>
        /// Generate random pass using default min and max char length
        /// </summary>
        /// <returns>random password</returns>
        public static string Generate()
        {
            return Generate(DEFAULT_MIN_PASSWORD_LENGTH, DEFAULT_MAX_PASSWORD_LENGTH);
        }
        /// <summary>
        /// Generate password in custom lenght
        /// </summary>
        /// <param name="length">pass length</param>
        /// <returns>random generated pass</returns>
        public static string Generate(int length)
        {
            return Generate(length, length);
        }
        /// <summary>
        /// Generate random pass in length between min and max
        /// </summary>
        /// <param name="min">min chars count</param>
        /// <param name="max">max chars count</param>
        /// <returns>random password</returns>
        /// <exception cref="Exception">incase min is not equal or bigger then max</exception>
        public static string Generate(int min, int max)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()-_=+[]{}|;:',.<>?/";
            if (min > max) throw new ArgumentException("Minimum length must be equal to or smaller than maximum length.");

            try
            {
                int bytesCount = random.Next(min, max);

                using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                {
                    byte[] randomBytes = new byte[bytesCount];
                    rng.GetBytes(randomBytes);

                    string password = string.Empty;
                    for (int i = 0; i < bytesCount; i++)
                    {
                        int index = randomBytes[i] % chars.Length;
                        password = string.Concat(password, chars[index]);
                    }

                    return password;
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Logger.Logger.WriteToLog($"Error generating password: {ex.Message}", LogLevel.Error);

                // Rethrow the exception to allow it to propagate
                throw;
            }
        }
    }
}