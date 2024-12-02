using Controller.Emoticon;
using Core.Service;
using Core.Service.Temp;
using Tools.Cool;
using UnityEngine;

namespace Tools.AI.Actions {
    [AddComponentMenu("Yiso/Character/AI/Actions/AIActionEmoticon")]
    public class YisoAIActionEmoticon : YisoAIAction {
        public YisoEmotionController.EmotionType emotionType;
        public YisoCooldown cooldown;

        protected YisoEmotionController emotionController;
        protected IYisoTempService TempService => YisoServiceProvider.Instance.Get<IYisoTempService>();

        public override void Initialization() {
            base.Initialization();
            cooldown.Initialization();
        }

        public override void PerformAction() {
            cooldown.Update();
        }

        public override void OnEnterState() {
            base.OnEnterState();
            if (emotionController == null) {
                emotionController =
                    Instantiate(TempService.GetGameManager().GameModules.AssetModule.EmoticonPrefab,
                        brain.owner.transform).GetComponent<YisoEmotionController>();
                emotionController.Initialization();
            }

            if (cooldown.Ready) {
                emotionController.ShowEmoticon(emotionType, () => !ActionInProgress);
                cooldown.Start();
            }
        }
    }
}