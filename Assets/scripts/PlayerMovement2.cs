using UnityEngine;
using FMODUnity;

public class PlayerMovement2 : MonoBehaviour
{
    [Header("Настройки движения")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isMovementBlocked;

    [Header("Проверка земли")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("FMOD Звук прыжка")]
    [SerializeField] private EventReference jumpEvent;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

        if (isMovementBlocked)
            return;

        float moveInput = 0f;

        // Стрелки ← →
        if (Input.GetKey(KeyCode.LeftArrow))
            moveInput = -1f;
        else if (Input.GetKey(KeyCode.RightArrow))
            moveInput = 1f;

        if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            RuntimeManager.PlayOneShot(jumpEvent, transform.position);
        }

        transform.position += new Vector3(moveInput * moveSpeed * Time.deltaTime, 0f, 0f);
    }

    public void SetMovementBlocked(bool blocked) => isMovementBlocked = blocked;
}
