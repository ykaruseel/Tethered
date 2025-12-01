// Assets/Scripts/Player/PlayerMovement.cs
using UnityEngine;
using Photon.Pun;
using FMODUnity;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Photon")]
    public PhotonView view;
    
    [Header("Animation")]
    public Animator anim;

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
    private float mobileInput;   // для кнопок на экране (телефон)

    private float stepTimer;
    private float StepInterval => 1f / stepsPerSecond;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        view = GetComponent<PhotonView>();
        anim = GetComponent<Animator>();
        stepTimer = StepInterval;
    }

    void Update()
    {
        // управляем только своим игроком
        if (view != null && !view.IsMine)
            return;
        
        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        }
        // --- движение по X ---
        float moveInput = 0f;

        // клавиатура
        if (Input.GetKey(KeyCode.A)) moveInput = -1f;
        else if (Input.GetKey(KeyCode.D)) moveInput = 1f;

        // если клавы нет — используем мобильный инпут
        if (Mathf.Approximately(moveInput, 0f))
            moveInput = mobileInput;

        Vector2 vel = rb.linearVelocity;
        vel.x = moveInput * moveSpeed;
        rb.linearVelocity = vel;

        // --- прыжок с клавы ---
        if (Input.GetKeyDown(KeyCode.W))
            TryJump();

        // --- шаги ---
        stepTimer -= Time.deltaTime;

        if (isGrounded &&
            !footstepEvent.IsNull &&
            stepTimer <= 0f &&
            Mathf.Abs(rb.linearVelocity.x) >= minSpeed)
        {
            RuntimeManager.PlayOneShot(footstepEvent, transform.position);
            stepTimer = StepInterval;
        }
    }

    void FixedUpdate()
    {
        if (groundCheckPoint == null) return;

        isGrounded = Physics2D.OverlapCircle(
            groundCheckPoint.position,
            groundCheckRadius,
            groundLayer
        );
    }

    // === Мобильные кнопки ===
    public void OnLeftDown() => mobileInput = -1f;
    public void OnRightDown() => mobileInput = 1f;
    public void OnButtonUp() => mobileInput = 0f;
    public void OnJumpDown() => TryJump();

    private void TryJump()
    {
        if (!isGrounded) return;

        Vector2 vel = rb.linearVelocity;
        vel.y = jumpForce;
        rb.linearVelocity = vel;

        if (!jumpEvent.IsNull)
            RuntimeManager.PlayOneShot(jumpEvent, transform.position);

        // чтобы шаг не прозвучал сразу после прыжка
        stepTimer = StepInterval;
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

