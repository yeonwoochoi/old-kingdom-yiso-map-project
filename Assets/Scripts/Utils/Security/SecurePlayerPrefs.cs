using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utils.Security {
    public static class SecurePlayerPrefs {
        private static AES128 aes128 = new ();
        private static JsonSerializerSettings serializerSettings = null;

        public static void Set(string key, object value) {
            serializerSettings ??= new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.All
            };
            
            var jsonValue = JsonConvert.SerializeObject(value, Formatting.Indented, serializerSettings);
            var encryptKey = aes128.Encrypt(key);
            var encryptValue = aes128.Encrypt(jsonValue);
            PlayerPrefs.SetString(encryptKey, encryptValue);
        }

        public static T Get<T>(string key) {
            serializerSettings ??= new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.All
            };
            
            key = aes128.Encrypt(key);
            string value;

            if (!PlayerPrefs.HasKey(key)) {
                return default;
            }

            value = PlayerPrefs.GetString(key, string.Empty);
            if (value == string.Empty) return default;
            value = aes128.Decrypt(value);
            return JsonConvert.DeserializeObject<T>(value, serializerSettings);
        }

        public static bool TryGet<T>(string key, out T value) {
            value = Get<T>(key);
            return value != null;
        }
        
        public static bool HasKey(string key) {
            key = aes128.Encrypt(key);
            return PlayerPrefs.HasKey(key);
        }

        public static void DeleteKey(string key) {
            key = aes128.Encrypt(key);
            PlayerPrefs.DeleteKey(key);
        }

        public static void DeleteAll() {
            PlayerPrefs.DeleteAll();
        }
    }

    public sealed class AES128 {
        private string Key {
            get {
                var md5 = MD5.Create();
                var salt = "";
                var result = md5.ComputeHash(Encoding.UTF8.GetBytes(salt));
                return Encoding.UTF8.GetString(result);
            }
        }

        private readonly RijndaelManaged rijndaelCipher;

        public AES128() {
            rijndaelCipher = new RijndaelManaged {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                KeySize = 128,
                BlockSize = 128
            };
        }

        public string Encrypt(string value) {
            var pwdBytes = Encoding.UTF8.GetBytes(Key);
            var keyBytes = new byte[16];

            var len = pwdBytes.Length;
            if (len > keyBytes.Length) len = keyBytes.Length;
            
            Array.Copy(pwdBytes, keyBytes, len);

            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = keyBytes;

            var transform = rijndaelCipher.CreateEncryptor();

            var plain = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(transform.TransformFinalBlock(plain, 0, plain.Length));
        }

        public string Decrypt(string value) {
            var encryptedData = Convert.FromBase64String(value);
            var pwdBytes = Encoding.UTF8.GetBytes(Key);
            var keyBytes = new byte[16];

            var len = pwdBytes.Length;
            if (len > keyBytes.Length) len = keyBytes.Length;
            
            Array.Copy(pwdBytes, keyBytes, len);

            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = keyBytes;

            var plain = rijndaelCipher.CreateDecryptor().TransformFinalBlock(encryptedData, 0, encryptedData.Length);
            return Encoding.UTF8.GetString(plain);
        }
    }
}