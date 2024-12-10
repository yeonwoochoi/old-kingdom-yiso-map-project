using Controller.Emoticon;
using Core.Domain.Item;
using Core.Domain.Quest;
using Core.Service;
using Core.Service.Character;
using Manager.Modules;
using Sirenix.OdinInspector;
using Tools.Cool;
using UnityEngine;
using UnityEngine.Events;

namespace Controller.Holder {
    [AddComponentMenu("Yiso/Controller/Holder/ItemHoldingController")]
    public class YisoItemHoldingController : YisoHoldingController {
        public enum ActionType {
            Add,
            Remove
        }

        public ActionType type = ActionType.Add;
        public YisoItemSO itemSO;
        public int amount;
        public bool tryOnce = true;
        [ShowIf("@!tryOnce")] public YisoCooldown cooldown;
        public UnityEvent onActionSuccess;
        public UnityEvent onActionFailed;

        public int ItemId => itemSO.id;
        public YisoGameQuestModule QuestModule => TempService.GetGameManager().GameModules.QuestModule;
        public override YisoEmotionController.EmotionType EmotionType => useCustomEmoticon ? customEmoticonType : YisoEmotionController.EmotionType.ATTENTION;

        protected override void Initialization() {
            if (initialized) return;
            base.Initialization();
            if (!tryOnce) cooldown.Initialization();
            initialized = true;
        }

        public override void OnUpdate() {
            base.OnUpdate();
            if (!initialized || tryOnce) return;
            cooldown.Update();
        }

        protected override void PerformInteraction() {
            if (type == ActionType.Add) {
                if (!YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().InventoryModule
                    .TryAddItem(ItemId, amount)) {
                    onActionFailed?.Invoke();
                    return;
                }

                QuestModule.UpdateQuestRequirement(YisoQuestRequirement.Types.ITEM, ItemId);
                onActionSuccess?.Invoke();
                if (tryOnce) {
                    Destroy(gameObject);
                }
                else {
                    cooldown.Start();
                    HideInteractButton();
                }
            }

            if (type == ActionType.Remove) {
                if (!YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().InventoryModule
                    .TryRemoveItem(ItemId, amount)) {
                    onActionFailed?.Invoke();
                    return;
                }

                QuestModule.UpdateQuestRequirement(YisoQuestRequirement.Types.ITEM, ItemId);
                onActionSuccess?.Invoke();
                if (tryOnce) {
                    Destroy(gameObject);
                }
                else {
                    cooldown.Start();
                    HideInteractButton();
                }
            }
        }

        protected override void ShowEmoticonIfConditionsMet() {
            if (isShowingEmoticon || !CanDisplayEmoticon()) return;
            isShowingEmoticon = true;
            emotionController.ShowEmoticon(itemSO.icon, () => !CanDisplayEmoticon(),
                () => { isShowingEmoticon = false; });
        }

        protected override bool CanInteract() {
            if (!base.CanInteract()) return false;
            return tryOnce || cooldown.Ready;
        }

        protected override bool CanDisplayEmoticon() {
            if (!tryOnce && !cooldown.Ready) return false;
            return base.CanDisplayEmoticon();
        }
    }
}