using UnityEngine;

namespace Test {
    public class TestMovement : MonoBehaviour {
        public float moveSpeed = 10f;

        private Rigidbody2D rb;
        private Animator animator;
        private Vector2 movement;
        private Vector2 lastMovement;
        private bool IsMoving => movement.magnitude >= 0.01f;

        private float X {
            get => IsMoving ? movement.x : lastMovement.x;
            set => movement.x = value;
        }
        private float Y {
            get => IsMoving ? movement.y : lastMovement.y;
            set => movement.y = value;
        }

        private void Start() {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }

        private void Update() {
            X = Input.GetAxisRaw("Horizontal");
            Y = Input.GetAxisRaw("Vertical");
            if (IsMoving) {
                lastMovement = movement;
            }
            UpdateAnimator();
        }

        private void FixedUpdate() {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }

        private void UpdateAnimator() {
            if (animator == null) return;
            animator.SetFloat("X", X);
            animator.SetFloat("Y", Y);
            animator.SetBool("IsMoving", IsMoving);
        }
    }
}