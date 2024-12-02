using System;

namespace Character.Core {
    public class YisoCharacterStates {
        public enum CharacterConditions {
            Normal,
            Frozen,
            Dead,
            Stunned,
        }

        public enum MovementStates {
            Null,
            Idle,
            Walking,
            Running,
            Dashing,
            Dodging,
            Attacking,
            SkillCasting
        }

        /// <summary>
        /// 낮은 숫자가 높은 우선순위
        /// </summary>
        [Flags]
        public enum FreezePriority {
            UIPopup = 1 << 0,
            CutscenePlayer = 1 << 1,
            CutsceneAction = 1 << 2,
            FieldEnter = 1 << 3,
            Respawn = 1 << 4,
            Portal = 1 << 5,
            PetRevive = 1 << 6,
            Default = 1 << 7
        }
    }
}