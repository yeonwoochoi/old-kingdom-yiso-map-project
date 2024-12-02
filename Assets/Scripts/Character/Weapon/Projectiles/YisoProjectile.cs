using System.Collections;
using Character.Health;
using Character.Health.Damage;
using Character.Weapon.Weapons.Base;
using Core.Service;
using Core.Service.ObjectPool;
using Sirenix.OdinInspector;
using Tools.Pool;
using UnityEngine;
using Utils.Beagle;

namespace Character.Weapon.Projectiles {
    [AddComponentMenu("Yiso/Weapons/Projectile/Projectile")]
    public class YisoProjectile : YisoPoolableObject {
        [Title("Movement")] public float speed = 200f; // current speed of this object
        public float acceleration = 0f;
        public Vector3 direction = Vector3.left; // current Direction of this object

        [Title("Spawn")]
        public float
            initialInvulnerabilityDuration = 0f; // the initial delay during which the projectile can't be destroyed

        public bool damageOwner = false;

        protected YisoWeapon weapon;
        protected GameObject owner;
        protected Vector3 movement;
        protected SpriteRenderer spriteRenderer;
        protected YisoDamageOnTouch damageOnTouch;
        protected Collider2D collider2D;
        protected Rigidbody2D rigidBody2D;
        protected YisoHealth health;
        protected bool canMove = true; // ex. Owner가 죽었을 때 => canMove = false

        protected float initialSpeed;

        protected YisoWeaponProjectile.ProjectileMovementType movementType =
            YisoWeaponProjectile.ProjectileMovementType.Direction;

        protected bool initialFlipX = false;
        protected Vector3 initialLocalScale;
        protected WaitForSeconds initialInvulnerabilityDurationWFS;

        private float projectileFlightElapsedTime = 0f; // projectile이란 오브젝트가 날아가는데 출발시점으로부터 얼마나 걸렸는지 시간을 저장
        private Vector3 targetPosition = Vector3.zero;
        private bool targeted = false;

        #region Public API

        public virtual void SetOwner(GameObject newOwner) {
            owner = newOwner;
            if (damageOnTouch == null) {
                damageOnTouch = gameObject.YisoGetComponentNoAlloc<YisoDamageOnTouch>();
            }

            if (damageOnTouch != null) {
                damageOnTouch.owner = newOwner;
                if (!damageOwner) {
                    damageOnTouch.ClearIgnoreList();
                    damageOnTouch.StartIgnoreObject(newOwner);
                }
            }
        }

        public virtual void SetWeapon(YisoWeapon newWeapon) {
            weapon = newWeapon;
        }

        public virtual void SetMovementMode(YisoWeaponProjectile.ProjectileMovementType moveType,
            GameObject target = null) {
            movementType = moveType;
            if (movementType == YisoWeaponProjectile.ProjectileMovementType.Target && target != null) {
                targetPosition = target.transform.position;
                targeted = true;
            }
            else {
                targeted = false;
            }
        }

        public virtual void SetDamage(float minDamage, float maxDamage) {
            if (damageOnTouch != null) {
                damageOnTouch.minDamageCaused = minDamage;
                damageOnTouch.maxDamageCaused = maxDamage;
            }
        }

        public virtual void SetDamageDelegate(YisoDamageOnTouch.DamageCalculateDelegate damageCalculateDelegate) {
            if (damageOnTouch != null) {
                damageOnTouch.damageCalculate = damageCalculateDelegate;
            }
        }

        /// <summary>
        /// Set Direction
        /// </summary>
        /// <param name="newDirection"></param>
        /// <param name="newRotation"></param>
        public virtual void SetDirection(Vector3 newDirection, Quaternion newRotation) {
            // 투척물 rotation 변경
            transform.rotation = newRotation;

            // 이 투척물 맞은 Collider의 Knock back 방향 결정
            direction = newDirection;
            if (damageOnTouch != null) {
                damageOnTouch.SetKnockBackScriptDirection(newDirection);
            }
        }

        public virtual void Stop() {
            if (collider2D != null) collider2D.enabled = false;
            canMove = false;
        }

        #endregion

        #region Initialization

        protected override void Awake() {
            initialSpeed = speed;
            health = GetComponent<YisoHealth>();
            collider2D = GetComponent<Collider2D>();
            rigidBody2D = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            damageOnTouch = GetComponent<YisoDamageOnTouch>();
            initialInvulnerabilityDurationWFS = new WaitForSeconds(initialInvulnerabilityDuration);
            if (spriteRenderer != null) initialFlipX = spriteRenderer.flipX;
            initialLocalScale = transform.localScale;
        }

        protected virtual void Initialization() {
            speed = initialSpeed;
            if (spriteRenderer != null) spriteRenderer.flipX = initialFlipX;
            transform.localScale = initialLocalScale;
            canMove = true;
            damageOnTouch?.InitializeFeedbacks();
            if (collider2D != null) collider2D.enabled = true;
        }

        /// <summary>
        /// 초기 무적 상태 (ex. Arrow 발사 초기 0.5f 동안 Disable 안됨)
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator InitialInvulnerability() {
            if (damageOnTouch == null) yield break;
            if (weapon == null) yield break;
            damageOnTouch.ClearIgnoreList();
            damageOnTouch.StartIgnoreObject(weapon.Owner.gameObject);
            yield return initialInvulnerabilityDurationWFS;
            if (damageOwner) {
                damageOnTouch.StopIgnoreObject(weapon.Owner.gameObject);
            }
        }

        #endregion

        #region Update

        public override void OnFixedUpdate() {
            if (canMove) {
                Move();
            }
        }

        protected virtual void Move() {
            movement = direction * (speed / 10) * Time.deltaTime;
            if (rigidBody2D != null) {
                if (movementType == YisoWeaponProjectile.ProjectileMovementType.Direction || !targeted) {
                    rigidBody2D.MovePosition(transform.position + movement);
                }
                else {
                    rigidBody2D.MovePosition(Vector2.MoveTowards(transform.position, targetPosition,
                        (speed / 10) * Time.deltaTime));
                }
            }

            speed += acceleration * Time.deltaTime;
        }

        #endregion

        protected virtual void OnDeath() {
            Stop();
        }

        protected override void OnEnable() {
            base.OnEnable();
            Initialization();
            if (initialInvulnerabilityDuration > 0) {
                StartCoroutine(InitialInvulnerability());
            }

            if (health != null) {
                health.onDeath += OnDeath;
            }
        }

        protected override void OnDisable() {
            base.OnDisable();
            if (health != null) {
                health.onDeath -= OnDeath;
            }

            YisoServiceProvider.Instance.Get<IYisoObjectPoolService>().ReleaseObject(gameObject);
        }
    }
}