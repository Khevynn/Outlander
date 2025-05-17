using System.Collections.Generic;
using UnityEngine;

public class RockSpawner : MonoBehaviour
{
    [Header("Rock Spawn")]
    [SerializeField] private Terrain terrain;
    [SerializeField] private List<GameObject> rockPrefabsList;
    [SerializeField] private int numberOfRocks = 100;
    [SerializeField] private float spawnRange = 100f;

    private void Start()
    {
        SpawnRocks();
    }

    private void SpawnRocks()
    {
        for (int i = 0; i < numberOfRocks; ++i)
        {
            var spawnPosition = GetRandomPositionNearCenter(spawnRange);

            // Get terrain normal using a raycast
            if (Physics.Raycast(spawnPosition + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
            {
                Vector3 normal = hit.normal;
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);

                var selectedPrefab = Random.Range(0, rockPrefabsList.Count); // Fix: should not subtract 1 here

                // Offset into the ground (0.5 units)
                Vector3 embeddedPosition = hit.point - normal * 0.5f;

                GameObject rock = Instantiate(rockPrefabsList[selectedPrefab], embeddedPosition, rotation, transform);
                rock.transform.Rotate(Vector3.up, Random.Range(0, 360f)); // Random Y rotation for variation
            }
        }
    }


    private Vector3 GetRandomPositionNearCenter(float range)
    {
        Vector3 terrainPos = terrain.transform.position;
        Vector3 terrainSize = terrain.terrainData.size;
    
        // Get terrain center in world space
        Vector3 center = terrainPos + new Vector3(terrainSize.x / 2f, 0, terrainSize.z / 2f);
    
        // Random offset within range
        float offsetX = Random.Range(-range, range);
        float offsetZ = Random.Range(-range, range);

        float x = center.x + offsetX;
        float z = center.z + offsetZ;
    
        float y = terrain.SampleHeight(new Vector3(x, 0, z)) + terrainPos.y;

        return new Vector3(x, y, z);
    }

}

