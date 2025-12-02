// Assets/Scripts/PlayerMovement2.cs
using UnityEngine;
using Photon.Pun;
using FMODUnity;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2 : MonoBehaviour
{
    [Header("Photon")]
    public PhotonView view;

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
    [Min(0.1f)] public float stepsPerSecond = 2.2f;
    [Min(0f)] public float minSpeed = 0.15f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isMovementBlocked;

    private float moveInput;     // итоговый инпут (клава/мобилка)
    private bool jumpRequested;
    private float mobileInput;   // от UI-кнопок

    private float stepTimer;
    private float StepInterval => 1f / stepsPerSecond;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        view = GetComponent<PhotonView>();

        if (groundCheckPoint == null)
            Debug.LogWarning($"{nameof(PlayerMovement2)} on {name}: groundCheckPoint is not set.");

        stepTimer = StepInterval;
    }

    void Update()
    {
        // управляем только своим локальным игроком
        if (view != null && !view.IsMine)
            return;

        if (isMovementBlocked)
        {
            moveInput = 0f;
            mobileInput = 0f;
            jumpRequested = false;
        }
        else
        {
            // --- клава (стрелки) ---
            float input = 0f;
            if (Input.GetKey(KeyCode.LeftArrow)) input = -1f;
            else if (Input.GetKey(KeyCode.RightArrow)) input = 1f;

            // если клава не жмётся — берём мобильный инпут
            if (Mathf.Approximately(input, 0f))
                input = mobileInput;

            moveInput = input;

            // прыжок с клавы
            if (Input.GetKeyDown(KeyCode.UpArrow))
                jumpRequested = true;
        }

        // --- шаги ---
        stepTimer -= Time.deltaTime;
        if (isGrounded &&
            Mathf.Abs(rb.linearVelocity.x) >= minSpeed &&
            stepTimer <= 0f &&
            !footstepEvent.IsNull)
        {
            RuntimeManager.PlayOneShot(footstepEvent, transform.position);
            stepTimer = StepInterval;
        }
    }

    void FixedUpdate()
    {
        // управляем только своим локальным игроком
        if (view != null && !view.IsMine)
            return;

        if (groundCheckPoint != null)
        {
            isGrounded = Physics2D.OverlapCircle(
                groundCheckPoint.position,
                groundCheckRadius,
                groundLayer
            );
        }

        var vel = rb.linearVelocity;
        vel.x = moveInput * moveSpeed;
        rb.linearVelocity = vel;

        if (jumpRequested && isGrounded && !isMovementBlocked)
        {
            vel = rb.linearVelocity;
            vel.y = jumpForce;
            rb.linearVelocity = vel;

            if (!jumpEvent.IsNull)
                RuntimeManager.PlayOneShot(jumpEvent, transform.position);

            // не даём сразу сыграть шаг
            stepTimer = StepInterval;
        }
jumpRequested = false;
    }

    public void SetMovementBlocked(bool blocked) => isMovementBlocked = blocked;

    // === Методы для мобильных кнопок UI ===

    public void OnLeftDown()
    {
        if (view != null && !view.IsMine) return;
        mobileInput = -1f;
    }

    public void OnRightDown()
    {
        if (view != null && !view.IsMine) return;
        mobileInput = 1f;
    }

    public void OnButtonUp()
    {
        if (view != null && !view.IsMine) return;
        mobileInput = 0f;
    }

    public void OnJumpDown()
    {
        if (view != null && !view.IsMine) return;
        jumpRequested = true;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }
#endif
}