using System;

namespace Core.Server.User {
    [Serializable]
    public class User {
        public int id;
        public string uuid;
        public string userId;
        public DeviceType deviceType;
    }

    public class AccessToken { }
}