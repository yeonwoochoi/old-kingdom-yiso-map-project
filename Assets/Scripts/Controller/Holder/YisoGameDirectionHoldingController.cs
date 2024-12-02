using Controller.Emoticon;
using Core.Domain.Direction;
using UnityEngine;

namespace Controller.Holder {
    [AddComponentMenu("Yiso/Controller/Holder/GameDirectionHoldingController")]
    public class YisoGameDirectionHoldingController : YisoHoldingController {
        public YisoGameDirectionSO gameDirectionSO;
        public bool showOnce = false;
        public override YisoEmotionController.EmotionType EmotionType => useCustomEmoticon ? customEmoticonType : YisoEmotionController.EmotionType.ATTENTION;

        protected override void Initialization() {
            if (initialized) return;
            base.Initialization();
            initialized = true;
        }

        protected override void PerformInteraction() {
            ShowGameDirectionPopup();
        }

        public virtual void ShowGameDirectionPopup() {
            if (gameDirectionSO == null) return;
            PopupUIService.ShowDirection(gameDirectionSO.id,
                () => { },
                () => {
                    if (showOnce) {
                        Destroy(gameObject);
                    }
                });
        }
    }
}