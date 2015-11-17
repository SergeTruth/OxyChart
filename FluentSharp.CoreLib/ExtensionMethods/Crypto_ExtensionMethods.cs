using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace FluentSharp.CoreLib
{
    public static class Crypto_ExtensionMethods
    {
        //Random numbers
        public static Random _random = new Random((int)DateTime.Now.Ticks);                     
        public static int random(this int maxValue)
        {
            return _random.Next(maxValue);
        }        
        public static string randomString(this int size)
        {
            // inspired from the accepted answer from http://stackoverflow.com/questions/1122483/c-random-string-generator            
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                var intValue = Convert.ToInt32(Math.Floor(93 * _random.NextDouble() + 33));  // gets a ASCII value from 33 till 126		        	
                var ch = Convert.ToChar(intValue);
                stringBuilder.Append(ch);
            }
            return stringBuilder.ToString();
        }
        
        public static string randomChars(this int size)
        {                        
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                var next = (byte) 255.random();
                stringBuilder.Append(next.@char());
            }
            return stringBuilder.ToString();
        }
        public static byte[] randomBytes(this int size)
        {                        
            var bytes = new List<Byte>();
            for (int i = 0; i < size; i++)
                bytes.Add((byte) 255.random());
            return bytes.toArray();
        }
        public static string randomNumbers(this int size)
        {
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                var intValue = Convert.ToInt32(Math.Floor(10 * _random.NextDouble() + 48));  // gets a ASCII value from 33 till 126		        	
                var ch = Convert.ToChar(intValue);
                stringBuilder.Append(ch);
            }
            return stringBuilder.ToString();
        }
        public static string randomLetters(this int size)
        {
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                var intValue = Convert.ToInt32(Math.Floor(26 * _random.NextDouble() + 65));  // gets a ASCII value from 33 till 126
                if (1000.random().isEven())			// if it is an even number
                    intValue += 32;				 	// make it lower case
                var ch = Convert.ToChar(intValue);
                stringBuilder.Append(ch);
            }
            return stringBuilder.ToString();
        }

        // MD5 String
        public static string md5Hash(this string stringToHash)
        {
            if (stringToHash.isNull())
                return "";

            var md5 = MD5.Create();

            var data = md5.ComputeHash(stringToHash.asciiBytes());
            return data.hexString();            
        }
        
        
        //AES
        //based on code sample from: http://msdn.microsoft.com/en-us/library/system.security.cryptography.aes(v=vs.100).aspx
        public static byte[] encrypt_AES(this string plainText, string key, string iv)
        {
            return plainText.encrypt_AES(key.hexStringToByteArray(), iv.hexStringToByteArray());
        }
        public static byte[] encrypt_AES(this string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Declare the streams used
            // to encrypt to an in memory
            // array of bytes.
            MemoryStream msEncrypt = null;
            CryptoStream csEncrypt = null;
            StreamWriter swEncrypt = null;

            // Declare the Aes object
            // used to encrypt the data.
            Aes aesAlg = null;

            // Declare the bytes used to hold the
            // encrypted data.
            //byte[] encrypted = null;

            try
            {
                // Create an Aes object
                // with the specified key and IV.
                aesAlg = Aes.Create();
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                msEncrypt = new MemoryStream();
                csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                swEncrypt = new StreamWriter(csEncrypt);

                //Write all data to the stream.
                swEncrypt.Write(plainText);

            }
            finally
            {
                // Clean things up.

                // Close the streams.
                if (swEncrypt != null)
                    swEncrypt.Close();
                if (csEncrypt != null)
                    csEncrypt.Close();
                if (msEncrypt != null)
                    msEncrypt.Close();

                // Clear the Aes object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            // Return the encrypted bytes from the memory stream.
            return msEncrypt.ToArray();
        }
        public static string decrypt_AES(this byte[] cipherText, string key, string iv)
        {
            return cipherText.decrypt_AES(key.hexStringToByteArray(), iv.hexStringToByteArray());
        }
        public static string decrypt_AES(this byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // TDeclare the streams used
            // to decrypt to an in memory
            // array of bytes.
            MemoryStream msDecrypt = null;
            CryptoStream csDecrypt = null;
            StreamReader srDecrypt = null;

            // Declare the Aes object
            // used to decrypt the data.
            Aes aesAlg = null;

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            try
            {
                // Create an Aes object
                // with the specified key and IV.
                aesAlg = Aes.Create();
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                msDecrypt = new MemoryStream(cipherText);
                csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                srDecrypt = new StreamReader(csDecrypt);

                // Read the decrypted bytes from the decrypting stream
                // and place them in a string.
                plaintext = srDecrypt.ReadToEnd();
            }
            finally
            {
                // Clean things up.

                // Close the streams.
                if (srDecrypt != null)
                    srDecrypt.Close();
                if (csDecrypt != null)
                    csDecrypt.Close();
                if (msDecrypt != null)
                    msDecrypt.Close();

                // Clear the Aes object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            return plaintext;
        }

    }
}
