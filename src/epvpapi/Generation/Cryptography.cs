using System;
using System.Security.Cryptography;
using System.Text;

namespace epvpapi.Generation
{
    public class Cryptography
    {
        /// <summary>
        /// Gets an md5 hash of the given string
        /// </summary>
        /// <param name="str"> The string which will be hashed </param>
        /// <returns> The md5 of the hashed string </returns>
        public static string GetMD5(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            using (var md5 = new MD5CryptoServiceProvider())
                bytes = md5.ComputeHash(bytes);
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}
