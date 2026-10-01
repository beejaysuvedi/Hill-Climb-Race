using UnityEngine;

public class StartGameManager : MonoBehaviour
{
    public GameObject startCanvas;

    private bool gameStarted = false;

    private void Start()
    {
        Time.timeScale = 0f;
        startCanvas.SetActive(true);
    }

    public void StartGame()
    {
        if (gameStarted)
            return;

        gameStarted = true;

        startCanvas.SetActive(false);
        Time.timeScale = 1f;
    }
}