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
        [SerializeField] private float groundCheckRadius = 0.3f;
        [SerializeField] private Transform groundCheckPoint;

        [Header("Cave Bounds")]
        [SerializeField] private float minX = -8f;
        [SerializeField] private float maxX = 8f;

        private Rigidbody2D rb;
        private StaminaSystem stamina;
        private Animator animator;
        private PlayerControls controls;
        private WeaponSystem _weapon;
        private SpriteRenderer _spriteRenderer;
        private bool isGrounded;

        private Vector2 moveInput;

        // Attack animation
        [Header("Attack Animation")]
        [SerializeField] private Sprite[] _attackSprites;
        [SerializeField] private float _attackFrameDuration = 0.08f;
        private Sprite _idleSprite;
        private Coroutine _attackRoutine;
        private bool _isAttacking;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            stamina = GetComponent<StaminaSystem>() ?? gameObject.AddComponent<StaminaSystem>();
            animator = GetComponent<Animator>();
            _weapon = GetComponent<WeaponSystem>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null) _idleSprite = _spriteRenderer.sprite;
            controls = new PlayerControls();

            // Rigidbody2D defaults
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // Collider setup: respect an authored collider, only add a fallback when none exists
            var col = GetComponent<Collider2D>();
            if (col == null)
            {
                var box = gameObject.AddComponent<BoxCollider2D>();
                box.size = new Vector2(0.9f, 2.9f);
                col = box;
            }
            gameObject.layer = 8; // Player layer

            // Ground check point
            if (groundCheckPoint == null)
            {
                var go = new GameObject("GroundCheck");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(0f, -col.bounds.extents.y, 0f);
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
            // Touch overlay wins when actively dragged
            if (Mathf.Abs(TouchInput.Horizontal) > 0.01f)
                moveInput = new Vector2(TouchInput.Horizontal, 0f);
            if (TouchInput.ConsumeJump())
                TryJump();
            if (TouchInput.ConsumeAttack() && _weapon != null)
            {
                if (_weapon.TryAttack() && !_isAttacking)
                    PlayAttackAnimation();
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

        private void OnJump(InputAction.CallbackContext context) => TryJump();

        private void TryJump()
        {
            if (isGrounded)
            {
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }

        private void PlayAttackAnimation()
        {
            if (_spriteRenderer == null || _attackSprites == null || _attackSprites.Length == 0) return;
            _attackRoutine = StartCoroutine(AttackRoutine());
        }

        private System.Collections.IEnumerator AttackRoutine()
        {
            _isAttacking = true;

            // Play each attack frame
            foreach (var frame in _attackSprites)
            {
                if (frame != null)
                    _spriteRenderer.sprite = frame;
                yield return new WaitForSeconds(_attackFrameDuration);
            }

            // Restore idle sprite
            if (_idleSprite != null)
                _spriteRenderer.sprite = _idleSprite;

            _isAttacking = false;
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
