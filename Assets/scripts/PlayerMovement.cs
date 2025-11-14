// Assets/Scripts/Player/PlayerMovement.cs
using UnityEngine;
using FMODUnity;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Движение")]
    [Min(0f)] public float moveSpeed = 5f;
    [Min(0f)] public float jumpForce = 10f;

    [Header("Проверка земли")]
    public Transform groundCheckPoint;
    [Min(0f)] public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("FMOD")]
    [SerializeField] private EventReference jumpEvent;
    [SerializeField] private EventReference footstepEvent;

    [Header("Шаги")]
    [Tooltip("Шагов в секунду при движении.")]
    [Min(0.1f)] public float stepsPerSecond = 2.2f;
    [Tooltip("Минимальная |vx|, чтобы считать, что игрок идёт.")]
    [Min(0f)] public float minSpeed = 0.15f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isMovementBlocked;
    private float moveInput;
    private bool jumpRequested;

    private float stepTimer;
    private float StepInterval => 1f / stepsPerSecond;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (groundCheckPoint == null)
            Debug.LogWarning($"{nameof(PlayerMovement)} on {name}: groundCheckPoint is not set.");
        stepTimer = StepInterval;
    }

    void Update()
    {
        // --- инпут ---
        if (isMovementBlocked)
        {
            moveInput = 0f;
            jumpRequested = false;
        }
        else
        {
            // WASD
            if (Input.GetKey(KeyCode.A)) moveInput = -1f;
            else if (Input.GetKey(KeyCode.D)) moveInput = 1f;
            else moveInput = 0f;

            if (Input.GetKeyDown(KeyCode.W))
                jumpRequested = true;
        }

        // --- таймер шагов ---
        stepTimer -= Time.deltaTime;
        if (isGrounded && Mathf.Abs(rb.linearVelocity.x) >= minSpeed && stepTimer <= 0f && !footstepEvent.IsNull)
        {
            RuntimeManager.PlayOneShot(footstepEvent, transform.position);
            stepTimer = StepInterval; // why: управляем частотой только числом stepsPerSecond
        }
    }

    void FixedUpdate()
    {
        // --- граундчек и физика ---
        if (groundCheckPoint != null)
            isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

        var vel = rb.linearVelocity;
        vel.x = moveInput * moveSpeed;
        rb.linearVelocity = vel;

        if (jumpRequested && isGrounded)
        {
            vel = rb.linearVelocity;
            vel.y = jumpForce;
            rb.linearVelocity = vel;
            if (!jumpEvent.IsNull)
                RuntimeManager.PlayOneShot(jumpEvent, transform.position);
        }

        jumpRequested = false;
    }

    public void SetMovementBlocked(bool blocked) => isMovementBlocked = blocked;

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }
#endif
}