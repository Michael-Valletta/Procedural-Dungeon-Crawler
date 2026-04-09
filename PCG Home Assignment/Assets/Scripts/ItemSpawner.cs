using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ItemSpawner : MonoBehaviour
{
    public GameObject playerPrefab;
    public List<GameObject> itemPrefabs;
    [Range(0, 1)] public float itemSpawnChance = 0.1f;

    public void SpawnEntities(HashSet<Vector2Int> floorPositions, Vector2Int startPos)
    {
        Instantiate(playerPrefab, new Vector3(startPos.x + 0.5f, startPos.y + 0.5f, 0), Quaternion.identity);

        foreach (var pos in floorPositions)
        {
            if (Vector2Int.Distance(pos, startPos) < 5f) continue;

            if (Random.value < itemSpawnChance)
            {
                GameObject itemToSpawn = itemPrefabs[Random.Range(0, itemPrefabs.Count)];
                GameObject spawnedItem = Instantiate(itemToSpawn, new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0), Quaternion.identity);
                ApplyDifficulty(spawnedItem, pos, startPos);
            }
        }
    }

    private void ApplyDifficulty(GameObject item, Vector2Int pos, Vector2Int startPos)
    {
        float distance = Vector2Int.Distance(pos, startPos);

        item.transform.localScale *= (1 + (distance * 0.01f));
    }
}