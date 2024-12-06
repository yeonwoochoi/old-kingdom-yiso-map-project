using Controller.Emoticon;
using Controller.Map;
using Core.Domain.Actor.Player.Modules.Quest;
using Core.Domain.Locale;
using Core.Service;
using Core.Service.Character;
using Core.Service.Game;
using Core.Service.UI.Game;
using Manager_Temp_;
using TMPro;
using Tools.Event;
using UI.HUD.Interact;
using UnityEngine;

namespace Controller.Holder {
    [AddComponentMenu("Yiso/Controller/Holder/QuestRewardBoxController")]
    public class YisoQuestRewardBoxHoldingController : YisoHoldingController, IYisoEventListener<YisoMapChangeEvent>, IYisoEventListener<YisoBountyChangeEvent> {
        [SerializeField] private TextMeshPro objectText;
        [SerializeField] private Animator animator;

        protected int questId;
        protected YisoLocale inventoryFullMessage;
        protected bool opened = false;
        protected int spawnMapId;

        private YisoPlayerQuestModule QuestModule =>
            YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().QuestModule;
        public override YisoEmotionController.EmotionType EmotionType => useCustomEmoticon ? customEmoticonType : YisoEmotionController.EmotionType.ATTENTION;

        protected const string OpenAnimationParameterName = "Open";
        protected int openAnimationParameter = Animator.StringToHash(OpenAnimationParameterName);

        public virtual void Initialization(int questId) {
            this.questId = questId;
            inventoryFullMessage = new YisoLocale {
                kr = "인벤토리가 가득 차서 퀘스트 아이템을 받을 수 없습니다.",
                en = "Inventory is full. You cannot receive the quest item."
            };
            spawnMapId = GameManager.Instance.CurrentMapController.CurrentMap.Id;
            opened = false;
            initialized = true;
        }

        protected override void ShowInteractButton() {
            objectText.gameObject.SetActive(true);
            base.ShowInteractButton();
        }

        protected override void HideInteractButton() {
            objectText.gameObject.SetActive(false);
            base.HideInteractButton();
        }

        protected override void PerformInteraction() {
            OpenChest();
        }

        protected virtual void OpenChest() {
            PopupUIService.ShowQuestCompletePopup(questId, GameManager.Instance.DeathCount, OnClickOkButton,
                OnClickCancelButton);
        }

        protected override void OnTriggerEnter2D(Collider2D other) {
            if (opened) return;
            base.OnTriggerEnter2D(other);
        }

        protected override void OnTriggerExit2D(Collider2D other) {
            if (opened) return;
            base.OnTriggerExit2D(other);
        }

        /// <summary>
        /// 보상을 받은 경우
        /// </summary>
        protected virtual void OnClickOkButton() {
            animator.SetBool(openAnimationParameter, true);

            // Complete Quest
            QuestModule.CompleteQuest(questId);
            objectText.gameObject.SetActive(false);
            HUDUIService.HideInteractButton(YisoHudUIInteractTypes.SPEECH);
            opened = true;
        }

        /// <summary>
        /// 인벤토리 창이 다 차있는 경우
        /// </summary>
        protected virtual void OnClickCancelButton() {
            var currentLocale = YisoServiceProvider.Instance.Get<IYisoGameService>().GetCurrentLocale();
            YisoServiceProvider.Instance.Get<IYisoGameUIService>().FloatingText(inventoryFullMessage[currentLocale]);
        }

        public void OnEvent(YisoMapChangeEvent e) {
            if (!e.isInitialMapLoad) return;
            if (e.currentMap.Id == spawnMapId) return;
            Destroy(gameObject);
        }

        public void OnEvent(YisoBountyChangeEvent e) {
            if (e.currentBounty.Id == spawnMapId) return;
            Destroy(gameObject);
        }

        protected override void OnEnable() {
            base.OnEnable();
            this.YisoEventStartListening<YisoMapChangeEvent>();
            this.YisoEventStartListening<YisoBountyChangeEvent>();
        }

        protected override void OnDisable() {
            base.OnDisable();
            this.YisoEventStopListening<YisoMapChangeEvent>();
            this.YisoEventStopListening<YisoBountyChangeEvent>();
        }
    }
}