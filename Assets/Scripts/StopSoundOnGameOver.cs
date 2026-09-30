using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class StopSoundOnGameOver : MonoBehaviour
{
    private AudioSource engineSound;

    private void Awake()
    {
        engineSound = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (GameManager.Instance == null)
            return;

        if (GameManager.Instance.IsGameOver &&
            engineSound.isPlaying)
        {
            engineSound.Stop();
        }
    }
}