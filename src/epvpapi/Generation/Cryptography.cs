using System;
using System.Security.Cryptography;
using System.Text;

namespace epvpapi.Generation
{
    public static class Cryptography
    {
        public static string GetMd5(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            using (var md5 = new MD5CryptoServiceProvider())
                bytes = md5.ComputeHash(bytes);
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}