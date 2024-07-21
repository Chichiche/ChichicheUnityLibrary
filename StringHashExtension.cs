using System.Security.Cryptography;
using System.Text;
using Cysharp.Text;

namespace Chichiche
{
    public static class StringHashExtension
    {
        public static string Md5(this string text)
        {
            var inputBytes = Encoding.UTF8.GetBytes(text);
            using var md5 = MD5.Create();
            var hashedBytes = md5.ComputeHash(inputBytes);
            using var builder = ZString.CreateStringBuilder(true);
            for (var i = 0; i < hashedBytes.Length; i++)
            {
                builder.Append(hashedBytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}