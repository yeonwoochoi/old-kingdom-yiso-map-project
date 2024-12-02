using System;
using Core.Bootstrap;
using Core.Domain.Locale;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Core.Domain.Data {
    [Serializable]
    public class YisoGameData {
        public YisoGameSettingData settingData = new();
        public YisoGameVersionData versionData = new();
        public bool playerDataSaved = false;
    }

    [Serializable]
    public class YisoGameSettingData {
        public event UnityAction<float> OnDifficultyChangedEvent;
        
        public int backgroundMusic = 100;
        public int effectSound = 100;
        public YisoLocale.Locale locale = YisoLocale.Locale.KR;
        public float difficulty = 1f;

        public void SetDifficulty(float difficulty) {
            this.difficulty = difficulty;
            OnDifficultyChangedEvent?.Invoke(difficulty);
        }
    }

    [Serializable]
    public class YisoGameVersionData {
        /**
         * MAJOR version when you make incompatible API changes
         * This version will follow Semantic Version <a href="https://semver.org/lang/ko/">Check</a>
         */
        public int major = 0;
        /**
         * MINOR version when you add functionality in a backward compatible manner
         * This version will follow Semantic Version <a href="https://semver.org/lang/ko/">Check</a>
         */
        public int minor = 0;
        /**
         * PATCH version when you make backward compatible bug fixes
         * This version will follow Semantic Version <a href="https://semver.org/lang/ko/">Check</a>
         */
        public int patch = 0;

        public bool CompareVersion(YisoBootstrap.BuildVersionInfo versionInfo) {
            return major == versionInfo.major && minor == versionInfo.minor && patch == versionInfo.patch;
        }

        public void UpdateVersion(YisoBootstrap.BuildVersionInfo versionInfo) {
            major = versionInfo.major;
            minor = versionInfo.minor;
            patch = versionInfo.patch;
        }

        public override string ToString() => $"{major}.{minor}.{patch}";
    }
}