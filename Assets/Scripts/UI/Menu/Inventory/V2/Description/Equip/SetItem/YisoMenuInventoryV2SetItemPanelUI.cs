using System.Collections.Generic;
using Core.Behaviour;
using TMPro;
using UnityEngine;

namespace UI.Menu.Inventory.V2.Description.Equip.SetItem {
    public class YisoMenuInventoryV2SetItemPanelUI : RunIBehaviour {
        [SerializeField] private TextMeshProUGUI setEffectTitleText;
        [SerializeField] private GameObject setEffectValueContent;

        private readonly List<ValueItem> items = new();
        
        public void SetTitle(string title) {
            setEffectTitleText.SetText(title);
        }

        public GameObject SetEffectValueContent => setEffectValueContent;

        public void SetValues(GameObject prefab, string[] values, Color activeColor) {
            setEffectTitleText.color = activeColor;
            foreach (var value in values) {
                var index = GetEmptyIndex();
                if (index == -1) {
                    var obj = GetValueObject(prefab);
                    items.Add(new ValueItem(obj));
                    index = items.Count - 1;
                }

                items[index].Active = true;
                items[index].Item.SetText(value, activeColor);
            }
        }

        public void Clear() {
            foreach (var item in items) {
                item.Active = false;
            }
        }

        private int GetEmptyIndex() {
            for (var i = 0; i < items.Count; i++) {
                if (items[i].Active) continue;
                return i;
            }

            return -1;
        }

        private YisoMenuInventoryV2SetItemValuePanelUI GetValueObject(GameObject prefab) {
            var obj = CreateObject<YisoMenuInventoryV2SetItemValuePanelUI>(prefab, setEffectValueContent.transform);
            obj.gameObject.SetActive(false);
            return obj;
        }

        private class ValueItem {
            private bool active = false;

            public bool Active {
                get => active;
                set {
                    active = value;
                    Item.gameObject.SetActive(value);
                }
            }
            public YisoMenuInventoryV2SetItemValuePanelUI Item { get; }

            public ValueItem(YisoMenuInventoryV2SetItemValuePanelUI item) {
                Item = item;
            }
        }
    }
}