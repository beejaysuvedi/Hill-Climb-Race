using System.Collections.Generic;
using UnityEngine;

public class FuelSpawner : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private CollectFuel fuelPrefab;
    [SerializeField] private EdgeCollider2D groundCollider;

    [Header("Spawn Settings")]
    [SerializeField] private float firstFuelDistance = 20f;
    [SerializeField] private float spacing = 30f;
    [SerializeField] private float spawnAhead = 40f;
    [SerializeField] private float heightAboveGround = 0.6f;
    [SerializeField] private float removeBehind = 30f;

    private readonly List<CollectFuel> spawned =
        new List<CollectFuel>();

    private float nextSpawnX;
    private float nextCheckTime;

    private void Start()
    {
        if (player == null || fuelPrefab == null ||
            groundCollider == null)
        {
            Debug.LogError(
                "FuelSpawner: Assign Player, Fuel Prefab and Ground Collider."
            );

            enabled = false;
            return;
        }

        nextSpawnX = player.position.x +
                     Mathf.Max(1f, firstFuelDistance);
    }

    private void Update()
    {
        if (player == null) return;

        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameOver) return;

        // Check five times per second.
        if (Time.time < nextCheckTime) return;
        nextCheckTime = Time.time + 0.2f;

        // Remove collected cans and cans far behind.
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] == null)
            {
                spawned.RemoveAt(i);
            }
            else if (spawned[i].transform.position.x <
                     player.position.x - removeBehind)
            {
                Destroy(spawned[i].gameObject);
                spawned.RemoveAt(i);
            }
        }

        if (nextSpawnX < player.position.x)
        {
            nextSpawnX = player.position.x +
                         Mathf.Max(1f, firstFuelDistance);
        }

        if (nextSpawnX > player.position.x + spawnAhead)
            return;

        if (!groundCollider.enabled ||
            !groundCollider.gameObject.activeInHierarchy)
            return;

        Bounds bounds = groundCollider.bounds;

        // Wait until the road exists at the spawn position.
        if (nextSpawnX <= bounds.min.x ||
            nextSpawnX >= bounds.max.x)
            return;

        Vector2 origin = new Vector2(
            nextSpawnX, bounds.max.y + 10f
        );

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            origin,
            Vector2.down,
            bounds.size.y + 20f,
            1 << groundCollider.gameObject.layer
        );

        foreach (RaycastHit2D hit in hits)
        {
            // Ignore everything except our ground.
            if (hit.collider != groundCollider) continue;

            Vector3 position = new Vector3(
                nextSpawnX,
                hit.point.y + heightAboveGround,
                groundCollider.transform.position.z
            );

            CollectFuel fuel = Instantiate(
                fuelPrefab, position, Quaternion.identity
            );

            fuel.Initialize(player);
            spawned.Add(fuel);

            nextSpawnX += Mathf.Max(1f, spacing);
            return;
        }
    }
}