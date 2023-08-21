using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace NrExtras.EncryptionHelper
{
    public static class EncryptionHelper
    {
        /// <summary>
        /// Encrypt key using local machine validation
        /// **Note** data encrypted on one machine, cannot be decrypted be another
        /// </summary>
        /// <param name="key">key to encrypt</param>
        /// <param name="optionalEntropy">null be default. Optional extra security</param>
        /// <returns>encrypted key</returns>
        public static string EncryptKey(string key, byte[]? optionalEntropy = null)
        {
            byte[] encryptedData = ProtectedData.Protect(Encoding.UTF8.GetBytes(key), optionalEntropy, DataProtectionScope.LocalMachine);
            return Convert.ToBase64String(encryptedData);
        }
        /// <summary>
        /// Decrypt key using local machine validtion
        /// </summary>
        /// <param name="encryptedKey">encrypted key to decrypt</param>
        /// <param name="optionalEntropy">null be default. incase this encrypted data have extra security</param>
        /// <returns>decrypted key</returns>
        public static string DecryptKey(string encryptedKey, byte[]? optionalEntropy = null)
        {
            //incase of empty encryped key - return empty string
            if (string.IsNullOrEmpty(encryptedKey)) return "";

            byte[] encryptedData = Convert.FromBase64String(encryptedKey);
            byte[] decryptedData = ProtectedData.Unprotect(encryptedData, optionalEntropy, DataProtectionScope.LocalMachine);
            return Encoding.UTF8.GetString(decryptedData);
        }

        #region aes encrypt/decrypt
        /// <summary>
        /// Encrypt string using AES - works with javascript version as well
        /// </summary>
        /// <param name="plainText">string to encrypt</param>
        /// <param name="key">key - must be 128 bit</param>
        /// <returns>encrypted string</returns>
        public static string EncryptStringAES(string plainText, string key)
        {
            var keybytes = Encoding.UTF8.GetBytes(key);
            var iv = Encoding.UTF8.GetBytes(key);
            var encryptedBytes = EncryptStringToBytes_Aes(plainText, keybytes, iv);
            string encrypted = Convert.ToBase64String(encryptedBytes);
            return encrypted;
        }
        private static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length != 16) // 128-bit key size
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }
        /// <summary>
        /// Decode AES string using key - works with javascript version as well
        /// </summary>
        /// <param name="encryptedValue">encoded string</param>
        /// <param name="key">decryption key - must be 128 bit</param>
        /// <returns>decoded string</returns>
        public static string DecryptStringAES(string encryptedValue, string key)
        {
            var keybytes = Encoding.UTF8.GetBytes(key);
            var iv = Encoding.UTF8.GetBytes(key);
            //DECRYPT FROM CRIPTOJS
            var encrypted = Convert.FromBase64String(encryptedValue);
            var decryptedFromJavascript = DecryptStringFromBytes_Aes(encrypted, keybytes, iv);
            return decryptedFromJavascript;
        }
        private static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length != 16) // 128-bit key size
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        /// <summary>
        /// Encrypt object
        /// </summary>
        /// <param name="obj">object to encrypt</param>
        /// <param name="key">encryptiong key - must be 128 bit</param>
        /// <returns>encrypted string (json object)</returns>
        public static string EncryptObjectAES(object obj, string key)
        {
            //conert to json, encrypt and return encrypted string
            return EncryptStringAES(JsonConvert.SerializeObject(obj), key);
        }

        /// <summary>
        /// Decrypt object from encrypted string
        /// </summary>
        /// <param name="encrypted">encrypted string</param>
        /// <param name="key">decryptiong key - must be 128 bit</param>
        /// <returns>decrypted object</returns>
        public static object DecryptStringToObjectAES(string encrypted, string key)
        {
            try
            {
                string json = DecryptStringAES(encrypted, key); //decrypt
                var obj = JsonConvert.DeserializeObject<object>(json); //deserialize
                if (obj == null) throw new Exception("Error deserializing object"); //error deserializing
                return obj; //return found object
            }
            catch
            {
                throw;
            }
        }
        #endregion
    }
}