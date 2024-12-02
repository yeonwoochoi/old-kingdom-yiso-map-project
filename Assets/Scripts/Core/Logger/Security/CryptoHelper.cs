using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Core.Logger.Security {
    public class CryptoHelper {
        private readonly string key;
        private readonly byte[] iv = new byte[16];

        public CryptoHelper(string key) {
            this.key = key;
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(key));
            Array.Copy(hash, iv, 16);
        }

        public string Encrypt(string plainText) {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key).Take(32).ToArray();
            aes.IV = iv;

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using var swEncrypt = new StreamWriter(csEncrypt);
            swEncrypt.Write(plainText);
            return Convert.ToBase64String(msEncrypt.ToArray());
        }

        public string Decrypt(string cipherText) {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key).Take(32).ToArray();
            aes.IV = iv;
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText));
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            return srDecrypt.ReadToEnd();
        }
    }
}