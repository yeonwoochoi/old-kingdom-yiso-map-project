using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Spawn {
    [Serializable]
    public class YisoObjectSpawnInAreaProperties {
        public enum SpawnAreaShapes {
            Circle,
            Box
        }

        public SpawnAreaShapes areaShape = SpawnAreaShapes.Circle;
        public Vector3 offset = Vector3.zero;
        public float moveTime = 1f;

        [ShowIf("areaShape", SpawnAreaShapes.Circle)]
        public float radius = 2f;

        [ShowIf("areaShape", SpawnAreaShapes.Box), MinValue(0)]
        public Vector2 size = Vector2.one;
    }

    public static class YisoObjectSpawnInArea {
        public static void ApplySpawnInAreaProperties(GameObject targetObj, YisoObjectSpawnInAreaProperties properties,
            Vector3 origin) {
            var offset = Vector3.zero;
            targetObj.transform.position = origin;

            if (properties.areaShape == YisoObjectSpawnInAreaProperties.SpawnAreaShapes.Circle) {
                var angle = Random.Range(0f, Mathf.PI * 2f); // 2파이 = 3.141592*2
                var distance = Random.Range(0f, properties.radius);

                var x = Mathf.Cos(angle) * distance + properties.offset.x;
                var y = Mathf.Sin(angle) * distance + properties.offset.y;
                offset = new Vector3(x, y, 0);
            }

            if (properties.areaShape == YisoObjectSpawnInAreaProperties.SpawnAreaShapes.Box) {
                var x = Random.Range(-properties.size.x, properties.size.x) / 2f;
                var y = Random.Range(-properties.size.y, properties.size.y) / 2f;
                offset = new Vector3(x, y, 0);
            }

            var dropPosition = origin + offset;

            targetObj.transform.DOJump(dropPosition, 1f, 1, properties.moveTime);
        }
    }
}