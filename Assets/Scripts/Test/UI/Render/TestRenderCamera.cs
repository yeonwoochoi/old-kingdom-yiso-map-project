using System;
using UnityEngine;

namespace Test.UI.Render {
    public class TestRenderCamera : MonoBehaviour {
        public Transform target;
        public float smoothTime = .15f;
        
        private Vector3 velocity = Vector3.zero;

        private void FixedUpdate() {
            var targetPos = target.position;

            targetPos.z = transform.position.z;
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
        }
    }
}