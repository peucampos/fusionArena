using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [Tooltip("A velocidade máxima de movimento do jogador.")]
    [SerializeField] private float moveSpeed = 5f; // Velocidade máxima de movimento do jogador

    [Tooltip("Controla o quão rápido o jogador acelera e desacelera.")]
    [SerializeField] private float acceleration = 10f; // Taxa de aceleração/desaceleração
    
    [Tooltip("A velocidade de movimento do jogador.")]
    public float playerMoveSpeed = 5f; // Torne isso acessível para outros scripts

    private Rigidbody2D rb; // Referência ao componente Rigidbody2D do jogador

    // Awake é chamado quando o script é carregado
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D não encontrado no GameObject do Player! Certifique-se de que o Player tem um Rigidbody2D anexado.");
        }
    }

    // FixedUpdate é chamado em intervalos de tempo fixos, ideal para operações de física
    private void FixedUpdate()
    {
        HandleSmoothMovementInput();
    }

    /// <summary>
    /// Captura a entrada do teclado (WASD) e aplica o movimento suave ao Rigidbody2D.
    /// </summary>
    private void HandleSmoothMovementInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // Cria um vetor de direção normalizado para a entrada desejada
        Vector2 inputDirection = new Vector2(moveX, moveY).normalized;

        // Calcula a velocidade alvo baseada na entrada
        Vector2 targetVelocity = inputDirection * moveSpeed;

        // Interpola a velocidade atual do Rigidbody2D para a velocidade alvo
        // Time.fixedDeltaTime garante que a suavização seja consistente independente da taxa de quadros fixa
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * acceleration);
    }
}