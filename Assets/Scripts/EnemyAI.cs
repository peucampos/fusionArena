using UnityEngine;
using System.Collections; // Necessário para Coroutines

public class EnemyAI : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [Tooltip("A velocidade para inimigos do tipo Normal (e Initial recém-spawnados).")]
    [SerializeField] private float normalEnemySpeed = 3f; // Nova velocidade para o tipo Normal (antes era mediumEnemySpeed)

    [Tooltip("A velocidade do inimigo do tipo Pequeno. (Mais rápido que Normal)")]
    [SerializeField] private float smallEnemySpeed = 4f; // Mantém a velocidade aumentada para Small

    [Tooltip("A velocidade do inimigo do tipo Grande. (Mais lento que Normal)")]
    [SerializeField] private float largeEnemySpeed = 2.0f; // Mantém a velocidade mais lenta para Large

    private float moveSpeed; // moveSpeed agora será a velocidade atual do inimigo.

    private Transform playerTransform; // Referência à posição do jogador
    private Rigidbody2D rb; // Referência ao Rigidbody2D do inimigo
    private EnemySpawner enemySpawner; // Referência ao EnemySpawner para comunicação

    private bool isMerging = false; // Flag para indicar que o inimigo está em processo de fusão ou recém-criado/fundido
    public bool IsInitialType { get; set; } = true; // True se o inimigo é do tipo 'Initial'. Será setado no OnEnable/Spawner.

    [Header("Categorias de Inimigo (para lógica de fusão)")]
    // REMOVIDO: fastThresholdSpeed não é mais necessário com o tipo Fast removido.
    // Os thresholds serão baseados nas próprias velocidades e escalas dos tipos.

    [Tooltip("Limite de tamanho/velocidade para ser considerado Grande (maior que normal).")]
    // Ajustado para usar a velocidade LargeEnemySpeed como um critério.
    // Vamos usar a velocidade LargeEnemySpeed para definir o limite inferior para Large.
    [SerializeField] private float largeThresholdSpeed = 2.5f; // Velocidade acima de largeEnemySpeed para ser large por velocidade.
    [Tooltip("Limite de tamanho para ser considerado Grande.")]
    [SerializeField] private float largeThresholdScale = 1.6f;

    [Tooltip("Limite de velocidade para ser considerado Pequeno (acima de normal).")]
    [SerializeField] private float smallThresholdSpeed = 3.5f; // Velocidade entre normal e smallEnemySpeed.
    [Tooltip("Limite de tamanho para ser considerado Pequeno.")]
    [SerializeField] private float smallThresholdScale = 0.9f;

    [Header("Configurações Visuais")]
    [Tooltip("Cor do inimigo inicial (default).")]
    [SerializeField] private Color initialEnemyColor = Color.cyan;
    [Tooltip("Cor do inimigo tipo Pequeno.")]
    [SerializeField] private Color smallEnemyColor = Color.green;
    [Tooltip("Cor do inimigo tipo Normal.")]
    [SerializeField] private Color normalEnemyColor = Color.blue; 
    [Tooltip("Cor do inimigo tipo Grande.")]
    [SerializeField] private Color largeEnemyColor = Color.magenta;

    private SpriteRenderer spriteRenderer; // Referência ao SpriteRenderer do inimigo

    [Header("Limites de Atributos")]
    [SerializeField] private float maxScale = 1.5f;
    [SerializeField] private float smallScale = 0.75f;
    [SerializeField] private float initialScale = 1f; // Escala padrão para inimigos recém-criados ou não modificados


    public enum EnemyType
    {
        Initial, // Inimigo recém-criado, sem categoria definida
        Small,   // Inimigos pequenos
        Large,   // Inimigos grandes
        Normal   // Inimigos de velocidade padrão (antes era Fast, agora representa o 3f)
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
        moveSpeed = normalEnemySpeed; // Inimigo Initial começa com a velocidade do tipo Normal (3f)
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

    /// <summary>
    /// Retorna o tipo atual do inimigo baseado em sua velocidade e escala.
    /// A ordem das verificações é crucial para a correta classificação.
    /// </summary>
    public EnemyType GetEnemyType()
    {
        if (IsInitialType)
        {
            return EnemyType.Initial;
        }
        else // Se não é mais inicial, aplica a lógica de Normal, Large, Small
        {
            // Ordem de prioridade para classificação (se as condições se sobrepuserem):
            // Small (mais rápido) > Large (maior) > Normal (padrão)

            // 1. Prioridade para SMALL: baseado em velocidade (o mais rápido)
            if (moveSpeed >= smallThresholdSpeed) // Se a velocidade é maior que o threshold de Small
            {
                return EnemyType.Small;
            }
            // 2. Prioridade para LARGE: baseado em escala (o maior)
            // Certifique-se de que não é Small pela velocidade e que a escala é grande
            else if (transform.localScale.x >= largeThresholdScale)
            {
                return EnemyType.Large;
            }
            // 3. Caso contrário, se não se encaixa em Small ou Large, é NORMAL
            // Este será o fallback para qualquer inimigo modificado que não seja Small ou Large.
            // A velocidade será a do tipo Normal (3f) se for transformado para ele.
            return EnemyType.Normal;
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
            case EnemyType.Normal: // AGORA É Normal
                spriteRenderer.color = normalEnemyColor; // Usa a nova cor para Normal
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
        EnemyAI otherEnemy = collision.gameObject.GetComponent<EnemyAI>();

        if (otherEnemy != null && otherEnemy != this && !this.isMerging && !otherEnemy.isMerging)
        {
            this.SetMergingState(true);
            otherEnemy.SetMergingState(true);
            HandleEnemyFusion(this, otherEnemy);
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            EnemyType currentEnemyCollidedType = GetEnemyType();
            if (currentEnemyCollidedType != EnemyType.Initial)
            {
                Debug.Log($"Inimigo modificado ({currentEnemyCollidedType}) colidiu com o Player! Game Over!");
                if (GameManager.Instance != null)
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

        // bool isEnemy1Initial = enemy1.IsInitialType; // Já temos na HandleEnemyFusion
        // bool isEnemy2Initial = enemy2.IsInitialType; // Já temos na HandleEnemyFusion

        // --- HIERARQUIA DE REGRAS ---

        // REGRA 1: Dois inimigos do tipo Initial colidem.
        if (enemy1.IsInitialType && enemy2.IsInitialType)
        {
            Debug.Log("REGRA 1: Dois inimigos iniciais colidiram! Um se transforma, o outro é desativado/reciclado.");
            EnemyAI transformedEnemy = (Random.value > 0.5f) ? enemy1 : enemy2;
            EnemyAI deactivatedEnemy = (transformedEnemy == enemy1) ? enemy2 : enemy1;

            if (enemySpawner != null)
            {
                enemySpawner.EnemyDeactivated();
            }
            deactivatedEnemy.gameObject.SetActive(false);

            transformedEnemy.IsInitialType = false; // MUITO IMPORTANTE: Garante que nunca mais será Initial

            // Escolha aleatória entre Small, Normal, Large
            int transformChoice = Random.Range(0, 3); // 0, 1, 2
            ApplyTransformation(transformedEnemy, transformChoice); // Aplica a transformação

            transformedEnemy.SetMergingState(false, 0.8f);
            transformedEnemy.UpdateEnemyColor();
            return;
        }

        // REGRA 2: Um inimigo Initial colide com um inimigo MODIFICADO.
        if ((enemy1.IsInitialType && !enemy2.IsInitialType) || (!enemy1.IsInitialType && enemy2.IsInitialType))
        {
            Debug.Log($"REGRA 2: Inimigo Initial colidiu com Modificado. Nenhuma ação de fusão. Apenas reseta estado de fusão.");
            enemy1.SetMergingState(false, 0.2f);
            enemy2.SetMergingState(false, 0.2f);
            return;
        }

        // A partir daqui, ambos os inimigos NÃO SÃO DO TIPO INITIAL.
        EnemyType type1 = enemy1.GetEnemyType();
        EnemyType type2 = enemy2.GetEnemyType();

        // REGRA 3: Inimigos do mesmo tipo (Small, Large, Normal) colidem.
        if (type1 == type2)
        {
            Debug.Log($"REGRA 3: Inimigos do mesmo tipo ({type1}) colidiram! Colisão ignorada, resetando estado de fusão.");
            enemy1.SetMergingState(false, 0.2f);
            enemy2.SetMergingState(false, 0.2f);
            return;
        }

        // REGRA 4: Inimigos de tipos diferentes (Small, Large, Normal) colidem.
        EnemyAI winner = null;
        EnemyAI loser = null;

        // Lógica de Pedra, Papel e Tesoura: Small (Pequeno) > Large (Grande) > Normal (Padrão) > Small
        // Atenção: A ordem aqui define a "vitória" e deve ser circular.
        // Small (mais rápido) vence Large (maior)
        // Large (maior) vence Normal (padrão)
        // Normal (padrão) vence Small (mais rápido) -- Invertido do Fast > Small. Agora Normal é o mais fraco no PPT?
        // Se Normal vence Small, a ordem é: Normal > Small > Large > Normal
        // Se Small vence Large, e Large vence Normal, e Normal vence Small, a ordem é:
        // Small > Large
        // Large > Normal
        // Normal > Small
        // Isso é uma cadeia.
        // Se a ordem for: Normal (3f) -> Small (4.5f) -> Large (2.0f) -> Normal
        // Normal (3f) > Small (4.5f) -> Fica estranho se Normal é mais lento.

        // RE-DEFININDO A LÓGICA DE PPT PARA SUAS VELOCIDADES:
        // Vamos pensar na "força" do inimigo: Pequeno (rápido) > Grande (robusto) > Normal (equilibrado)
        // Pequeno (mais rápido) vence Grande (mais lento)
        // Grande (mais lento/robusto) vence Normal (velocidade média)
        // Normal (velocidade média) vence Pequeno (mais rápido) - para criar um ciclo.

        if ((type1 == EnemyType.Small && type2 == EnemyType.Large) || // Small vence Large
            (type1 == EnemyType.Large && type2 == EnemyType.Normal) || // Large vence Normal
            (type1 == EnemyType.Normal && type2 == EnemyType.Small))  // Normal vence Small
        {
            winner = enemy1; loser = enemy2;
        }
        else if ((type2 == EnemyType.Small && type1 == EnemyType.Large) ||
                 (type2 == EnemyType.Large && type1 == EnemyType.Normal) ||
                 (type2 == EnemyType.Normal && type1 == EnemyType.Small))
        {
            winner = enemy2; loser = enemy1;
        }

        if (winner == null || loser == null)
        {
            Debug.LogError("ERRO LÓGICO: Colisão entre tipos diferentes de inimigos modificados sem vencedor/perdedor claro. Verifique a lógica de PPT ou se todos os tipos foram considerados!");
            enemy1.SetMergingState(false, 0.2f);
            enemy2.SetMergingState(false, 0.2f);
            return;
        }

        Debug.Log($"REGRA 4: Batalha PPT: {winner.name} ({winner.GetEnemyType()}) venceu {loser.name} ({loser.GetEnemyType()})!");
        Debug.Log($"{loser.name} (Perdedor) se transformará no tipo do vencedor ({winner.GetEnemyType()}).");

        ApplyTransformation(loser, winner.GetEnemyType());

        winner.SetMergingState(false, 0.8f);
        loser.SetMergingState(false, 0.8f);

        return;
    }

    /// <summary>
    /// Método auxiliar para aplicar as transformações de escala e velocidade baseadas no tipo de inimigo.
    /// Usado tanto na transformação do Initial quanto na transformação do perdedor no PPT.
    /// </summary>
    /// <param name="enemy">O inimigo a ser transformado.</param>
    /// <param name="targetType">O tipo para o qual o inimigo deve ser transformado.</param>
    private void ApplyTransformation(EnemyAI enemy, EnemyType targetType)
    {
        // Garante que o inimigo não é mais do tipo Initial após a transformação
        enemy.IsInitialType = false;

        switch (targetType)
        {
            case EnemyType.Small:
                enemy.SetScale(smallScale);
                enemy.SetSpeed(smallEnemySpeed); // Velocidade do Small (4.5f)
                Debug.Log($"{enemy.name} agora é Small: Velocidade={enemy.moveSpeed}, Escala={enemy.transform.localScale.x}");
                break;
            case EnemyType.Normal: // Tipo renomeado para Normal
                enemy.SetSpeed(normalEnemySpeed); // Velocidade do Normal (3f)
                enemy.SetScale(initialScale); // Normal mantém escala inicial
                Debug.Log($"{enemy.name} agora é Normal: Velocidade={enemy.moveSpeed}, Escala={enemy.transform.localScale.x}");
                break;
            case EnemyType.Large:
                enemy.SetScale(maxScale);
                enemy.SetSpeed(largeEnemySpeed); // Velocidade do Large (2.0f)
                Debug.Log($"{enemy.name} agora é Large: Velocidade={enemy.moveSpeed}, Escala={enemy.transform.localScale.x}");
                break;
            default:
                Debug.LogWarning($"Tentando aplicar transformação para tipo inesperado: {targetType}");
                break;
        }
        enemy.UpdateEnemyColor(); // Garante que a cor é atualizada após a transformação
    }

    /// <summary>
    /// Método auxiliar para aplicar as transformações de escala e velocidade baseadas em uma escolha aleatória (0=Small, 1=Normal, 2=Large).
    /// Usado especificamente para a transformação de dois inimigos Initial.
    /// </summary>
    /// <param name="enemy">O inimigo a ser transformado.</param>
    /// <param name="transformChoice">A escolha aleatória: 0 (Small), 1 (Normal), 2 (Large).</param>
    private void ApplyTransformation(EnemyAI enemy, int transformChoice)
    {
        enemy.IsInitialType = false; // Garante que não é Initial

        switch (transformChoice)
        {
            case 0: // Transforma em Pequeno
                Debug.Log($"{enemy.name} se transformou em Pequeno (por escolha aleatória)!");
                ApplyTransformation(enemy, EnemyType.Small);
                break;
            case 1: // Transforma em Normal
                Debug.Log($"{enemy.name} se transformou em Normal (por escolha aleatória)!");
                ApplyTransformation(enemy, EnemyType.Normal);
                break;
            case 2: // Transforma em Grande
                Debug.Log($"{enemy.name} se transformou em Grande (por escolha aleatória)!");
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
        // Se GameManager.globalMaxEnemySpeed for usado como um *limite absoluto*,
        // ele ainda pode ser aplicado aqui. Caso contrário, esta linha pode ser removida.
        // Por exemplo, se você *sempre* quer que a velocidade seja exatamente a definida para o tipo:
        moveSpeed = newSpeed;

        // Se você QUER um limite global, mantenha a linha abaixo:
        // moveSpeed = Mathf.Min(newSpeed, GameManager.globalMaxEnemySpeed);
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