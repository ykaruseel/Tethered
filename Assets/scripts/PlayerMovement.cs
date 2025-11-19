using UnityEngine;
using Photon.Pun; // ВАЖНО: Добавили библиотеку Photon
using FMODUnity;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Photon")]
    public PhotonView view; // Ссылка на сетевой компонент

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

    private Rigidbody2D rb;
    private bool isGrounded;
    private float mobileInput = 0f; // Переменная для телефона

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        view = GetComponent<PhotonView>(); // Автоматически находим компонент
    }

    void Update()
    {
        // 1. СЕТЕВАЯ ЗАЩИТА: Если это чужой игрок — выходим и не управляем им
        if (view.IsMine == false) return;

        // 2. ИНПУТ (Клавиатура + Телефон)
        float moveInput = 0f;
        
        if (Input.GetKey(KeyCode.A)) moveInput = -1f;
        else if (Input.GetKey(KeyCode.D)) moveInput = 1f;
        
        // Если клавиатуру не трогают, берем управление с телефона
        if (moveInput == 0) moveInput = mobileInput;

        // Применяем движение
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Прыжок (Клавиатура)
        if (Input.GetKeyDown(KeyCode.W))
        {
            TryJump();
        }
    }

    void FixedUpdate()
    {
        if (groundCheckPoint != null)
            isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
    }

    // --- ПУБЛИЧНЫЕ ФУНКЦИИ ДЛЯ КНОПОК UI ---
    
    public void OnLeftDown() => mobileInput = -1f;   // Палец нажал "Влево"
    public void OnRightDown() => mobileInput = 1f;   // Палец нажал "Вправо"
    public void OnButtonUp() => mobileInput = 0f;    // Палец убрали
    public void OnJumpDown() => TryJump();           // Палец нажал "Прыжок"

    private void TryJump()
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (!jumpEvent.IsNull) RuntimeManager.PlayOneShot(jumpEvent, transform.position);
        }
    }
}