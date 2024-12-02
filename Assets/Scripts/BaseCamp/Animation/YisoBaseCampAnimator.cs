using Domain.Direction;
using UnityEngine;

namespace BaseCamp.Animation {
    public class YisoBaseCampAnimator {
        public static class Parameters {
            public static readonly int X = Animator.StringToHash("X");
            public static readonly int Y = Animator.StringToHash("Y");
            public static readonly int IsMoving = Animator.StringToHash("IsMoving");
        }

        protected readonly Animator animator;

        protected YisoBaseCampAnimator(Animator animator) {
            this.animator = animator;
        }

        public YisoObjectDirection Direction {
            get {
                var x = animator.GetFloat(Parameters.X);
                var y = animator.GetFloat(Parameters.Y);
                return new Vector2(x, y).ToObjectDirection();
            }
            set {
                var xy = value.ToVector2();
                animator.SetFloat(Parameters.X, xy.x);
                animator.SetFloat(Parameters.Y, xy.y);
            }
        }

        public bool IsMoving {
            get => animator.GetBool(Parameters.IsMoving);
            set => animator.SetBool(Parameters.IsMoving, value);
        }
    }
}