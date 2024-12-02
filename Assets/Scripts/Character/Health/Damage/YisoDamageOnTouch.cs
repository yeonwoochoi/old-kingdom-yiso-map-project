using System;
using System.Collections.Generic;
using Core.Behaviour;
using Core.Domain.Actor.Attack;
using Sirenix.OdinInspector;
using Tools.Collider;
using Tools.Feedback;
using Tools.Feedback.Camera;
using Tools.Feedback.Core;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


using Utils.Beagle;
using Random = UnityEngine.Random;

namespace Character.Health.Damage {
    /// <summary>
    /// Add this component to an object and it will cause damage to objects that collide with it. 
    /// </summary>
    [AddComponentMenu("Yiso/Character/Damage/Damage On Touch")]
    public class YisoDamageOnTouch : RunIBehaviour {
        /// <summary>
        /// 외부에서 Damage on touch를 초기화(생성/설정) 및 On/Off하고 싶을때
        /// </summary>
        [Serializable]
        public class DamageAreaSettings {
            public enum DamageAreaShapes {
                Rectangle,
                Circle,
                Arc
            }

            public enum DamageAreaModes {
                Generated,
                Existing
            }

            [Title("Damage Area")] public DamageAreaModes damageAreaMode = DamageAreaModes.Generated;

            [ShowIf("damageAreaMode", DamageAreaModes.Generated)]
            public DamageAreaShapes damageAreaShape = DamageAreaShapes.Rectangle;

            [ShowIf("damageAreaMode", DamageAreaModes.Generated)]
            public Vector3 areaOffset = new(1, 0);

            [ShowIf("damageAreaMode", DamageAreaModes.Generated)]
            public Vector3 areaSize = new(1, 1);

            [ShowIf("damageAreaShape", DamageAreaShapes.Arc)]
            public float angle = 120f;

            [ShowIf("damageAreaMode", DamageAreaModes.Generated)]
            public TriggerAndCollisionMask triggerFilter = AllowedTriggerCallbacks;

            [ShowIf("damageAreaMode", DamageAreaModes.Generated)]
            public YisoFeedBacks hitDamageableFeedback;

            [ShowIf("damageAreaMode", DamageAreaModes.Generated)]
            public YisoFeedBacks hitNonDamageableFeedback;

            [ShowIf("damageAreaMode", DamageAreaModes.Generated)]
            public YisoFeedBacks hitAnythingFeedback;

            [ShowIf("damageAreaMode", DamageAreaModes.Generated)]
            public DamageCalculationMethod damageCalculationMethodType = DamageCalculationMethod.Delegate;

            [ShowIf("damageAreaMode", DamageAreaModes.Generated)]
            public float minDamage = 10f;

            [ShowIf("damageAreaMode", DamageAreaModes.Generated)]
            public float maxDamage = 100f;

            [ShowIf("damageAreaMode", DamageAreaModes.Existing)]
            public YisoDamageOnTouch existingDamageOnTouch;

            [Title("Damage Caused")] public LayerMask targetLayerMask;
            public bool knockBack = true;
            public Vector3 knockBackForce = new(10f, 2f, 0f);
            public KnockBackDirections knockBackDirection = KnockBackDirections.BasedOnOwnerPosition;
            public float invincibilityDuration = 0.5f;
            public bool canDamageOwner = false;

            protected Collider2D damageAreaCollider2D;
            protected BoxCollider2D boxCollider2D;
            protected CircleCollider2D circleCollider2D;
            protected PolygonCollider2D polygonCollider2D;
            protected ArcCollider2D arcCollider2D;

            public GameObject DamageArea { get; private set; }
            public YisoDamageOnTouch DamageOnTouch { get; private set; }

            public virtual void Initialization(GameObject owner, GameObject parent,
                DamageCalculateDelegate damageCalculateDelegate) {
                if (damageAreaMode == DamageAreaModes.Existing && existingDamageOnTouch != null) {
                    DamageArea = existingDamageOnTouch.gameObject;
                    damageAreaCollider2D = existingDamageOnTouch.gameObject.GetComponent<Collider2D>();
                    DamageOnTouch = existingDamageOnTouch;
                }

                if (damageAreaMode == DamageAreaModes.Generated) {
                    // Create damage area gameObject
                    DamageArea = new GameObject(parent.name + "DamageArea");
                    DamageArea.transform.position = parent.transform.position;
                    DamageArea.transform.rotation = parent.transform.rotation;
                    DamageArea.transform.SetParent(parent.transform);
                    DamageArea.transform.localScale = Vector3.one;
                    DamageArea.layer = parent.layer;

                    // Add Collider2D
                    if (damageAreaShape == DamageAreaShapes.Rectangle) {
                        boxCollider2D = DamageArea.AddComponent<BoxCollider2D>();
                        boxCollider2D.offset = areaOffset;
                        boxCollider2D.size = areaSize;
                        damageAreaCollider2D = boxCollider2D;
                        damageAreaCollider2D.isTrigger = true;
                    }

                    if (damageAreaShape == DamageAreaShapes.Circle) {
                        circleCollider2D = DamageArea.AddComponent<CircleCollider2D>();
                        circleCollider2D.offset = areaOffset;
                        circleCollider2D.radius = areaSize.x / 2;
                        damageAreaCollider2D = circleCollider2D;
                        damageAreaCollider2D.isTrigger = true;
                    }

                    if (damageAreaShape == DamageAreaShapes.Arc) {
                        var rotation = -angle / 2f;
                        if (rotation < 0) rotation += 360;
                        if (rotation > 360) rotation -= 360;
                        polygonCollider2D = DamageArea.AddComponent<PolygonCollider2D>();
                        polygonCollider2D.offset = areaOffset;
                        arcCollider2D = DamageArea.AddComponent<ArcCollider2D>();
                        arcCollider2D.Radius = areaSize.x;
                        arcCollider2D.PizzaSlice = true;
                        arcCollider2D.TotalAngle = angle;
                        arcCollider2D.Thickness = 1f;
                        arcCollider2D.OffsetRotation = rotation;
                        damageAreaCollider2D = polygonCollider2D;
                        polygonCollider2D.isTrigger = true;
                    }

                    // Add Rigidbody2D
                    var rigidbody2D = DamageArea.AddComponent<Rigidbody2D>();
                    rigidbody2D.isKinematic = true;
                    rigidbody2D.sleepMode = RigidbodySleepMode2D.NeverSleep;

                    // Set Damage On Touch
                    DamageOnTouch = DamageArea.AddComponent<YisoDamageOnTouch>();
                    DamageOnTouch.SetGizmoOffset(areaOffset);
                    DamageOnTouch.targetLayerMask = targetLayerMask;
                    DamageOnTouch.damageDirectionMode = DamageDirections.BasedOnOwnerPosition;
                    DamageOnTouch.damageCalculationMethodType = damageCalculationMethodType;
                    DamageOnTouch.minDamageCaused = minDamage;
                    DamageOnTouch.maxDamageCaused = maxDamage;
                    DamageOnTouch.knockBack = knockBack;
                    DamageOnTouch.knockBackForce = knockBackForce;
                    DamageOnTouch.knockBackDirection = knockBackDirection;
                    DamageOnTouch.invincibilityDuration = invincibilityDuration;
                    DamageOnTouch.hitDamageableFeedback = hitDamageableFeedback;
                    DamageOnTouch.hitNonDamageableFeedback = hitNonDamageableFeedback;
                    DamageOnTouch.hitAnythingFeedback = hitAnythingFeedback;
                    DamageOnTouch.triggerFilter = triggerFilter;
                }

                if (DamageOnTouch != null) {
                    DamageOnTouch.damageCalculate = damageCalculateDelegate;
                    if (!canDamageOwner && owner != null) DamageOnTouch.StartIgnoreObject(owner.gameObject);
                }
            }

            public virtual void EnableDamageArea() {
                if (damageAreaCollider2D != null) damageAreaCollider2D.enabled = true;
            }

            public virtual void DisableDamageArea() {
                if (damageAreaCollider2D != null) damageAreaCollider2D.enabled = false;
            }
        }

        [Flags]
        public enum TriggerAndCollisionMask {
            IgnoreAll = 0,
            OnTriggerEnter = 1 << 0,
            OnTriggerStay = 1 << 1,
            OnTriggerEnter2D = 1 << 6,
            OnTriggerStay2D = 1 << 7,

            All_3D = OnTriggerEnter | OnTriggerStay,
            All_2D = OnTriggerEnter2D | OnTriggerStay2D,
            All = All_3D | All_2D
        }

        public enum DamageCalculationMethod {
            Range,
            Delegate
        }

        public enum DamageDirections {
            BasedOnOwnerPosition,
            BasedOnVelocity,
            BasedOnScriptDirection
        }

        public enum KnockBackDirections {
            BasedOnOwnerPosition,
            BasedOnSelfPosition,
            BasedOnScriptDirection
        }

        public const TriggerAndCollisionMask AllowedTriggerCallbacks = TriggerAndCollisionMask.All_2D;

        public delegate YisoAttack DamageCalculateDelegate(GameObject target);

        [Title("Targets")] public LayerMask targetLayerMask;
        [ReadOnly] public GameObject owner;
        public TriggerAndCollisionMask triggerFilter = AllowedTriggerCallbacks;

        [Title("Damage")] public DamageCalculationMethod damageCalculationMethodType = DamageCalculationMethod.Delegate;

        [ShowIf("damageCalculationMethodType", DamageCalculationMethod.Delegate)]
        public DamageCalculateDelegate damageCalculate;

        [ShowIf("damageCalculationMethodType", DamageCalculationMethod.Range)]
        public float minDamageCaused = 10f;

        [ShowIf("damageCalculationMethodType", DamageCalculationMethod.Range)]
        public float maxDamageCaused = 100f;

        // TODO : Yiso Equip Item Domain으로 빼야함
        public List<YisoTypedDamage> typedDamages;
        public DamageDirections damageDirectionMode = DamageDirections.BasedOnVelocity;

        [Title("KnockBack")] public bool knockBack = false;
        [ShowIf("knockBack")] public KnockBackDirections knockBackDirection = KnockBackDirections.BasedOnOwnerPosition;
        [ShowIf("knockBack")] public Vector3 knockBackForce = new Vector3(10, 10, 10);
        [ShowIf("knockBack")] public float criticalKnockbackMultiplier = 2f;

        [Title("Invincibility")] public float invincibilityDuration = 0f;

        [Title("Damage over time")] public bool repeatDamageOverTime = false; // 지속적으로 데미지 입힐건지 (ex. 독 데미지)
        [ShowIf("repeatDamageOverTime")] public int amountOfRepeats = 3;
        [ShowIf("repeatDamageOverTime")] public float durationBetweenRepeats = 1f;
        [ShowIf("repeatDamageOverTime")] public bool damageOverTimeInterruptible = true;
        [ShowIf("repeatDamageOverTime")] public YisoDamageType repeatedDamageType;

        [Title("Damage Taken")]
        // ex. 화살을 예로 설명하면,
        // 화살의 Hp는 1 (YisoHealth도 같이 갖고 있게 할거임)
        // 이 화살이 어떤 물체와 Colliding했을 때 자체 데미지를 (-2) 입게해
        // Death가 되도록 할 예정 -> Damage On Destroy 되도록
        public YisoHealth damageTakenHealth;

        public float damageTakenEveryTime = 0; // 뭐든 부딪혔을때 받는 데미지
        public float damageTakenDamageable = 0; // YisoHealth를 갖고 있고 Hp > 0 인 물체
        public float damageTakenNonDamageable = 0; // YisoHealth를 갖고 있지 않은 물체
        public bool damageTakenKnockBack = false;
        [ShowIf("damageTakenKnockBack")] public Vector3 damageTakenKnockBackForce = Vector3.zero;
        [ShowIf("damageTakenKnockBack")] public float damageTakenInvincibilityDuration = 0.0f;

        [Title("Feedbacks")] public YisoFeedBacks hitDamageableFeedback;
        public YisoFeedBacks hitNonDamageableFeedback;
        public YisoFeedBacks hitAnythingFeedback;

        protected bool initializedFeedbacks = false;
        protected YisoHealth colliderHealth;
        protected TopDownController topDownController;
        protected TopDownController colliderTopDownController;
        protected List<GameObject> ignoredGameObjects; // 말 그대로 Damage 제외할 GameObject 리스트

        protected Vector3 velocity;
        protected Vector3 lastPosition; // velocity 계산할 때 이전 프레임에서의 위치
        protected Vector3 lastDamagePosition; // Damage Direction 계산할 때 이전 프레임에서의 위치
        protected Vector3 lastFramePosition; // knock back 계산할 때 이전 프레임에서의 위치
        protected Vector3 damageDirection; // Collider 입장에서 데미지를 받은 방향 
        protected Vector3 tempKnockBackForce;
        protected Vector3 damageScriptDirection;
        protected Vector3 knockBackScriptDirection;

        #region Initialization

        protected override void Awake() {
            Initialization();
        }

        protected override void OnEnable() {
            base.OnEnable();
            lastPosition = transform.position;
            lastDamagePosition = transform.position;
        }


        protected override void OnDisable() {
            base.OnDisable();
            ClearIgnoreList();
        }

        /// <summary>
        /// 인스펙터 창에서 스크립스의 프로퍼티 수정될 떄마다 호출되는 Event Function
        /// </summary>
        protected virtual void OnValidate() {
            triggerFilter &= AllowedTriggerCallbacks;
        }

        protected virtual void Initialization() {
            InitializeIgnoreList();
            GrabComponents();
            InitializeGizmos();
            InitializeColliders();
            InitializeFeedbacks();
        }

        protected virtual void GrabComponents() {
            if (damageTakenHealth == null) damageTakenHealth = GetComponent<YisoHealth>();
            topDownController = GetComponent<TopDownController>();
            boxCollider2D = GetComponent<BoxCollider2D>();
            circleCollider2D = GetComponent<CircleCollider2D>();
            arcCollider2D = GetComponent<ArcCollider2D>();
            lastDamagePosition = transform.position;
        }

        protected virtual void InitializeColliders() {
            if (boxCollider2D != null) {
                SetGizmoOffset(boxCollider2D.offset);
                boxCollider2D.isTrigger = true;
            }

            if (circleCollider2D != null) {
                SetGizmoOffset(circleCollider2D.offset);
                circleCollider2D.isTrigger = true;
            }

            if (arcCollider2D != null) {
                polygonCollider2D = GetComponent<PolygonCollider2D>();
                SetGizmoOffset(polygonCollider2D.offset);
                polygonCollider2D.isTrigger = true;
            }
        }

        protected virtual void InitializeIgnoreList() {
            ignoredGameObjects ??= new List<GameObject>();
        }

        public virtual void InitializeFeedbacks() {
            if (initializedFeedbacks) return;
            hitDamageableFeedback?.Initialization(gameObject);
            hitNonDamageableFeedback?.Initialization(gameObject);
            hitAnythingFeedback?.Initialization(gameObject);
            initializedFeedbacks = true;
        }

        #endregion

        #region Gizmo

        protected CircleCollider2D circleCollider2D;
        protected BoxCollider2D boxCollider2D;
        protected ArcCollider2D arcCollider2D;
        protected PolygonCollider2D polygonCollider2D;
        protected Color gizmosColor;
        protected Vector3 gizmoOffset;

        protected virtual void InitializeGizmos() {
            gizmosColor = Color.red;
            gizmosColor.a = 0.25f;
        }

        public virtual void SetGizmoOffset(Vector3 newOffset) {
            gizmoOffset = newOffset;
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos() {
            Gizmos.color = gizmosColor;
            Handles.color = gizmosColor;

            if (boxCollider2D != null) {
                if (boxCollider2D.enabled) {
#if UNITY_EDITOR
                    YisoDebugUtils.DrawGizmoCube(transform, gizmoOffset, boxCollider2D.size, false);
#endif
                }
                else {
#if UNITY_EDITOR
                    YisoDebugUtils.DrawGizmoCube(transform, gizmoOffset, boxCollider2D.size, true);
#endif
                }
            }

            if (circleCollider2D != null) {
                Matrix4x4 rotationMatrix = transform.localToWorldMatrix;
                Gizmos.matrix = rotationMatrix;
                if (circleCollider2D.enabled) {
#if UNITY_EDITOR
                    Gizmos.DrawSphere((Vector2) gizmoOffset, circleCollider2D.radius);
#endif
                }
                else {
                    Gizmos.DrawWireSphere((Vector2) gizmoOffset, circleCollider2D.radius);
                }
            }

            if (polygonCollider2D != null) {
                if (polygonCollider2D.enabled) {
                    YisoDebugUtils.DrawGizmoArc(transform, arcCollider2D.TotalAngle, arcCollider2D.Radius, false);
                }
                else {
                    YisoDebugUtils.DrawGizmoArc(transform, arcCollider2D.TotalAngle, arcCollider2D.Radius, true);
                }
            }
        }
#endif

        #endregion

        #region Ignore Object (Public)

        public virtual void StartIgnoreObject(GameObject newIgnoredGameObject) {
            InitializeIgnoreList();
            ignoredGameObjects.Add(newIgnoredGameObject);
        }

        public virtual void StopIgnoreObject(GameObject ignoredGameObject) {
            ignoredGameObjects?.Remove(ignoredGameObject);
        }

        public virtual void ClearIgnoreList() {
            InitializeIgnoreList();
            ignoredGameObjects.Clear();
        }

        #endregion

        #region Update

        public override void OnUpdate() {
            ComputeVelocity();
        }

        public override void OnLateUpdate() {
            lastFramePosition = transform.position;
        }

        protected virtual void ComputeVelocity() {
            // Pause(Time.timescale = 0f)인 경우 Time.deltaTime = 0f가 된다
            if (Time.deltaTime != 0f) {
                // TODO : 왜 반대로 뺐는지 이해가 안됨.
                velocity = (lastPosition - transform.position) / Time.deltaTime;

                // last Damage Position의 경우 (Direction 결정할때 사용됨)
                // 작은 값 변화로 방향이 확확 바뀌는 것을 방지하기 위해
                // 0.5값보다 큰 경우에만 값 업데이트함.
                if (Vector3.Distance(lastDamagePosition, transform.position) > 0.5f) {
                    lastDamagePosition = transform.position;
                }

                lastPosition = transform.position;
            }
        }

        #endregion

        #region CollisionDetection

        public virtual void OnTriggerEnter2D(Collider2D collider) {
            if ((triggerFilter & TriggerAndCollisionMask.OnTriggerEnter2D) == 0) return;
            if (collider.isTrigger) return;
            Colliding(collider.gameObject);
        }

        public virtual void OnTriggerStay2D(Collider2D collider) {
            if ((triggerFilter & TriggerAndCollisionMask.OnTriggerStay2D) == 0) return;
            if (collider.isTrigger) return;
            Colliding(collider.gameObject);
        }

        protected virtual void Colliding(GameObject collider) {
            if (!CheckCollidingAvailability(collider)) return;

            // cache reset
            colliderTopDownController = null;
            colliderHealth = collider.gameObject.YisoGetComponentNoAlloc<YisoHealth>();
            if (colliderHealth == null) {
                // Projectile은 target하는 gameObject가 따로 있음 (맞추기 너무 힘들어서)
                colliderHealth = collider.gameObject.GetComponentInParent<YisoHealth>();
            }

            // Colliding
            if (colliderHealth != null) {
                if (colliderHealth.currentHealth > 0) {
                    // Colliding (Damageable)
                    OnCollideWithDamageable(colliderHealth);
                }
            }
            else {
                // Colliding (Non-Damageable)
                OnCollideWithNonDamageable();
            }

            // Colliding (Anything)
            OnAnyCollision();
        }

        protected virtual void OnCollideWithDamageable(YisoHealth health) {
            if (health.CanTakeDamageThisFrame()) {
                colliderTopDownController = health.gameObject.YisoGetComponentNoAlloc<TopDownController>();

                hitDamageableFeedback?.PlayFeedbacks(transform.position);

                // Mathf.Max(maxDamageCaused, minDamageCaused) : 더 큰 값 반환
                var damageResult = CalculateDamage(health.gameObject);
                if (damageResult.Damages == null || damageResult.Damages.Count == 0) return;
                
                // TODO: 임시로 짠 코드. Feedback으로 통합
                if (owner.CompareTag("Player")) {
                    var intensity = damageResult.ExistCritical ? 4f : 8f;
                    YisoCameraShakeEvent.Trigger(new ShakeItem(.1f, intensity, 1f));
                }

                ApplyKnockBack(damageResult, typedDamages);

                DetermineDamageDirection();

                if (repeatDamageOverTime) {
                    colliderHealth.DamageOverTime(damageResult, gameObject,
                        invincibilityDuration, damageDirection, typedDamages, amountOfRepeats,
                        durationBetweenRepeats, damageOverTimeInterruptible, repeatedDamageType);
                }
                else {
                    colliderHealth.Damage(damageResult, gameObject,
                        invincibilityDuration, damageDirection, false, typedDamages);
                }
            }

            if (damageTakenEveryTime + damageTakenDamageable > 0) {
                SelfDamage(damageTakenEveryTime + damageTakenDamageable);
            }
        }

        protected virtual void OnCollideWithNonDamageable() {
            if (damageTakenEveryTime + damageTakenNonDamageable > 0) {
                SelfDamage(damageTakenEveryTime + damageTakenNonDamageable);
            }

            hitNonDamageableFeedback?.PlayFeedbacks(transform.position);
        }

        protected virtual void OnAnyCollision() {
            hitAnythingFeedback?.PlayFeedbacks(transform.position);
        }

        protected virtual bool CheckCollidingAvailability(GameObject collider) {
            if (!isActiveAndEnabled) return false;
            if (ignoredGameObjects.Contains(collider)) return false;
            if (!YisoLayerUtils.CheckLayerInLayerMask(collider.layer, targetLayerMask)) return false;
            if (Time.time == 0f) return false;
            return true;
        }

        protected virtual void DetermineDamageDirection() {
            switch (damageDirectionMode) {
                case DamageDirections.BasedOnOwnerPosition:
                    if (owner == null) owner = gameObject;
                    damageDirection = colliderHealth.transform.position - owner.transform.position;
                    damageDirection.z = 0f;
                    break;
                case DamageDirections.BasedOnVelocity:
                    damageDirection = transform.position - lastDamagePosition;
                    break;
                case DamageDirections.BasedOnScriptDirection:
                    damageDirection = damageScriptDirection;
                    break;
            }

            damageDirection = damageDirection.normalized;
        }

        #endregion

        #region KnockBack

        public virtual void SetKnockBackScriptDirection(Vector3 newDirection) {
            knockBackScriptDirection = newDirection;
        }

        public virtual void SetDamageScriptDirection(Vector3 newDirection) {
            damageScriptDirection = newDirection;
        }

        /// <summary>
        /// Calculate Knock back force and direction and then, apply knock back
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="typedDamages"></param>
        protected virtual void ApplyKnockBack(YisoAttack damage, List<YisoTypedDamage> typedDamages) {
            if (CheckApplyKnockBack((float) damage.TotalDamage, typedDamages)) {
                // Calculate knock back "force"
                tempKnockBackForce = knockBackForce * colliderHealth.knockBackForceMultiplier *
                                     (damage.ExistCritical ? criticalKnockbackMultiplier : 1f);
                tempKnockBackForce = colliderHealth.ComputeKnockBackForce(tempKnockBackForce, typedDamages);

                // Apply Knock back "direction"
                switch (knockBackDirection) {
                    case KnockBackDirections.BasedOnOwnerPosition:
                        if (owner == null) owner = gameObject;
                        // tempKnockBackForce의 크기는 유지하면서 (maxMagnitudeDelta 파라미터가 0)
                        // 방향만 조정하는 것. (maxRadianDelta 파라미터가 10f)
                        // 360도 = 6.28319 이므로 10f면 다 커버됨.
                        tempKnockBackForce = Vector3.RotateTowards(
                            tempKnockBackForce,
                            (colliderTopDownController.transform.position - owner.transform.position).normalized,
                            10f,
                            0f
                        );
                        break;
                    case KnockBackDirections.BasedOnSelfPosition:
                        tempKnockBackForce = Vector3.RotateTowards(
                            tempKnockBackForce,
                            (colliderTopDownController.transform.position - transform.position).normalized,
                            10f,
                            0f
                        );
                        break;
                    case KnockBackDirections.BasedOnScriptDirection:
                        tempKnockBackForce = knockBackScriptDirection * knockBackForce.magnitude;
                        break;
                }

                // Apply knock back
                if (knockBack) {
                    colliderTopDownController.Impact(tempKnockBackForce.normalized, tempKnockBackForce.magnitude);
                }
            }
        }

        /// <summary>
        /// Can Apply Knock Back
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="typedDamages"></param>
        /// <returns></returns>
        protected virtual bool CheckApplyKnockBack(float damage, List<YisoTypedDamage> typedDamages) {
            if (colliderHealth.immuneToKnockBackIfZeroDamage) {
                if (colliderHealth.ComputeDamageOutput(damage, typedDamages, false) == 0) {
                    return false;
                }
            }

            return colliderTopDownController != null
                   && knockBackForce != Vector3.zero
                   && !colliderHealth.isInvulnerable
                   && colliderHealth.CanGetKnockBack(typedDamages);
        }

        #endregion

        #region Self Damage

        protected virtual void SelfDamage(float damage) {
            if (damageTakenHealth != null) {
                damageDirection = Vector3.up;
                damageTakenHealth.Damage(new YisoAttack(damage), gameObject, damageTakenInvincibilityDuration,
                    damageDirection, true);
            }

            if (topDownController != null && colliderTopDownController != null) {
                var totalVelocity = colliderTopDownController.speed + velocity;
                var knockBackForce =
                    Vector3.RotateTowards(damageTakenKnockBackForce, totalVelocity.normalized, 10f, 0f);

                if (damageTakenKnockBack) {
                    topDownController.Impact(knockBackForce);
                }
            }
        }

        #endregion

        #region Calculate

        protected virtual YisoAttack CalculateDamage(GameObject target) {
            if (damageCalculationMethodType == DamageCalculationMethod.Delegate && damageCalculate != null) {
                return damageCalculate(target);
            }

            return new YisoAttack(Random.Range(minDamageCaused, Mathf.Max(maxDamageCaused, minDamageCaused)));
        }

        #endregion
    }
}