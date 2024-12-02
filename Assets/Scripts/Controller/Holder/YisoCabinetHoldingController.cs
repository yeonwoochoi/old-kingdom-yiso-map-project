using Controller.Emoticon;
using Core.Domain.Cabinet;
using Core.Domain.Locale;
using Core.Service;
using Core.Service.Game;
using UnityEngine;

namespace Controller.Holder {
    [AddComponentMenu("Yiso/Controller/Holder/CabinetHoldingController")]
    public class YisoCabinetHoldingController : YisoHoldingController {
        public YisoCabinetSO cabinetSO;
        public bool showOnce = false;

        public YisoLocale.Locale CurrentLocale => YisoServiceProvider.Instance.Get<IYisoGameService>().GetCurrentLocale();
        public override YisoEmotionController.EmotionType EmotionType => useCustomEmoticon ? customEmoticonType : YisoEmotionController.EmotionType.ATTENTION;

        protected YisoLocale cabinetOkButtonName;
        protected YisoLocale cabinetCancelButtonName;

        protected override void Initialization() {
            if (initialized) return;
            base.Initialization();
            cabinetOkButtonName = new YisoLocale {
                [YisoLocale.Locale.KR] = "확인",
                [YisoLocale.Locale.EN] = "취소"
            };
            cabinetCancelButtonName = new YisoLocale {
                [YisoLocale.Locale.KR] = "Ok",
                [YisoLocale.Locale.EN] = "Cancel"
            };
            initialized = true;
        }

        protected override void PerformInteraction() {
            ShowCabinetPopup();
        }

        protected virtual void ShowCabinetPopup() {
            if (cabinetSO == null) return;
            PopupUIService.ShowCabinetPopup(cabinetSO.id,
                () => { },
                cabinetOkButtonName[CurrentLocale], cabinetCancelButtonName[CurrentLocale],
                () => {
                    if (showOnce) {
                        Destroy(gameObject);
                    }
                },
                () => {
                    if (showOnce) {
                        Destroy(gameObject);
                    }
                });
        }
    }
}