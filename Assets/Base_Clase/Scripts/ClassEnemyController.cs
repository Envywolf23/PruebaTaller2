using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script para controlar el comportamiento básico de un enemigo en Unity 2D.
/// El enemigo patrulla entre dos puntos, se voltea al cambiar de dirección,
/// y al chocar con el jugador le causa daño y lo empuja.
/// </summary>
public class ClassEnemyController : MonoBehaviour
{
    // ============================================================
    // VARIABLES DE PATRULLA
    // ============================================================

    public Transform[] enemyMovementPoints;    // Puntos entre los que patrulla el enemigo (ej: A y B).
    [SerializeField] private Transform actualObjective; // Punto actual hacia el que se mueve el enemigo.
    [SerializeField] private Rigidbody2D rb;   // Rigidbody2D para mover al enemigo físicamente.
    [SerializeField] private Animator enemyAnimator; // Controlador de animaciones del enemigo.

    public float enemySpeed;                   // Velocidad de movimiento del enemigo.
    public float detectionRadius = 0.5f;       // Distancia mínima para considerar que llegó al objetivo.

    Vector2 movement;                          // Dirección de movimiento (izquierda o derecha).

    // ============================================================
    // VARIABLES DE ATAQUE
    // ============================================================

    public float enemyDamage;                  // Daño que causa al jugador.
    public float enemyHitForceX;               // Fuerza horizontal del golpe que recibe el jugador.
    public float enemyHitForceY;               // Fuerza vertical del golpe que recibe el jugador.

    // ============================================================
    // MÉTODOS UNITY
    // ============================================================

    void Start()
    {
        // Al iniciar, el enemigo se dirige hacia el primer punto (A).
        actualObjective = enemyMovementPoints[0];

        // Obtenemos referencias necesarias al Animator y Rigidbody.
        enemyAnimator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Calculamos la distancia entre el enemigo y el objetivo actual.
        float distanceToObjective = Vector2.Distance(transform.position, actualObjective.position);

        // Si llegamos suficientemente cerca al objetivo...
        if (distanceToObjective < detectionRadius)
        {
            // ...y estamos en el punto A, cambiamos al punto B.
            if (actualObjective == enemyMovementPoints[0])
            {
                actualObjective = enemyMovementPoints[1];
                Flip(); // Volteamos al enemigo.
            }
            // ...y si estamos en el punto B, cambiamos al punto A.
            else if (actualObjective == enemyMovementPoints[1])
            {
                actualObjective = enemyMovementPoints[0];
                Flip();
            }
        }

        // Dirección hacia el objetivo (normalizada).
        Vector2 direction = (actualObjective.position - transform.position).normalized;

        // Redondeamos la dirección en X (-1 o 1).
        int roundedDirection = Mathf.RoundToInt(direction.x);

        // Movemos solo en el eje X (horizontal).
        movement = new Vector2(roundedDirection, 0);

        // Pasamos el valor al Animator para controlar animaciones.
        enemyAnimator.SetFloat("Direction", roundedDirection);

        // Movemos al enemigo suavemente hacia el objetivo.
        rb.MovePosition(rb.position + movement * enemySpeed * Time.deltaTime);
    }

    // ============================================================
    // MÉTODOS AUXILIARES
    // ============================================================

    // Invierte la escala en X para voltear al enemigo.
    private void Flip()
    {
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    // ============================================================
    // COLISIÓN CON EL JUGADOR
    // ============================================================

    // Detecta cuando el enemigo choca con el jugador.
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Obtenemos el script del jugador (debe tener ClassPlayerMovement).
            ClassPlayerMovement player = collision.gameObject.GetComponent<ClassPlayerMovement>();

            // Aplicamos daño al jugador.
            player.TakeDamage(enemyDamage);

            // Configuramos valores del golpe (knockback).
            player.hitTime = 0.5f;
            player.hitForceX = enemyHitForceX;
            player.hitForceY = enemyHitForceY;

            // Revisamos desde qué lado golpeó el enemigo al jugador.
            if (collision.transform.position.x <= transform.position.x)
            {
                // El jugador está a la izquierda del enemigo.
                player.hitFromRight = true;
            }
            else if (collision.transform.position.x > transform.position.x)
            {
                // El jugador está a la derecha del enemigo.
                player.hitFromRight = false;
            }

            // Activamos animación de ataque.
            enemyAnimator.SetTrigger("Attack");
        }
    }
}
