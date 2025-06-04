// GameManager.cs

using UnityEngine;
using UnityEngine.SceneManagement; // Necess�rio para recarregar a cena ou carregar outra

public class GameManager : MonoBehaviour
{
    // Padr�o Singleton: Garante que s� existe uma inst�ncia do GameManager.
    public static GameManager Instance { get; private set; }

    // Vari�vel global para a velocidade m�xima do inimigo (usada em EnemyAI.SetSpeed)
    // Se voc� n�o tiver um GameManager para isso, pode ser uma vari�vel em EnemyAI ou em um ScriptableObject.
    public static float globalMaxEnemySpeed = 7f; // Ajuste este valor conforme necess�rio

    // Flag para verificar se o jogo j� terminou
    private bool isGameOver = false;

    // Awake � chamado quando o script � carregado
    private void Awake()
    {
        // Implementa��o do Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destr�i esta nova inst�ncia se j� existir outra
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Mant�m o GameManager ativo entre cenas (opcional, dependendo do seu fluxo de jogo)

        Debug.Log("GameManager inicializado.");
    }

    /// <summary>
    /// M�todo chamado quando o jogo deve terminar.
    /// </summary>
    public void GameOver()
    {
        if (isGameOver) return; // Evita m�ltiplas chamadas de Game Over

        isGameOver = true;
        Debug.Log("FIM DE JOGO!");

        // Aqui voc� pode adicionar l�gica para:
        Time.timeScale = 0f; // Pausa o jogo

        // Exibir uma tela de Game Over (voc� precisaria ter uma UI para isso)
        // Por exemplo: gameOverPanel.SetActive(true);

        // Recarregar a cena atual ap�s um atraso, ou carregar uma cena de Game Over
        // StartCoroutine(ReloadSceneAfterDelay(2f));
    }

    // Exemplo de como voc� pode reiniciar a cena
    private System.Collections.IEnumerator ReloadSceneAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Usa WaitForSecondsRealtime para ignorar Time.timeScale = 0
        Time.timeScale = 1f; // Volta o tempo ao normal antes de recarregar
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Recarrega a cena atual
    }

    // Voc� pode adicionar um m�todo para reiniciar o jogo manualmente, talvez de um bot�o de UI
    public void RestartGame()
    {
        Time.timeScale = 1f; // Garante que o tempo est� normal
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Recarrega a cena atual
        isGameOver = false; // Reseta a flag de Game Over
    }
}