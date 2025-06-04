// GameManager.cs

using UnityEngine;
using UnityEngine.SceneManagement; // Necessário para recarregar a cena ou carregar outra

public class GameManager : MonoBehaviour
{
    // Padrão Singleton: Garante que só existe uma instância do GameManager.
    public static GameManager Instance { get; private set; }

    // Variável global para a velocidade máxima do inimigo (usada em EnemyAI.SetSpeed)
    // Se você não tiver um GameManager para isso, pode ser uma variável em EnemyAI ou em um ScriptableObject.
    public static float globalMaxEnemySpeed = 7f; // Ajuste este valor conforme necessário

    // Flag para verificar se o jogo já terminou
    private bool isGameOver = false;

    // Awake é chamado quando o script é carregado
    private void Awake()
    {
        // Implementação do Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destrói esta nova instância se já existir outra
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Mantém o GameManager ativo entre cenas (opcional, dependendo do seu fluxo de jogo)

        Debug.Log("GameManager inicializado.");
    }

    /// <summary>
    /// Método chamado quando o jogo deve terminar.
    /// </summary>
    public void GameOver()
    {
        if (isGameOver) return; // Evita múltiplas chamadas de Game Over

        isGameOver = true;
        Debug.Log("FIM DE JOGO!");

        // Aqui você pode adicionar lógica para:
        Time.timeScale = 0f; // Pausa o jogo

        // Exibir uma tela de Game Over (você precisaria ter uma UI para isso)
        // Por exemplo: gameOverPanel.SetActive(true);

        // Recarregar a cena atual após um atraso, ou carregar uma cena de Game Over
        // StartCoroutine(ReloadSceneAfterDelay(2f));
    }

    // Exemplo de como você pode reiniciar a cena
    private System.Collections.IEnumerator ReloadSceneAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Usa WaitForSecondsRealtime para ignorar Time.timeScale = 0
        Time.timeScale = 1f; // Volta o tempo ao normal antes de recarregar
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Recarrega a cena atual
    }

    // Você pode adicionar um método para reiniciar o jogo manualmente, talvez de um botão de UI
    public void RestartGame()
    {
        Time.timeScale = 1f; // Garante que o tempo está normal
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Recarrega a cena atual
        isGameOver = false; // Reseta a flag de Game Over
    }
}