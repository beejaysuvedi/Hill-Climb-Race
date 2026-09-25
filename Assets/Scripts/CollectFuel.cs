using UnityEngine;

public class CollectFuel : MonoBehaviour
{
    [SerializeField] private FuelController fuelController;

    private Transform player;
    private bool collected;

    public void Initialize(Transform vehicle)
    {
        player = vehicle;
        collected = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameOver) return;

        bool isPlayer =
            player != null && other.transform.IsChildOf(player);

        // Support manually placed cans using the Player tag.
        if (player == null)
        {
            for (Transform t = other.transform;
                 t != null; t = t.parent)
            {
                if (t.CompareTag("Player"))
                {
                    isPlayer = true;
                    break;
                }
            }
        }

        if (!isPlayer) return;

        FuelController controller = fuelController != null
            ? fuelController
            : FuelController.Instance;

        if (controller == null) return;

        collected = true;
        controller.FillFuel();
        Destroy(gameObject);
    }
}