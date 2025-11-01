using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Настройки движения")]
    [Tooltip("Скорость движения персонажа")]
    public float moveSpeed = 5f;

    [Tooltip("Сила прыжка")]
    public float jumpForce = 10f;

    // Ссылка на компонент Rigidbody2D
    private Rigidbody2D rb;
    private float moveInput; // Переменная для хранения ввода (-1, 0, или 1)

    [Header("Проверка земли (Ground Check)")]
    [Tooltip("Находится ли персонаж на земле?")]
    private bool isGrounded;
    
    [Tooltip("Объект-точка для проверки земли под ногами")]
    public Transform groundCheckPoint; 
    
    [Tooltip("Радиус круга для проверки")]
    public float groundCheckRadius = 0.2f; 
    
    [Tooltip("Слой, который считается 'землей'")]
    public LayerMask groundLayer; 
    

    // Start вызывается один раз при запуске
    void Start()
    {
        // Получаем компонент Rigidbody2D с этого же объекта
        rb = GetComponent<Rigidbody2D>();
    }

    // Update вызывается каждый кадр (для считывания ввода)
    void Update()
    {
        // 1. ПРОВЕРКА, НА ЗЕМЛЕ ЛИ МЫ
        // Создаем невидимый круг в точке groundCheckPoint. Если он пересекается с чем-то на слое groundLayer, то isGrounded = true
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

        // 2. СЧИТЫВАНИЕ ВВОДА ДЛЯ ДВИЖЕНИЯ
        // Получаем нажатие клавиш A/D или стрелок влево/вправо. Возвращает -1 (влево), 1 (вправо) или 0.
        moveInput = Input.GetAxisRaw("Horizontal");

        // 3. СЧИТЫВАНИЕ ВВОДА ДЛЯ ПРЫЖКА
        // Если нажата кнопка "Jump" (по умолчанию - Пробел) И персонаж на земле
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Придаем персонажу вертикальную скорость (совершаем прыжок)
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    // FixedUpdate вызывается фиксированное количество раз в секунду (для физики)
    void FixedUpdate()
    {
        // 4. ПРИМЕНЕНИЕ ДВИЖЕНИЯ
        // Задаем горизонтальную скорость персонажу, сохраняя его текущую вертикальную скорость (чтобы не отменять прыжок или падение)
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }
}
