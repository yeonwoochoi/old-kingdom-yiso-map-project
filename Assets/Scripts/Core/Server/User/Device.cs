using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Core.Server.User {
    [Serializable]
    public class Device {
        public DeviceType type;
        public string uuid;

        public static Device Create() {
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
            var uuid = SystemInfo.deviceUniqueIdentifier;
            return new Device {
                type = deviceType,
                uuid = uuid
            };
        }

        public bool Compare(Device other) => other.type == type && other.uuid == uuid;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DeviceType {
        ANDROID,
        IOS,
        MAC_OS_X,
        WINDOWS,
        EDITOR
    }
}