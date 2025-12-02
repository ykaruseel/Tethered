// Assets/Scripts/Player/PlayerMovement2.cs (ФИНАЛЬНАЯ СЕТЕВАЯ ВЕРСИЯ)
using UnityEngine;
using Photon.Pun; // 👈 1. ДОБАВЛЕНО: Для сетевой логики
using FMODUnity;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2 : MonoBehaviour
{
    // --- СЕТЕВЫЕ И АНИМАЦИОННЫЕ ПОЛЯ ---
    [Header("Photon")]
    public PhotonView view; 

    [Header("Animation")]
    public Animator anim; // Для анимаций

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
    
    // 👈 2. ДОБАВЛЕНО: Переменные для сетевого ввода
    private float moveInput;    // Итоговый инпут (клава/мобилка)
    private float mobileInput;  // Ввод от UI-кнопок
    private bool jumpRequested;

    private float stepTimer;
    private float StepInterval => 1f / stepsPerSecond;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        view = GetComponent<PhotonView>();   // 👈 3. НАХОДИМ PhotonView
        anim = GetComponent<Animator>();     // Находим Animator
        
        if (groundCheckPoint == null)
            Debug.LogWarning($"{nameof(PlayerMovement2)} on {name}: groundCheckPoint is not set.");
        stepTimer = StepInterval;
    }

    void Update()
    {
        // 🛑 СЕТЕВАЯ БЛОКИРОВКА: Управляем только своим игроком
        if (view != null && !view.IsMine)
            return;
        
        // --- ДВИЖЕНИЕ И ИНПУТ ---
        
        // --- Логика блокировки ---
        if (isMovementBlocked)
        {
            moveInput = 0f;
            mobileInput = 0f; 
            jumpRequested = false;
        }
        else
        {
            // --- Клавиатурный инпут (стрелки) ---
            float input = 0f;
            if (Input.GetKey(KeyCode.LeftArrow)) input = -1f;
            else if (Input.GetKey(KeyCode.RightArrow)) input = 1f;

            // 👈 ИНТЕГРАЦИЯ: Если клава не жмётся — используем мобильный инпут
            if (Mathf.Approximately(input, 0f))
                input = mobileInput;

            moveInput = input;

            // Прыжок с клавы
            if (Input.GetKeyDown(KeyCode.UpArrow))
                jumpRequested = true;
        }
        
        // 👈 АНИМАЦИЯ: Передаем скорость в Animator
        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        }

        // --- шаги (FMOD) ---
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
        // 🛑 СЕТЕВАЯ БЛОКИРОВКА: Здесь тоже, чтобы Rigidbody не конфликтовал
        if (view != null && !view.IsMine)
            return;

        // --- граундчек ---
        if (groundCheckPoint != null)
        {
            isGrounded = Physics2D.OverlapCircle(
                groundCheckPoint.position,
                groundCheckRadius,
                groundLayer
            );
        }

        // --- Физика движения ---
        var vel = rb.linearVelocity;
        vel.x = moveInput * moveSpeed;
        rb.linearVelocity = vel;

        // --- Физика прыжка ---
        if (jumpRequested && isGrounded && !isMovementBlocked)
        {
            vel = rb.linearVelocity;
            vel.y = jumpForce;
            rb.linearVelocity = vel;

            if (!jumpEvent.IsNull)
                RuntimeManager.PlayOneShot(jumpEvent, transform.position);

            stepTimer = StepInterval; // сброс шага
        }
        jumpRequested = false; // сброс запроса
    }

    public void SetMovementBlocked(bool blocked) => isMovementBlocked = blocked;

    // ----------------------------------------------------
    // 👈 4. МЕТОДЫ ДЛЯ МОБИЛЬНЫХ КНОПОК UI (Вызываются из MobileControl.cs)
    // ----------------------------------------------------

    public void OnLeftDown() => mobileInput = -1f;
    public void OnRightDown() => mobileInput = 1f;
    public void OnButtonUp() => mobileInput = 0f;

    public void OnJumpDown()
    {
        // Только запрашиваем прыжок, физика выполнится в FixedUpdate
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