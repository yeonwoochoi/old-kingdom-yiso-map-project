using System.Collections;
using System.Collections.Generic;
using Character.Ability;
using Character.Core;
using Core.Behaviour;
using Sirenix.OdinInspector;
using Tools.Feedback;
using Tools.Feedback.Core;
using UI.HUD.Interact;
using UnityEngine;
using UnityEngine.Events;
using Utils.Beagle;

namespace Tools.Environment {
    public abstract class YisoActionTriggerZone : RunIBehaviour {
        [Title("Settings")] public bool activable = true; // false면 작동 안함
        public bool autoActivation = false; // 버튼을 누르지 않고도 바로 Action Trigger (ex. 포탈)
        [ShowIf("autoActivation")] public float autoActivationDelay = 0f;

        public bool
            onlyOneActivationAtOnce = true; // 하나의 object에게만 trigger됨. 다른 object가 들어와도 이 object가 나갈때까지 trigger안됨.

        [Title("Requirement")] public LayerMask targetLayerMask = ~0;
        public bool isActivationForCharacter = true;
        [ShowIf("isActivationForCharacter")] public bool isActivationForPlayerOnly = true;

        [Title("Feedbacks")] public YisoFeedBacks activationFeedback;
        public YisoFeedBacks activationFailedFeedback;

        [Title("Actions")] public UnityEvent onActivation;
        public UnityEvent onStay;
        public UnityEvent onExit;

        [Title("Number Of Activation")] public bool unlimitedActivations = true;
        [ShowIf("@!unlimitedActivations")] public int maxNumberOfActivations = 1;
        public float delayBetweenUses = 0f;

        protected YisoHudUIInteractTypes interactType;
        protected Collider2D collider2D;
        protected List<GameObject> collidingObjects;
        protected bool staying = false;
        protected bool autoActivationInProgress = false;
        protected YisoCharacter player;
        protected Coroutine autoActivationCoroutine;
        protected float lastActivationTimeStamp;
        protected int numberOfActivationLeft;
        protected YisoCharacterAreaButtonActivator characterAreaButtonActivator;

        #region Initialization

        protected override void OnEnable() {
            base.OnEnable();
            Initialization();
        }

        protected virtual void Initialization() {
            collider2D = gameObject.GetComponent<Collider2D>();
            collidingObjects = new List<GameObject>();
            numberOfActivationLeft = maxNumberOfActivations;

            activationFeedback?.Initialization(gameObject);
            activationFailedFeedback?.Initialization(gameObject);
        }

        #endregion

        #region Core

        /// <summary>
        /// Auto Activation (버튼을 누르지 않아도 실행) 되는 경우 (ex.포탈) 이 코루틴을 실행시킴
        /// Trigger Enter되면 Delay이후 바로 Trigger Action
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator TriggerButtonActionCo() {
            if (autoActivationDelay <= 0f) {
                TriggerButtonAction();
                yield break;
            }

            autoActivationInProgress = true;
            yield return new WaitForSeconds(autoActivationDelay);
            autoActivationInProgress = false;
            TriggerButtonAction();
        }

        /// <summary>
        /// Main Action
        /// 이거 상속받아서 사용하면 됨
        /// AutoActivation이면 TriggerButtonActionCo에서 실행됨
        /// AutoActivation 아닌 경우 CharacterAreaButtonActivator에서 실행됨
        /// </summary>
        public virtual void TriggerButtonAction() {
            if (!CheckNumberOfUses()) {
                TriggerError();
                return;
            }

            staying = true;
            ActivateArea();
        }

        protected virtual void ActivateArea() {
            lastActivationTimeStamp = Time.time;

            onActivation?.Invoke();
            activationFeedback?.PlayFeedbacks(transform.position);

            numberOfActivationLeft--;
        }

        protected virtual void TriggerStayAction() {
            if (staying && onStay != null) {
                onStay.Invoke();
            }
        }

        protected virtual void TriggerExitAction(GameObject collider) {
            staying = false;
            onExit?.Invoke();
        }

        protected virtual void TriggerError() {
            // TODO: Popup을 띄우든 Button이 흔들리는 Animation을 실행하든지 해서 Trigger를 할 수 없다는 것을 보여줘야함
            activationFailedFeedback?.PlayFeedbacks(transform.position);
        }

        #endregion

        #region Update

        public override void OnUpdate() {
            base.OnUpdate();
            TriggerStayAction();
        }

        #endregion

        #region Colliding

        protected virtual void OnTriggerEnter2D(Collider2D collider) {
            if (!CheckAreaActivation(collider.gameObject)) return;

            collidingObjects.Add(collider.gameObject);
            if (!CheckIfLastCollider(collider.gameObject)) return;

            var character = collider.gameObject.YisoGetComponentNoAlloc<YisoCharacter>();
            if (character == null) return;
            if (character.characterType == YisoCharacter.CharacterTypes.Player) player = character;
            characterAreaButtonActivator = character.FindAbility<YisoCharacterAreaButtonActivator>();
            if (characterAreaButtonActivator != null) {
                characterAreaButtonActivator.InteractType = interactType;
                characterAreaButtonActivator.IsInButtonActivatedArea = true;
                characterAreaButtonActivator.buttonActivatedArea = this;
                characterAreaButtonActivator.IsAutoActivatedArea = autoActivation;
            }

            if (autoActivation) {
                autoActivationCoroutine = StartCoroutine(TriggerButtonActionCo());
            }
        }

        protected virtual void OnTriggerExit2D(Collider2D collider) {
            if (!CheckAreaActivation(collider.gameObject)) return;

            collidingObjects.Remove(collider.gameObject);
            if (!CheckIfLastCollider(collider.gameObject)) return;

            autoActivationInProgress = false;
            if (autoActivationCoroutine != null) StopCoroutine(autoActivationCoroutine);

            characterAreaButtonActivator = collider.gameObject.YisoGetComponentNoAlloc<YisoCharacter>()
                ?.FindAbility<YisoCharacterAreaButtonActivator>();
            if (characterAreaButtonActivator != null) {
                characterAreaButtonActivator.IsInButtonActivatedArea = false;
                characterAreaButtonActivator.IsAutoActivatedArea = false;
                characterAreaButtonActivator.buttonActivatedArea = null;
            }

            TriggerExitAction(collider.gameObject);
        }

        #endregion

        #region Check

        /// <summary>
        /// Area를 활성화시킬지 여부를 체크
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        protected virtual bool CheckAreaActivation(GameObject collider) {
            if (!YisoLayerUtils.CheckLayerInLayerMask(collider.layer, targetLayerMask)) return false;
            if (isActivationForCharacter) {
                var character = collider.gameObject.YisoGetComponentNoAlloc<YisoCharacter>();
                if (character == null) return false;
                if (isActivationForPlayerOnly) {
                    if (character.characterType != YisoCharacter.CharacterTypes.Player) return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 해당 collider가 마지막으로 남은 충돌 객체인지 확인 
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        protected virtual bool CheckIfLastCollider(GameObject collider) {
            if (onlyOneActivationAtOnce) {
                if (collidingObjects.Count > 0) {
                    var isLastObject = true;
                    foreach (var collidingObject in collidingObjects) {
                        // 나랑 다른 애가 아직 Colliding중
                        if (collidingObject != null && collidingObject != collider) {
                            isLastObject = false;
                        }
                    }

                    return true;
                }
            }

            return true;
        }

        /// <summary>
        /// 말 그대로 Activation 몇 번 남았는지 체크
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckNumberOfUses() {
            if (!activable) return false;
            if (Time.time - lastActivationTimeStamp < delayBetweenUses) return false;
            if (unlimitedActivations) return true;
            if (numberOfActivationLeft > 0) return true;
            return false;
        }

        #endregion

        #region Public API

        public virtual void MakeActivable() {
            activable = true;
        }

        public virtual void MakeUnactivable() {
            activable = false;
        }

        public virtual void ToggleActivable() {
            activable = !activable;
        }

        #endregion
    }
}