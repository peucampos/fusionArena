// EnemySpawner.cs

using UnityEngine;
using System.Collections; // Necess�rio para Coroutines

public class EnemySpawner : MonoBehaviour
{
    [Header("Configura��es do Spawner")]
    [Tooltip("O Prefab do inimigo a ser instanciado.")]
    [SerializeField] private GameObject enemyPrefab; // Refer�ncia ao prefab do inimigo
    [Tooltip("O tempo entre cada ciclo de spawn de inimigos.")]
    [SerializeField] private float spawnInterval = 3f; // Intervalo de tempo para spawnar inimigos
    [Tooltip("A quantidade de inimigos a serem spawnados por ciclo.")]
    [SerializeField] private int enemiesPerSpawn = 1; // Quantidade de inimigos por spawn

    [Header("Pontos de Spawn")]
    [Tooltip("Arraste os 4 GameObjects dos pontos de spawn aqui.")]
    [SerializeField] private Transform[] spawnPoints; // Array para os pontos de spawn

    [Header("Limites do Jogo")]
    [Tooltip("O n�mero m�ximo de inimigos ativos na cena para evitar sobrecarga.")]
    [SerializeField] private int maxEnemies = 50; // Limite para evitar sobrecarga

    // Contador de inimigos ativos (para a pontua��o e limite)
    public int CurrentActiveEnemies { get; private set; } = 0; // Propriedade p�blica, mas setada privadamente

    // Start � chamado antes da primeira atualiza��o do frame
    private void Start()
    {
        // Inicia a corrotina de spawn de inimigos
        StartCoroutine(SpawnEnemiesRoutine());
    }

    /// <summary>
    /// Corrotina para spawnar inimigos em intervalos regulares, respeitando o limite.
    /// </summary>
    private IEnumerator SpawnEnemiesRoutine()
    {
        while (true) // Loop infinito para continuar spawnando
        {
            if (CurrentActiveEnemies < maxEnemies) // S� spawna se estiver abaixo do limite
            {
                for (int i = 0; i < enemiesPerSpawn; i++)
                {
                    SpawnEnemy();
                }
            }
            yield return new WaitForSeconds(spawnInterval); // Espera o intervalo antes do pr�ximo ciclo de spawn
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
    /// Spawna um n�mero espec�fico de inimigos em uma posi��o dada.
    /// Usado principalmente para a mec�nica de multiplica��o da fus�o.
    /// </summary>
    /// <param name="position">A posi��o onde os inimigos ser�o spawnados.</param>
    /// <param name="count">O n�mero de inimigos a serem spawnados.</param>
    /// <param name="initialScale">A escala inicial dos inimigos spawnados.</param>
    public void SpawnEnemiesAtPosition(Vector2 position, int count, float initialScale = 1f)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab n�o atribu�do no Spawner!", this);
            return;
        }

        // Adiciona um pequeno offset para que os inimigos n�o nas�am exatamente no mesmo lugar e colidam instantaneamente
        float spawnOffset = 1f; // Ajuste este valor conforme o tamanho dos seus inimigos

        for (int i = 0; i < count; i++)
        {
            if (CurrentActiveEnemies >= maxEnemies) // Verifica o limite antes de spawnar cada um
            {
                Debug.LogWarning("Limite m�ximo de inimigos atingido. N�o foi poss�vel spawnar todos os inimigos da multiplica��o.");
                break; // Sai do loop se o limite for atingido
            }

            // Gera um offset aleat�rio em X e Y
            Vector2 randomOffset = new Vector2(Random.Range(-spawnOffset, spawnOffset), Random.Range(-spawnOffset, spawnOffset));
            Vector2 spawnLocation = position + randomOffset; // Adiciona o offset � posi��o original

            GameObject newEnemyObj = Instantiate(enemyPrefab, spawnLocation, Quaternion.identity);
            EnemyAI newEnemyAI = newEnemyObj.GetComponent<EnemyAI>();
            if (newEnemyAI != null)
            {
                newEnemyAI.SetScale(initialScale);
                newEnemyAI.SetMergingState(true, 0.5f); // Os inimigos rec�m-spawnados tamb�m come�am "imunes" por 0.5s
                newEnemyAI.IsInitialType = true; // Definir como tipo Initial
                newEnemyAI.UpdateEnemyColor(); // Chame para garantir a cor inicial dos multiplicados
            }
            CurrentActiveEnemies++; // Incrementa a contagem de inimigos ativos
            // Debug.Log($"Inimigo (Multiplicado) spawnado. Inimigos Ativos: {CurrentActiveEnemies}"); // Para debug
        }
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