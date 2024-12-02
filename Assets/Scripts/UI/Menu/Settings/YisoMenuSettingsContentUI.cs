using Core.Domain.Types;
using DG.Tweening;
using UI.Menu.Base;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu.Settings {
    public class YisoMenuSettingsContentUI : YisoMenuBasePanelUI {
        [SerializeField] private ScrollRect scrollRect;

        protected override void OnVisible() {
            scrollRect.DOVerticalNormalizedPos(1f, 0.25f);
        }

        public override void ClearPanel() {
            
        }

        public override YisoMenuTypes GetMenuType() => YisoMenuTypes.SETTINGS;
    }
}