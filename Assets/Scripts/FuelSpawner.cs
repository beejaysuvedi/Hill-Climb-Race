using System.Collections.Generic;
using UnityEngine;

public class FuelSpawner : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private CollectFuel fuelPrefab;
    [SerializeField] private EdgeCollider2D groundCollider;

    [Header("Spawn Settings")]
    [SerializeField] private float firstFuelDistance = 50f;
    [SerializeField] private float spacing = 80f;
    [SerializeField] private float spawnAhead = 80f;
    [SerializeField] private float heightAboveGround = 2.0f;
    [SerializeField] private float removeBehind = 30f;

    private readonly List<CollectFuel> spawned =
        new List<CollectFuel>();

    private float nextSpawnX;
    private float nextCheckTime;

    private void Start()
    {
        if (player == null || fuelPrefab == null || groundCollider == null)
        {
            Debug.LogError(
                "FuelSpawner: Assign Player, Fuel Prefab and Ground Collider."
            );

            enabled = false;
            return;
        }

        // Remove any fuel canisters that were already placed
        // manually in the scene.
        CollectFuel[] oldFuels =
            FindObjectsByType<CollectFuel>(
                FindObjectsSortMode.None
            );

        foreach (CollectFuel fuel in oldFuels)
        {
            Destroy(fuel.gameObject);
        }

        // Set the position of the first generated fuel.
        nextSpawnX =
            player.position.x +
            Mathf.Max(1f, firstFuelDistance);
    }

    private void Update()
    {
        if (player == null)
            return;

        // Stop spawning after game over.
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameOver)
        {
            return;
        }

        // Check five times every second.
        if (Time.time < nextCheckTime)
            return;

        nextCheckTime = Time.time + 0.2f;

        // Remove collected fuel cans or cans far behind.
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] == null)
            {
                spawned.RemoveAt(i);
            }
            else if (
                spawned[i].transform.position.x <
                player.position.x - removeBehind
            )
            {
                Destroy(spawned[i].gameObject);
                spawned.RemoveAt(i);
            }
        }

        // If the next planned fuel is already behind the player,
        // move the next spawn position forward.
        if (nextSpawnX < player.position.x)
        {
            nextSpawnX =
                player.position.x +
                Mathf.Max(1f, firstFuelDistance);
        }

        // Do not spawn until the planned position is close enough.
        if (nextSpawnX >
            player.position.x + spawnAhead)
        {
            return;
        }

        if (!groundCollider.enabled ||
            !groundCollider.gameObject.activeInHierarchy)
        {
            return;
        }

        Bounds bounds = groundCollider.bounds;

        // Wait until road exists at the spawn position.
        if (nextSpawnX <= bounds.min.x ||
            nextSpawnX >= bounds.max.x)
        {
            return;
        }

        // Start the raycast above the road.
        Vector2 origin = new Vector2(
            nextSpawnX,
            bounds.max.y + 10f
        );

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            origin,
            Vector2.down,
            bounds.size.y + 20f,
            1 << groundCollider.gameObject.layer
        );

        foreach (RaycastHit2D hit in hits)
        {
            // Ignore anything that is not our road.
            if (hit.collider != groundCollider)
                continue;

            // Position fuel above the road.
            Vector3 position = new Vector3(
                nextSpawnX,
                hit.point.y + heightAboveGround,
                groundCollider.transform.position.z
            );

            // Spawn fuel.
            CollectFuel fuel = Instantiate(
                fuelPrefab,
                position,
                Quaternion.identity
            );

            fuel.Initialize(player);

            spawned.Add(fuel);

            // Set the next fuel position.
            nextSpawnX +=
                Mathf.Max(1f, spacing);

            return;
        }
    }
}