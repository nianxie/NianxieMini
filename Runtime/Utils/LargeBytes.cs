using System;
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace Nianxie.Utils
{
    public class LargeBytes
    {
        public readonly byte[] data;

        public LargeBytes(byte[] _data)
        {
            data = _data;
        }

        public string Md5Base64()
        {
            using (MD5 md5 = MD5.Create())
            {
                // 计算字节数组的哈希值
                byte[] hashBytes = md5.ComputeHash(data);
                return Convert.ToBase64String(hashBytes);
            }
        }

        public string ToUtf8String()
        {
            return Encoding.UTF8.GetString(data);
        }
        public static LargeBytes FromUtf8String(string str)
        {
            var data = Encoding.UTF8.GetBytes(str);
            return new LargeBytes(data);
        }
    }
}