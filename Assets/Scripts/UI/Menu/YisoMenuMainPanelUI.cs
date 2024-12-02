using System;
using System.Collections.Generic;
using Core.Behaviour;
using Core.Domain.Actor.Player;
using Core.Domain.Item;
using Core.Domain.Quest;
using Core.Domain.Types;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UI.Menu.Base;
using UI.Menu.Inventory;
using UI.Menu.Inventory.V2;
using UI.Menu.Quest;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu {
    public class YisoMenuMainPanelUI : YisoUIController {
        [SerializeField, Title("Tabs")] private List<Toggle> tabs;
        [SerializeField, Title("Contents")] private List<YisoMenuBasePanelUI> panelUis;
        [SerializeField, Title("Title")] private TextMeshProUGUI titleText;
        [SerializeField] private List<Image> indicators;
        [SerializeField] private Color activeIndicatorColor;
        [SerializeField] private Color nonActiveIndicatorColor;
        [SerializeField, Title("Overlay")] private YisoMenuOverlayUI overlayUI;


        private readonly Dictionary<YisoMenuTypes, int> cachedIndexDict = new();
        
        private CanvasGroup canvasGroup;

        private YisoMenuTypes currentType;

        public void Show(YisoMenuTypes type, object data) {
            if (type != currentType) {
                var toggleIndex = cachedIndexDict[type];
                tabs[toggleIndex].isOn = true;
            }
            currentType = type;
            var index = cachedIndexDict[type];
            panelUis[index].Visible(true, data);
            SetIndicator(index, true);
            canvasGroup.Visible(true);
            titleText.SetText(type.ToString(CurrentLocale));
        }

        public void Hide() {
            canvasGroup.DOVisible(0f, .25f);
            var index = cachedIndexDict[currentType];
            panelUis[index].Visible(false);
        }

        protected override void OnEnable() {
            base.OnEnable();
            var inventoryPanel = GetMenuContent<YisoMenuInventoryV2ContentUI>();
        }

        protected override void OnDisable() {
            base.OnDisable();
            var inventoryPanel = GetMenuContent<YisoMenuInventoryV2ContentUI>();
        }

        protected override void Start() {
            canvasGroup = GetComponent<CanvasGroup>();

            for (var i = 0; i < panelUis.Count; i++) {
                cachedIndexDict[panelUis[i].GetMenuType()] = i;
            }
            
            for (var i = 0; i < tabs.Count; i++) {
                tabs[i].onValueChanged.AddListener(OnToggleTab(i));
            }
            
            for (var i = 0; i < indicators.Count; i++) SetIndicator(i, false);
        }
        
        private UnityAction<bool> OnToggleTab(int index) => flag => {
            panelUis[index].Visible(flag);
            SetIndicator(index, flag);
            if (flag) {
                titleText.SetText(index.ToEnum<YisoMenuTypes>().ToString(CurrentLocale));
                currentType = panelUis[index].GetMenuType();
            }
            else panelUis[index].ClearPanel();
        };

        private void SetIndicator(int index, bool flag) {
            indicators[index].color = flag ? activeIndicatorColor : nonActiveIndicatorColor;
        }

        private T GetMenuContent<T>() where T : YisoMenuBasePanelUI => (T) panelUis.Find(panel => panel is T);
    }
}