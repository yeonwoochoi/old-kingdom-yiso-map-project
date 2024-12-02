using Core.Domain.Locale;
using UnityEngine;

namespace Core.Domain.Settings {
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Yiso/Settings/Game Settings")]
    public class YisoGameSettingsSO : ScriptableObject {
        public YisoLocale.Locale baseLocale = YisoLocale.Locale.KR;
    }
}