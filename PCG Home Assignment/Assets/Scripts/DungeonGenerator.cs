using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    public DungeonSettings settings;
    public Tilemap floorMap;
    public Tilemap wallMap;
    public GameObject playerPrefab;
    public GameObject enemyPrefab;

    private int[,] map;
    private Vector2Int playerSpawnPoint;
    private List<GameObject> activeEnemies = new List<GameObject>();

    [ContextMenu("Generate Dungeon")]
    public void Generate()
    {
        GameObject oldPlayer = GameObject.FindWithTag("Player");
        if (oldPlayer != null) DestroyImmediate(oldPlayer);

        foreach (GameObject e in activeEnemies)
        {
            if (e != null) DestroyImmediate(e);
        }
        activeEnemies.Clear();

        GameObject[] strayEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject e in strayEnemies) DestroyImmediate(e);

        floorMap.ClearAllTiles();
        wallMap.ClearAllTiles();
        map = new int[settings.width, settings.height];

        System.Random rng = new System.Random(settings.seed);
        Vector2Int previousRoomCenter = Vector2Int.zero;

        for (int i = 0; i < settings.roomCount; i++)
        {
            int rw = rng.Next(settings.minRoomSize, settings.maxRoomSize);
            int rh = rng.Next(settings.minRoomSize, settings.maxRoomSize);

            int rx = rng.Next(2, settings.width - rw - 2);
            int ry = rng.Next(2, settings.height - rh - 2);

            CarveRoom(rx, ry, rw, rh);

            Vector2Int currentRoomCenter = new Vector2Int(rx + rw / 2, ry + rh / 2);

            if (i == 0)
            {
                playerSpawnPoint = currentRoomCenter;
            }
            else
            {
                CreateCorridor(previousRoomCenter, currentRoomCenter);
            }

            previousRoomCenter = currentRoomCenter;
        }
        RenderMap();

        if (playerPrefab != null)
        {
            Vector3 spawnPos = new Vector3(playerSpawnPoint.x + 0.5f, playerSpawnPoint.y + 0.5f, -1f);
            GameObject playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

            if (Camera.main.GetComponent<CameraScript>() != null)
                Camera.main.GetComponent<CameraScript>().target = playerInstance.transform;
        }

        if (enemyPrefab != null)
        {
            SpawnEnemies(rng);
        }
    }

    void RenderMap()
    {
        for (int x = 0; x < settings.width; x++)
        {
            for (int y = 0; y < settings.height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                if (map[x, y] == 1)
                {
                    float noiseValue = Mathf.PerlinNoise(x * settings.noiseScale, y * settings.noiseScale);
                    if (noiseValue > 0.7f) floorMap.SetTile(pos, settings.mossyFloorTile);
                    else if (noiseValue < 0.2f) floorMap.SetTile(pos, settings.crackedFloorTile);
                    else floorMap.SetTile(pos, settings.floorTile);
                }
                else if (HasFloorNeighbor(x, y))
                {
                    AssignDirectionalWall(x, y, pos);
                }
            }
        }
    }

    void AssignDirectionalWall(int x, int y, Vector3Int pos)
    {
        bool floorBelow = (y > 0 && map[x, y - 1] == 1);
        bool floorAbove = (y < settings.height - 1 && map[x, y + 1] == 1);
        bool floorLeft = (x > 0 && map[x - 1, y] == 1);
        bool floorRight = (x < settings.width - 1 && map[x + 1, y] == 1);

        if (floorBelow) wallMap.SetTile(pos, settings.wallTop);
        else if (floorAbove) wallMap.SetTile(pos, settings.wallBottom);
        else if (floorRight) wallMap.SetTile(pos, settings.wallLeft);
        else if (floorLeft) wallMap.SetTile(pos, settings.wallRight);
        else wallMap.SetTile(pos, settings.wallTile);
    }

    void CreateCorridor(Vector2Int start, Vector2Int end)
    {
        Vector2Int current = start;
        while (current.x != end.x)
        {
            int dir = (end.x > current.x) ? 1 : -1;
            SetFloorTile(current.x, current.y);
            SetFloorTile(current.x, current.y + 1); 
            current.x += dir;
        }
        while (current.y != end.y)
        {
            int dir = (end.y > current.y) ? 1 : -1;
            SetFloorTile(current.x, current.y);
            SetFloorTile(current.x + 1, current.y); 
            current.y += dir;
        }
    }

    void SetFloorTile(int x, int y)
    {
        if (x >= 0 && x < settings.width && y >= 0 && y < settings.height)
            map[x, y] = 1;
    }

    void CarveRoom(int x, int y, int w, int h)
    {
        for (int i = x; i < x + w; i++)
            for (int j = y; j < y + h; j++)
                map[i, j] = 1;
    }

    bool HasFloorNeighbor(int x, int y)
    {
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
            {
                int cx = x + i; int cy = y + j;
                if (cx >= 0 && cx < settings.width && cy >= 0 && cy < settings.height)
                    if (map[cx, cy] == 1) return true;
            }
        return false;
    }

    void SpawnEnemies(System.Random rng)
    {
        int enemiesToSpawn = settings.roomCount * 2;
        int spawnedCount = 0;
        int attempts = 0;

        while (spawnedCount < enemiesToSpawn && attempts < 500)
        {
            attempts++;
            int x = rng.Next(1, settings.width - 1);
            int y = rng.Next(1, settings.height - 1);

            float distToSpawn = Vector2.Distance(new Vector2(x, y), playerSpawnPoint);

            if (map[x, y] == 1 && IsInRoom(x, y) && distToSpawn > 6f)
            {
                Vector3 spawnPos = new Vector3(x + 0.5f, y + 0.5f, -1f);
                if (Physics2D.OverlapCircle(spawnPos, 0.4f) == null)
                {
                    GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                    activeEnemies.Add(enemy);
                    spawnedCount++;
                }
            }
        }
    }

    bool IsInRoom(int x, int y)
    {
        int floorNeighbors = 0;
        if (x > 0 && map[x - 1, y] == 1) floorNeighbors++;
        if (x < settings.width - 1 && map[x + 1, y] == 1) floorNeighbors++;
        if (y > 0 && map[x, y - 1] == 1) floorNeighbors++;
        if (y < settings.height - 1 && map[x, y + 1] == 1) floorNeighbors++;
        return floorNeighbors == 4;
    }
}