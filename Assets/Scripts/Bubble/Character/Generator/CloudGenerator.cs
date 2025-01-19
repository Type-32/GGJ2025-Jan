using System.Collections.Generic;
using UnityEngine;

namespace Bubble.Character.Generator
{
    public class CloudGenerator : MonoBehaviour
    {
        public GameObject[] cloudPrefabs; // Array of cloud prefabs
        public Transform player; // Reference to the player's transform
        public float bottomConeSize = 10f; // Cone size at the bottom
        public float topConeSize = 20f; // Cone size at the top
        public float minSpawnDistance = 5f; // Minimum distance from the player to spawn clouds
        public float minCloudSize = 1f; // Minimum size of clouds
        public float maxCloudSize = 2f; // Maximum size of clouds
        public float coneAngle = 45f; // Angle of the cone in degrees
        public int cloudNumber = 50; // Total number of clouds to generate
        public int maxHeight = 100; // Maximum height for cloud generation
        public float baseHeightOffset = 5f; // Height offset above the player's starting position
        public float minCloudDistance = 3f; // Minimum distance between clouds
        public float maxCloudDistance = 10f; // Maximum distance between clouds

        private Vector2 initialPlayerPosition; // Player's initial position
        private List<GameObject> cache = new(); // Cache for generated clouds
        private List<Vector2> cloudPositions = new(); // Track cloud positions to prevent overlap

        void Start()
        {
            initialPlayerPosition = new Vector2(player.position.x, player.position.y + baseHeightOffset); // Store the player's initial position with offset

            // Generate clouds all at once
            GenerateClouds(cloudNumber, maxHeight);
        }

        [NaughtyAttributes.Button]
        public void PreviewGenerate()
        {
            initialPlayerPosition = new Vector2(player.position.x, player.position.y + baseHeightOffset); // Store the player's initial position with offset

            GenerateClouds(cloudNumber, maxHeight);
        }

        [NaughtyAttributes.Button]
        public void Clear()
        {
            cache.ForEach(obj =>
            {
                DestroyImmediate(obj);
            });
            cache.Clear();
            cloudPositions.Clear();
        }

        public void GenerateClouds(int numClouds, float maxHeight)
        {
            // Ensure at least two clouds are in the player's initial view
            SpawnInitialViewClouds();

            // Generate the remaining clouds
            for (int i = 0; i < numClouds - 2; i++)
            {
                // Get a random position within the cone-shaped area and height range
                Vector2 spawnPosition = GetValidSpawnPosition(maxHeight);

                // If no valid position is found, skip this cloud
                if (spawnPosition == Vector2.zero)
                {
                    Debug.LogWarning($"Failed to find a valid position for cloud {i + 1}");
                    continue;
                }

                // Choose a random cloud prefab
                GameObject cloudPrefab = cloudPrefabs[Random.Range(0, cloudPrefabs.Length)];

                // Instantiate the cloud
                GameObject cloud = Instantiate(cloudPrefab, spawnPosition, Quaternion.identity);

                // Scale the cloud based on height
                float height = spawnPosition.y - initialPlayerPosition.y;
                float sizeMultiplier = Mathf.Clamp01(1 - (height / maxHeight)); // Smaller as height increases
                float cloudSize = Mathf.Lerp(minCloudSize, maxCloudSize, sizeMultiplier); // Scale size based on height
                cloud.transform.localScale = Vector3.one * cloudSize;

                // Add the cloud to the cache and track its position
                cache.Add(cloud);
                cloudPositions.Add(spawnPosition);
            }
        }

        void SpawnInitialViewClouds()
        {
            // Spawn two clouds within the player's initial view
            for (int i = 0; i < 2; i++)
            {
                // Get a random position within the player's initial view
                Vector2 spawnPosition = GetInitialViewSpawnPosition();

                // Choose a random cloud prefab
                GameObject cloudPrefab = cloudPrefabs[Random.Range(0, cloudPrefabs.Length)];

                // Instantiate the cloud
                GameObject cloud = Instantiate(cloudPrefab, spawnPosition, Quaternion.identity);

                // Scale the cloud (medium size for initial view)
                float cloudSize = Random.Range(minCloudSize, maxCloudSize);
                cloud.transform.localScale = Vector3.one * cloudSize;

                // Add the cloud to the cache and track its position
                cache.Add(cloud);
                cloudPositions.Add(spawnPosition);
            }
        }

        Vector2 GetInitialViewSpawnPosition()
        {
            // Get the camera's viewport bounds in world coordinates
            Vector2 viewportMin = Camera.main.ViewportToWorldPoint(new Vector2(0, 0));
            Vector2 viewportMax = Camera.main.ViewportToWorldPoint(new Vector2(1, 1));

            // Calculate a random position within the player's initial view
            float randomX = Random.Range(viewportMin.x, viewportMax.x);
            float randomY = Random.Range(viewportMin.y, viewportMax.y);

            // Ensure the cloud is above the player's starting position
            randomY = Mathf.Max(randomY, initialPlayerPosition.y);

            return new Vector2(randomX, randomY);
        }

        Vector2 GetValidSpawnPosition(float maxHeight)
        {
            int maxAttempts = 1000; // Increased maximum attempts to find a valid position
            for (int i = 0; i < maxAttempts; i++)
            {
                // Calculate a random angle within the cone
                float randomAngle = Random.Range(-coneAngle / 2f, coneAngle / 2f);

                // Convert the angle to a direction vector
                Vector2 spawnDirection = (Vector2)(Quaternion.Euler(0, 0, randomAngle) * Vector3.up);

                // Calculate the spawn height and interpolate the cone size
                float spawnHeight = Random.Range(0f, maxHeight);
                float coneSize = Mathf.Lerp(bottomConeSize, topConeSize, spawnHeight / maxHeight);

                // Calculate the spawn position within the cone and height range
                float spawnDistance = Random.Range(minSpawnDistance, coneSize);
                Vector2 spawnPosition = initialPlayerPosition + spawnDirection * spawnDistance + Vector2.up * spawnHeight;

                // Ensure the cloud is above the player's starting position
                if (spawnPosition.y < initialPlayerPosition.y)
                    continue;

                // Check if the spawn position is valid (no overlaps and not too close to the player)
                if (IsPositionValid(spawnPosition))
                {
                    return spawnPosition;
                }
            }

            // If no valid position is found after max attempts, return Vector2.zero
            Debug.LogWarning("Failed to find a valid spawn position after maximum attempts.");
            return Vector2.zero;
        }

        bool IsPositionValid(Vector2 position)
        {
            // Check if the position is too close to any existing cloud
            foreach (Vector2 cloudPos in cloudPositions)
            {
                float distance = Vector2.Distance(position, cloudPos);

                // Ensure the distance is within the min and max range
                if (distance < minCloudDistance)
                {
                    return false; // Position is too close to another cloud
                }
            }

            // Check if the position is too close to the player
            if (Vector2.Distance(position, initialPlayerPosition) < minSpawnDistance)
            {
                return false; // Position is too close to the player
            }

            return true; // Position is valid
        }

        void OnDrawGizmosSelected()
        {
            // Draw the cone-shaped spawn area in the editor
            if (player != null)
            {
                Gizmos.color = Color.cyan;

                // Draw the bottom cone boundaries
                Vector2 bottomConeLeft = (Vector2)(Quaternion.Euler(0, 0, -coneAngle / 2f) * Vector3.up) * bottomConeSize;
                Vector2 bottomConeRight = (Vector2)(Quaternion.Euler(0, 0, coneAngle / 2f) * Vector3.up) * bottomConeSize;

                Gizmos.DrawLine(initialPlayerPosition, initialPlayerPosition + bottomConeLeft);
                Gizmos.DrawLine(initialPlayerPosition, initialPlayerPosition + bottomConeRight);

                // Draw the top cone boundaries
                Vector2 topConeLeft = (Vector2)(Quaternion.Euler(0, 0, -coneAngle / 2f) * Vector3.up) * topConeSize;
                Vector2 topConeRight = (Vector2)(Quaternion.Euler(0, 0, coneAngle / 2f) * Vector3.up) * topConeSize;

                Vector2 topPosition = initialPlayerPosition + Vector2.up * maxHeight;
                Gizmos.DrawLine(topPosition, topPosition + topConeLeft);
                Gizmos.DrawLine(topPosition, topPosition + topConeRight);

                // Draw the sides of the cone
                Gizmos.DrawLine(initialPlayerPosition + bottomConeLeft, topPosition + topConeLeft);
                Gizmos.DrawLine(initialPlayerPosition + bottomConeRight, topPosition + topConeRight);
            }
        }
    }
}