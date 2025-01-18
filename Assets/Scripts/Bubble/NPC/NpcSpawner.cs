using UnityEngine;

namespace Bubble.NPC
{
    using UnityEngine;

    public class NpcSpawner : MonoBehaviour
    {
        public GameObject npcPrefab; // Reference to the NPC prefab
        public float spawnInterval = 2f; // Time between spawns

        void Start()
        {
            // Start spawning NPCs at regular intervals
            InvokeRepeating(nameof(SpawnNpc), 0f, spawnInterval);
        }

        private void SpawnNpc()
        {
            // Instantiate the NPC prefab
            Instantiate(npcPrefab, Vector3.zero, Quaternion.identity);
        }
    }
}