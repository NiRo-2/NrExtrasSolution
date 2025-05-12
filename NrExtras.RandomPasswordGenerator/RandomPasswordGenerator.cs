using System.Security.Cryptography;

namespace NrExtras.RandomPasswordGenerator
{
    public static class RandomPasswordGenerator
    {
        private static readonly string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
        private static readonly string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly string Digits = "0123456789";
        private static readonly string Symbols = "!@#$%^&*()-_=+[]{}|;:',.<>?/";

        // Define default min and max password lengths.
        private static int DEFAULT_MIN_PASSWORD_LENGTH = 8;
        private static int DEFAULT_MAX_PASSWORD_LENGTH = 10;

        /// <summary>
        /// Generate 512 bit pass
        /// </summary>
        /// <returns>random password</returns>
        public static string Generate512BitPassword()
        {
            return Generate(64);
        }
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
        /// <param name="useLowercaseLetters">true by default</param>
        /// <param name="useUppercaseLetters">true by default</param>
        /// <param name="useDigits">true by default</param>
        /// <param name="useSymbols">true by default</param>
        /// <returns>random generated pass</returns>
        public static string Generate(int length, bool useLowercaseLetters = true, bool useUppercaseLetters = true, bool useDigits = true, bool useSymbols = true)
        {
            return Generate(length, length,useLowercaseLetters,useUppercaseLetters,useDigits,useSymbols);
        }

        /// <summary>
        /// Generate a random password with a specified length and character set.
        /// </summary>
        /// <param name="min">min chars count</param>
        /// <param name="max">max chars count</param>
        /// <param name="useLowercaseLetters">true by default</param>
        /// <param name="useUppercaseLetters">true by default</param>
        /// <param name="useDigits">true by default</param>
        /// <param name="useSymbols">true by default</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception">incase min is not equal or bigger then max</exception>
        /// 
        public static string Generate(int min, int max,bool useLowercaseLetters = true,bool useUppercaseLetters = true,bool useDigits = true,bool useSymbols = true)
        {
            string chars = string.Empty;
            if (useLowercaseLetters)
                chars += LowercaseLetters;
            if (useUppercaseLetters)
                chars += UppercaseLetters;
            if (useDigits)
                chars += Digits;
            if (useSymbols)
                chars += Symbols;

            if (string.IsNullOrEmpty(chars))
                throw new ArgumentException("At least one character set must be selected.");

            try
            {
                // Validate the min and max values
                if (min > max)
                    throw new ArgumentException("Minimum length must be equal to or smaller than maximum length.");

                int length = RandomNumberGenerator.GetInt32(min, max + 1); // max is exclusive
                byte[] randomBytes = new byte[length];
                RandomNumberGenerator.Fill(randomBytes);

                char[] result = new char[length];
                for (int i = 0; i < length; i++)
                {
                    int index = randomBytes[i] % chars.Length;
                    result[i] = chars[index];
                }

                return new string(result);
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating password: " + ex.Message, ex);
            }
        }
    }
}