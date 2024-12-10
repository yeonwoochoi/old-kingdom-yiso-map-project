using Character.Core;
using Core.Behaviour;
using Core.Service;
using Core.Service.UI.HUD;
using Cutscene.Scripts.Control.Cutscene;
using Manager;
using Sirenix.OdinInspector;
using Tools.Event;
using Unity.VisualScripting;
using UnityEngine;
using Utils.Beagle;

namespace Controller.UI {
    [AddComponentMenu("Yiso/Controller/UI/RevivalHUDController")]
    public class YisoRevivalHUDController : RunIBehaviour, IYisoEventListener<YisoCutsceneStateChangeEvent> {
        [Title("Settings")] public float reviveTriggerRadius = 4f;

        protected YisoCharacter character;
        protected CircleCollider2D circleCollider2D;
        protected bool isRevived = false;
        protected YisoCharacter player;

        protected const string SpellCastAnimationParameterName = "IsSpellCast";
        protected int spellCastAnimationParameter;

        public bool IsSpellCast { get; protected set; } = false;

        protected override void Awake() {
            character = gameObject.GetComponentInParent<YisoCharacter>();
        }

        protected override void Start() {
            circleCollider2D = gameObject.GetOrAddComponent<CircleCollider2D>();
            if (circleCollider2D != null) {
                circleCollider2D.radius = reviveTriggerRadius;
                circleCollider2D.isTrigger = true;
            }

            if (GameManager.HasInstance) {
                YisoAnimatorUtils.AddAnimatorParameterIfExists(GameManager.Instance.Player.Animator,
                    SpellCastAnimationParameterName, out spellCastAnimationParameter,
                    AnimatorControllerParameterType.Bool, GameManager.Instance.Player.AnimatorParameters);
            }
        }

        protected virtual void OnDeath() {
            if (character == null) return;
            if (character.conditionState.CurrentState == YisoCharacterStates.CharacterConditions.Dead) {
                isRevived = false;
            }
        }

        protected virtual void Revive(float progress, bool completed, bool stopped) {
            StartRevive();
            if (completed) {
                character.RespawnAt(YisoCharacter.FacingDirections.South, true);
                YisoServiceProvider.Instance.Get<IYisoHUDUIService>().SwitchToAttack();
                StopRevive();
                isRevived = true;
            }

            if (stopped) {
                StopRevive();
            }
        }

        protected virtual void StartRevive() {
            GameManager.Instance.FreezeCharacter(0f, YisoCharacterStates.FreezePriority.PetRevive, false);
            IsSpellCast = true;
            YisoAnimatorUtils.UpdateAnimatorBool(GameManager.Instance.Player.Animator, spellCastAnimationParameter,
                IsSpellCast, GameManager.Instance.Player.AnimatorParameters);
        }

        protected virtual void StopRevive() {
            GameManager.Instance.UnFreezeCharacter(0f, YisoCharacterStates.FreezePriority.PetRevive, false);
            IsSpellCast = false;
            YisoAnimatorUtils.UpdateAnimatorBool(GameManager.Instance.Player.Animator, spellCastAnimationParameter,
                IsSpellCast, GameManager.Instance.Player.AnimatorParameters);
        }

        protected virtual void OnTriggerEnter2D(Collider2D other) {
            if (character == null) return;
            if (character.conditionState.CurrentState != YisoCharacterStates.CharacterConditions.Dead) return;
            if (isRevived) return;
            if (!LayerManager.CheckIfInLayer(other.gameObject, LayerManager.PlayerLayerMask)) return;
            YisoServiceProvider.Instance.Get<IYisoHUDUIService>().SwitchToRevive(Revive);
        }

        protected virtual void OnTriggerExit2D(Collider2D other) {
            if (character == null) return;
            if (!LayerManager.CheckIfInLayer(other.gameObject, LayerManager.PlayerLayerMask)) return;
            YisoServiceProvider.Instance.Get<IYisoHUDUIService>().SwitchToAttack();
            if (IsSpellCast) StopRevive();
        }

        public void OnEvent(YisoCutsceneStateChangeEvent e) {
            if (IsSpellCast) StopRevive();
        }

        protected override void OnEnable() {
            base.OnEnable();
            character.conditionState.OnStateChange += OnDeath;
            isRevived = false;
            this.YisoEventStartListening();
        }

        protected override void OnDisable() {
            base.OnDisable();
            character.conditionState.OnStateChange -= OnDeath;
            this.YisoEventStopListening();
        }
    }
}