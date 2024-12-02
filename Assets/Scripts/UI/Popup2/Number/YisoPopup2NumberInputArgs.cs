using System.Collections.Generic;
using UnityEngine.Events;

namespace UI.Popup2.Number {
    public class YisoPopup2NumberInputArgs {
        public int MaxDigits { get; set; } = 3;
        public int MaxValue { get; set; } = 0;
        public List<UnityAction<int>> OkCbList { get; } = new();
        public int StartValue { get; set; } = 0;
    }
}