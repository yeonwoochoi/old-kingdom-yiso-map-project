using Controller.Emoticon;
using Core.Domain.Types;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Controller.Holder {
    [AddComponentMenu("Yiso/Controller/Holder/StoreHoldingController")]
    public class YisoStoreHoldingController: YisoHoldingController {
        [Title("Settings")]
        public TextMeshPro objectText;
        public bool hasStoreId = true;
        [ShowIf("hasStoreId")] public int storeId = -1;
        public override YisoEmotionController.EmotionType EmotionType => useCustomEmoticon ? customEmoticonType : YisoEmotionController.EmotionType.ATTENTION;

        protected override void PerformInteraction() {
            base.PerformInteraction();
            var data = hasStoreId ? (object) storeId : null;
            UIService.ShowInteractUI(YisoInteractTypes.STORE, data);
        }
        
        protected override bool CanInteractByMouseClick() {
            return useMouseClickInteractionOnDesktop && !isMobile;
        }
        
        protected override void ShowInteractButton() {
            objectText.gameObject.SetActive(true);
            base.ShowInteractButton();
        }

        protected override void HideInteractButton() {
            objectText.gameObject.SetActive(false);
            base.HideInteractButton();
        }
    }
}