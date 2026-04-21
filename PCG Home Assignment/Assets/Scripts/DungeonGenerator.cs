using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    public DungeonSettings settings;
    public Tilemap floorMap;
    public Tilemap wallMap;
    public Tilemap decoMap;
    public GameObject playerPrefab;
    public GameObject[] enemyPrefabs;

    [Header("Exits & Items")]
    public GameObject portalPrefab;
    public GameObject healthPotionPrefab;
    public GameObject speedBallPrefab;

    private int[,] map;
    private List<Vector2Int> roomCenters = new List<Vector2Int>();
    private Vector3 playerSpawnLocation;
    private bool portalSpawned = false;

    [Header("Furniture Prefabs")]
    public GameObject cratePrefab;
    public GameObject TablePrefab;
    public GameObject BarrelPrefab;

    [ContextMenu("Generate Dungeon")]

    void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ResetMetrics();
        }
        Generate();
    }
    public void Generate()
    {
        ClearDungeon();
        portalSpawned = false;

        int level = (LevelManager.Instance != null) ? LevelManager.Instance.currentLevel : 1;
        int currentWidth = settings.width + (level * 2) + 10;
        int currentHeight = settings.height + (level * 2) + 10;

        map = new int[currentWidth, currentHeight];
        roomCenters.Clear();
        System.Random rng = new System.Random(settings.seed);

        for (int i = 0; i < settings.roomCount; i++)
        {
            bool isTreasureRoom = (i == settings.roomCount - 1);
            bool placed = false;
            int attempts = 0;
            int maxAttempts = isTreasureRoom ? 500 : 50;

            while (!placed && attempts < maxAttempts)
            {
                int rw = isTreasureRoom ? 6 : rng.Next(settings.minRoomSize, settings.maxRoomSize);
                int rh = isTreasureRoom ? 6 : rng.Next(settings.minRoomSize, settings.maxRoomSize);
                int rx = rng.Next(3, currentWidth - rw - 3);
                int ry = rng.Next(3, currentHeight - rh - 3);

                if (CanPlaceRoom(rx, ry, rw, rh, currentWidth, currentHeight))
                {
                    int theme = isTreasureRoom ? 7 : (i % 4 == 1 ? 2 : i % 4 == 2 ? 4 : i % 4 == 3 ? 5 : 1);
                    CarveRoom(rx, ry, rw, rh, theme);
                    Vector2Int center = new Vector2Int(rx + rw / 2, ry + rh / 2);
                    roomCenters.Add(center);
                    placed = true;
                }
                attempts++;
            }
        }

        for (int i = 0; i < roomCenters.Count - 1; i++)
            CreateWideCorridor(roomCenters[i], roomCenters[i + 1]);

        RenderDungeon(rng, currentWidth, currentHeight, level);
        SpawnAndAutoLink(level);
    }

    public void SpawnExitPortal()
    {
        if (portalSpawned) return;
        if (portalPrefab == null)
        {
            Debug.LogError("Portal Prefab is missing from DungeonGen");
            return;
        }

        GameObject player = GameObject.FindWithTag("Player");
        Vector3 spawnPos;

        if (player != null)
        {
            spawnPos = player.transform.position + new Vector3(2, 0, 0);
        }
        else
        {
            spawnPos = new Vector3(roomCenters[0].x + 2, roomCenters[0].y, -1f);
        }

        GameObject portal = Instantiate(portalPrefab, spawnPos, Quaternion.identity);
        portal.name = "DEBUG_NEARBY_PORTAL";
        portalSpawned = true;

        if (portal.TryGetComponent<SpriteRenderer>(out var sr))
        {
            sr.sortingOrder = 100;
        }

        Debug.Log("PORTAL SPAWNED NEAR PLAYER AT: " + spawnPos);
    }

    void RenderDungeon(System.Random rng, int w, int h, int level)
    {
        float noiseScale = 0.06f;
        float seedOffset = (float)rng.NextDouble() * 1000f;
        float hazardChance = 0.03f + (level * 0.005f);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                int type = map[x, y];
                if (type > 0)
                {
                    TileBase fTile = settings.floorTile;
                    float noiseValue = Mathf.PerlinNoise(x * noiseScale + seedOffset, y * noiseScale + seedOffset);

                    if (type == 2) fTile = settings.waterTile;
                    else if (type == 4) fTile = (noiseValue > 0.6f) ? settings.mossyFloorTile : settings.floorTile;
                    else if (type == 5) fTile = (noiseValue > 0.5f) ? settings.mossyFloorTile : settings.lavaTile;

                    if ((type == 4 || type == 5) && !decoMap.HasTile(pos))
                    {
                        if (rng.NextDouble() < 0.15f) 
                        {
                            decoMap.SetTile(pos, settings.mossyFloorTile);
                            decoMap.SetTransformMatrix(pos, Matrix4x4.Scale(new Vector3(0.7f, 0.7f, 1f)));
                        }
                    }

                    if (type != 7 && type != 2 && rng.NextDouble() < hazardChance)
                    {
                        if (rng.NextDouble() < 0.5f) decoMap.SetTile(pos, settings.spikeTrapTile);
                        else fTile = settings.poisonPoolTile;
                    }

                    floorMap.SetTile(pos, fTile);

                    if (type == 7 && rng.NextDouble() < 0.6f)
                    {
                        decoMap.SetTile(pos, (rng.Next(0, 2) == 0) ? settings.chestTile : settings.goldTile);
                    }

                    if (decoMap.HasTile(pos))
                    {
                        float s = (decoMap.GetTile(pos) == settings.torchTile) ? 0.4f : 0.6f;
                        if (decoMap.GetTile(pos) != settings.mossyFloorTile)
                        {
                            decoMap.SetTransformMatrix(pos, Matrix4x4.Scale(new Vector3(s, s, 1f)));
                        }
                    }
                    if (type != 0 && type != 2 && type != 7)
                    {
                        if (rng.NextDouble() < 0.01f)
                        {
                            GameObject potionToSpawn = (rng.Next(0, 2) == 0) ? healthPotionPrefab : speedBallPrefab;
                            Instantiate(potionToSpawn, new Vector3(x, y, -1f), Quaternion.identity, transform);
                        }
                    }
                }
                else if (map[x, y] == 0 && HasNeighbor(x, y, w, h, out int theme))
                {
                    PlaceWallWithThemedRotation(x, y, theme, w, h);
                }
            }
        }
    }

    void PlaceWallWithThemedRotation(int x, int y, int theme, int w, int h)
    {
        int wallTheme = (theme == 5) ? 4 : (theme == 7) ? 1 : theme;
        Vector3Int pos = new Vector3Int(x, y, 0);
        bool t = IsWalkable(x, y + 1, w, h);
        bool b = IsWalkable(x, y - 1, w, h);
        bool l = IsWalkable(x - 1, y, w, h);
        bool r = IsWalkable(x + 1, y, w, h);

        TileBase tileToPlace = settings.wallTile;
        float rotZ = 0f;
        TileBase corner = (wallTheme == 2) ? settings.waterCorner : (wallTheme == 4) ? settings.mossCorner : settings.standardCorner;

        if (t && l) { tileToPlace = corner; rotZ = 0; }
        else if (t && r) { tileToPlace = corner; rotZ = -90f; }
        else if (b && l) { tileToPlace = corner; rotZ = 90f; }
        else if (b && r) { tileToPlace = corner; rotZ = 180f; }
        else if (t) tileToPlace = (wallTheme == 2) ? settings.waterWallBottom : (wallTheme == 4) ? settings.mossWallBottom : settings.wallBottom;
        else if (b) tileToPlace = (wallTheme == 2) ? settings.waterWallTop : (wallTheme == 4) ? settings.mossWallTop : settings.wallTop;
        else if (l) tileToPlace = (wallTheme == 2) ? settings.waterWallRight : (wallTheme == 4) ? settings.mossWallRight : settings.wallRight;
        else if (r) tileToPlace = (wallTheme == 2) ? settings.waterWallLeft : (wallTheme == 4) ? settings.mossWallLeft : settings.wallLeft;

        wallMap.SetTile(pos, tileToPlace);
        wallMap.SetTransformMatrix(pos, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, rotZ), Vector3.one));
    }

    bool IsWalkable(int x, int y, int w, int h) => x >= 0 && x < w && y >= 0 && y < h && map[x, y] > 0;

    void CreateWideCorridor(Vector2Int s, Vector2Int e)
    {
        Vector2Int c = s;
        System.Random rng = new System.Random(settings.seed + s.x + s.y);

        while (c.x != e.x)
        {
            SetC(c.x, c.y);
            SetC(c.x, c.y + 1);
            if (rng.NextDouble() < 0.03f)
            {
                GameObject prefab = rng.Next(0, 2) == 0 ? cratePrefab : BarrelPrefab;
                Instantiate(prefab, new Vector3(c.x, c.y, -1f), Quaternion.identity);
            }
            c.x += (e.x > c.x) ? 1 : -1;
        }

        while (c.y != e.y)
        {
            SetC(c.x, c.y);
            SetC(c.x + 1, c.y);

            if (rng.NextDouble() < 0.03f)
            {
                GameObject prefab = rng.Next(0, 2) == 0 ? cratePrefab : BarrelPrefab;
                Instantiate(prefab, new Vector3(c.x, c.y, -1f), Quaternion.identity);
            }
            c.y += (e.y > c.y) ? 1 : -1;
        }
    }

    void SetC(int x, int y) { if (x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1)) if (map[x, y] == 0) map[x, y] = 1; }

    bool CanPlaceRoom(int x, int y, int w, int h, int mapW, int mapH)
    {
        for (int i = x - 2; i < x + w + 2; i++)
            for (int j = y - 2; j < y + h + 2; j++)
                if (i < 0 || i >= mapW || j < 0 || j >= mapH || map[i, j] != 0) return false;
        return true;
    }

    void CarveRoom(int x, int y, int w, int h, int t) { for (int i = x; i < x + w; i++) for (int j = y; j < y + h; j++) map[i, j] = t; }

    bool HasNeighbor(int x, int y, int w, int h, out int t)
    {
        t = 1;
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
                if (IsWalkable(x + i, y + j, w, h)) { t = map[x + i, y + j]; return true; }
        return false;
    }

    void SpawnAndAutoLink(int level)
    {
        if (roomCenters.Count == 0) return;
        playerSpawnLocation = new Vector3(roomCenters[0].x, roomCenters[0].y, -1f);
        GameObject p = Instantiate(playerPrefab, playerSpawnLocation, Quaternion.identity);
        Camera.main.GetComponent<CameraScript>().target = p.transform;

        PlayerScript s = p.GetComponent<PlayerScript>();
        if (s != null)
        {
            s.floorTilemap = floorMap;
            s.decoTilemap = decoMap;
            s.lavaTile = settings.lavaTile;
            s.waterTile = settings.waterTile;
            s.poisonPoolTile = settings.poisonPoolTile;
            s.spikeTrapTile = settings.spikeTrapTile;
        }

        for (int i = 1; i < roomCenters.Count; i++)
        {
            float dda = (LevelManager.Instance != null) ? LevelManager.Instance.difficultyScore : 1.0f;
            int theme = map[roomCenters[i].x, roomCenters[i].y];

            SpawnFurniture(roomCenters[i], theme, dda);

            if (theme == 7)
            {
                Instantiate(healthPotionPrefab, new Vector3(roomCenters[i].x + 1, roomCenters[i].y, -1), Quaternion.identity);
                Instantiate(healthPotionPrefab, new Vector3(roomCenters[i].x - 1, roomCenters[i].y, -1), Quaternion.identity);
                Instantiate(healthPotionPrefab, new Vector3(roomCenters[i].x - 2, roomCenters[i].y, -1), Quaternion.identity);
                Instantiate(healthPotionPrefab, new Vector3(roomCenters[i].x + 2, roomCenters[i].y, -1), Quaternion.identity);
                Instantiate(speedBallPrefab, new Vector3(roomCenters[i].x, roomCenters[i].y + 1, -1), Quaternion.identity);
                Instantiate(speedBallPrefab, new Vector3(roomCenters[i].x, roomCenters[i].y - 1, -1), Quaternion.identity);
                Instantiate(speedBallPrefab, new Vector3(roomCenters[i].x, roomCenters[i].y - 2, -1), Quaternion.identity);
                Instantiate(speedBallPrefab, new Vector3(roomCenters[i].x, roomCenters[i].y + 2, -1), Quaternion.identity);
                continue;
            }

            GameObject prefab = (theme == 4) ? enemyPrefabs[0] : (theme == 2) ? enemyPrefabs[1] : (theme == 5) ? enemyPrefabs[2] : enemyPrefabs[0];

            int baseCount = 2 + (level);
            int finalCount = Mathf.RoundToInt(baseCount * dda);
            for (int j = 0; j < finalCount; j++)
            {
                Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
                Instantiate(prefab, new Vector3(roomCenters[i].x, roomCenters[i].y, -1f) + offset, Quaternion.identity);
            }
        }
    }

    void ClearDungeon()
    {
        floorMap.ClearAllTiles();
        wallMap.ClearAllTiles();
        decoMap.ClearAllTiles();
        foreach (var e in GameObject.FindGameObjectsWithTag("Enemy")) DestroyImmediate(e);
        foreach (var i in GameObject.FindGameObjectsWithTag("Item")) DestroyImmediate(i);
        foreach (var p in GameObject.FindGameObjectsWithTag("Portal")) DestroyImmediate(p);
        if (GameObject.FindWithTag("Player")) DestroyImmediate(GameObject.FindWithTag("Player"));
    }

    void SpawnFurniture(Vector2Int center, int theme, float dda)
    {
        if (theme == 2) return;

        System.Random rng = new System.Random(settings.seed + center.x + center.y);

        if (theme == 7)
        {
            Instantiate(TablePrefab, new Vector3(center.x - 2, center.y - 1, -1), Quaternion.identity);
            Instantiate(TablePrefab, new Vector3(center.x + 2, center.y + 1, -1), Quaternion.identity);
            return;
        }

        int barrelCount = (dda > 1.1f) ? rng.Next(2, 4) : rng.Next(0, 2);
        int crateCount = (dda < 0.9f) ? rng.Next(3, 5) : rng.Next(1, 3);

        for (int i = 0; i < barrelCount; i++)
        {
            Vector3 pos = GetPosNearWall(center, rng);
            Instantiate(BarrelPrefab, pos, Quaternion.identity);
        }

        for (int i = 0; i < crateCount; i++)
        {
            Vector3 pos = GetPosNearWall(center, rng);
            Instantiate(cratePrefab, pos, Quaternion.identity);
        }
    }

    Vector3 GetPosNearWall(Vector2Int center, System.Random rng)
    {
        int offX = rng.Next(0, 2) == 0 ? rng.Next(-3, -1) : rng.Next(2, 4);
        int offY = rng.Next(0, 2) == 0 ? rng.Next(-3, -1) : rng.Next(2, 4);
        return new Vector3(center.x + offX, center.y + offY, -1f);
    }
}