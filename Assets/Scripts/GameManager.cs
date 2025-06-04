using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; // Certifique-se de que esta linha est� presente

public class GameManager : MonoBehaviour
{
    // Singleton pattern
    public static GameManager Instance { get; private set; }

    [Header("Configura��es do Jogo")]
    private bool gameStarted = false;
    private bool gameOver = false;

    // Vari�veis de pontua��o
    [Header("Configura��es de Pontua��o")]
    public int currentScore = 0; // Vari�vel para armazenar a pontua��o total
    public int scorePerNormalEnemy = 1; // Pontos por inimigo normal ativo
    public int scorePerModifiedEnemy = 2; // Pontos por inimigo modificado ativo
    public int scorePerFusion = 3; // Pontos por fus�o
    public int scorePerModifiedDestroyingModified = 4; // Pontos por modificado destruir modificado
    public int scorePer30Seconds = 30; // Pontos a cada 30 segundos (1 ponto por segundo de jogo a cada 30 segundos)

    [SerializeField] private EnemySpawner enemySpawner; // Refer�ncia ao EnemySpawner
    [SerializeField] private TMP_Text score; // Refer�ncia ao componente de texto para exibir a pontua��o

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
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
        currentScore = 0; // Reseta a pontua��o ao iniciar o jogo

        // Inicia as corrotinas de pontua��o
        StartCoroutine(CalculateActiveEnemiesScoreRoutine());
        StartCoroutine(CalculateTimeScoreRoutine());
    }

    private void Update()
    {
        if (!gameStarted || gameOver) return;

        score.text = currentScore.ToString(); // Atualiza o texto da pontua��o
    }

    public void GameOver()
    {
        if (gameOver) return; // Evita m�ltiplas chamadas

        gameOver = true;
        Debug.Log("Game Over! Sua pontua��o final: " + currentScore);
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Adiciona pontos � pontua��o total.
    /// </summary>
    /// <param name="points">Quantidade de pontos a adicionar.</param>
    public void AddScore(int points)
    {
        if (!gameOver)
        {
            currentScore += points;
            Debug.Log($"Pontua��o atual: {currentScore}"); // Para debug
            // Futuramente, voc� pode atualizar a UI aqui
        }
    }

    /// <summary>
    /// Corrotina para calcular e adicionar pontos de inimigos ativos a cada 3 segundos.
    /// </summary>
    private System.Collections.IEnumerator CalculateActiveEnemiesScoreRoutine()
    {
        while (!gameOver)
        {
            if (enemySpawner != null)
            {
                int normalEnemiesCount = 0;
                int modifiedEnemiesCount = 0;

                // Percorrer todos os inimigos ativos na cena para classific�-los
                // Isso assume que todos os inimigos t�m a tag "Enemy"
                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
                foreach (GameObject enemyObj in enemies)
                {
                    EnemyAI enemy = enemyObj.GetComponent<EnemyAI>();
                    if (enemy != null)
                    {
                        if (enemy.IsInitialType) // Initial � o "normal" inicial
                        {
                            normalEnemiesCount++;
                        }
                        else // Tudo que n�o � Initial � "modificado"
                        {
                            modifiedEnemiesCount++;
                        }
                    }
                }

                AddScore(normalEnemiesCount * scorePerNormalEnemy);
                AddScore(modifiedEnemiesCount * scorePerModifiedEnemy);
            }
            yield return new WaitForSeconds(3f); // A cada 3 segundos
        }
    }

    /// <summary>
    /// Corrotina para adicionar pontos com base no tempo de jogo a cada 30 segundos.
    /// </summary>
    private System.Collections.IEnumerator CalculateTimeScoreRoutine()
    {
        while (!gameOver)
        {
            yield return new WaitForSeconds(30f); // Espera 30 segundos
            if (!gameOver) // Verifica novamente, caso o jogo tenha terminado durante a espera
            {
                // Calcula quantos "blocos" de 30 segundos j� passaram
                // Poder�amos usar (gameDuration - timeRemaining) para calcular o tempo passado
                // mas a regra "30 ganha 30" implica em 1 ponto por segundo, contado a cada 30s.
                // Uma forma mais simples: A cada 30s, adiciona 'scorePer30Seconds' pontos.
                AddScore(scorePer30Seconds);
            }
        }
    }
}