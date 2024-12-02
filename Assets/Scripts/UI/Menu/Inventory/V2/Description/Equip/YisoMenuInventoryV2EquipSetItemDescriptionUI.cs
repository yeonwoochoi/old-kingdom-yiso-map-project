using System.Collections.Generic;
using System.Linq;
using Core.Domain.Item;
using Core.Domain.Locale;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Character;
using Core.Service.Data.Item;
using Core.Service.ObjectPool;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UI.Menu.Inventory.V2.Description.Equip.SetItem;
using UnityEngine;
using Utils.Extensions;

namespace UI.Menu.Inventory.V2.Description.Equip {
    public class YisoMenuInventoryV2EquipSetItemDescriptionUI : YisoUIController {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI setTitleText;
        [SerializeField, Title("Title Prefab")] private GameObject setItemTitleTextObject;
        [SerializeField] private GameObject setItemTitleContentObject;
        [SerializeField, Title("Effect Prefab")] private GameObject setEffectPanelPrefab;
        [SerializeField] private GameObject setEffectPanelContent;
        [SerializeField] private GameObject setEffectValuePrefab;
        [SerializeField, Title("Color")] private Color equippedItemColor;
        [SerializeField] private Color notEquippedItemColor;
        

        private readonly List<GameObject> itemTitleTextObjects = new();
        private readonly List<YisoMenuInventoryV2SetItemPanelUI> effectPanelObjects = new();
        
        private IYisoObjectPoolService objectPoolService;
        private IYisoItemService itemService;

        private YisoSetItem setItem;

        /*public void Visible(bool flag) {
            canvasGroup.Visible(flag);
        }
        
        protected override void Start() {
            objectPoolService = YisoServiceProvider.Instance.Get<IYisoObjectPoolService>();
            
            objectPoolService.WarmPool(setItemTitleTextObject, 6, setItemTitleContentObject);
            objectPoolService.WarmPool(setEffectPanelPrefab, 5, setEffectPanelContent);
            itemService = YisoServiceProvider.Instance.Get<IYisoItemService>();
        }

        public bool SetItem(bool equipped, YisoEquipItem item) {
            if (!itemService.TryGetSetItem(item.SetItemId, out setItem)) return false;
            SetItem(equipped);
            return true;
        }

        public void Clear() {
            foreach (var obj in itemTitleTextObjects) {
                objectPoolService.ReleaseObject(obj);
            }

            foreach (var obj in effectPanelObjects) {
                obj.Clear();
                objectPoolService.ReleaseObject(obj.gameObject);
            }
            
            itemTitleTextObjects.Clear();
            effectPanelObjects.Clear();
        }

        private void SetItem(bool equipped) {
            if (equipped) SetItemTitles();
            SetEffects();
        }

        private void SetItemTitles() {
            var inventoryModule = YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().InventoryModule;
            
            var items = setItem.ItemIds
                .OrderBy(id => id)
                .Select(id => itemService.GetItemOrElseThrow(id))
                .Cast<YisoEquipItem>().ToList();
            

            foreach (var item in items) {
                var id = item.Id;
                var itemName = item.GetName(CurrentLocale);
                var equipped = inventoryModule.IsEquipped(item.Slot, id);
                var color = equipped ? equippedItemColor : notEquippedItemColor;
                var itemNameObject = objectPoolService.SpawnObject<TextMeshProUGUI>(setItemTitleTextObject, setItemTitleContentObject);
                itemNameObject.SetText(itemName);
                itemNameObject.color = color;
                itemTitleTextObjects.Add(itemNameObject.gameObject);
            }
        }

        private void SetEffects() {
            var keys = setItem.GetKeys();
            while (keys.TryDequeue(out var key)) {
                SetEffect(key);
            }
        }

        private void SetEffect(int level, List<YisoSetItem.ItemEffect> effects) {
            var effectPanel =
                objectPoolService.SpawnObject<YisoMenuInventoryV2SetItemPanelUI>(setEffectPanelPrefab,
                    setEffectPanelContent);
            var effectTexts = effects.Select(e => e.effect.ToUIText(e.value)).ToArray();
            var title = CurrentLocale == YisoLocale.Locale.KR ? $"{level}세트 효과" : $"{level} Set Effects";
            effectPanel.SetTitle(title);
            effectPanel.SetValues(setEffectValuePrefab, effectTexts);
            
            effectPanelObjects.Add(effectPanel);

        }

        private void SetEffect(int level) {
            var effectPanel =
                objectPoolService.SpawnObject<YisoMenuInventoryV2SetItemPanelUI>(setEffectPanelPrefab,
                    setEffectPanelContent);

            var effectTexts = setItem.GetEffectUIText(level);
            var title = CurrentLocale == YisoLocale.Locale.KR ? $"{level}세트 효과" : $"{level} Set Effects";
            effectPanel.SetTitle(title);
            effectPanel.SetValues(setEffectValuePrefab, effectTexts);
            
            effectPanelObjects.Add(effectPanel);
            
            Debug.Log($"{title} => {effectTexts}");
        }*/
    }
}