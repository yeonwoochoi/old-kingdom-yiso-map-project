using System;
using System.Collections;
using System.Collections.Generic;
using Camera;
using Character.Core;
using Controller.Map;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Scene;
using Core.Service.UI.Popup;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Manager_Temp_;
using Sirenix.OdinInspector;
using UI.HUD.Interact;
using UnityEngine;
using UnityEngine.Events;

namespace Tools.Environment {
    [AddComponentMenu("Yiso/Environment/Portal")]
    public class YisoPortal : YisoActionTriggerZone {
        public enum PortalColor {
            Aqua = 0,
            Blue = 1,
            Gold = 2,
            Gray = 3,
            Green = 4,
            LightBlue = 5,
            Pink = 6,
            Purple = 7,
            Red = 8,
            Yellow = 9
        }

        public enum PortalOption {
            GoToAnotherPortal,
            // GoToNextStage,
            GoToAnotherScene
        }

        [Title("Destination")] public PortalOption portalOption;

        [ShowIf("portalOption", PortalOption.GoToAnotherPortal)]
        public YisoPortal connectedPortal;

        [ShowIf("portalOption", PortalOption.GoToAnotherPortal)]
        public Vector3 exitOffset;

        [ShowIf("portalOption", PortalOption.GoToAnotherPortal)]
        public bool destroyAfterUse = false;

        [ShowIf("portalOption", PortalOption.GoToAnotherScene)]
        public YisoSceneTypes destinationScene = YisoSceneTypes.BASE_CAMP;

        [Title("Color")] public PortalColor portalColor;

        [Title("Actions")] public UnityEvent onTeleport;

        public YisoNavigationZoneController ParentZone { get; set; }

        protected List<Transform> ignoreList;
        protected Animator animator;

        protected readonly float initialDelay = 0.1f;
        protected readonly float fadeOutDuration = 0.3f;
        protected readonly float afterTeleportDelay = 0.6f;
        protected readonly float fadeInDuration = 0.3f;
        protected readonly float finalDelay = 0.1f;

        protected readonly string portalColorParameterName = "Type";

        protected override void Awake() {
            base.Awake();
            InitializePortal();
        }

        protected virtual void InitializePortal() {
            ignoreList = new List<Transform>();
            animator = GetComponent<Animator>();
            if (animator != null) {
                animator.SetInteger(portalColorParameterName, (int) portalColor);
            }

            interactType = YisoHudUIInteractTypes.ENTER;
        }

        /// <summary>
        /// Main Action
        /// AutoActivation이면 TriggerButtonActionCo에서 실행됨
        /// AutoActivation아닌 경우 OnTriggerEnter2D에서 실행됨
        /// </summary>
        public override void TriggerButtonAction() {
            base.TriggerButtonAction();
            Teleport(player.gameObject);
        }

        protected override void TriggerExitAction(GameObject collider) {
            RemoveIgnoreList(collider.transform);
            base.TriggerExitAction(collider);
        }

        protected override void OnTriggerEnter2D(Collider2D collider) {
            if (ignoreList.Contains(collider.transform)) return;
            base.OnTriggerEnter2D(collider);
        }

        #region Core (Teleport)

        /// <summary>
        /// Teleport하고 싶으면 이 함수 호출하면 됨.
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void Teleport(GameObject collider) {
            switch (portalOption) {
                case PortalOption.GoToAnotherPortal:
                    if (connectedPortal != null) {
                        StartCoroutine(GoToAnotherPortalSequenceCo(collider));
                    }

                    break;
                // case PortalOption.GoToNextStage:
                //     StartCoroutine(GoToNextStageSequenceCo(collider));
                //     break;
                case PortalOption.GoToAnotherScene:
                    StartCoroutine(GoToAnotherSceneSequenceCo(collider));
                    break;
            }
        }

        #endregion

        #region Go To Another Portal

        protected virtual IEnumerator GoToAnotherPortalSequenceCo(GameObject collider) {
            SequenceStart();
            yield return new WaitForSeconds(initialDelay);
            yield return FadeOut().WaitForCompletion();;
            TeleportCollider(collider);
            yield return new WaitForSeconds(afterTeleportDelay);
            yield return FadeIn().WaitForCompletion();;
            AfterFadeIn(collider);
            yield return new WaitForSeconds(finalDelay);
            SequenceEnd(collider);
        }

        protected virtual void SequenceStart() {
            YisoCameraEvent.Trigger(YisoCameraEventTypes.StopFollowing);
            player?.Freeze(YisoCharacterStates.FreezePriority.Portal);
        }

        protected virtual TweenerCore<float, float, FloatOptions> FadeOut() {
            return GameManager.Instance.Fade(true, fadeOutDuration);
        }

        protected virtual void TeleportCollider(GameObject collider) {
            collider.transform.position = connectedPortal.transform.position + connectedPortal.exitOffset;
            RemoveIgnoreList(collider.transform);
            connectedPortal.AddToIgnoreList(collider.transform, 1f);
            onTeleport?.Invoke();
        }

        protected virtual TweenerCore<float, float, FloatOptions> FadeIn() {
            YisoCameraEvent.Trigger(YisoCameraEventTypes.StartFollowing);
            return GameManager.Instance.Fade(false, fadeInDuration);
        }


        protected virtual void AfterFadeIn(GameObject collider) {
        }

        protected virtual void SequenceEnd(GameObject collider) {
            player?.UnFreeze(YisoCharacterStates.FreezePriority.Portal);
            if (destroyAfterUse) {
                Destroy(gameObject);
            }
        }

        #endregion

        // #region Go To Next Stage
        //
        // protected virtual IEnumerator GoToNextStageSequenceCo(GameObject collider) {
        //     // 1.Sequence Start
        //     player?.Freeze(YisoCharacterStates.FreezePriority.Portal);
        //
        //     yield return new WaitForSeconds(initialDelay);
        //
        //     // 2.Show Pop up
        //     if (!GameManager.HasInstance) yield break;
        //     if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Story) {
        //         if (!StageManager.HasInstance) yield break;
        //         StageManager.Instance.ShowStageClearPopup();
        //     }
        //     else if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Bounty) {
        //         if (!BountyManager.HasInstance) yield break;
        //         BountyManager.Instance.ShowBountyClearPopup();
        //     }
        // }
        //
        // #endregion

        #region Go To Another Scene

        protected virtual IEnumerator GoToAnotherSceneSequenceCo(GameObject collider) {
            // 1.Sequence Start
            player?.Freeze(YisoCharacterStates.FreezePriority.Portal);

            yield return new WaitForSeconds(initialDelay);

            // 2.Show Pop up
            var sceneName = "";
            var nextScene = destinationScene;
            switch (destinationScene) {
                case YisoSceneTypes.NONE:
                case YisoSceneTypes.INIT:
                case YisoSceneTypes.UI:
                    nextScene = YisoSceneTypes.INIT;
                    sceneName = "초기 화면으로";
                    break;
                case YisoSceneTypes.GAME:
                case YisoSceneTypes.STORY:
                case YisoSceneTypes.COOP:
                    nextScene = YisoSceneTypes.STORY;
                    sceneName = "스토리 모드로";
                    break;
                case YisoSceneTypes.BASE_CAMP:
                    nextScene = YisoSceneTypes.BASE_CAMP;
                    sceneName = "베이스 캠프로";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            YisoServiceProvider.Instance.Get<IYisoPopupUIService>()
                .AlertS("알림", $"{sceneName} 이동하시겠습니까?",
                    () => { YisoServiceProvider.Instance.Get<IYisoSceneService>().LoadScene(nextScene); },
                    () => player?.UnFreeze(YisoCharacterStates.FreezePriority.Portal));
        }

        #endregion

        #region Ignore

        public virtual void AddToIgnoreList(Transform objectToIgnore) {
            if (!ignoreList.Contains(objectToIgnore)) {
                ignoreList.Add(objectToIgnore);
            }
        }

        public virtual void AddToIgnoreList(Transform objectToIgnore, float coolDownTime) {
            if (!ignoreList.Contains(objectToIgnore)) {
                ignoreList.Add(objectToIgnore);
                StartCoroutine(RemoveFromIgnoreListAfterDelayCo(objectToIgnore, coolDownTime));
            }
        }

        protected virtual void RemoveIgnoreList(Transform objectToIgnore) {
            if (ignoreList.Contains(objectToIgnore)) {
                ignoreList.Remove(objectToIgnore);
            }
        }

        private IEnumerator RemoveFromIgnoreListAfterDelayCo(Transform objectToIgnore, float coolDownTime) {
            if (coolDownTime <= 0f || objectToIgnore == null) yield break;
            yield return new WaitForSeconds(coolDownTime);
            RemoveIgnoreList(objectToIgnore);
        }

        #endregion
    }
}