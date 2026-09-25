using UnityEngine;
using TMPro;

public class DisplayDistanceText : MonoBehaviour
{
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private Transform player;

    private float startX;

    private void Start()
    {
        startX = player.position.x;
        distanceText.text = "0 m";
    }

    private void Update()
    {
        float distance = Mathf.Max(
            0f,
            player.position.x - startX
        );

        distanceText.text = distance.ToString("F0") + " m";
    }
}