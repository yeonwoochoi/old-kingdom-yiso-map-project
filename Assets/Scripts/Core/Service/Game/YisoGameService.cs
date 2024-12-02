using System;
using Core.Domain.Locale;
using Core.Domain.Settings;
using Core.Domain.Types;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Service.Game {
    public class YisoGameService : IYisoGameService {
        private YisoLocale.Locale currentLocale = YisoLocale.Locale.KR;

        public YisoLocale.Locale GetCurrentLocale() => currentLocale;

        private readonly bool developMode;
        private readonly bool saveData;

        public YisoPlatforms GetPlatform() {
#if UNITY_IOS
            return YisoPlayforms.IOS;
#elif UNITY_ANDROID
            return YisoPlatforms.ANDROID;
#endif
            return YisoPlatforms.PC;
        }

        public bool IsMobile() => GetPlatform() is YisoPlatforms.IOS or YisoPlatforms.ANDROID;

        public void SetCurrentLocale(YisoLocale.Locale locale) {
            currentLocale = locale;
        }

        public bool IsDevelopMode() => developMode;
        public bool IsSaveData() => saveData;
        
        public bool IsReady() => true;

        private YisoGameService(Settings settings) {
            developMode = settings.developMode;
            saveData = settings.saveData;
        }
        internal static YisoGameService CreateService(Settings settings) => new(settings);

        public void OnDestroy() { }
        
        [Serializable]
        public class Settings {
            public bool developMode;
            public bool saveData = true;
        }
    }
}