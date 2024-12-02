using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BaseCamp.Animation;

using BaseCamp.Animation;
using Core.Behaviour;
using Core.Logger;
using Core.Service;
using Core.Service.Game;
using Core.Service.Log;
using Core.Service.ObjectPool;
using Domain.Direction;
using Pathfinding;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils.Extensions;

namespace BaseCamp.Player {
    public class YisoBaseCampPlayerController : RunIBehaviour {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField, Title("Prefab")] private GameObject footprintPrefab;
        [SerializeField] private GameObject footprintContent;
        
        private Rigidbody2D rgbd2D;
        internal Vector2 movementInput = Vector2.zero;
        internal YisoBaseCampPlayerAnimator animator;
        internal readonly InteractInfo interactInfo = new();
        private YisoBaseCampPlayerEventHandler eventHandler;
        private YisoObjectDirection direction = YisoObjectDirection.DOWN;
        private YisoBaseCampPlayerMouseController mouseController;

        private Seeker seeker;
        private AIPath aiPath;
        private AIDestinationSetter destinationSetter;

        private IYisoObjectPoolService poolService;
        
        private readonly List<Action> onEnables = new();
        private readonly List<Action> onDisables = new();

        internal bool joystickInput = false;
        
        public bool ActiveInteract { get; set; } = false;

        private bool moving = false;
        private Transform destination = null;

        public bool isMobile = false;

        private float interval = 1f;
        private List<SpriteRenderer> footPrints = new();
        
        protected override void Awake() {
            base.Awake();
            // isMobile = YisoServiceProvider.Instance.Get<IYisoGameService>().IsMobile();
            isMobile = true;
            if (isMobile)
                eventHandler = new YisoBaseCampPlayerEventHandler(this);
            else {
                mouseController = GetComponent<YisoBaseCampPlayerMouseController>();
                SetPathfinding();
            }
        }

        protected override void OnEnable() {
            base.OnEnable();
            foreach (var onEnable in onEnables) onEnable();
            if (!isMobile)
                mouseController.OnMouseClickEvent += OnMouseClick;
        }

        protected override void Start() {
            base.Start();
            rgbd2D = GetComponent<Rigidbody2D>();
            animator = new YisoBaseCampPlayerAnimator(GetComponent<Animator>()) {
                Direction = direction
            };
            poolService = YisoServiceProvider.Instance.Get<IYisoObjectPoolService>();
            poolService.WarmPool(footprintPrefab, 100, footprintContent);
        }

        protected override void OnDisable() {
            base.OnDisable();
            foreach (var onDisable in onDisables) onDisable();
            if (!isMobile)
                mouseController.OnMouseClickEvent -= OnMouseClick;
        }

        public override void OnUpdate() {
            if (isMobile) return;
            if (!moving) return;
            
            var isMet = ReachedDestination(destination, 0.2f, out var moveDir);
            animator.IsMoving = !isMet;

            if (isMet) {
                destinationSetter.target = null;
                destination = null;
                moving = false;
                aiPath.enabled = false;
                destinationSetter.enabled = false;
            } else {
                var hit = Physics2D.Raycast(transform.position, aiPath.velocity.normalized, 0.5f);
                if (hit.collider != null) {
                    seeker.StartPath(transform.position, destination.position, OnCompletePath);
                }
                
                direction = moveDir.normalized.ToObjectDirection();
                animator.Direction = direction;
            }
        }

        private void OnCompletePath(Path path) {
            if (path.error) {
                var logger = YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<YisoBaseCampPlayerController>();
                logger.Error($"Path failed: {path.errorLog}");
                return;
            }
            
            /*var abPath = path as ABPath;
            ClearFootprints();
            PlaceFootprints(abPath.vectorPath);*/
        }

        private void PlaceFootprints(List<Vector3> path) {
            for (var i = 0; i < path.Count - 1; i++) {
                var start = path[i];
                var end = path[i + 1];
                var distance = start.GetDistance(end);
                var dir = (end - start).normalized;

                for (var d = 0f; d < distance; d += interval) {
                    var position = start + dir * d;
                    var sr = poolService.SpawnObject<SpriteRenderer>(footprintPrefab, footprintContent);
                    sr.gameObject.transform.position = position;
                    footPrints.Add(sr);
                }
            }
        }

        private void ClearFootprints() {
            foreach (var footprint in footPrints) {
                poolService.ReleaseObject(footprint.gameObject);
            }
            
            footPrints.Clear();
        }

        private bool ReachedDestination(Transform target, float threshold, out Vector3 moveDirection) {
            moveDirection = target.position - transform.position;
            return moveDirection.sqrMagnitude <= threshold;
        }

        public void AddOnEnable([NotNull] Action onEnable) {
            if (onEnable == null) throw new NullReferenceException("OnEnable action must not be null!");
            onEnables.Add(onEnable);
        }

        public void AddOnDisable([NotNull] Action onDisable) {
            if (onDisable == null) throw new NullReferenceException("OnDisable action must not be null!");
            onDisables.Add(onDisable);
        }
        
        public override void OnFixedUpdate() {
            if (!isMobile) return;
            if (!animator.IsMoving) {
                rgbd2D.velocity = Vector2.zero;
                return;
            }
            movementInput.Normalize();
            rgbd2D.MovePosition(rgbd2D.position + movementInput * (moveSpeed * Time.deltaTime));
            direction = movementInput.ToObjectDirection();
            animator.Direction = direction;
        }

        private void SetPathfinding() {
            aiPath = GetOrAddComponent<AIPath>();
            destinationSetter = GetOrAddComponent<AIDestinationSetter>();
            seeker = GetOrAddComponent<Seeker>();

            aiPath.enabled = false;
            destinationSetter.enabled = false;
            
            aiPath.orientation = OrientationMode.YAxisForward;
            aiPath.radius = 0.5f;
            aiPath.gravity = Vector3.zero;
            aiPath.endReachedDistance = 0f;
            aiPath.maxSpeed = 4f;
            aiPath.enableRotation = false;
        }

        private void OnMouseClick(YisoBaseCampPlayerMouseController.ButtonTypes type, YisoBaseCampPlayerMouseController.Modes mode, GameObject targetObject) {
            if (type == YisoBaseCampPlayerMouseController.ButtonTypes.RIGHT) {
                destination = targetObject.transform;

                if (!aiPath.enabled)
                    aiPath.enabled = true;
                if (!destinationSetter.enabled)
                    destinationSetter.enabled = true;
                
                moving = true;
            } else {
                if (targetObject == null) return;
                var isMet = ReachedDestination(targetObject.transform, 2.5f, out _);
                if (isMet) {
                    interactInfo.OnClick?.Invoke();
                }
            }
        }

        internal class InteractInfo {
            private Action onClick = null;
            public bool Active { get; set; } = false;

            public Action OnClick {
                get => onClick;
                set {
                    onClick = value;
                    if (onClick != null) Active = true;
                }
            }

            public void Reset() {
                Active = false;
                OnClick = null;
            }
        }
    }
}