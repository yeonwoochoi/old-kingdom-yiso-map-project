using System;
using System.Collections;
using System.Collections.Generic;
using Character.Ability;
using Character.Core;
using Character.Health.Damage;
using Character.Weapon.Aim;
using Character.Weapon.Projectiles;
using Core.Domain.Actor.Attack;
using Core.Service;
using Core.Service.ObjectPool;
using Sirenix.OdinInspector;
using Tools.Collider;
using Tools.Feedback;
using Tools.Feedback.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.Beagle;
using Random = UnityEngine.Random;

namespace Character.Weapon.Weapons.Base {
    public abstract class YisoWeaponProjectile : YisoWeapon {
        /// <summary>
        /// Direction : 조준 방향으로 쭉 나가게끔
        /// Target: Target을 스캔해서 목표점으로 날아가게끔 
        /// </summary>
        public enum ProjectileMovementType {
            Direction,
            Target
        }

        public enum ProjectileSpawnPositionType {
            Transform,
            Vector
        }

        [Title("Projectile")] public GameObject projectilePrefab;
        public int poolSize = 30;
        public int projectilesPerShot = 1; // 한 번 쏠 때 몇 개씩 쏘는지
        public float projectilesPerShotDelayBetween = 0.3f; // projectiles Per Shot 사이 간격
        public ProjectileMovementType projectileMovementType = ProjectileMovementType.Direction;

        public YisoDamageOnTouch.DamageCalculationMethod damageCalculationMethod =
            YisoDamageOnTouch.DamageCalculationMethod.Delegate;

        [ShowIf("damageCalculationMethod", YisoDamageOnTouch.DamageCalculationMethod.Range)]
        public float minDamage = 10f;

        [ShowIf("damageCalculationMethod", YisoDamageOnTouch.DamageCalculationMethod.Range)]
        public float maxDamage = 100f;

        [Title("Detect Area")] [ShowIf("projectileMovementType", ProjectileMovementType.Target)]
        public float detectorAreaRange = 10f; // Scan 범위

        [ShowIf("projectileMovementType", ProjectileMovementType.Target)]
        public LayerMask detectLayerMask; // targeting할 layer

        [ShowIf("projectileMovementType", ProjectileMovementType.Target)] [ReadOnly]
        public GameObject detectedGameObject; // targeting 하는 gameObject 보여줌

        [Title("Spawn Transform")] public float spawnDelay = 0.6f;
        public ProjectileSpawnPositionType projectileSpawnPositionType;

        [ShowIf("projectileSpawnPositionType", ProjectileSpawnPositionType.Vector)]
        public Vector3 projectileSpawnOffsetE = Vector3.right; // Projectile의 소환 위치 East (Vector3)

        [ShowIf("projectileSpawnPositionType", ProjectileSpawnPositionType.Vector)]
        public Vector3 projectileSpawnOffsetW = Vector3.left; // Projectile의 소환 위치 West (Vector3)

        [ShowIf("projectileSpawnPositionType", ProjectileSpawnPositionType.Vector)]
        public Vector3 projectileSpawnOffsetS = Vector3.down; // Projectile의 소환 위치 South (Vector3)

        [ShowIf("projectileSpawnPositionType", ProjectileSpawnPositionType.Vector)]
        public Vector3 projectileSpawnOffsetN = Vector3.up; // Projectile의 소환 위치 North (Vector3)

        [ShowIf("projectileSpawnPositionType", ProjectileSpawnPositionType.Transform)]
        public Transform spawnTransformE; // Projectile의 소환 위치 E (Transform)

        [ShowIf("projectileSpawnPositionType", ProjectileSpawnPositionType.Transform)]
        public Transform spawnTransformW; // Projectile의 소환 위치 W (Transform)

        [ShowIf("projectileSpawnPositionType", ProjectileSpawnPositionType.Transform)]
        public Transform spawnTransformS; // Projectile의 소환 위치 S (Transform)

        [ShowIf("projectileSpawnPositionType", ProjectileSpawnPositionType.Transform)]
        public Transform spawnTransformN; // Projectile의 소환 위치 N (Transform)

        [Title("Spread")] public Vector3 spread = Vector3.zero;

        [Title("Spawn Feedbacks")]
        public List<YisoFeedBacks> spawnFeedbacks = new List<YisoFeedBacks>(); // spawnArrayIndex에 맞게 하나씩 순차적으로 재생됨

        [ReadOnly] public Vector3 spawnPosition = Vector3.zero;

        protected bool poolInitialized = false;
        protected YisoCharacter.FacingDirections currentDirection;
        protected Vector3 defaultProjectileDirection = Vector3.forward;
        protected Vector3 randomSpreadDirection;
        protected int spawnArrayIndex;
        protected YisoDetectArea detectAreaController;
        protected GameObject projectilesHolder;
        public IYisoObjectPoolService PoolService => YisoServiceProvider.Instance.Get<IYisoObjectPoolService>();

        #region Initialization

        public override void Initialization() {
            base.Initialization();
            weaponAim = GetComponent<YisoWeaponAim>();

            // Create Detect Area
            if (projectileMovementType == ProjectileMovementType.Target) {
                var detectArea = new GameObject();
                detectArea.name = name + "DetectArea";
                detectArea.transform.position = transform.position;
                detectArea.transform.rotation = transform.rotation;
                detectArea.transform.SetParent(transform);
                detectArea.transform.localScale = Vector3.one;
                detectArea.layer = gameObject.layer;
                detectAreaController = detectArea.AddComponent<YisoDetectArea>();
                detectAreaController.TargetLayerMask = detectLayerMask;
                detectAreaController.SetOwner(gameObject);
                detectAreaController.SetRange(detectorAreaRange);
                detectAreaController.StartIgnoreObject(owner.gameObject);
                ActivateDetectArea(false);
            }

            // Set projectile object pool
            InitializePool();
            currentDirection = YisoPhysicsUtils.GetDirectionFromVector(weaponAim.CurrentAimAbsolute);
        }

        #endregion

        #region Core

        protected override IEnumerator WeaponUseCo() {
            if (!poolInitialized) yield break;

            // Enable Detect Area
            ActivateDetectArea();

            yield return new WaitForSeconds(spawnDelay);

            // Spawn Projectile
            for (var i = 0; i < projectilesPerShot; i++) {
                // Determine Spawn Position
                DetermineSpawnPosition();
                SpawnProjectile(spawnPosition);
                PlaySpawnFeedbacks();

                spawnArrayIndex++;
                if (spawnArrayIndex >= projectilesPerShot) {
                    spawnArrayIndex = 0;
                }

                yield return new WaitForSeconds(projectilesPerShotDelayBetween);
            }

            // Disable Detect Area
            ActivateDetectArea(false);
            
            // Reset secondary movement
            if (owner.LinkedInputManager != null) owner.LinkedInputManager.ResetSecondaryMovement();
        }

        protected virtual YisoProjectile GetOneProjectile() {
            return PoolService.SpawnObject<YisoProjectile>(projectilePrefab);
        }

        /// <summary>
        /// 한 개만 Object Pool에서 꺼내 Active하는 method
        /// </summary>
        /// <param name="spawnPosition"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected virtual GameObject SpawnProjectile(Vector3 spawnPosition) {
            // Get Projectile from Pooler
            var projectile = GetOneProjectile();
            if (projectile == null) return null;

            var nextGameObject = projectile.gameObject;
            if (nextGameObject == null) return null;

            // Set Projectile Spawn Position
            nextGameObject.transform.position = spawnPosition;

            // Set Weapon, Owner
            if (projectile != null) {
                // Init Projectile
                if (owner != null) projectile.SetOwner(owner.gameObject);
                projectile.SetWeapon(this);
                if (damageCalculationMethod == YisoDamageOnTouch.DamageCalculationMethod.Delegate)
                    projectile.SetDamageDelegate(CalculateDamage);
                if (damageCalculationMethod == YisoDamageOnTouch.DamageCalculationMethod.Range)
                    projectile.SetDamage(minDamage, maxDamage);

                // Set Projectile Movement Type (Direction vs Target)
                detectedGameObject = detectAreaController.TargetObject;
                projectile.SetMovementMode(projectileMovementType, detectedGameObject);
            }

            nextGameObject.gameObject.SetActive(true);

            // Set Direction
            if (projectile != null) {
                Quaternion spreadQuaternion;
                if (projectileMovementType == ProjectileMovementType.Direction || detectedGameObject == null) {
                    randomSpreadDirection.x = Random.Range(-spread.x, spread.x);
                    randomSpreadDirection.y = Random.Range(-spread.y, spread.y);
                    randomSpreadDirection.z = Random.Range(-spread.z, spread.z);
                    spreadQuaternion = Quaternion.Euler(randomSpreadDirection);
                }
                else {
                    var angle = Vector2.SignedAngle(transform.right,
                        detectedGameObject.transform.position - spawnPosition);
                    spreadQuaternion = Quaternion.Euler(Vector3.forward * angle);
                }

                if (owner == null) {
                    projectile.SetDirection(spreadQuaternion * transform.rotation * defaultProjectileDirection,
                        transform.rotation);
                }
                else {
                    if (owner.Orientation2D != null) {
                        var newDirection = spreadQuaternion * transform.right;
                        projectile.SetDirection(newDirection, spreadQuaternion * transform.rotation);
                    }
                }
            }

            return nextGameObject;
        }

        public virtual void DetermineSpawnPosition() {
            currentDirection = YisoPhysicsUtils.GetDirectionFromVector(weaponAim.CurrentAimAbsolute);
            switch (projectileSpawnPositionType) {
                case ProjectileSpawnPositionType.Vector:
                    switch (currentDirection) {
                        case YisoCharacter.FacingDirections.West:
                            spawnPosition = transform.position + transform.rotation * projectileSpawnOffsetW;
                            break;
                        case YisoCharacter.FacingDirections.East:
                            spawnPosition = transform.position + transform.rotation * projectileSpawnOffsetE;
                            break;
                        case YisoCharacter.FacingDirections.North:
                            spawnPosition = transform.position + transform.rotation * projectileSpawnOffsetN;
                            break;
                        case YisoCharacter.FacingDirections.South:
                            spawnPosition = transform.position + transform.rotation * projectileSpawnOffsetS;
                            break;
                        default:
                            spawnPosition = transform.position + transform.rotation * Vector3.zero;
                            break;
                    }

                    break;
                case ProjectileSpawnPositionType.Transform:
                    spawnPosition = currentDirection switch {
                        YisoCharacter.FacingDirections.West => spawnTransformW.position,
                        YisoCharacter.FacingDirections.East => spawnTransformE.position,
                        YisoCharacter.FacingDirections.North => spawnTransformN.position,
                        YisoCharacter.FacingDirections.South => spawnTransformS.position,
                        _ => owner.transform.position
                    };
                    break;
            }
        }

        #endregion

        #region Detect Area

        protected virtual void ActivateDetectArea(bool activate = true) {
            if (projectileMovementType == ProjectileMovementType.Target) {
                detectAreaController.Activate(activate);
            }
        }

        #endregion

        #region Feedback

        protected virtual void PlaySpawnFeedbacks() {
            if (spawnFeedbacks.Count > 0) {
                spawnFeedbacks[spawnArrayIndex]?.PlayFeedbacks();
            }
        }

        #endregion

        #region Pool

        protected virtual void InitializePool() {
            if (poolInitialized) return;
            var parentObjectName = $"[Pooler] {owner.gameObject.name} {projectilePrefab.name}";
            projectilesHolder = GameObject.Find(parentObjectName);
            if (projectilesHolder == null) projectilesHolder = new GameObject(parentObjectName);
            SceneManager.MoveGameObjectToScene(projectilesHolder, owner.gameObject.scene);
            PoolService.WarmPool(projectilePrefab, poolSize, projectilesHolder);
            poolInitialized = true;
        }

        protected virtual void ReleasePool() {
            if (!poolInitialized) return;
            // PoolService.ReleaseAllObject(projectilePrefab);
            if (projectilesHolder != null) projectilesHolder.ReleaseAllChildObjects();
            poolInitialized = false;
        }

        #endregion

        #region Calculate Damage

        protected virtual YisoAttack CalculateDamage(GameObject target) {
            if (target != null && target.TryGetComponent<YisoCharacterStat>(out var targetStat)) {
                return characterStat.CombatStat.CreateAttack(targetStat.CombatStat);
            }

            // Debug.LogWarning($"Target \"{target.name}\" has no character stat ability");
            return new YisoAttack(Random.Range(minDamage, Mathf.Max(maxDamage, minDamage)));
        }

        #endregion

        protected override void OnDestroy() {
            base.OnDestroy();
            ReleasePool();
        }
    }
}