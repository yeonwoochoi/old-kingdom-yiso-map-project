using System;
using Sirenix.OdinInspector;

namespace Core.Domain.Data {
    [Serializable]
    public class DataVersionInfo {
        [HorizontalGroup("Version")]
        public int major;
        [HorizontalGroup("Version/Minor")]
        public int minor;
        [HorizontalGroup("Version/Patch")]
        public int patch;

        public bool IsMatched(DataVersionInfo other) => major == other.major && minor == other.minor && patch == other.patch;

        public void Copy(DataVersionInfo other) {
            major = other.major;
            minor = other.minor;
            patch = other.patch;
        }
    }
}