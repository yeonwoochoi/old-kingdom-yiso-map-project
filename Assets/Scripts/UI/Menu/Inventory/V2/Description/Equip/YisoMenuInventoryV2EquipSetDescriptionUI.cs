using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain.Item;
using Core.Service;
using Core.Service.Character;
using Core.Service.Data.Item;
using Core.Service.ObjectPool;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UI.Menu.Inventory.V2.Description.Equip.SetItem;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu.Inventory.V2.Description.Equip {
    public class YisoMenuInventoryV2EquipSetDescriptionUI : YisoUIController {
        [SerializeField, Title("Set Title")] private TextMeshProUGUI setTitleText;
        [SerializeField, Title("Set Item Title")] private TextMeshProUGUI[] setItemTitleTexts;
        [SerializeField, Title("Prefab")] private GameObject setEffectValuePrefab;
        [SerializeField] private GameObject[] setEffectContents;
        [SerializeField] private YisoMenuInventoryV2SetItemPanelUI[] setEffectPanelUIs;
        [SerializeField, Title("Color")] private Color equippedItemColor;
        [SerializeField] private Color notEquippedItemColor;
        [SerializeField] private RectTransform contentRect;
        
        private readonly Dictionary<int, YisoMenuInventoryV2SetItemPanelUI> setEffectPanelDict = new();

        private IYisoItemService itemService;

        private CanvasGroup canvasGroup;
        private ScrollRect scrollRect;

        private YisoSetItem setItem;
        
        protected override void Start() {
            canvasGroup = GetComponent<CanvasGroup>();

            itemService = YisoServiceProvider.Instance.Get<IYisoItemService>();

            for (var i = 0; i < setEffectPanelUIs.Length; i++) {
                setEffectPanelDict[i + 2] = setEffectPanelUIs[i];
            }
            
            foreach (var panel in setEffectPanelDict.Values) {
                panel.gameObject.SetActive(false);
            }

            scrollRect = GetComponent<ScrollRect>();
        }

        public void Visible(bool flag) {
            canvasGroup.Visible(flag);
        }

        public void Clear() {
            foreach (var title in setItemTitleTexts) {
                title.SetText("");
                title.gameObject.SetActive(false);
            }
            
            foreach (var panel in setEffectPanelDict.Values) {
                panel.Clear();
                panel.gameObject.SetActive(false);
            }
            
            scrollRect.verticalNormalizedPosition = 1f;
        }

        public bool SetItem(YisoEquipItem item) {
            if (!itemService.TryGetSetItem(item.SetItemId, out setItem)) return false;
            var equipCount = SetItemTitles();
            SetEffects(equipCount);
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            return true;
        }

        private int SetItemTitles() {
            var inventoryModule = YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().InventoryModule;

            var items = setItem.ItemIds
                .OrderBy(id => id)
                .Select(id => itemService.GetItemOrElseThrow(id))
                .Cast<YisoEquipItem>().ToList();

            var count = 0;
            for (var i = 0; i < items.Count; i++) {
                var item = items[i];
                var id = item.Id;
                var slot = item.Slot;
                var itemName = item.GetName(CurrentLocale);
                var equipped = inventoryModule.IsEquipped(slot, id);
                setItemTitleTexts[i].gameObject.SetActive(true);
                setItemTitleTexts[i].SetText(itemName);
                setItemTitleTexts[i].color = equipped ? equippedItemColor : notEquippedItemColor;
                if (equipped) count += 1;
            }

            return count;
        }

        private void SetEffects(int equipCount) {
            var keys = setItem.GetKeys();
            while (keys.TryDequeue(out var key)) {
                var active = key <= equipCount;
                SetEffect(key, active);
            }
        }

        private void SetEffect(int level, bool active) {
            var effectTexts = setItem.GetEffectUIText(level, CurrentLocale);
            var panel = setEffectPanelDict[level];
            panel.gameObject.SetActive(true);
            panel.SetValues(setEffectValuePrefab, effectTexts, active ? equippedItemColor : notEquippedItemColor);
        }
    }
}