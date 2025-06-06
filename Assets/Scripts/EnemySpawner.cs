// EnemySpawner.cs

using UnityEngine;
using System.Collections; // Necessário para Coroutines

public class EnemySpawner : MonoBehaviour
{
    [Header("Configurações do Spawner")]
    [Tooltip("O Prefab do inimigo a ser instanciado.")]
    [SerializeField] private GameObject enemyPrefab;
    [Tooltip("O tempo entre cada ciclo de spawn de inimigos.")]
    [SerializeField] private float spawnInterval = 3f; // Torne este private para ser controlado por um método
    [Tooltip("O tempo mínimo que o spawnInterval pode atingir.")]
    [SerializeField] private float minSpawnInterval = 1f; // NOVO: Limite mínimo para o tempo de spawn

    [Tooltip("A quantidade de inimigos a serem spawnados por ciclo.")]
    [SerializeField] private int enemiesPerSpawn = 1;

    [Header("Pontos de Spawn")]
    [Tooltip("Arraste os 4 GameObjects dos pontos de spawn aqui.")]
    [SerializeField] public Transform[] spawnPoints; // DEIXE PUBLIC para o GameManager acessar

    [Header("Limites do Jogo")]
    [Tooltip("O número máximo de inimigos ativos na cena para evitar sobrecarga.")]
    [SerializeField] private int maxEnemies = 50;

    // ... (sua variável initialEnemyScale)
    [Header("Configurações Visuais de Inimigos")]
    [Tooltip("Escala inicial padrão para inimigos recém-criados ou não modificados.")]
    public float initialEnemyScale = 1f;


    public int CurrentActiveEnemies { get; private set; } = 0;

    // Start é chamado antes da primeira atualização do frame
    private void Start()
    {
        // Certifica-se de que a corrotina de spawn está usando o valor inicial correto
        StopAllCoroutines(); // Para garantir que não há corrotinas antigas rodando
        StartCoroutine(SpawnEnemiesRoutine());
    }

    // NOVO: Adicione este método para ajustar o intervalo de spawn
    /// <summary>
    /// Ajusta o intervalo de spawn, respeitando o limite mínimo.
    /// </summary>
    /// <param name="amount">A quantidade a ser subtraída do intervalo.</param>
    public void DecreaseSpawnInterval(float amount)
    {
        spawnInterval = Mathf.Max(spawnInterval - amount, minSpawnInterval);
        Debug.Log($"Intervalo de Spawn Diminuído para: {spawnInterval:F2}s");
        GameManager.Instance.currentLevel++;

        // É crucial reiniciar a corrotina para que ela use o novo intervalo
        StopAllCoroutines(); // Para a corrotina atual
        StartCoroutine(SpawnEnemiesRoutine()); // Inicia uma nova com o novo intervalo
    }

    /// <summary>
    /// Corrotina para spawnar inimigos em intervalos regulares, respeitando o limite.
    /// </summary>
    private IEnumerator SpawnEnemiesRoutine()
    {
        while (true)
        {
            if (CurrentActiveEnemies < maxEnemies)
            {
                for (int i = 0; i < enemiesPerSpawn; i++)
                {
                    SpawnEnemy();
                }
            }
            yield return new WaitForSeconds(spawnInterval); // AGORA USA A VARIÁVEL ATUALIZADA
        }
    }

    // ... (restante dos seus métodos SpawnEnemy e SpawnEnemiesAtPosition)
    // Certifique-se de que SpawnEnemiesAtPosition usa o overrideScale corretamente
    public void SpawnEnemiesAtPosition(Vector2 position, int count, float overrideScale = -1f)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab não atribuído no Spawner!", this);
            return;
        }

        float finalScale = (overrideScale > 0) ? overrideScale : initialEnemyScale;

        float spawnOffset = 1f; // Ajuste este valor conforme o tamanho dos seus inimigos

        for (int i = 0; i < count; i++)
        {
            if (CurrentActiveEnemies >= maxEnemies)
            {
                Debug.LogWarning("Limite máximo de inimigos atingido. Não foi possível spawnar todos os inimigos da multiplicação.");
                break;
            }

            Vector2 randomOffset = new Vector2(Random.Range(-spawnOffset, spawnOffset), Random.Range(-spawnOffset, spawnOffset));
            Vector2 spawnLocation = position + randomOffset;

            GameObject newEnemyObj = Instantiate(enemyPrefab, spawnLocation, Quaternion.identity);
            EnemyAI newEnemyAI = newEnemyObj.GetComponent<EnemyAI>();
            if (newEnemyAI != null)
            {
                newEnemyAI.SetScale(finalScale); // CORRIGIDO PARA USAR finalScale
                newEnemyAI.SetMergingState(true, 0.5f);
                newEnemyAI.IsInitialType = true;
                newEnemyAI.UpdateEnemyColor();
            }
            CurrentActiveEnemies++;
        }
    }

    /// <summary>
    /// Spawna um único inimigo em um dos pontos de spawn definidos.
    /// </summary>
    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab não atribuído no Spawner!", this);
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Nenhum ponto de spawn atribuído no Spawner! Atribua os 4 pontos no Inspector.", this);
            return;
        }

        // Escolhe um ponto de spawn aleatoriamente do array
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform chosenSpawnPoint = spawnPoints[randomIndex];

        // Instancia o inimigo na posição do ponto de spawn escolhido
        GameObject newEnemyObj = Instantiate(enemyPrefab, chosenSpawnPoint.position, Quaternion.identity);
        EnemyAI newEnemyAI = newEnemyObj.GetComponent<EnemyAI>();
        if (newEnemyAI != null)
        {
            newEnemyAI.SetMergingState(true, 0.5f); // Os inimigos recém-spawnados começam "imunes" por 0.5s
            newEnemyAI.IsInitialType = true; // Definir como tipo Initial
            newEnemyAI.UpdateEnemyColor(); // Cor inicial do inimigo
        }
        CurrentActiveEnemies++; // Incrementa a contagem de inimigos ativos
        // Debug.Log($"Inimigo spawnado. Inimigos Ativos: {CurrentActiveEnemies}"); // Para debug
    }

    
    /// <summary>
    /// Decrementa a contagem de inimigos ativos quando um inimigo é desativado/destruído.
    /// </summary>
    public void EnemyDeactivated()
    {
        CurrentActiveEnemies--;
        if (CurrentActiveEnemies < 0) CurrentActiveEnemies = 0; // Garante que não fique negativo
        // Debug.Log($"Inimigo desativado. Inimigos Ativos: {CurrentActiveEnemies}"); // Para debug
    }
}