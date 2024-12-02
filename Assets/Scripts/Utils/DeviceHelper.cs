using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Utils.ObjectId;
using DeviceType = Core.Server.User.DeviceType;

namespace Utils {
    public static class DeviceHelper {
        public static DeviceType GetDeviceType() {
            var deviceType = DeviceType.EDITOR;
#if UNITY_ANDROID && !UNITY_EDITOR
            deviceType = DeviceType.ANDROID;
#elif UNITY_IOS && !UNITY_EDITOR
            deviceType = DeviceType.IOS;
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
            deviceType = DeviceType.MAC_OS_X;
#elif UNITY_STANDALONE_WIN && !UNITY_EDITOR
            deviceType = DeviceType.WINDOWS;
#endif
            return deviceType;
        }

        public static string GetUUID() => SystemInfo.deviceUniqueIdentifier;

        public static string GenerateDeviceId(string uuid) {
            if (uuid == string.Empty)
                uuid = GetUUID();

            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(uuid));
            return Convert.ToBase64String(bytes);
        }

        public static string GeneratePassword() => YisoObjectID.GenerateString();
    }
}