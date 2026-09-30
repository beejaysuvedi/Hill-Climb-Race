using UnityEngine;

public class StartGameManager : MonoBehaviour
{
    public GameObject startCanvas;

    private bool gameStarted = false;

    void Start()
    {
        Time.timeScale = 0f;
        startCanvas.SetActive(true);
    }

    void Update()
    {
        if (!gameStarted && Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
        }
    }

    void StartGame()
    {
        gameStarted = true;

        startCanvas.SetActive(false);
        Time.timeScale = 1f;
    }
}