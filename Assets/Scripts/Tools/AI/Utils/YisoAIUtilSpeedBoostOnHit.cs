using System.Collections;
using Character.Ability;
using Character.Core;
using Character.Health;
using Core.Behaviour;
using Tools.Cool;
using UnityEngine;

namespace Tools.AI.Utils {
    /// <summary>
    /// 맞았을 떄 일시적으로 속도 올려주는 Util Component
    /// </summary>
    [AddComponentMenu("Yiso/Character/AI/Utils/AIUtilSpeedBoostOnHit")]
    public class YisoAIUtilSpeedBoostOnHit : RunIBehaviour {
        public float speedMultiplier = 2f;
        public float duration = 2f;
        public YisoCooldown cooldown;

        protected YisoCharacterMovement characterMovement;
        protected YisoHealth health;

        protected override void Awake() {
            health = gameObject.GetComponentInParent<YisoHealth>();
            characterMovement = gameObject.GetComponentInParent<YisoCharacter>().FindAbility<YisoCharacterMovement>();
            cooldown.Initialization();
        }

        protected override void OnEnable() {
            base.OnEnable();
            if (health == null) return;
            health.onHit += OnHit;
        }

        protected override void OnDisable() {
            base.OnDisable();
            if (health == null) return;
            health.onHit -= OnHit;
        }

        public override void OnUpdate() {
            cooldown.Update();
        }

        protected virtual void OnHit(GameObject attacker) {
            if (!cooldown.Ready) return;
            cooldown.Start();
            characterMovement?.ApplyMovementMultiplier(speedMultiplier, duration);
        }
    }
}