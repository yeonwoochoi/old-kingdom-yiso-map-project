using System;
using System.Collections.Generic;
using Core.Behaviour;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils.Beagle;

namespace Tools.Movement.PathMovement {
    public enum PathPositionType {
        Vector,
        Transform
    }
    
    /// <summary>
    /// This class describes a node on an YisoPath
    /// </summary>
    [Serializable]
    public class YisoPathMovementElement {
        public PathPositionType positionType = PathPositionType.Vector;
        [ShowIf("positionType", PathPositionType.Vector)] public Vector3 pathElementPosition;
        [ShowIf("positionType", PathPositionType.Transform)] public Transform pathElementTransform;
        public float delay;
        
        public Vector3 PathElement {
            get =>
                positionType == PathPositionType.Vector
                    ? pathElementPosition
                    : pathElementTransform.position;
            set {
                if (positionType == PathPositionType.Vector) {
                    pathElementPosition = value;
                }
                else {
                    pathElementTransform.position = value;
                }
            }
        }
    }

    /// <summary>
    /// Add this component to an object and you'll be able to define a path, that can then be used by another component
    /// </summary>
    [AddComponentMenu("Yiso/Tools/Movement/Path")]
    public class YisoPath : RunIBehaviour {
        public enum CycleOptions {
            BackAndForth,
            Loop,
            OnlyOnce
        }

        public enum MovementDirection {
            Ascending,
            Descending
        }

        [Title("Path")] public CycleOptions cycleOption;
        public MovementDirection loopInitialMovementDirection = MovementDirection.Ascending;
        public List<YisoPathMovementElement> pathElements; // world position 이 아니라 캐릭터 초기 위치에 대한 offset값임

        public float
            minDistanceToGoal =
                .1f; // the minimum distance to a point at which we'll arbitrarily decide the point's been reached

        public int Direction => direction;
        public int CurrentIndex => currentIndex;
        public Vector3 CurrentPoint => initialPosition + currentPoint.Current;

        protected bool initialized = false;
        protected bool canMove = false;

        protected bool originalTransformPositionStatus = false; // originalTransformPosition이 Setting 되었는지
        protected Vector3 originalTransformPosition;

        protected int direction = 1;
        protected Vector3 initialPosition;

        protected IEnumerator<Vector3> currentPoint;
        protected Vector3 previousPoint = Vector3.zero;
        protected int currentIndex;

        protected float distanceToNextPoint;
        protected bool endReached = false;


        #region Initialization

        protected override void Start() {
            if (initialized) return;
            Initialization();
        }

        public virtual void Initialization() {
            endReached = false;
            canMove = true;

            // Path Element 없으면 Exit
            if (pathElements == null || pathElements.Count < 1) return;

            // 첫번째 Node Position이 Vector3.zero가 아니면 Offset 설정 해야함
            if (pathElements[0].PathElement != Vector3.zero) {
                var firstPathPosition = pathElements[0].PathElement;
                transform.position += firstPathPosition;

                foreach (var element in pathElements) {
                    element.PathElement -= firstPathPosition;
                }
            }

            // Set Direction
            direction = loopInitialMovementDirection == MovementDirection.Ascending ? 1 : -1;

            // we initialize our path enumerator
            initialPosition = transform.position;
            currentPoint = GetPathEnumerator();
            previousPoint = currentPoint.Current;
            currentPoint.MoveNext();

            // initial positioning
            if (!originalTransformPositionStatus) {
                originalTransformPositionStatus = true;
                originalTransformPosition = transform.position;
            }

            transform.position = originalTransformPosition + currentPoint.Current;

            initialized = true;
        }

        #endregion

        #region Update

        public override void OnUpdate() {
            if (pathElements == null || pathElements.Count < 1 || endReached || !canMove) return;
            ComputePath();
        }

        #endregion

        #region Path

        public virtual void ChangeDirection() {
            direction = -direction;
            currentPoint.MoveNext();
        }

        protected virtual void ComputePath() {
            distanceToNextPoint = (transform.position - (originalTransformPosition + currentPoint.Current)).magnitude;
            if (distanceToNextPoint < minDistanceToGoal) {
                previousPoint = currentPoint.Current;
                currentPoint.MoveNext();
            }
        }

        protected virtual IEnumerator<Vector3> GetPathEnumerator() {
            if (pathElements == null || pathElements.Count < 1) yield break;

            var index = 0;
            currentIndex = index;
            while (true) {
                currentIndex = index;
                yield return pathElements[index].PathElement;

                if (pathElements.Count <= 1) continue;
                switch (cycleOption) {
                    case CycleOptions.BackAndForth:
                        if (index <= 0) {
                            direction = 1;
                        }
                        else if (index >= pathElements.Count - 1) {
                            direction = -1;
                        }

                        index += direction;
                        break;
                    case CycleOptions.Loop:
                        index += direction;
                        if (index < 0) {
                            index = pathElements.Count - 1;
                        }
                        else if (index > pathElements.Count - 1) {
                            index = 0;
                        }

                        break;
                    case CycleOptions.OnlyOnce:
                        if (index <= 0) {
                            direction = 1;
                        }
                        else if (index >= pathElements.Count - 1) {
                            direction = 0;
                            endReached = true;
                        }

                        index += direction;
                        break;
                }
            }
        }

        #endregion

        #region Gizmo

        public virtual void OnDrawGizmos() {
#if UNITY_EDITOR
            if (pathElements == null || pathElements.Count < 1) return;

            if (!originalTransformPositionStatus) {
                originalTransformPosition = transform.position;
                originalTransformPositionStatus = true;
            }

            if (transform.hasChanged && !initialized) {
                originalTransformPosition = transform.position;
            }

            for (var i = 0; i < pathElements.Count; i++) {
#if UNITY_EDITOR
                YisoDebugUtils.DrawGizmoPoint(originalTransformPosition + pathElements[i].PathElement, 0.2f,
                    Color.green);
#endif

                if (i + 1 < pathElements.Count) {
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(originalTransformPosition + pathElements[i].PathElement,
                        originalTransformPosition + pathElements[i + 1].PathElement);
                }

                if (i == pathElements.Count - 1 && cycleOption == CycleOptions.Loop) {
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(originalTransformPosition + pathElements[0].PathElement,
                        originalTransformPosition + pathElements[i].PathElement);
                }
            }

            if (Application.isPlaying) {
                if (currentPoint != null) {
#if UNITY_EDITOR
                    YisoDebugUtils.DrawGizmoPoint(originalTransformPosition + currentPoint.Current, 0.2f, Color.blue);
                    YisoDebugUtils.DrawGizmoPoint(originalTransformPosition + previousPoint, 0.2f, Color.red);
#endif
                }
            }
#endif
        }

        #endregion
    }
}