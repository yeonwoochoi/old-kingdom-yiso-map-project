using System;
using System.Text;
using Character.Weapon;
using Core.Domain.Bounty;
using Core.Domain.Cabinet;
using Core.Domain.Dialogue;
using Core.Domain.Direction;
using Core.Domain.Drop;
using Core.Domain.Item;
using Core.Domain.Quest.SO;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Character;
using Core.Service.Data;
using Core.Service.Data.Item;
using Core.Service.Game;
using Core.Service.Log;
using Core.Service.UI;
using Core.Service.UI.Game;
using Core.Service.UI.Global;
using Core.Service.UI.HUD;
using Core.Service.UI.Popup;
using Core.Service.UI.Popup2;
using Sirenix.OdinInspector;
using UI.HUD.Timer;
using UI.Popup.Quest;
using UnityEngine;
using Utils.Extensions;

namespace Test {
    public class TestService : MonoBehaviour {
        private YisoQuestSO questSO;
        public YisoCabinetSO testCabinet;
        public YisoBountySO testBounty;
        [Title("Dialogue")] private YisoDialogueSO testDialogue;
        [Title("Direction")] private YisoGameDirectionSO testDirection;
        [Title("Floating")] public string text;
        public Color color = Color.white;

        public void ShowWelcome() {
            YisoServiceProvider.Instance.Get<IYisoPopupUIService>().ShowWelcome();
        }

        public void RandomItem(int count = 10) {
            for (var i = 0; i < count; i++) {
                var item = YisoServiceProvider.Instance.Get<IYisoItemService>().CreateRandomItem() as YisoEquipItem;
                Debug.Log($"[{item.InvType}] {item.GetName()}({item.Rank})");
            }
        }

        public void RandomWeapon(int count, YisoWeapon.AttackType type, YisoEquipRanks rank) {
            for (var i = 0; i < count; i++) {
                var item = YisoServiceProvider.Instance.Get<IYisoItemService>().CreateRandomWeapon(type, rank) as YisoEquipItem;
                Debug.Log($"[{item.Rank}] {item.GetName()}");
            }
        }

        public void ShowHUD() {
            YisoServiceProvider.Instance.Get<IYisoUIService>().TestShowHUD();
        }

        public void TestShowStageLoadingComment(bool hideHud, int stage = 1) {
            YisoServiceProvider.Instance.Get<IYisoGameUIService>()
                .ShowStageLoadingComment(hideHud, stage, () => {
                    Debug.Log("[TEST] Test Show Stage Loading Comment Completed");
                });
        }

        public void TestShowStoryClearPopup(bool hasNextStage = true) {
            YisoServiceProvider.Instance.Get<IYisoGameUIService>()
                .ShowStoryClearPopup(() => {
                    Debug.Log("OnClickBaseCamp");
                }, () => Debug.Log("OnClickStory"), existNextStage: hasNextStage);
        }

        public void TestNumberInput(int digits = 3) {
            YisoServiceProvider.Instance.Get<IYisoPopup2UIService>()
                .ShowNumberInput(null);
        }

        public void TestFloating() {
            YisoServiceProvider.Instance.Get<IYisoGameUIService>()
                .FloatingText("This is Test STR", () => Debug.Log("OnCompleted"));
        }

        public void ShowQuest(YisoPopupQuestContentUI.Types type) {
            var quest = questSO.CreateQuest();
            var service = YisoServiceProvider.Instance.Get<IYisoPopupUIService>();
            switch (type) {
                case YisoPopupQuestContentUI.Types.START:
                    service.ShowQuestStartPopup(quest.Id, () => Debug.Log("OnClickOk"), () => Debug.Log("OnClickCancel"));
                    break;
                case YisoPopupQuestContentUI.Types.RE_START:
                    service.ShowQuestRestartPopup(quest.Id, () => Debug.Log("OnClickOk"), () => Debug.Log("OnClickCancel"));
                    break;
                case YisoPopupQuestContentUI.Types.COMPLETE:
                    service.ShowQuestCompletePopup(quest.Id, 0,() => Debug.Log("OnClickOk"), () => Debug.Log("OnClickCancel"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void ShowDialogue() {
            YisoServiceProvider.Instance.Get<IYisoPopupUIService>()
                .ShowDialogue(testDialogue.id);
        }

        public void ShowSaving() {
            YisoServiceProvider.Instance.Get<IYisoGlobalUIService>()
                .ShowSaving();
        }

        public void ShowDirection() {
            YisoServiceProvider.Instance.Get<IYisoPopupUIService>()
                .ShowDirection(testDirection.id, () => {
                    Debug.Log("On Open Popup");
                }, () => Debug.Log("On Complete"));
        }
        
        public void ShowFloatingText() {
            YisoServiceProvider.Instance.Get<IYisoGlobalUIService>()
                .FloatingText(text, color);
        }

        public void ShowCabinet() {
            YisoServiceProvider.Instance.Get<IYisoPopupUIService>().ShowCabinetPopup(testCabinet.id, beforeOpenPopup:
                () => { });
        }

        public void StartTimer(float time = 10f, bool hideWhenDone = true) {
            var args = YisoPlayerHUDTimerEventArgs.Builder(time)
                .HideWhenDone(hideWhenDone)
                .AddOnStart(() => Debug.Log("OnStart"))
                .AddOnProgress(value => Debug.Log($"OnProgress: {value}"))
                .AddOnComplete(() => Debug.Log("OnComplete"))
                .Build();
            YisoServiceProvider.Instance.Get<IYisoHUDUIService>()
                .RaiseTimer(YisoPlayerHUDTimerUI.Actions.START, args);
        }

        [Button]
        public void ShowBountyPopup() {
            YisoServiceProvider.Instance.Get<IYisoPopupUIService>()
                .ShowDevBountyPopup(testBounty);
        }

        [Button]
        public void ShowBountyClearPopup() {
            YisoServiceProvider.Instance.Get<IYisoPopupUIService>()
                .ShowBountyClearPopup(() => {
                    Debug.Log("Done!");
                });
        }
    }
}