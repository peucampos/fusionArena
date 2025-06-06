// EnemySpawner.cs

using UnityEngine;
using System.Collections; // Necess�rio para Coroutines

public class EnemySpawner : MonoBehaviour
{
    [Header("Configura��es do Spawner")]
    [Tooltip("O Prefab do inimigo a ser instanciado.")]
    [SerializeField] private GameObject enemyPrefab;
    [Tooltip("O tempo entre cada ciclo de spawn de inimigos.")]
    [SerializeField] private float spawnInterval = 3f; // Torne este private para ser controlado por um m�todo
    [Tooltip("O tempo m�nimo que o spawnInterval pode atingir.")]
    [SerializeField] private float minSpawnInterval = 1f; // NOVO: Limite m�nimo para o tempo de spawn

    [Tooltip("A quantidade de inimigos a serem spawnados por ciclo.")]
    [SerializeField] private int enemiesPerSpawn = 1;

    [Header("Pontos de Spawn")]
    [Tooltip("Arraste os 4 GameObjects dos pontos de spawn aqui.")]
    [SerializeField] public Transform[] spawnPoints; // DEIXE PUBLIC para o GameManager acessar

    [Header("Limites do Jogo")]
    [Tooltip("O n�mero m�ximo de inimigos ativos na cena para evitar sobrecarga.")]
    [SerializeField] private int maxEnemies = 50;

    // ... (sua vari�vel initialEnemyScale)
    [Header("Configura��es Visuais de Inimigos")]
    [Tooltip("Escala inicial padr�o para inimigos rec�m-criados ou n�o modificados.")]
    public float initialEnemyScale = 1f;


    public int CurrentActiveEnemies { get; private set; } = 0;

    // Start � chamado antes da primeira atualiza��o do frame
    private void Start()
    {
        // Certifica-se de que a corrotina de spawn est� usando o valor inicial correto
        StopAllCoroutines(); // Para garantir que n�o h� corrotinas antigas rodando
        StartCoroutine(SpawnEnemiesRoutine());
    }

    // NOVO: Adicione este m�todo para ajustar o intervalo de spawn
    /// <summary>
    /// Ajusta o intervalo de spawn, respeitando o limite m�nimo.
    /// </summary>
    /// <param name="amount">A quantidade a ser subtra�da do intervalo.</param>
    public void DecreaseSpawnInterval(float amount)
    {
        spawnInterval = Mathf.Max(spawnInterval - amount, minSpawnInterval);
        Debug.Log($"Intervalo de Spawn Diminu�do para: {spawnInterval:F2}s");
        GameManager.Instance.currentLevel++;

        // � crucial reiniciar a corrotina para que ela use o novo intervalo
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
            yield return new WaitForSeconds(spawnInterval); // AGORA USA A VARI�VEL ATUALIZADA
        }
    }

    // ... (restante dos seus m�todos SpawnEnemy e SpawnEnemiesAtPosition)
    // Certifique-se de que SpawnEnemiesAtPosition usa o overrideScale corretamente
    public void SpawnEnemiesAtPosition(Vector2 position, int count, float overrideScale = -1f)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab n�o atribu�do no Spawner!", this);
            return;
        }

        float finalScale = (overrideScale > 0) ? overrideScale : initialEnemyScale;

        float spawnOffset = 1f; // Ajuste este valor conforme o tamanho dos seus inimigos

        for (int i = 0; i < count; i++)
        {
            if (CurrentActiveEnemies >= maxEnemies)
            {
                Debug.LogWarning("Limite m�ximo de inimigos atingido. N�o foi poss�vel spawnar todos os inimigos da multiplica��o.");
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
    /// Spawna um �nico inimigo em um dos pontos de spawn definidos.
    /// </summary>
    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab n�o atribu�do no Spawner!", this);
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Nenhum ponto de spawn atribu�do no Spawner! Atribua os 4 pontos no Inspector.", this);
            return;
        }

        // Escolhe um ponto de spawn aleatoriamente do array
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform chosenSpawnPoint = spawnPoints[randomIndex];

        // Instancia o inimigo na posi��o do ponto de spawn escolhido
        GameObject newEnemyObj = Instantiate(enemyPrefab, chosenSpawnPoint.position, Quaternion.identity);
        EnemyAI newEnemyAI = newEnemyObj.GetComponent<EnemyAI>();
        if (newEnemyAI != null)
        {
            newEnemyAI.SetMergingState(true, 0.5f); // Os inimigos rec�m-spawnados come�am "imunes" por 0.5s
            newEnemyAI.IsInitialType = true; // Definir como tipo Initial
            newEnemyAI.UpdateEnemyColor(); // Cor inicial do inimigo
        }
        CurrentActiveEnemies++; // Incrementa a contagem de inimigos ativos
        // Debug.Log($"Inimigo spawnado. Inimigos Ativos: {CurrentActiveEnemies}"); // Para debug
    }

    
    /// <summary>
    /// Decrementa a contagem de inimigos ativos quando um inimigo � desativado/destru�do.
    /// </summary>
    public void EnemyDeactivated()
    {
        CurrentActiveEnemies--;
        if (CurrentActiveEnemies < 0) CurrentActiveEnemies = 0; // Garante que n�o fique negativo
        // Debug.Log($"Inimigo desativado. Inimigos Ativos: {CurrentActiveEnemies}"); // Para debug
    }
}