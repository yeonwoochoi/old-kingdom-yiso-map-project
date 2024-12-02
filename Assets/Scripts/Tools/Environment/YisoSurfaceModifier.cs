using Core.Behaviour;
using UnityEngine;

namespace Tools.Environment {
    /// <summary>
    /// 기존 바닥과 다르게 다른 마찰력을 가지고 있는 바닥의 경우 적용됨.
    /// </summary>
    [AddComponentMenu("Yiso/Environment/Surface Modifier")]
    public class YisoSurfaceModifier : RunIBehaviour {
        public float friction;
        public Vector3 addedForce = Vector3.zero;

        public void OnTriggerStay2D(Collider2D other) {
            // TODO : 해당 표면에 있을 때 마찰력 변화시키면 됨.
        }
    }
}