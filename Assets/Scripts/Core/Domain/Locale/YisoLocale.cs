using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Domain.Locale {
    [Serializable]
    public class YisoLocale {
        [TextArea, LabelText("Korean")] public string kr;
        [TextArea, LabelText("English")] public string en;

        public enum Locale {
            KR, EN
        }

        public string this[Locale locale] {
            get => locale == Locale.KR ? kr : en;
            set {
                if (locale == Locale.KR) kr = value;
                else en = value;
            }
        }
    }
}