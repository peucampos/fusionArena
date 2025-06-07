using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configurações do Jogo")]
    private bool gameStarted = false;
    private bool gameOver = false;

    // ... (Variáveis de pontuação existentes)
    [Header("Configurações de Pontuação")]
    public int currentScore = 0;
    public int currentLevel = 1;
    public int scorePerNormalEnemy = 1;
    public int scorePerModifiedEnemy = 2;
    public int scorePerFusion = 3;
    public int scorePerModifiedDestroyingModified = 4;

    // Nova regra: Presença Massiva de Inimigos (AGORA COM DIMINUIÇÃO DE TEMPO DE SPAWN)
    [Header("Nova Regra: Presença Massiva de Inimigos")]
    [Tooltip("Número mínimo de inimigos do mesmo tipo (modificado) na tela para ativá-lo.")]
    [SerializeField] private int minEnemiesForMassPresence = 5;
    [Tooltip("Pontos concedidos pela ativação da regra de presença massiva.")]
    [SerializeField] private int scoreForMassPresence = 30;
    [Tooltip("Intervalo de tempo para verificar a presença massiva de inimigos.")]
    [SerializeField] private float massPresenceCheckInterval = 1f;
    [Tooltip("Quantidade em segundos que o tempo de spawn diminui quando a regra é ativada.")]
    [SerializeField] private float spawnDecreaseAmount = 0.25f; // NOVO: Quantidade de diminuição

    private EnemySpawner enemySpawner;

    [SerializeField] private TMP_Text score;
    [SerializeField] private GameObject tryAgainBtn;
    [SerializeField] private TMP_Text level;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }

        enemySpawner = GetComponent<EnemySpawner>(); // Tenta obter o EnemySpawner do mesmo GameObject
        if (enemySpawner == null)
        {
            Debug.LogError("EnemySpawner component não encontrado NO MESMO GameObject do GameManager! Certifique-se de que o script EnemySpawner está anexado a este GameObject.", this);
        }

    }

    private void Start()
    {
        StartGame();
    }

    private void StartGame()
    {
        gameStarted = true;
        gameOver = false;
        currentScore = 0;

        StartCoroutine(CalculateActiveEnemiesScoreRoutine());
        StartCoroutine(CheckForMassEnemyPresenceRoutine());
    }

    private void Update()
    {
        if (!gameStarted || gameOver) return;

        score.text = currentScore.ToString();
        level.text = currentLevel.ToString();
    }

    public void GameOver()
    {
        if (gameOver) return;

        gameOver = true;
        Debug.Log("Game Over! Sua pontuação final: " + currentScore);
        tryAgainBtn.SetActive(true);
        Time.timeScale = 0;
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void TryAgain()
    {
        tryAgainBtn.SetActive(false);
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Pause()
    { 
        if (gameOver) return;

        Time.timeScale = Time.timeScale == 0 ? 1 : 0;
    }

    public void AddScore(int points)
    {
        if (!gameOver)
        {
            currentScore += points;
            Debug.Log($"Pontuação atual: {currentScore}");
        }
    }

    private IEnumerator CalculateActiveEnemiesScoreRoutine()
    {
        while (!gameOver)
        {
            if (enemySpawner != null)
            {
                int normalEnemiesCount = 0;
                int modifiedEnemiesCount = 0;

                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
                foreach (GameObject enemyObj in enemies)
                {
                    EnemyAI enemy = enemyObj.GetComponent<EnemyAI>();
                    if (enemy != null)
                    {
                        if (enemy.IsInitialType)
                        {
                            normalEnemiesCount++;
                        }
                        else
                        {
                            modifiedEnemiesCount++;
                        }
                    }
                }

                AddScore(normalEnemiesCount * scorePerNormalEnemy);
                AddScore(modifiedEnemiesCount * scorePerModifiedEnemy);
            }
            yield return new WaitForSeconds(3f);
        }
    }

    /// <summary>
    /// Corrotina para verificar periodicamente a presença massiva de inimigos do mesmo tipo.
    /// </summary>
    private IEnumerator CheckForMassEnemyPresenceRoutine()
    {
        Debug.Log("CheckForMassEnemyPresenceRoutine: Iniciada."); // Adicione esta linha
        while (!gameOver)
        {
            yield return new WaitForSeconds(massPresenceCheckInterval);

            if (gameOver)
            {
                Debug.Log("CheckForMassEnemyPresenceRoutine: Jogo terminou, rotina parada."); // Adicione esta linha
                break;
            }

            Debug.Log($"CheckForMassEnemyPresenceRoutine: Verificando inimigos ativos. (Intervalo: {massPresenceCheckInterval}s)"); // Adicione esta linha

            Dictionary<EnemyAI.EnemyType, List<EnemyAI>> modifiedEnemiesByType = new Dictionary<EnemyAI.EnemyType, List<EnemyAI>>();

            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            Debug.Log($"CheckForMassEnemyPresenceRoutine: Encontrados {enemies.Length} GameObjects com a tag 'Enemy'."); // Adicione esta linha

            foreach (GameObject enemyObj in enemies)
            {
                EnemyAI enemy = enemyObj.GetComponent<EnemyAI>();
                if (enemy != null) // Verifique se o componente EnemyAI existe
                {
                    // Debug.Log($"Inimigo encontrado: {enemyObj.name}, IsInitialType: {enemy.IsInitialType}, Type: {enemy.GetEnemyType()}"); // Detalhado, pode gerar muitos logs

                    if (!enemy.IsInitialType) // Apenas inimigos MODIFICADOS são contados
                    {
                        EnemyAI.EnemyType type = enemy.GetEnemyType();
                        // Embora o !enemy.IsInitialType já filtre, uma redundância para clareza
                        if (type != EnemyAI.EnemyType.Initial)
                        {
                            if (!modifiedEnemiesByType.ContainsKey(type))
                            {
                                modifiedEnemiesByType.Add(type, new List<EnemyAI>());
                            }
                            modifiedEnemiesByType[type].Add(enemy);
                        }
                    }
                }
            }

            if (modifiedEnemiesByType.Count == 0)
            {
                Debug.Log("CheckForMassEnemyPresenceRoutine: Nenhum inimigo modificado encontrado nesta checagem.");
            }
            foreach (var entry in modifiedEnemiesByType)
            {
                Debug.Log($"CheckForMassEnemyPresenceRoutine: Tipo {entry.Key} - Contagem: {entry.Value.Count}");
            }
            // --- Fim das novas linhas de contagem ---

            foreach (var entry in modifiedEnemiesByType)
            {
                EnemyAI.EnemyType type = entry.Key;
                List<EnemyAI> enemiesOfThisType = entry.Value;

                if (enemiesOfThisType.Count >= minEnemiesForMassPresence)
                {
                    Debug.Log($"Regra de Presença Massiva ATIVADA! {enemiesOfThisType.Count} inimigos do tipo {type} na tela.");

                    Debug.Log("Iniciando destruição dos inimigos de presença massiva..."); // NOVO LOG
                    foreach (EnemyAI enemyToDestroy in enemiesOfThisType)
                    {
                        if (enemyToDestroy != null && enemyToDestroy.gameObject.activeInHierarchy)
                        {
                            Debug.Log($"Tentando destruir inimigo: {enemyToDestroy.name} (Tipo: {enemyToDestroy.GetEnemyType()})"); // NOVO LOG
                            enemySpawner.EnemyDeactivated(); // Decrementa a contagem de inimigos ativos no spawner
                            Destroy(enemyToDestroy.gameObject);
                            // A destruição não é instantânea. O objeto será destruído no final do frame.
                            // Para depuração, você pode verificar se ele ainda existe *neste frame*.
                            // Mas o importante é que no próximo frame ele deve sumir.
                        }
                        else
                        {
                            Debug.LogWarning($"Inimigo para destruir é nulo ou inativo na lista: {enemyToDestroy?.name ?? "Nulo/Destruído Externamente"}"); // NOVO LOG para casos estranhos
                        }
                    }
                    Debug.Log("Destruição dos inimigos de presença massiva concluída."); // NOVO LOG

                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySFX(AudioManager.Instance.massPresenceDestroySFX); // Toca o som de fusão
                    }
                    // Diminuir o tempo de spawn
                    if (enemySpawner != null)
                    {
                        enemySpawner.DecreaseSpawnInterval(spawnDecreaseAmount);
                        Debug.Log($"Chamando DecreaseSpawnInterval no EnemySpawner."); // NOVO LOG
                    }
                    else
                    {
                        Debug.LogWarning("EnemySpawner não encontrado para diminuir o tempo de spawn."); // Este já existia
                    }

                    // Adicionar pontos ao jogador
                    AddScore(scoreForMassPresence);
                    Debug.Log($"Pontos por presença massiva: +{scoreForMassPresence}. Pontuação total: {currentScore}"); // ATUALIZADO: Mostra total

                    break; // IMPORTANTE: Saia do loop após ativar a regra para evitar ativações múltiplas no mesmo frame

                }
            }
        }
    }

}