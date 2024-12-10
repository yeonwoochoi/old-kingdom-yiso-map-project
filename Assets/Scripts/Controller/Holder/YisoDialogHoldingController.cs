using System;
using Controller.Emoticon;
using Core.Domain.Actor.Npc;
using Core.Domain.Dialogue;
using Core.Domain.Quest;
using Cutscene.Scripts.Control.Cutscene.Trigger;
using Manager.Modules;
using Sirenix.OdinInspector;
using Tools.Cutscene;
using UnityEngine;
using UnityEngine.Playables;

namespace Controller.Holder {
    /// <summary>
    /// Dialog 를 들고 있는 Character Controller
    /// Dialog Panel 보여준 후 Quest Requirement (Npc) Update함.
    /// </summary>
    [AddComponentMenu("Yiso/Controller/Holder/DialogHoldingController")]
    public class YisoDialogHoldingController : YisoHoldingController {
        [Serializable]
        public struct DialogueTimelineInfo {
            public PlayableAsset timeline;
            public float initialTime;
            public DirectorWrapMode extrapolationMode;
            public Canvas canvas;
        }

        public enum DialoguePlayType {
            ScriptableObject,
            Timeline,
            Cutscene
        }

        [Title("Quest")] public bool questDialog = true;
        [ShowIf("questDialog")] public YisoNpcSO npcSo;

        [Title("Dialog")] public DialoguePlayType playType = DialoguePlayType.ScriptableObject;

        [ShowIf("playType", DialoguePlayType.ScriptableObject)]
        public YisoDialogueSO dialogueSO;

        [ShowIf("playType", DialoguePlayType.Timeline)]
        public DialogueTimelineInfo timelineInfo;

        [ShowIf("playType", DialoguePlayType.Cutscene)]
        public YisoCutsceneTrigger cutsceneTrigger;

        public bool playOnce = true;

        private PlayableDirector director;
        private bool isPlayed = false;
        private YisoGameQuestModule QuestModule => TempService.GetGameManager().GameModules.QuestModule;
        public override YisoEmotionController.EmotionType EmotionType => useCustomEmoticon ? customEmoticonType : YisoEmotionController.EmotionType.CONVERSATION;

        protected override void Initialization() {
            if (initialized) return;
            base.Initialization();

            if (playType == DialoguePlayType.Timeline) InitializeTimeline();
            RegisterCutsceneCallback();
            
            initialized = true;
        }

        private void InitializeTimeline() {
            director = GetOrAddComponent<PlayableDirector>();
            director.playOnAwake = false;
            director.extrapolationMode = timelineInfo.extrapolationMode;
            director.initialTime = timelineInfo.initialTime;
            director.playableAsset = timelineInfo.timeline;
            SetDirectorInCutsceneCanvas();
        }
        
        private void RegisterCutsceneCallback() {
            if (playType != DialoguePlayType.Cutscene) return;
            cutsceneTrigger.AddStopCallback(OnStopDialogue);
        }

        protected override void PerformInteraction() {
            if (playType == DialoguePlayType.ScriptableObject) PlayDialogueScriptableObject();
            else if (playType == DialoguePlayType.Timeline) PlayTimeline();
            else if (playType == DialoguePlayType.Cutscene) PlayCutscene();
        }
        
        private void PlayDialogueScriptableObject() {
            if (dialogueSO == null) return;
            PopupUIService.ShowDialogue(dialogueSO.id, OnPlayDialogue, OnStopDialogue);
            isPlayed = true;
        }

        private void PlayTimeline() {
            if (director == null) return;
            director.Play();
            isPlayed = true;
        }

        private void PlayCutscene() {
            if (cutsceneTrigger == null) return;
            cutsceneTrigger.Play();
            isPlayed = true;
        }

        /// <summary>
        /// Interact 가능 여부를 return
        /// </summary>
        /// <returns></returns>
        protected override bool CanInteract() {
            if (!base.CanInteract()) return false;
            if (playOnce && isPlayed) return false;
            if (questDialog && npcSo == null) return false;
            if (playType == DialoguePlayType.Cutscene && !cutsceneTrigger.CanPlay()) return false;

            return true;
        }

        protected override bool CanDisplayEmoticon() {
            if (playOnce && isPlayed) return false;
            if (questDialog && npcSo == null) return false;
            if (playType == DialoguePlayType.Cutscene && !cutsceneTrigger.CanPlay()) return false;
            return base.CanDisplayEmoticon();
        }

        protected virtual void UpdateQuestDialogueRequirement() {
            QuestModule.UpdateQuestRequirement(YisoQuestRequirement.Types.NPC, npcSo.id);
        }

        #region Callback

        protected virtual void OnPlayDialogue() {
            if (playType != DialoguePlayType.Cutscene) InactivateHUD();
            if (playType == DialoguePlayType.Timeline) ActivateCanvasUI();
        }

        protected virtual void OnStopDialogue() {
            OnStopDialogue(null);
        }

        protected virtual void OnStopDialogue(PlayableDirector playableDirector) {
            if (playType != DialoguePlayType.Cutscene) ActivateHUD();
            if (playType == DialoguePlayType.Timeline) InactivateCanvasUI();
            if (questDialog) UpdateQuestDialogueRequirement();
        }

        #endregion

        #region HUD & UI

        private bool isHudVisible = true;
        private bool isCanvasVisible = true;

        protected virtual void ActivateHUD() {
            if (isHudVisible) return;
            UIService.ActiveOnlyHudUI(true);
            isHudVisible = true;
        }

        protected virtual void InactivateHUD() {
            if (!isHudVisible) return;
            UIService.ActiveOnlyHudUI(false);
            isHudVisible = false;
        }

        protected virtual void ActivateCanvasUI() {
            if (playType != DialoguePlayType.Timeline || isCanvasVisible) return;
            timelineInfo.canvas.gameObject.SetActive(true);
            isCanvasVisible = true;
        }

        protected virtual void InactivateCanvasUI() {
            if (playType != DialoguePlayType.Timeline || !isCanvasVisible) return;
            timelineInfo.canvas.gameObject.SetActive(false);
            isCanvasVisible = false;
        }

        protected virtual void SetDirectorInCutsceneCanvas() {
            var cutsceneTriggers = timelineInfo.canvas.GetComponentsInChildren<YisoBaseCutsceneTrigger>();
            var skipButtons = timelineInfo.canvas.GetComponentsInChildren<YisoCutsceneSkipButtonController>();
            foreach (var trigger in cutsceneTriggers) {
                trigger.director = director;
            }

            foreach (var skipButton in skipButtons) {
                skipButton.director = director;
            }
        }

        #endregion

        protected override void OnEnable() {
            base.OnEnable();
            if (playType == DialoguePlayType.Timeline) {
                director.stopped += OnStopDialogue;
            }
        }

        protected override void OnDisable() {
            base.OnDisable();
            if (playType == DialoguePlayType.Timeline) {
                director.stopped -= OnStopDialogue;
            }
        }
    }
}