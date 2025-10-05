using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ClassPlayerMovement : MonoBehaviour
{
    // Clase Miercoles - Viernes

    // ============================================================
    // VARIABLES DE MOVIMIENTO
    // ============================================================

    public float speed = 5f;              // Velocidad horizontal del jugador.
    public float horizontal;              // Valor de entrada horizontal (-1 a la izquierda, 1 a la derecha).
    public float jumpForce = 5f;          // Fuerza con la que el personaje saltará.

    public Rigidbody2D rb;                // Referencia al Rigidbody2D para aplicar movimiento físico.

    // ============================================================
    // VARIABLES DE CHEQUEO DE SUELO
    // ============================================================

    public Transform groundCheck;         // Punto desde el que se detecta el suelo.
    public LayerMask groundLayer;         // Capas que cuentan como "suelo".
    public float groundRadius;            // Radio del círculo de detección para verificar si está tocando el suelo.

    // ============================================================
    // VARIABLES DE VIDA E INTERFAZ
    // ============================================================

    private float maxHealth = 100f;       // Vida máxima del personaje.
    private float health;                 // Vida actual.
    public TextMeshProUGUI healthText;    // Texto de UI (TextMeshPro) para mostrar la vida.

    // ============================================================
    // VARIABLES DE RETROCESO O KNOCKBACK
    // ============================================================

    public float hitForceX;
    public float hitForceY;
    public bool hitFromRight;

    public float hitTime;

    // ============================================================
    // VARIABLES DE ANIMACIÓN
    // ============================================================

    [SerializeField] private Animator playerAnimator;

    private bool isFacingRight = true;

    // ============================================================
    // VARIABLES DE SISTEMA DE PARTICULAS
    // ============================================================

    [SerializeField] private ParticleSystem hit_PS;
    public Sprite[] hit_PS_Sprites;

    // ============================================================
    // MÉTODOS UNITY
    // ============================================================

    void Start()
    {
        // Inicializamos la vida al máximo.
        health = maxHealth;

        // Mostramos en pantalla la vida actual en la UI.
        healthText.text = $"Health: {health}/{maxHealth}";

        // Obtenemos la referencia al Animator.
        playerAnimator = GetComponent<Animator>();

        hit_PS = GetComponentInChildren<ParticleSystem>();
    }

    void Update()
    {
        // Actualizamos el parámetro "Direction" del Animator con el valor horizontal.
        playerAnimator.SetFloat("Direction", horizontal);

        // Si el personaje está mirando a la derecha y se mueve a la izquierda, o viceversa, llamamos a Flip().
        if (isFacingRight == true && horizontal < 0)
        {
            Flip();
        }
        else if (isFacingRight == false && horizontal > 0)
        {
            Flip();
        }

        if (hitTime <= 0) // Si no me han golpeado o ya paso el tiempo de golpe
        {
            // Movimiento horizontal: se ajusta la velocidad en el eje X
            // usando el valor recibido de "horizontal".
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);

            // Alternativa (comentada): aplicar fuerza en lugar de fijar velocidad directamente.
            // Esto haría que el movimiento se sintiera más "resbaloso" o físico.
            // rb.AddForce(new Vector2(horizontal * speed, 0));
        }
        else // Si me han golpeado, es decir que hitTime es mayor 0
        {
            if (hitFromRight) // Si me golpearon por la derecha
            {
                rb.velocity = new Vector2(-hitForceX, hitForceY);
            }
            else if (!hitFromRight) // Si me golpearon por la izquierda
            {
                rb.velocity = new Vector2(hitForceX, hitForceY);
            }

            // Decrementamos el tiempo restante de retroceso.
            hitTime -= Time.deltaTime;
        }
    }

    // ============================================================
    // MÉTODOS DE INPUT (NUEVO INPUT SYSTEM)
    // ============================================================

    // Se llama cuando el jugador mueve el control en el eje horizontal.
    public void Move(InputAction.CallbackContext context)
    {
        // Guardamos el valor en la variable horizontal (-1, 0, 1).
        horizontal = context.ReadValue<Vector2>().x;
    }

    // Se llama cuando el jugador presiona el botón de salto.
    public void Jump(InputAction.CallbackContext context)
    {
        // "performed" indica que el botón fue presionado.
        // Se permite saltar solo si estamos en el suelo (OnGrounded).
        if (context.performed == true && OnGrounded())
        {
            // Se aplica un cambio en la velocidad vertical para saltar.
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    // ============================================================
    // MÉTODOS AUXILIARES
    // ============================================================

    // Detecta si el jugador está tocando el suelo usando un círculo
    // en la posición de groundCheck.
    public bool OnGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
    }

    // Invierte la escala del personaje en el eje X para que mire al lado contrario.
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    // ============================================================
    // MÉTODOS DE VIDA Y UI
    // ============================================================

    // Resta vida al personaje y actualiza el texto en pantalla.
    public void TakeDamage(float damage)
    {
        health -= damage;
        healthText.text = $"Health: {health}/{maxHealth}";

        ParticleSystem.Burst burst = hit_PS.emission.GetBurst(0);
        burst.count = 30;
        hit_PS.emission.SetBurst(0, burst);

        ParticleSystem.MainModule main = hit_PS.main;
        main.startColor = Color.red;

        hit_PS.textureSheetAnimation.SetSprite(0, hit_PS_Sprites[0]);
        hit_PS.Play();
    }

    // Aumenta vida al personaje (sin pasar del máximo) y actualiza la UI.
    public void AddHealth(float _health)
    {
        if (health + _health > maxHealth)
        {
            health = maxHealth;
        }
        else
        {
            health += _health;
        }

        healthText.text = $"Health: {health}/{maxHealth}";

        hit_PS.textureSheetAnimation.SetSprite(0, hit_PS_Sprites[1]);
        hit_PS.Play();
    }

}
