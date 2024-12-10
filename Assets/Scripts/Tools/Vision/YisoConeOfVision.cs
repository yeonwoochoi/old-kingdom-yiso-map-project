using System;
using System.Collections.Generic;
using Core.Behaviour;
using Manager;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils.Beagle;

namespace Tools.Vision {
    [Serializable]
    [AddComponentMenu("Yiso/Tools/Vision/ConeOfVision")]
    public class YisoConeOfVision : RunIBehaviour {
        [Title("Vision")] public LayerMask obstacleMask = LayerManager.ObstaclesLayerMask | LayerManager.MapLayerMask;
        public float visionRadius = 8f;
        [Range(0f, 360f)] public float visionAngle = 180f;
        [Range(0f, 360f)] public float angleOffset = 0f;
        [ReadOnly] public Vector3 direction;
        [ReadOnly] public Vector3 eulerAngles;

        [Title("Target scanning")] public bool shouldScanForTargets = true;
        public LayerMask targetMask = LayerManager.PlayerLayerMask;
        public float scanFrequencyInSeconds = 1f;
        [ReadOnly] public List<Transform> visibleTargets = new();

        [Title("Mesh")] public bool shouldDrawMesh = true;
        public int edgePrecision = 3;
        public float edgeThreshold = 0.5f;

        protected MeshFilter visionMeshFilter;
        protected Mesh visionMesh;
        protected float meshDensity = 1f;
        protected float lastScanTimestamp;


        #region Initialization

        protected override void Awake() {
            visionMesh = new Mesh();
            direction = Vector3.right;
            if (visionMeshFilter == null) {
                visionMeshFilter = gameObject.GetComponentInChildren<MeshFilter>();
            }

            if (shouldDrawMesh) {
                visionMeshFilter.mesh = visionMesh;
            }
        }

        #endregion

        #region Public API

        public virtual void SetDirectionAndAngles(Vector3 newDirection, Vector3 angle) {
            direction = newDirection;
            eulerAngles = angle;
            eulerAngles.y += angleOffset;
        }

        #endregion

        #region Update

        public override void OnLateUpdate() {
            // scanFrequencyInSeconds에 한번씩 target scan함.
            if (Time.time - lastScanTimestamp > scanFrequencyInSeconds && shouldScanForTargets) {
                ScanForTargets();
            }

            DrawMesh();
        }

        /// <summary>
        /// 해당 시야 내에 있는 적들 탐지해 visibleTargets에 추가함. 
        /// </summary>
        protected virtual void ScanForTargets() {
            lastScanTimestamp = Time.time;
            visibleTargets.Clear();

            // Radius 내에 있는 모든 Collider 리스트를 return
            var targetsWithinDistance = Physics2D.OverlapCircleAll(transform.position, visionRadius, targetMask);
            foreach (var targetCollider in targetsWithinDistance) {
                var target = targetCollider.transform;
                var directionToTarget = (target.position - transform.position).normalized;

                // 시야 각도 (visionAngle) 내에 있으면
                if (Vector3.Angle(direction, directionToTarget) < visionAngle / 2f) {
                    var distanceToTarget = Vector3.Distance(transform.position, target.position);
                    // 해당 방향으로 Raycast 쏴봐 (중간에 벽(Obstacle)이 있을 수도 있잖아
                    var scanForTargetsHit2D = Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget,
                        obstacleMask);
                    // Obstacle 없으면 target에 추가
                    if (!scanForTargetsHit2D) {
                        visibleTargets.Add(target);
                    }
                }
            }
        }

        #endregion

        #region Mesh

        public struct RaycastData {
            public bool hit; // 맞았는지 여부
            public Vector3 point; // raycast 쏴서 맞은 위치
            public float distance; // ray origin으로부터 거리
            public float angle;

            public RaycastData(bool hit, Vector3 point, float distance, float angle) {
                this.hit = hit;
                this.point = point;
                this.distance = distance;
                this.angle = angle;
            }
        }

        public struct MeshEdgePosition {
            public Vector3 pointA;
            public Vector3 pointB;

            public MeshEdgePosition(Vector3 pointA, Vector3 pointB) {
                this.pointA = pointA;
                this.pointB = pointB;
            }
        }

        protected int numberOfVerticesLastTime = 0;
        protected Vector3[] vertices;
        protected int[] triangles;

        protected virtual void DrawMesh() {
            if (!shouldDrawMesh) return;

            var steps = Mathf.RoundToInt(meshDensity * visionAngle);
            var stepsAngle = visionAngle / steps;

            var viewPoints = new List<Vector3>();
            var oldViewCast = new RaycastData();

            // TODO : 여기부터 이해 안됨.
            for (int i = 0; i < steps; i++) {
                var angle = stepsAngle * i + eulerAngles.y - visionAngle / 2f;
                var viewCast = RaycastAtAngle(angle);

                if (i > 0) {
                    var thresholdExceeded = Mathf.Abs(oldViewCast.distance - viewCast.distance) > edgeThreshold;
                    if ((oldViewCast.hit != viewCast.hit) || (oldViewCast.hit && viewCast.hit && thresholdExceeded)) {
                        var edge = FindMeshEdgePosition(oldViewCast, viewCast);
                        if (edge.pointA != Vector3.zero) {
                            viewPoints.Add(edge.pointA);
                        }

                        if (edge.pointB != Vector3.zero) {
                            viewPoints.Add(edge.pointB);
                        }
                    }
                }

                viewPoints.Add(viewCast.point);
                oldViewCast = viewCast;
            }

            var numberOfVertices = viewPoints.Count + 1;
            if (numberOfVertices != numberOfVerticesLastTime) {
                Array.Resize(ref vertices, numberOfVertices);
                Array.Resize(ref triangles, (numberOfVertices - 2) * 3);
            }

            vertices[0].x = 0;
            vertices[0].y = 0;
            vertices[0].z = 0;

            for (int i = 0; i < numberOfVertices - 1; i++) {
                vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);
                if (i < numberOfVertices - 2) {
                    triangles[i * 3] = 0;
                    triangles[i * 3 + 1] = i + 1;
                    triangles[i * 3 + 2] = i + 2;
                }
            }

            visionMesh.Clear();
            visionMesh.vertices = vertices;
            visionMesh.triangles = triangles;
            visionMesh.RecalculateNormals();

            numberOfVerticesLastTime = numberOfVertices;
        }

        private MeshEdgePosition FindMeshEdgePosition(RaycastData minimumViewCast, RaycastData maximumViewCast) {
            var minAngle = minimumViewCast.angle;
            var maxAngle = maximumViewCast.angle;

            var minPoint = minimumViewCast.point;
            var maxPoint = maximumViewCast.point;

            for (int i = 0; i < edgePrecision; i++) {
                var angle = (minAngle + maxAngle) / 2;
                var newViewCast = RaycastAtAngle(angle);
                // TODO : 여기부터 이해 안됨.
                var thresholdExceeded = Mathf.Abs(minimumViewCast.distance - newViewCast.distance) > edgeThreshold;
                if (newViewCast.hit == minimumViewCast.hit && !thresholdExceeded) {
                    minAngle = angle;
                    minPoint = minimumViewCast.point;
                }
                else {
                    maxAngle = angle;
                    maxPoint = maximumViewCast.point;
                }
            }

            return new MeshEdgePosition(minPoint, maxPoint);
        }

        /// <summary>
        /// 해당 Angle에 Raycast 쏴서 Hit Object를 return
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private RaycastData RaycastAtAngle(float angle) {
            var directionFromAngle = YisoMathUtils.DirectionFromAngle2D(angle, 0f);

            var raycastAtAngleHit2D =
                Physics2D.Raycast(transform.position, directionFromAngle, visionRadius, obstacleMask);
            var returnRaycastData = new RaycastData();

            if (raycastAtAngleHit2D) {
                returnRaycastData.hit = true;
                returnRaycastData.point = raycastAtAngleHit2D.point;
                returnRaycastData.distance = raycastAtAngleHit2D.distance;
                returnRaycastData.angle = angle;
            }
            else {
                returnRaycastData.hit = false;
                returnRaycastData.point = transform.position + directionFromAngle * visionRadius;
                returnRaycastData.distance = visionRadius;
                returnRaycastData.angle = angle;
            }

            return returnRaycastData;
        }

        #endregion
    }
}