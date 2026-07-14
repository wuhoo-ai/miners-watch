using UnityEngine;
using UnityEngine.InputSystem;

namespace MinersWatch
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Jump")]
        [SerializeField] private float jumpForce = 8f;

        [Header("Ground Check")]
        [SerializeField] private LayerMask groundLayer = 1 << 6; // Layer 6 = Ground
        [SerializeField] private float groundCheckRadius = 0.05f;
        [SerializeField] private Transform groundCheckPoint;

        [Header("Cave Bounds")]
        [SerializeField] private float minX = -8f;
        [SerializeField] private float maxX = 8f;

        private Rigidbody2D rb;
        private StaminaSystem stamina;
        private Animator animator;
        private PlayerControls controls;
        private bool isGrounded;

        private Vector2 moveInput;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            stamina = GetComponent<StaminaSystem>() ?? gameObject.AddComponent<StaminaSystem>();
            animator = GetComponent<Animator>();
            controls = new PlayerControls();

            // Rigidbody2D defaults
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // Collider setup
            var col = GetComponent<BoxCollider2D>();
            col.size = new Vector2(0.48f, 0.48f);
            gameObject.layer = 8; // Player layer

            // Ground check point
            if (groundCheckPoint == null)
            {
                var go = new GameObject("GroundCheck");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(0f, -col.size.y / 2f, 0f);
                groundCheckPoint = go.transform;
            }
        }

        private void OnEnable()
        {
            if (controls != null)
            {
                controls.Enable();
                controls.Player.Jump.performed += OnJump;
            }
        }

        private void OnDisable()
        {
            if (controls != null)
            {
                controls.Player.Jump.performed -= OnJump;
                controls.Disable();
            }
        }

        private void OnDestroy()
        {
            controls?.Dispose();
        }

        private void Update()
        {
            if (controls != null)
            {
                moveInput = controls.Player.Move.ReadValue<Vector2>();
            }
        }

        private void FixedUpdate()
        {
            // Ground check
            if (groundCheckPoint != null)
            {
                isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
            }
            else
            {
                isGrounded = Physics2D.OverlapCircle(transform.position, groundCheckRadius, groundLayer);
            }

            // Horizontal movement
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

            // Update animator if present
            if (animator != null)
            {
                animator.SetBool("IsMoving", Mathf.Abs(moveInput.x) > 0.01f);
                animator.SetBool("IsGrounded", isGrounded);
            }
        }

        private void LateUpdate()
        {
            // Clamp to cave bounds
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            transform.position = pos;
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            if (isGrounded)
            {
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (groundCheckPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
            }
        }
#endif
    }
}
