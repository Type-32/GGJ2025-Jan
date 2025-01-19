using System;
using UnityEngine;

namespace Bubble.Character.Generator
{
    using UnityEngine;

    public class CloudGenerator : MonoBehaviour
    {
        public GameObject[] cloudPrefabs; // Array of cloud prefabs
        public Transform player; // Reference to the player's transform
        public float spawnRadius = 10f; // Radius around the player where clouds can spawn
        public float minSpawnDistance = 5f; // Minimum distance from the player to spawn clouds
        public float spawnRate = 1f; // Base rate of cloud generation (clouds per second)
        public AnimationCurve spawnRateOverHeight; // Curve to control spawn rate over height
        public AnimationCurve cloudSizeOverHeight; // Curve to control cloud size over height
        public float maxCloudSize = 2f; // Maximum size of clouds

        private float nextSpawnTime;
        private float lastPlayerY; // Store the player's Y position from the last frame

        void Start()
        {
            nextSpawnTime = Time.time + 1f / spawnRate;
            lastPlayerY = player.position.y; // Initialize the player's Y position
        }

        void Update()
        {
            // Check if the player is moving upwards
            if (player.position.y > lastPlayerY)
            {
                // Check if it's time to spawn a new cloud
                if (Time.time >= nextSpawnTime)
                {
                    SpawnCloud();
                    nextSpawnTime = Time.time + 1f / GetSpawnRate();
                }
            }

            // Update the player's Y position for the next frame
            lastPlayerY = player.position.y;
        }

        void SpawnCloud()
        {
            // Get a random position outside the camera's view, biased upwards
            Vector2 spawnPosition = GetRandomSpawnPosition();

            // Choose a random cloud prefab
            GameObject cloudPrefab = cloudPrefabs[Random.Range(0, cloudPrefabs.Length)];

            // Instantiate the cloud
            GameObject cloud = Instantiate(cloudPrefab, spawnPosition, Quaternion.identity);

            // Scale the cloud based on height
            float height = spawnPosition.y;
            float sizeMultiplier = cloudSizeOverHeight.Evaluate(height);
            cloud.transform.localScale = Vector3.one * Mathf.Clamp(sizeMultiplier, 0.1f, maxCloudSize);
        }

        Vector2 GetRandomSpawnPosition()
        {
            // Get the camera's viewport bounds in world coordinates
            Vector2 viewportMin = Camera.main.ViewportToWorldPoint(new Vector2(0, 0));
            Vector2 viewportMax = Camera.main.ViewportToWorldPoint(new Vector2(1, 1));

            // Calculate a random position outside the camera's view, biased upwards
            float randomX = Random.Range(viewportMin.x - spawnRadius, viewportMax.x + spawnRadius);
            float randomY = player.position.y + Random.Range(minSpawnDistance, spawnRadius);

            // Ensure the cloud spawns outside the camera's view
            if (randomX > viewportMin.x && randomX < viewportMax.x)
            {
                randomX = Random.value > 0.5f ? viewportMax.x + spawnRadius : viewportMin.x - spawnRadius;
            }

            return new Vector2(randomX, randomY);
        }

        float GetSpawnRate()
        {
            // Adjust spawn rate based on player height
            float height = player.position.y;
            return spawnRate * spawnRateOverHeight.Evaluate(height);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
            Gizmos.DrawWireSphere(transform.position, minSpawnDistance);
        }
    }
}