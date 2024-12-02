using System;
using Core.Behaviour;
using UnityEngine;

namespace UI.Menu.Skill.Cam {
    public class YisoMenuSkillUICamera : RunIBehaviour {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothTime = .15f;
            
        private Vector3 velocity = Vector3.zero;

        private void FixedUpdate() {
            var targetPos = target.position;

            targetPos.z = transform.position.z;
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
        }
    }
}