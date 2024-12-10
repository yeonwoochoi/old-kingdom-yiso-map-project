using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Character.Ability;
using Character.Core;
using Character.Health.Damage;
using Character.Health.Damage.Resistance;
using Core.Behaviour;
using Core.Domain.Actor.Attack;
using Manager.Modules;
using Sirenix.OdinInspector;
using Spawn;
using Tools.Event;
using Tools.Feedback;
using Tools.Feedback.Core;
using Tools.Layer;
using UI.Components;
using UnityEngine;
using Utils.Beagle;

namespace Character.Health {
    /// <summary>
    /// 전역적으로 Health Change Event 등록하고 싶으면 여기다 하면 됨
    /// </summary>
    public struct HealthChangedEvent {
        public YisoHealth affectedHealth;
        public float newHealth;

        public HealthChangedEvent(YisoHealth affectedHealth, float newHealth) {
            this.affectedHealth = affectedHealth;
            this.newHealth = newHealth;
        }

        private static HealthChangedEvent e;

        public static void Trigger(YisoHealth affectedHealth, float newHealth) {
            e.affectedHealth = affectedHealth;
            e.newHealth = newHealth;
            YisoEventManager.TriggerEvent(e);
        }
    }

    [AddComponentMenu("Yiso/Character/Core/Health")]
    public class YisoHealth : RunIBehaviour {
        [Title("Setting")] public GameObject model;
        public Animator targetAnimator;
        public bool isPermanentlyInvulnerable = false;
        [ReadOnly] public bool isInvulnerable = false; // true면 데미지 안 받음 (무적)

        [Title("Health")] public float currentHealth;
        public float initialHealth = 10;
        public float maximumHealth = 10;
        public bool resetHealthOnEnable = true; // OnEnable에서 Health Reset될지 말지

        [Title("Feedback")] public YisoFeedBacks damageFeedbacks;
        public YisoFeedBacks deathFeedbacks;

        [Title("KnockBack")] public bool immuneToKnockBack = false;
        public bool immuneToKnockBackIfZeroDamage = false;
        public float knockBackForceMultiplier = 1f;

        [Title("Death")] public bool destroyOnDeath = true;
        public float delayBeforeDestruction = 3f;
        public bool respawnAtInitialLocation = false;
        public bool disableControllerOnDeath = true;
        public bool disableModelOnDeath = false;
        public bool disableCollisionsOnDeath = true;
        public bool disableChildCollisionsOnDeath = false;

        [ShowIf("disableChildCollisionsOnDeath")]
        public Collider2D[] disableColliders;

        public bool changeLayerOnDeath = true;

        [Title("Layer")] public YisoLayer layerOnEnable;
        public YisoLayer layerOnDeath;

        [Title("Resistance")] public YisoDamageResistanceProcessor targetDamageResistanceProcessor;

        public float LastDamage { get; set; }
        public Vector3 LastDamageDirection { get; set; }
        public bool IsAlive => currentHealth > 0;

        public delegate void OnHitDelegate(GameObject attacker);

        public OnHitDelegate onHit;

        public delegate void OnReviveDelegate();

        public OnReviveDelegate onRevive;

        public delegate void OnDeathDelegate();

        public OnDeathDelegate onDeath;


        protected bool initialized = false;
        protected Vector3 initialPosition;
        protected Renderer renderer;
        protected YisoCharacter character;
        protected YisoCharacterMovement characterMovement;
        protected YisoCharacterDeath characterDeath;
        protected TopDownController controller;
        protected YisoHealthBar healthBar;
        protected YisoCharacterAutoRespawn autoRespawn;
        protected Collider2D collider2D;

        /// <summary>
        /// Damage Over Time (ex.독 데미지)
        /// Interruptible (ex.해독제)
        /// </summary>
        protected class InterruptibleDamageOverTimeCoroutine {
            public Coroutine damageOverTimeCoroutine;
            public YisoDamageType damageOverTimeType;
        }

        protected List<InterruptibleDamageOverTimeCoroutine> damageOverTimeCoroutines;
        protected List<InterruptibleDamageOverTimeCoroutine> interruptiblesDamageOverTimeCoroutines;

        #region Initialization

        protected override void Awake() {
            Initialization();
            InitializeCurrentHealth();
        }

        /// <summary>
        /// Revive될때 호출
        /// </summary>
        protected override void OnEnable() {
            if (resetHealthOnEnable) {
                InitializeCurrentHealth();
            }

            if (model != null) {
                model.SetActive(true);
            }

            SetSortingLayer(layerOnEnable);
            DamageEnabled();
            ShowHealthBar();
        }

        protected override void Start() {
            SetAnimator();
            InitializeAnimatorParameters();
        }

        protected override void OnDisable() {
            CancelInvoke();
            HideHealthBar();
        }

        public virtual void Initialization() {
            // Get Yiso Character
            character = gameObject.GetComponentInParent<YisoCharacter>();

            // Activate Model
            if (model != null) {
                model.SetActive(true);
            }

            // Get Renderer
            if (gameObject.GetComponentInParent<Renderer>() != null) {
                renderer = GetComponentInParent<Renderer>();
            }

            // Get another Components
            autoRespawn = gameObject.GetComponentInParent<YisoCharacterAutoRespawn>();
            healthBar = gameObject.GetComponentInParent<YisoHealthBar>();
            controller = gameObject.GetComponentInParent<TopDownController>();
            collider2D = gameObject.GetComponentInParent<Collider2D>();

            // Get Character Movement
            if (character != null) {
                characterDeath = character.FindAbility<YisoCharacterDeath>();
                characterMovement = character.FindAbility<YisoCharacterMovement>();
                if (character.characterModel != null) {
                    if (character.characterModel.GetComponentInChildren<Renderer>() != null) {
                        // 즉 부모, 자식 컴포넌트까지 다 훑어서 Renderer 찾는 과정임
                        renderer = character.characterModel.GetComponentInChildren<Renderer>();
                    }
                }
            }

            // Set Initial Layer
            SetSortingLayer(layerOnEnable);

            // Initialize Damage Over Time List
            damageOverTimeCoroutines = new List<InterruptibleDamageOverTimeCoroutine>();
            interruptiblesDamageOverTimeCoroutines = new List<InterruptibleDamageOverTimeCoroutine>();

            // Initialize Feedbacks
            damageFeedbacks?.Initialization(gameObject);
            deathFeedbacks?.Initialization(gameObject);

            // Store Initial Position
            StoreInitialPosition();

            // Done
            initialized = true;

            // Damaged Enabled
            DamageEnabled();
        }

        public virtual void StoreInitialPosition() {
            initialPosition = transform.position;
        }

        public virtual void InitializeCurrentHealth() {
            SetHealth(initialHealth);
        }

        #endregion

        #region Animator

        public virtual void SetAnimator() {
            if (targetAnimator == null) {
                BindAnimator();
            }

            if (targetAnimator != null) {
                targetAnimator.logWarnings = false;
            }
        }

        protected virtual void BindAnimator() {
            if (character != null) {
                if (character.characterAnimator != null) {
                    targetAnimator = character.characterAnimator;
                }
                else {
                    targetAnimator = GetComponent<Animator>();
                }
            }
            else {
                targetAnimator = GetComponent<Animator>();
            }
        }

        protected virtual void InitializeAnimatorParameters() {
            if (targetAnimator == null || character == null) return;
        }

        #endregion

        #region Core

        /// <summary>
        /// 데미지
        /// </summary>
        /// <param name="damageResult"></param>
        /// <param name="attacker"></param>
        /// <param name="invincibilityDuration"></param>
        /// <param name="damageDirection">데미지 받은 방향</param>
        /// <param name="typedDamages"></param>
        public virtual void Damage(YisoAttack damageResult, GameObject attacker, float invincibilityDuration,
            Vector3 damageDirection, bool selfDamage = false, List<YisoTypedDamage> typedDamages = null) {
            if (!CanTakeDamageThisFrame()) return;
            var isDeath = false;

            ApplyDamage(damageResult, out var currentHp, out isDeath);
            if (currentHp <= 0f) currentHp = 0f;
            SetHealth(currentHp);

            UpdateHealthBar(true);

            onHit?.Invoke(attacker);

            LastDamage = (float) damageResult.TotalDamage;
            LastDamageDirection = damageDirection;

            StartCoroutine(InvokeDamageEffectCo(damageResult, selfDamage, typedDamages));

            // 전역으로 Event 적용하고 싶으면 쓰면 됨
            // MMDamageTakenEvent.Trigger(this, attacker, CurrentHealth, damage, previousHealth);

            damageFeedbacks?.PlayFeedbacks(transform.position);

            if (invincibilityDuration > 0) {
                DamageDisabled();
                StartCoroutine(DamageEnabled(invincibilityDuration));
            }

            ComputeCharacterConditionStateChanges(typedDamages);
            ComputeCharacterMovementMultipliers(typedDamages);

            if (isDeath) {
                currentHealth = 0;
                if (characterDeath == null) Dead();
                else characterDeath.PlayDeathAction(attacker, damageResult, Dead);
            }
        }

        /// <summary>
        /// 회복
        /// </summary>
        public virtual void ReceiveHealth(float health) {
            SetHealth(Mathf.Min(currentHealth + health, maximumHealth));
            UpdateHealthBar(true);
        }

        /// <summary>
        /// 사망
        /// </summary>
        public virtual void Dead() {
            // Set Health 0
            SetHealth(0);

            // Stop All Damage Over Time (poison)
            StopAllDamageOverTime();

            // Disable Damage
            DamageDisabled();

            // Play Death Feedbacks
            deathFeedbacks?.PlayFeedbacks(transform.position);

            // Disable Colliders
            if (disableCollisionsOnDeath) {
                if (collider2D != null) collider2D.enabled = false;
                if (controller != null) controller.CollisionsOff();
                if (disableChildCollisionsOnDeath) {
                    foreach (var collider in gameObject.GetComponentsInChildren<Collider2D>()) {
                        collider.enabled = false;
                    }
                }
            }

            // Set Sorting Layer
            if (changeLayerOnDeath) SetSortingLayer(layerOnDeath);

            // Invoke Callback
            onDeath?.Invoke();

            // Disable Top down controller
            if (disableControllerOnDeath && controller != null) {
                controller.enabled = false;
            }

            // Inactivate character model 
            if (disableModelOnDeath && model != null) {
                model.SetActive(false);
            }

            // Destroy Object ()
            if (delayBeforeDestruction > 0f) {
                Invoke(nameof(DestroyObject), delayBeforeDestruction);
            }
            else {
                DestroyObject();
            }
        }

        /// <summary>
        /// 부활
        /// </summary>
        public virtual void Revive() {
            if (!initialized) return;

            // Reactivate collider
            if (collider2D != null) collider2D.enabled = true;

            // Reactivate child colliders
            if (disableChildCollisionsOnDeath) {
                if (disableColliders == null || disableColliders.Length == 0) {
                    foreach (var collider in gameObject.GetComponentsInChildren<Collider2D>()) {
                        collider.enabled = true;
                    }
                }
                else {
                    foreach (var disableCollider in disableColliders) {
                        disableCollider.enabled = true;
                    }
                }
            }

            // Reset Layer
            if (changeLayerOnDeath) {
                SetSortingLayer(layerOnEnable);
            }

            // Reset Top Down Controller
            // Reactive parent collider
            if (controller != null) {
                controller.enabled = true;
                controller.CollisionsOn();
                controller.Reset();
            }

            // Set character to respawn location
            if (respawnAtInitialLocation) {
                transform.position = initialPosition;
            }

            if (healthBar != null) {
                healthBar.Initialization();
            }

            Initialization();
            InitializeCurrentHealth();
            onRevive?.Invoke();
        }

        #endregion

        #region Check

        /// <summary>
        /// 이번 Frame에서 데미지 받을 수 있는지 없는지
        /// </summary>
        /// <returns></returns>
        public virtual bool CanTakeDamageThisFrame() {
            if (isPermanentlyInvulnerable) return false;
            if (isInvulnerable) return false;
            if (!enabled) return false;
            if (currentHealth <= 0 && initialHealth != 0) return false;
            if (character != null &&
                character.conditionState.CurrentState == YisoCharacterStates.CharacterConditions.Frozen) return false;
            return true;
        }

        public virtual bool CanGetKnockBack(List<YisoTypedDamage> typedDamages) {
            if (immuneToKnockBack) return false;

            if (targetDamageResistanceProcessor != null) {
                if (targetDamageResistanceProcessor.isActiveAndEnabled) {
                    var checkResistance = targetDamageResistanceProcessor.CheckPreventKnockBack(typedDamages);
                    if (checkResistance) return false;
                }
            }

            return true;
        }

        #endregion

        #region Setter

        /// <summary>
        /// 말 그대로 Health 값만 설정
        /// Update Health Bar 에서 
        /// </summary>
        /// <param name="newValue"></param>
        public virtual void SetHealth(float newValue) {
            currentHealth = newValue;
            UpdateHealthBar(false);
            HealthChangedEvent.Trigger(this, newValue);
        }

        public virtual void ResetHealthToMaxHealth() {
            SetHealth(maximumHealth);
        }

        protected virtual void SetSortingLayer(YisoLayer newLayer) {
            if (renderer != null) {
                renderer.sortingLayerName = newLayer.LayerType.ToString();
                if (renderer.TryGetComponent<PositionRendererSorter>(out var rendererSorter)) {
                    rendererSorter.Offset = newLayer.OrderInLayerOffset;
                }
            }
        }

        #endregion

        #region Damage Over Time

        /// <summary>
        /// Applies damage over time, for the specified amount of repeats
        /// </summary>
        /// <param name="damageResult">데미지 양</param>
        /// <param name="attacker">공격자 데미지 준 오브젝트</param>
        /// <param name="invincibilityDuration">무적 기간</param>
        /// <param name="damageDirection">공격받은 방향</param>
        /// <param name="typedDamages">속성데미지 리스트</param>
        /// <param name="amountOfRepeats">반복 횟수</param>
        /// <param name="durationBetweenRepeats">반복당 사이 간격</param>
        /// <param name="interruptible">도중에 중단 가능한지 여부 (해독제)</param>
        /// <param name="damageType">데미지 유형 SO (전기, 불, 독 등등)</param>
        public virtual void DamageOverTime(YisoAttack damageResult, GameObject attacker,
            float invincibilityDuration, Vector3 damageDirection, List<YisoTypedDamage> typedDamages = null,
            int amountOfRepeats = 0, float durationBetweenRepeats = 1f, bool interruptible = true,
            YisoDamageType damageType = null) {
            if (ComputeDamageOutput((float) damageResult.TotalDamage, typedDamages, false) == 0) return;

            var damageOverTime = new InterruptibleDamageOverTimeCoroutine {
                damageOverTimeType = damageType,
                damageOverTimeCoroutine = StartCoroutine(DamageOverTimeCo(damageResult, attacker,
                    invincibilityDuration, damageDirection, typedDamages, amountOfRepeats, durationBetweenRepeats))
            };

            damageOverTimeCoroutines.Add(damageOverTime);
            if (interruptible) {
                interruptiblesDamageOverTimeCoroutines.Add(damageOverTime);
            }
        }

        protected virtual IEnumerator DamageOverTimeCo(YisoAttack damageResult, GameObject attacker,
            float invincibilityDuration, Vector3 damageDirection, List<YisoTypedDamage> typedDamages = null,
            int amountOfRepeats = 0, float durationBetweenRepeats = 1f) {
            for (var i = 0; i < amountOfRepeats; i++) {
                Damage(damageResult, attacker, invincibilityDuration, damageDirection, false, typedDamages);
                yield return YisoCoroutineUtils.WaitFor(durationBetweenRepeats);
            }
        }

        public virtual void StopAllDamageOverTime() {
            foreach (var coroutine in damageOverTimeCoroutines) {
                StopCoroutine(coroutine.damageOverTimeCoroutine);
            }

            damageOverTimeCoroutines.Clear();
        }

        public virtual void InterruptAllDamageOverTime() {
            foreach (var coroutine in interruptiblesDamageOverTimeCoroutines) {
                StopCoroutine(coroutine.damageOverTimeCoroutine);
            }

            interruptiblesDamageOverTimeCoroutines.Clear();
        }

        public virtual void InterruptAllDamageOverTimeOfType(YisoDamageType damageType) {
            foreach (var coroutine in interruptiblesDamageOverTimeCoroutines) {
                if (coroutine.damageOverTimeType == damageType) {
                    StopCoroutine(coroutine.damageOverTimeCoroutine);
                }
            }

            targetDamageResistanceProcessor?.InterruptDamageOverTime(damageType);
        }

        #endregion

        #region Compute Damage

        /// <summary>
        /// Returns the damage this health should take after processing potential resistances
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="typedDamages"></param>
        /// <param name="damagedApplied">Damage 받았을 때 YisoFeedbacks 실행할건지 말지 (cf. 독 데미지 같이 지속적으로 데미지 받는 경우는 매번 Feedback 실행 안함)</param>
        /// <returns></returns>
        public virtual float ComputeDamageOutput(float damage, List<YisoTypedDamage> typedDamages = null,
            bool damagedApplied = false) {
            if (isPermanentlyInvulnerable) return 0f;
            if (isInvulnerable) return 0f;

            var totalDamage = 0f;

            if (targetDamageResistanceProcessor != null) {
                if (targetDamageResistanceProcessor.isActiveAndEnabled) {
                    totalDamage = targetDamageResistanceProcessor.ProcessDamage(damage, typedDamages, damagedApplied);
                }
            }
            else {
                totalDamage = damage;
                if (typedDamages != null) {
                    foreach (var typedDamage in typedDamages) {
                        totalDamage += typedDamage.DamageCaused;
                    }
                }
            }

            return totalDamage;
        }

        protected virtual void ComputeCharacterConditionStateChanges(List<YisoTypedDamage> typedDamages) {
            if (typedDamages == null || character == null) return;
            foreach (var typedDamage in typedDamages) {
                if (typedDamage.forceCharacterCondition) {
                    if (targetDamageResistanceProcessor != null) {
                        if (targetDamageResistanceProcessor.isActiveAndEnabled) {
                            var checkResistance =
                                targetDamageResistanceProcessor.CheckPreventCharacterConditionChange(typedDamage
                                    .associatedDamageType);
                            if (checkResistance) continue;
                        }
                    }

                    character.ChangeCharacterConditionTemporarily(typedDamage.forcedCondition,
                        typedDamage.forcedConditionDuration, typedDamage.resetControllerForces);
                }
            }
        }

        protected virtual void ComputeCharacterMovementMultipliers(List<YisoTypedDamage> typedDamages) {
            if (typedDamages == null || character == null) return;
            foreach (var typedDamage in typedDamages) {
                if (typedDamage.applyMovementMultiplier) {
                    if (targetDamageResistanceProcessor != null) {
                        if (targetDamageResistanceProcessor.isActiveAndEnabled) {
                            var checkResistance =
                                targetDamageResistanceProcessor.CheckPreventMovementModifier(typedDamage
                                    .associatedDamageType);
                            if (checkResistance) continue;
                        }
                    }

                    characterMovement?.ApplyMovementMultiplier(typedDamage.movementMultiplier,
                        typedDamage.movementMultiplierDuration);
                }
            }
        }

        public virtual Vector3
            ComputeKnockBackForce(Vector3 knockBackForce, List<YisoTypedDamage> typedDamages = null) {
            return targetDamageResistanceProcessor == null
                ? knockBackForce
                : targetDamageResistanceProcessor.ProcessKnockBackForce(knockBackForce, typedDamages);
        }

        #endregion

        #region DamageDisablingAPIs

        public virtual void DamageEnabled() {
            isInvulnerable = false;
        }

        public virtual void DamageDisabled() {
            isInvulnerable = true;
        }

        public virtual IEnumerator DamageEnabled(float delay) {
            yield return YisoCoroutineUtils.WaitFor(delay);
            isInvulnerable = false;
        }

        #endregion

        #region Etc

        protected virtual void DestroyObject() {
            if (autoRespawn == null) {
                if (destroyOnDeath) {
                    gameObject.SetActive(false);
                }
            }
            else {
                autoRespawn.Kill();
            }
        }

        #endregion

        #region Calculate

        protected virtual void ApplyDamage(YisoAttack damageResult, out float currentHp, out bool death) {
            if (character != null && character.CharacterStat != null) {
                death = character.CharacterStat.CombatStat.GetAttack(damageResult).Death;
                currentHp = character.CharacterStat.CombatStat.GetHp();
                return;
            }

            currentHp = currentHealth - (float) damageResult.TotalDamage;
            death = currentHp <= 0f;
        }

        #endregion

        #region Health Bar

        /// <summary>
        /// Current Health를 Health Bar에 반영
        /// </summary>
        /// <param name="show">항상 Health Bar를 보여주는 캐릭터(이소)도 있지만,
        /// 맞았을때 일정시간동안만 Health Bar가 떠있는 캐릭도 있음(일반 몹)
        /// 즉, 후자의 경우 Health Bar 업뎃 하면서 Health Bar를 일정 시간동안 보여줄 지 말지
        /// </param>
        public virtual void UpdateHealthBar(bool show) {
            if (healthBar != null) {
                healthBar.UpdateBar(currentHealth, 0f, maximumHealth, show);
            }
        }

        protected virtual void ShowHealthBar() {
            if (healthBar != null) {
                healthBar.ShowBar(true);
            }
        }

        protected virtual void HideHealthBar() {
            if (healthBar != null) {
                healthBar.ShowBar(false);
            }
        }

        #endregion

        #region Damage Effect

        protected virtual IEnumerator InvokeDamageEffectCo(YisoAttack damageResult, bool selfDamage, List<YisoTypedDamage> typedDamages = null) {
            var damages = damageResult.IsSingleAttack
                ? new[] {(damageResult.TotalDamage, damageResult.ExistCritical)} // 단일 공격
                : damageResult.Damages.Select(d => (d.Damage, d.IsCritical)); // 다중 공격

            foreach (var (damageValue, isCritical) in damages) {
                var singleDamage = ComputeDamageOutput((float) damageValue, typedDamages, true);
                if (!selfDamage) {
                    DamageEffectEvent.Trigger(transform.position, singleDamage,
                        isCritical); // floating damage effect (text로)
                }

                yield return damageResult.IsSingleAttack ? null : new WaitForSeconds(0.1f);
            }
        }

        #endregion
    }
}