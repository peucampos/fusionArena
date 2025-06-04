using UnityEngine;
using System.Collections; // Necessário para Coroutines

public class EnemyAI : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [Tooltip("A velocidade inicial de perseguição do inimigo.")]
    [SerializeField] private float initialMoveSpeed = 3f;

    private float moveSpeed; // moveSpeed agora será a velocidade atual do inimigo.

    private Transform playerTransform; // Referência à posição do jogador
    private Rigidbody2D rb; // Referência ao Rigidbody2D do inimigo
    private EnemySpawner enemySpawner; // Referência ao EnemySpawner para comunicação

    private bool isMerging = false; // Flag para indicar que o inimigo está em processo de fusão ou recém-criado/fundido
    public bool IsInitialType { get; set; } = true; // True se o inimigo é do tipo 'Initial'. Será setado no OnEnable/Spawner.

    [Header("Categorias de Inimigo (para lógica de fusão)")]
    [Tooltip("Limite de velocidade para ser considerado Rápido.")]
    [SerializeField] private float fastThresholdSpeed = 4.5f; // Ajuste conforme a velocidade base e incrementos
    [Tooltip("Limite de tamanho para ser considerado Grande.")]
    [SerializeField] private float largeThresholdScale = 1.6f; // Ajuste conforme o tamanho base e incrementos
    [Tooltip("Limite de tamanho para ser considerado Pequeno.")]
    [SerializeField] private float smallThresholdScale = 0.9f; // Novo threshold para Small.

    [Header("Configurações Visuais")]
    [Tooltip("Cor do inimigo inicial (default).")]
    [SerializeField] private Color initialEnemyColor = Color.cyan;
    [Tooltip("Cor do inimigo tipo Pequeno.")]
    [SerializeField] private Color smallEnemyColor = Color.green;
    [Tooltip("Cor do inimigo tipo Rápido.")]
    [SerializeField] private Color fastEnemyColor = Color.blue;
    [Tooltip("Cor do inimigo tipo Grande.")]
    [SerializeField] private Color largeEnemyColor = Color.magenta;

    private SpriteRenderer spriteRenderer; // Referência ao SpriteRenderer do inimigo

    [Header("Limites de Atributos")]
    [SerializeField] private float maxSpeed = 4f;
    [SerializeField] private float maxScale = 1.5f;
    [SerializeField] private float smallScale = 0.75f;
    [SerializeField] private float initialScale = 1f; // Escala padrão para inimigos recém-criados ou não modificados


    public enum EnemyType
    {
        Initial, // Inimigo recém-criado, sem categoria definida
        Small,   // Inimigos pequenos
        Large,   // Inimigos grandes
        Fast     // Inimigos rápidos
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D não encontrado no GameObject do Enemy! Certifique-se de que o Enemy tem um Rigidbody2D anexado.", this);
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer não encontrado no GameObject do Enemy!", this);
        }

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player não encontrado na cena! Certifique-se de que o GameObject do jogador tem a tag 'Player'.", this);
        }

        GameObject gameManagerObject = GameObject.Find("GameManager");
        if (gameManagerObject != null)
        {
            enemySpawner = gameManagerObject.GetComponent<EnemySpawner>();
        }
        else
        {
            Debug.LogError("GameManager (com EnemySpawner) não encontrado na cena! O spawn e fusão de inimigos podem não funcionar corretamente.", this);
        }
    }

    private void OnEnable()
    {
        // Reseta o estado para um inimigo 'novo' ou 'recém-ativado do pool'
        moveSpeed = initialMoveSpeed;
        transform.localScale = Vector3.one * initialScale;
        IsInitialType = true; // Define como Initial por padrão ao ser ativado
        isMerging = false; // Garante que não está em estado de fusão
        UpdateEnemyColor(); // Atualiza a cor
    }

    private void FixedUpdate()
    {
        if (playerTransform != null)
        {
            MoveTowardsPlayer();
        }
    }

    public EnemyType GetEnemyType()
    {
        if (IsInitialType)
        {
            return EnemyType.Initial;
        }
        else // Se não é mais inicial, aplica a lógica de Fast, Large, Small
        {
            // Prioridade: Rápido > Grande > Pequeno
            // A ordem aqui define qual tipo é retornado se o inimigo se encaixar em múltiplas categorias
            // (ex: se é rápido e grande, ele será classificado como Rápido)
            if (moveSpeed >= fastThresholdSpeed)
            {
                return EnemyType.Fast;
            }
            else if (transform.localScale.x >= largeThresholdScale)
            {
                return EnemyType.Large;
            }
            else if (transform.localScale.x <= smallThresholdScale)
            {
                return EnemyType.Small;
            }
            // Se não se encaixa claramente em Fast, Large ou Small, mas não é Initial.
            // Isso pode acontecer se ele está em uma escala/velocidade intermediária.
            // Neste caso, ele será Small por padrão, como um "inimigo normal" não-Initial.
            return EnemyType.Small;
        }
    }

    public void UpdateEnemyColor()
    {
        if (spriteRenderer == null) return;

        EnemyType currentType = GetEnemyType();

        switch (currentType)
        {
            case EnemyType.Initial:
                spriteRenderer.color = initialEnemyColor;
                break;
            case EnemyType.Small:
                spriteRenderer.color = smallEnemyColor;
                break;
            case EnemyType.Fast:
                spriteRenderer.color = fastEnemyColor;
                break;
            case EnemyType.Large:
                spriteRenderer.color = largeEnemyColor;
                break;
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        rb.linearVelocity = directionToPlayer * moveSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Verifica se o objeto com o qual colidimos é outro inimigo e se nenhum dos dois está em processo de fusão
        EnemyAI otherEnemy = collision.gameObject.GetComponent<EnemyAI>();

        if (otherEnemy != null && otherEnemy != this && !this.isMerging && !otherEnemy.isMerging)
        {
            // Ambos os inimigos envolvidos na fusão devem ser marcados como 'isMerging' para evitar colisões duplas enquanto processa
            this.SetMergingState(true);
            otherEnemy.SetMergingState(true);

            // Passa ambos os inimigos para o gerenciador de fusão
            HandleEnemyFusion(this, otherEnemy);
        }
        // LÓGICA DE FIM DE JOGO
        else if (collision.gameObject.CompareTag("Player"))
        {
            // Obtém o tipo do inimigo que colidiu com o jogador
            EnemyType currentEnemyCollidedType = GetEnemyType();

            // Se o inimigo não for do tipo Initial, então é Game Over
            if (currentEnemyCollidedType != EnemyType.Initial)
            {
                Debug.Log($"Inimigo modificado ({currentEnemyCollidedType}) colidiu com o Player! Game Over!");

                // Notifica o GameManager que o jogo acabou
                if (GameManager.Instance != null) // Usando um padrão Singleton para o GameManager
                {
                    GameManager.Instance.GameOver();
                }
                else
                {
                    Debug.LogError("GameManager.Instance não encontrado! Certifique-se de que o GameManager está ativo e implementa o Singleton.");
                }
            }
            else
            {
                // Se o inimigo é do tipo Initial, apenas Debug.Log ou nenhuma ação,
                // pois você não quer que Initial cause Game Over.
                Debug.Log("Inimigo Initial colidiu com o Player. Nenhuma ação de Game Over.");
            }
        }
    }


    /// <summary>
    /// Método para lidar com a fusão de inimigos.
    /// </summary>
    private void HandleEnemyFusion(EnemyAI enemy1, EnemyAI enemy2)
    {
        Debug.Log($"Fusão entre {enemy1.name} ({enemy1.GetEnemyType()}) e {enemy2.name} ({enemy2.GetEnemyType()})");

        Vector2 fusionPosition = (enemy1.transform.position + enemy2.transform.position) / 2f;

        // Acessa as propriedades IsInitialType diretamente para esta lógica primária
        bool isEnemy1Initial = enemy1.IsInitialType;
        bool isEnemy2Initial = enemy2.IsInitialType;

        // --- HIERARQUIA DE REGRAS ---

        // REGRA 1: Dois inimigos do tipo Initial colidem.
        // Um se transforma, o outro é destruído. ESTA É A ÚNICA FORMA DE UM INITIAL SER DESTRUÍDO.
        if (isEnemy1Initial && isEnemy2Initial)
        {
            Debug.Log("REGRA 1: Dois inimigos iniciais colidiram! Um se transforma, o outro é destruído.");
            EnemyAI transformedEnemy = (Random.value > 0.5f) ? enemy1 : enemy2;
            EnemyAI deactivatedEnemy = (transformedEnemy == enemy1) ? enemy2 : enemy1;

            if (enemySpawner != null)
            {
                enemySpawner.EnemyDeactivated(); // Decrementa a contagem de inimigos ativos
            }
            deactivatedEnemy.gameObject.SetActive(false); // Destrói/desativa o inimigo perdedor

            transformedEnemy.IsInitialType = false; // MUITO IMPORTANTE: Garante que nunca mais será Initial

            int transformChoice = Random.Range(0, 3); // 0, 1, 2 para Small, Fast, Large
            ApplyTransformation(transformedEnemy, transformChoice); // Aplica a transformação

            transformedEnemy.SetMergingState(false, 0.8f);
            transformedEnemy.UpdateEnemyColor(); // Atualiza a cor para o novo tipo
            return; // Encerra o método
        }

        // REGRA 2: Um inimigo Initial colide com um inimigo MODIFICADO.
        // Nenhuma ação de fusão ou destruição ocorre. Eles apenas se afastam.
        if ((isEnemy1Initial && !isEnemy2Initial) || (!isEnemy1Initial && isEnemy2Initial))
        {
            Debug.Log($"REGRA 2: Inimigo Initial colidiu com Modificado. Nenhuma ação de fusão. Apenas reseta estado de fusão.");
            // Apenas reseta o estado de isMerging para que possam colidir novamente após um breve atraso.
            enemy1.SetMergingState(false, 0.2f);
            enemy2.SetMergingState(false, 0.2f);
            return; // Encerra o método
        }

        // A partir daqui, ambos os inimigos NÃO SÃO DO TIPO INITIAL.
        EnemyType type1 = enemy1.GetEnemyType();
        EnemyType type2 = enemy2.GetEnemyType();

        // REGRA 3 (CORRIGIDA): Inimigos do mesmo tipo (Small, Fast, Large) colidem.
        // A colisão será IGNORADA. Eles continuam existindo normalmente.
        if (type1 == type2)
        {
            Debug.Log($"REGRA 3: Inimigos do mesmo tipo ({type1}) colidiram! Colisão ignorada, resetando estado de fusão.");
            // Apenas reseta o estado de isMerging para que possam colidir novamente.
            enemy1.SetMergingState(false, 0.2f);
            enemy2.SetMergingState(false, 0.2f);
            return; // Encerra o método
        }

        // REGRA 4 (CORRIGIDA): Inimigos de tipos diferentes (Small, Fast, Large) colidem.
        // Lógica de Pedra, Papel e Tesoura (Rápido > Pequeno > Grande > Rápido).
        // O perdedor se TRANSFORMA no tipo do vencedor (NÃO É DESTRUÍDO).
        EnemyAI winner = null;
        EnemyAI loser = null;

        // Rápido > Pequeno > Grande > Rápido
        if ((type1 == EnemyType.Fast && type2 == EnemyType.Small) ||
            (type1 == EnemyType.Small && type2 == EnemyType.Large) ||
            (type1 == EnemyType.Large && type2 == EnemyType.Fast))
        {
            winner = enemy1; loser = enemy2;
        }
        else if ((type2 == EnemyType.Fast && type1 == EnemyType.Small) ||
                 (type2 == EnemyType.Small && type1 == EnemyType.Large) ||
                 (type2 == EnemyType.Large && type1 == EnemyType.Fast))
        {
            winner = enemy2; loser = enemy1;
        }

        // Este bloco só será atingido se a lógica de PPT acima não cobrir todos os pares de tipos MODIFICADOS.
        if (winner == null || loser == null)
        {
            Debug.LogError("ERRO LÓGICO: Colisão entre tipos diferentes de inimigos modificados sem vencedor/perdedor claro. Verifique a lógica de PPT ou se todos os tipos foram considerados!");
            // Em caso de erro grave, apenas reseta o estado de fusão para que eles não travem.
            enemy1.SetMergingState(false, 0.2f);
            enemy2.SetMergingState(false, 0.2f);
            return;
        }

        // Se chegamos aqui, temos um winner e um loser definidos pela lógica de PPT.
        Debug.Log($"REGRA 4: Batalha PPT: {winner.name} ({winner.GetEnemyType()}) venceu {loser.name} ({loser.GetEnemyType()})!");
        Debug.Log($"{loser.name} (Perdedor) se transformará no tipo do vencedor ({winner.GetEnemyType()}).");

        // O perdedor se transforma no tipo do vencedor
        // 'IsInitialType' já deve ser false para inimigos que chegaram à Regra 4.

        ApplyTransformation(loser, winner.GetEnemyType()); // Aplica a transformação ao perdedor

        // Ambos os inimigos (vencedor e perdedor transformado) devem estar livres para futuras fusões
        winner.SetMergingState(false, 0.8f);
        loser.SetMergingState(false, 0.8f);

        return; // Encerra o método
    }

    /// <summary>
    /// Método auxiliar para aplicar as transformações de escala e velocidade baseadas no tipo de inimigo.
    /// Usado tanto na transformação do Initial quanto na transformação do perdedor no PPT.
    /// </summary>
    /// <param name="enemy">O inimigo a ser transformado.</param>
    /// <param name="targetType">O tipo para o qual o inimigo deve ser transformado.</param>
    private void ApplyTransformation(EnemyAI enemy, EnemyType targetType)
    {
        switch (targetType)
        {
            case EnemyType.Small:
                enemy.SetScale(smallScale);
                enemy.SetSpeed(initialMoveSpeed);
                break;
            case EnemyType.Fast:
                enemy.SetSpeed(maxSpeed);
                enemy.SetScale(initialScale); // Fast mantém escala inicial
                break;
            case EnemyType.Large:
                enemy.SetScale(maxScale);
                enemy.SetSpeed(initialMoveSpeed); // Large mantém velocidade inicial
                break;
            default:
                Debug.LogWarning($"Tentando aplicar transformação para tipo inesperado: {targetType}");
                break;
        }
        enemy.UpdateEnemyColor(); // Garante que a cor é atualizada após a transformação
    }

    /// <summary>
    /// Método auxiliar para aplicar as transformações de escala e velocidade baseadas em uma escolha aleatória (0=Small, 1=Fast, 2=Large).
    /// Usado especificamente para a transformação de dois inimigos Initial.
    /// </summary>
    /// <param name="enemy">O inimigo a ser transformado.</param>
    /// <param name="transformChoice">A escolha aleatória: 0 (Small), 1 (Fast), 2 (Large).</param>
    private void ApplyTransformation(EnemyAI enemy, int transformChoice)
    {
        switch (transformChoice)
        {
            case 0: // Transforma em Pequeno
                Debug.Log($"{enemy.name} se transformou em Pequeno!");
                ApplyTransformation(enemy, EnemyType.Small);
                break;
            case 1: // Transforma em Rápido
                Debug.Log($"{enemy.name} se transformou em Rápido!");
                ApplyTransformation(enemy, EnemyType.Fast);
                break;
            case 2: // Transforma em Grande
                Debug.Log($"{enemy.name} se transformou em Grande!");
                ApplyTransformation(enemy, EnemyType.Large);
                break;
        }
    }


    /// <summary>
    /// Altera a velocidade de perseguição do inimigo.
    /// </summary>
    /// <param name="newSpeed">A nova velocidade do inimigo.</param>
    public void SetSpeed(float newSpeed)
    {
        moveSpeed = Mathf.Min(newSpeed, GameManager.globalMaxEnemySpeed);
    }

    /// <summary>
    /// Altera a escala do inimigo.
    /// </summary>
    /// <param name="newScale">A nova escala (tamanho) do inimigo.</param>
    public void SetScale(float newScale)
    {
        transform.localScale = Vector3.one * newScale;
    }

    /// <summary>
    /// Define o estado de fusão do inimigo (temporariamente imune a novas fusões).
    /// </summary>
    /// <param name="state">True para imune, false para normal.</param>
    /// <param name="duration">Duração da imunidade. Se 0 ou menos, permanece no estado definido.</param>
    public void SetMergingState(bool state, float duration = 0f)
    {
        isMerging = state;
        if (state && duration > 0f)
        {
            StartCoroutine(ResetMergingState(duration));
        }
    }

    /// <summary>
    /// Corrotina para redefinir o estado de fusão após um atraso.
    /// </summary>
    private IEnumerator ResetMergingState(float delay)
    {
        yield return new WaitForSeconds(delay);
        isMerging = false;
    }
}