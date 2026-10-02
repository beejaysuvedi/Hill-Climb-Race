using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameObject gameOverCanvas;

    [Header("Game Over UI")]
    [SerializeField] private TMP_Text finalDistanceText;

    [Header("Player")]
    [SerializeField] private Transform player;

    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        IsGameOver = false;
    }

    private void Start()
    {
        gameOverCanvas.SetActive(false);
    }

    public void GameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;

        // Show current distance when the player dies
        if (player != null && finalDistanceText != null)
        {
            int distance = Mathf.RoundToInt(player.position.x);
            finalDistanceText.text = "Your Score: " + distance + " m";
        }

        gameOverCanvas.SetActive(true);

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}