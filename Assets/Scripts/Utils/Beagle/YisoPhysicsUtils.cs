using System;
using System.Collections.Generic;
using Character.Core;
using Core.Logger;
using Core.Service;
using Core.Service.Log;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utils.Beagle {
    public static class YisoPhysicsUtils {
        public static IYisoLogService LogService => YisoServiceProvider.Instance.Get<IYisoLogService>();
        public static RaycastHit2D RayCast(Vector2 rayOriginPoint, Vector2 rayDirection, float rayDistance,
            LayerMask mask, Color color, bool drawGizmo = false) {
#if UNITY_EDITOR
            if (drawGizmo) {
                Debug.DrawRay(rayOriginPoint, rayDirection * rayDistance, color);
            }
#endif
            return Physics2D.Raycast(rayOriginPoint, rayDirection, rayDistance, mask);
        }

        public static YisoCharacter.FacingDirections GetDirectionFromVector(Vector3 vector3) {
            if (Mathf.Abs(vector3.y) > Mathf.Abs(vector3.x)) {
                return vector3.y > 0
                    ? YisoCharacter.FacingDirections.North
                    : YisoCharacter.FacingDirections.South;
            }
            else {
                return vector3.x > 0
                    ? YisoCharacter.FacingDirections.East
                    : YisoCharacter.FacingDirections.West;
            }
        }

        public static Vector3 GetVectorFromDirection(YisoCharacter.FacingDirections direction) {
            switch (direction) {
                case YisoCharacter.FacingDirections.West:
                    return Vector3.left;
                case YisoCharacter.FacingDirections.East:
                    return Vector3.right;
                case YisoCharacter.FacingDirections.North:
                    return Vector3.up;
                case YisoCharacter.FacingDirections.South:
                    return Vector3.down;
            }

            return Vector3.zero;
        }

        public static T GetClosestObject<T>(List<T> objects, Transform origin) where T : MonoBehaviour {
            T closestObject = null;
            var closestDistance = Mathf.Infinity; // 제곱 거리로 초기화

            // 현재 GameObject의 위치
            var currentPosition = origin.position;

            // 리스트의 모든 GameObject를 반복하여 가장 가까운 것을 찾음
            foreach (var obj in objects) {
                var directionToTarget = (obj.transform.position - currentPosition).sqrMagnitude;

                // 현재까지 발견한 가장 가까운 거리보다 더 가까운 거리를 발견하면 업데이트
                if (directionToTarget < closestDistance) {
                    closestDistance = directionToTarget;
                    closestObject = obj;
                }
            }

            return closestObject;
        }

        public static Vector2 FindValidPositionInCircle(Vector2 origin, float radius, LayerMask obstacleMask,
            int maxAttempts = 30) {
            for (var i = 0; i < maxAttempts; i++) {
                // 플레이어 주변의 랜덤 위치를 생성
                var randomPosition = origin + Random.insideUnitCircle * radius;

                // 해당 위치가 지정한 레이어와 충돌하는지 검사
                if (!Physics2D.OverlapCircle(randomPosition, 0.5f, obstacleMask)) {
                    return randomPosition; // 충돌하지 않으면 해당 위치 반환
                }
            }

            // 유효한 위치를 찾지 못한 경우 플레이어의 현재 위치 반환 (혹은 다른 대체 로직 사용)
            LogService.GetLogger("[Physics Utils] Valid spawn position not found.");
            return origin;
        }

        public static Transform FindNearestCharacterTarget(Transform origin, float detectRadius, Bounds colliderBounds,
            LayerMask targetMask, LayerMask obstacleMask, bool obstacleDetection, int maxDetectTargetCount = 20) {
            var potentialTargets = new List<Transform>();
            var detectedTargets = new Collider2D[maxDetectTargetCount];
            var numberOfDetectedTargets =
                Physics2D.OverlapCircleNonAlloc(origin.position, detectRadius, detectedTargets, targetMask);
            if (numberOfDetectedTargets == 0) return null;

            var min = Mathf.Min(maxDetectTargetCount, numberOfDetectedTargets);
            for (var i = 0; i < min; i++) {
                if (detectedTargets[i] == null) continue;
                if (detectedTargets[i].gameObject == origin.gameObject ||
                    detectedTargets[i].transform.IsChildOf(origin)) continue;

                var targetCharacter = detectedTargets[i].GetComponentInChildren<YisoCharacter>();
                if (targetCharacter == null) continue;
                if (targetCharacter.conditionState.CurrentState ==
                    YisoCharacterStates.CharacterConditions.Dead) continue;

                potentialTargets.Add(detectedTargets[i].gameObject.transform);
            }

            if (potentialTargets.Count == 0) return null;

            // we sort our targets by distance
            potentialTargets.Sort(delegate(Transform a, Transform b) {
                if (a == null || b == null) return 0;
                return Vector2.Distance(origin.position, a.transform.position)
                    .CompareTo(Vector2.Distance(origin.position, b.transform.position));
            });

            if (!obstacleDetection) return potentialTargets[0];

            foreach (var target in potentialTargets) {
                var boxcastDirection = target.gameObject.YisoGetComponentNoAlloc<Collider2D>().bounds.center -
                                       colliderBounds.center;
                var hit = YisoDebugUtils.RayCast(colliderBounds.center, boxcastDirection, boxcastDirection.magnitude,
                    obstacleMask, Color.yellow, true);
                if (!hit) return target;
            }

            return null;
        }

        public static Vector3 GetRandomPositionInRadius(Vector3 origin, float minRadius = 0f, float maxRadius = 2f) {
            var angle = Random.Range(0f, Mathf.PI * 2f); // 2파이 = 3.141592*2
            var distance = Random.Range(minRadius, maxRadius);

            var x = Mathf.Cos(angle) * distance;
            var y = Mathf.Sin(angle) * distance;
            return new Vector3(origin.x + x, origin.y + y, origin.z);
        }
    }
}