using UnityEngine;
using UnityEngine.UI;

public class MusicToggle : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite musicOnSprite;
    [SerializeField] private Sprite musicOffSprite;

    private bool musicOn = true;

    public void ToggleMusic()
    {
        musicOn = !musicOn;

        if (musicOn)
        {
            musicSource.UnPause();
            iconImage.sprite = musicOnSprite;
        }
        else
        {
            musicSource.Pause();
            iconImage.sprite = musicOffSprite;
        }
    }
}