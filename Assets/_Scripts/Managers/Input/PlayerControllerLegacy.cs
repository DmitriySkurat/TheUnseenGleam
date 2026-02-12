using UnityEngine;

public class PlayerControllerLegacy : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runMultiplier = 1.5f;
    [SerializeField] private float jumpForce = 7f;

    [Header("References")]
    [SerializeField] private InputManager input; // Назначь в инспекторе или найди в Awake
    [SerializeField] private Rigidbody2D rb; // Для примера используем 2D физику

    private Vector2 _moveDirection;
    private bool _isRunning;
    private bool _isGrounded;

    private void Awake()
    {
        // Если забыл назначить в инспекторе, попробуем найти на этом же объекте
        if (input == null) input = GetComponent<InputManager>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        // Подписываемся на события
        input.OnMove += OnMove;
        input.OnJumpStarted += OnJump;
        input.OnRunToggled += OnRun;
    }

    private void OnDisable()
    {
        // Обязательно отписываемся!
        input.OnMove -= OnMove;
        input.OnJumpStarted -= OnJump;
        input.OnRunToggled -= OnRun;
    }

    private void Update()
    {
        // Логика перемещения обычно в Update (или FixedUpdate для физики)
        ApplyMovement();
    }

    private void ApplyMovement()
    {
        float currentSpeed = moveSpeed * (_isRunning ? runMultiplier : 1f);
        rb.linearVelocity = new Vector2(_moveDirection.x * currentSpeed, rb.linearVelocity.y);
    }

    // Методы-обработчики событий
    private void OnMove(Vector2 direction)
    {
        _moveDirection = direction;
    }

    private void OnJump()
    {
        if (_isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    private void OnRun(bool isRunning)
    {
        _isRunning = isRunning;
    }

    // Простая проверка земли (для примера)
    private void OnCollisionStay2D(Collision2D collision) => _isGrounded = true;
    private void OnCollisionExit2D(Collision2D collision) => _isGrounded = false;
}