using System;
using System.Security.Cryptography;
using System.Text;

namespace epvpapi.Generation
{
    public class Cryptography
    {
        public static string GetMD5(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            using (var md5 = new MD5CryptoServiceProvider())
                bytes = md5.ComputeHash(bytes);
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}
