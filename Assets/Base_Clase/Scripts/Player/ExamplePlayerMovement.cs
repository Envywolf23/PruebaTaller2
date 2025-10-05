using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Script de ejemplo para controlar el movimiento de un personaje en Unity 2D.
/// Incluye l�gica de movimiento, salto, gravedad personalizada, animaciones
/// y un sistema b�sico de vida conectado con una interfaz de TextMeshPro.
/// </summary>

public class ExamplePlayerMovement : MonoBehaviour
{
    // ============================================================
    // VARIABLES DE SUELO
    // ============================================================
    #region Ground Check Variables
    [Header("Ground Check variables")]
    [Space(2)]
    [SerializeField] private Transform groundCheck;      // Objeto que marca la posici�n desde donde se revisa si tocamos el suelo.
    [SerializeField] private LayerMask groundLayer;      // Capas que cuentan como "suelo" (ej: plataformas, piso).
    [SerializeField] private float groundRadius;         // Radio de detecci�n para el chequeo de suelo.
    private Rigidbody2D rb;                              // Referencia al Rigidbody2D del jugador (necesario para la f�sica).
    [Space(5)]
    #endregion

    // ============================================================
    // VARIABLES DE VELOCIDAD Y MOVIMIENTO
    // ============================================================
    #region Movement Variables
    [Header("Speed variables")]
    [Space(2)]
    public float speed;                                  // Velocidad actual del personaje.
    public float normalSpeed = 8;                        // Velocidad base (caminar).
    public float runSpeed = 20;                          // Velocidad al correr.
    [SerializeField] private float acceleration;         // Aceleraci�n cuando el jugador se mueve.
    [SerializeField] private float decceleration;        // Desaceleraci�n cuando deja de moverse.
    [SerializeField] private float velPower;             // Exponente que suaviza la curva de aceleraci�n.
    public bool isFacingRight;                           // Indica si el personaje mira hacia la derecha.
    private float horizontal;                            // Entrada horizontal del jugador (-1 a la izquierda, 1 a la derecha).

    [Space(5)]
    #endregion

    // ============================================================
    // VARIABLES DE SALTO
    // ============================================================
    #region Jumping Variables
    [Header("Jumping variables")]
    [Space(2)]
    public float jumpingPower;                           // Fuerza del salto.
    [SerializeField] private bool isJumping;             // Indica si el personaje est� en pleno salto.
    [SerializeField] private bool isFalling;             // Indica si el personaje est� cayendo.
    [SerializeField] private float lastGroundedTime;     // Tiempo desde la �ltima vez que toc� el suelo.
    [SerializeField] private float coyoteTime;           // Tiempo extra permitido para saltar despu�s de salir del suelo (efecto "coyote").
    [SerializeField] private float jumpBufferTime = 0.2f;// Tiempo que se guarda si el jugador presiona salto antes de tocar el suelo.
    [SerializeField] private float jumpBufferCounter;    // Contador que mide el buffer de salto.

    [Space(5)]
    #endregion

    // ============================================================
    // VARIABLES DE GRAVEDAD
    // ============================================================
    #region Gravity Variables
    [Header("Gravity variables")]
    [Space(2)]
    [SerializeField] private float gravityScale;         // Escala de gravedad normal.
    [SerializeField] private float jumpCutMultiplier;    // Multiplicador que reduce el salto si se suelta la tecla antes de tiempo.
    [SerializeField] private float fallGravityMultiplier;// Multiplicador que aumenta la gravedad al caer (salto m�s realista).
    [SerializeField] private float maxFallSpeed;         // Velocidad m�xima de ca�da.
    #endregion

    // ============================================================
    // VARIABLES DE ANIMACI�N
    // ============================================================
    #region Animation Variables
    [Header("Animation variables")]
    [Space(2)]
    [SerializeField] private Animator playerAnimator;    // Controlador de animaciones del personaje.
    #endregion

    // ============================================================
    // VARIABLES DE VIDA E INTERFAZ
    // ============================================================
    #region Health and UI Variables
    [Header("Health variables")]
    [Space(2)]
    [SerializeField] private float health;               // Vida actual del jugador.
    [SerializeField] private float maxHealth = 100f;     // Vida m�xima posible.
    
    [Header("UI variables")]
    [Space(2)]
    public TextMeshProUGUI healthText;                   // Texto de UI (TextMeshPro) que muestra la vida.
    #endregion

    // ============================================================
    // M�TODOS UNITY
    // ============================================================
    #region Unity Methods
    void Start()
    {
        // Obtenemos las referencias a Rigidbody y Animator al inicio.
        rb = GetComponent<Rigidbody2D>();
        gravityScale = rb.gravityScale;
        playerAnimator = GetComponent<Animator>();

        // Inicializamos la vida al m�ximo.
        health = maxHealth;

        // Mostramos en pantalla la vida actual en la UI.
        healthText.text = $"Health: {health}/{maxHealth}";
    }

    private void FixedUpdate()
    {
        // ==============================
        // CONTADORES DE TIEMPO
        // ==============================
        #region Counters
        // Reducimos los contadores de tiempo en cada frame fijo.
        lastGroundedTime -= Time.deltaTime;
        jumpBufferCounter -= Time.deltaTime;
        #endregion

        // ==============================
        // L�GICA DE MOVIMIENTO
        // ==============================
        #region Movement Logic

        float targetSpeed = horizontal * speed; // Velocidad deseada seg�n la entrada del jugador.
        float speedDif = targetSpeed - rb.velocity.x; // Diferencia entre velocidad deseada y actual.

        // Elegimos aceleraci�n o desaceleraci�n.
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;

        // Calculamos la fuerza de movimiento.
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
        rb.AddForce(movement * Vector2.right);

        // Animaci�n de caminar/correr seg�n la entrada horizontal.
        playerAnimator.SetFloat("Movement", horizontal);

        // Si cambiamos de direcci�n, volteamos el sprite.
        if (!isFacingRight && horizontal > 0f) Flip();
        else if (isFacingRight && horizontal < 0f) Flip();

        #endregion

        // ==============================
        // L�GICA DE SALTO
        // ==============================
        #region Jumping Logic

        // Guardamos tiempo desde que tocamos el suelo (para coyote time).
        if (!isJumping && IsGrounded())
        {
            lastGroundedTime = coyoteTime;
        }

        // Detectamos transici�n de salto a ca�da.
        if (isJumping && rb.velocity.y < 0)
        {
            isJumping = false;
            isFalling = true;
            playerAnimator.SetTrigger("Fall");
        }

        // Detectamos si aterrizamos despu�s de caer.
        if (isFalling && IsGrounded())
        {
            isFalling = false;
            playerAnimator.SetTrigger("Land");
        }

        #endregion

        // ==============================
        // L�GICA DE GRAVEDAD
        // ==============================
        #region Gravity Logic

        // Si estamos cayendo, aplicamos mayor gravedad y limitamos velocidad m�xima de ca�da.
        if (rb.velocity.y < 0f)
        {
            rb.gravityScale = gravityScale * fallGravityMultiplier;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }

        #endregion
    }
    #endregion

    // ============================================================
    // M�TODOS AUXILIARES
    // ============================================================
    #region Auxiliary Methods

    // Verifica si el jugador est� tocando el suelo mediante un c�rculo en groundCheck.
    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
    }

    // Voltea el sprite cuando cambiamos de direcci�n.
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    // Valida si el jugador puede saltar.
    private bool CanJump()
    {
        return lastGroundedTime > 0 && !isJumping && jumpBufferCounter > 0;
    }
    #endregion

    // ============================================================
    // M�TODOS DE INPUT (NUEVO INPUT SYSTEM)
    // ============================================================
    #region Input Methods (New Input System)
    // Movimiento horizontal (Input System).
    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
    }

    // Correr (aumenta velocidad y cambia animaci�n).
    public void Run(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            speed = runSpeed;
            playerAnimator.SetBool("IsRunning", true);
        }

        if (context.canceled)
        {
            speed = normalSpeed;
            playerAnimator.SetBool("IsRunning", false);
        }
    }

    // Saltar (considera coyote time y jump buffer).
    public void Jump(InputAction.CallbackContext context)
    {
        jumpBufferCounter = jumpBufferTime; // Activamos buffer de salto.

        if (context.performed && CanJump())
        {
            rb.gravityScale = gravityScale;
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
            lastGroundedTime = 0f;
            isJumping = true;
            playerAnimator.SetTrigger("Jump");
        }

        // Si soltamos el salto antes de tiempo, reducimos la fuerza (salto m�s bajo).
        if (context.canceled && rb.velocity.y > 0)
        {
            rb.AddForce(Vector2.down * rb.velocity.y * (1 - jumpCutMultiplier), ForceMode2D.Impulse);
        }
    }
    #endregion

    // ============================================================
    // M�TODOS DE VIDA Y UI
    // ============================================================
    #region Health and UI Methods

    // Reducir vida y actualizar UI.
    public void TakeDamage(float damage)
    {
        health -= damage;
        healthText.text = $"Health: {health}/{maxHealth}";
    }

    // Recuperar vida y actualizar UI.
    public void AddHealth(float _health)
    {
        if (health + _health > maxHealth) health = maxHealth;
        else health += _health;

        healthText.text = $"Health: {health}/{maxHealth}";
    }
    #endregion
}
