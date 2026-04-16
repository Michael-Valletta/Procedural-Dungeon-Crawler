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

    [Header("Item Spawns")]
    public GameObject healthPotionPrefab;
    public GameObject speedBallPrefab;

    private int[,] map;
    private List<Vector2Int> roomCenters = new List<Vector2Int>();

    [ContextMenu("Generate Dungeon")]
    public void Generate()
    {
        ClearDungeon();
        int level = (LevelManager.Instance != null) ? LevelManager.Instance.currentLevel : 1;
        int currentWidth = settings.width + (level * 2);
        int currentHeight = settings.height + (level * 2);

        map = new int[currentWidth, currentHeight];
        roomCenters.Clear();
        System.Random rng = new System.Random(settings.seed);

        for (int i = 0; i < settings.roomCount; i++)
        {
            bool isTreasureRoom = (i == settings.roomCount - 1);
            int rw = isTreasureRoom ? rng.Next(4, 6) : rng.Next(settings.minRoomSize, settings.maxRoomSize);
            int rh = isTreasureRoom ? rng.Next(4, 6) : rng.Next(settings.minRoomSize, settings.maxRoomSize);
            int rx = rng.Next(3, currentWidth - rw - 3);
            int ry = rng.Next(3, currentHeight - rh - 3);

            if (CanPlaceRoom(rx, ry, rw, rh, currentWidth, currentHeight))
            {
                int theme = isTreasureRoom ? 7 : (i % 4 == 1 ? 2 : i % 4 == 2 ? 4 : i % 4 == 3 ? 5 : 1);
                CarveRoom(rx, ry, rw, rh, theme);
                roomCenters.Add(new Vector2Int(rx + rw / 2, ry + rh / 2));
            }
        }

        for (int i = 0; i < roomCenters.Count - 1; i++)
            CreateWideCorridor(roomCenters[i], roomCenters[i + 1]);

        RenderDungeon(rng, currentWidth, currentHeight, level);
        SpawnAndAutoLink(level);
    }

    void RenderDungeon(System.Random rng, int w, int h, int level)
    {
        float roomHazardChance = 0.02f + (level * 0.005f);
        float corridorHazardChance = 0.005f;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                int type = map[x, y];
                if (type > 0)
                {
                    TileBase fTile = settings.floorTile;
                    float currentChance = (type == 1) ? corridorHazardChance : roomHazardChance;

                    if (type == 2) fTile = settings.waterTile;
                    else if (type == 4) fTile = settings.mossyFloorTile;
                    else if (type == 5) fTile = (rng.NextDouble() < 0.6f) ? settings.mossyFloorTile : settings.lavaTile;

                    if (type != 7 && type != 2 && rng.NextDouble() < currentChance)
                    {
                        if (rng.NextDouble() < 0.5f) decoMap.SetTile(pos, settings.spikeTrapTile);
                        else fTile = settings.poisonPoolTile;
                    }

                    floorMap.SetTile(pos, fTile);

                    if (type == 7 && rng.NextDouble() < 0.6f)
                        decoMap.SetTile(pos, (rng.Next(0, 2) == 0) ? settings.chestTile : settings.goldTile);

                    if (type != 7 && type != 2 && fTile == settings.floorTile && rng.NextDouble() < 0.005f)
                    {
                        Instantiate(rng.Next(0, 2) == 0 ? healthPotionPrefab : speedBallPrefab, new Vector3(x, y, -1), Quaternion.identity);
                    }

                    if (decoMap.HasTile(pos))
                    {
                        float scale = (decoMap.GetTile(pos) == settings.torchTile) ? 0.4f : 0.6f;
                        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
                        decoMap.SetTransformMatrix(pos, matrix);
                    }
                }
                else if (HasNeighbor(x, y, w, h, out int theme))
                {
                    wallMap.SetTile(pos, GetThemedWall(x, y, theme, w, h));

                    if (IsWalkable(x, y - 1, w, h) && rng.NextDouble() < 0.12f)
                    {
                        decoMap.SetTile(pos, settings.torchTile);
                        Matrix4x4 tMat = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(0.4f, 0.4f, 1f));
                        decoMap.SetTransformMatrix(pos, tMat);
                    }
                }
            }
        }
    }

    TileBase GetThemedWall(int x, int y, int theme, int w, int h)
    {
        int wallTheme = (theme == 5) ? 4 : (theme == 7) ? 1 : theme;
        if (IsWalkable(x, y + 1, w, h)) return (wallTheme == 2) ? settings.waterWallBottom : (wallTheme == 4) ? settings.mossWallBottom : settings.wallBottom;
        if (IsWalkable(x, y - 1, w, h)) return (wallTheme == 2) ? settings.waterWallTop : (wallTheme == 4) ? settings.mossWallTop : settings.wallTop;
        if (IsWalkable(x - 1, y, w, h)) return (wallTheme == 2) ? settings.waterWallRight : (wallTheme == 4) ? settings.mossWallRight : settings.wallRight;
        if (IsWalkable(x + 1, y, w, h)) return (wallTheme == 2) ? settings.waterWallLeft : (wallTheme == 4) ? settings.mossWallLeft : settings.wallLeft;
        return settings.wallTile;
    }

    bool IsWalkable(int x, int y, int w, int h) => x >= 0 && x < w && y >= 0 && y < h && map[x, y] > 0;

    void CreateWideCorridor(Vector2Int s, Vector2Int e)
    {
        Vector2Int c = s;
        while (c.x != e.x) { SetC(c.x, c.y); SetC(c.x, c.y + 1); c.x += (e.x > c.x) ? 1 : -1; }
        while (c.y != e.y) { SetC(c.x, c.y); SetC(c.x + 1, c.y); c.y += (e.y > c.y) ? 1 : -1; }
    }

    void SetC(int x, int y) {
        if (map[x, y] == 0) map[x, y] = 1;
    }

    bool CanPlaceRoom(int x, int y, int w, int h, int mapW, int mapH)
    {
        for (int i = x - 2; i < x + w + 2; i++)
            for (int j = y - 2; j < y + h + 2; j++)
                if (i < 0 || i >= mapW || j < 0 || j >= mapH || map[i, j] != 0) return false;
        return true;
    }

    void CarveRoom(int x, int y, int w, int h, int t) { 
        for (int i = x; i < x + w; i++) for (int j = y; j < y + h; j++) map[i, j] = t; 
    }

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
        Vector3 playerPos = new Vector3(roomCenters[0].x, roomCenters[0].y, -1f);
        GameObject p = Instantiate(playerPrefab, playerPos, Quaternion.identity);
        Camera.main.GetComponent<CameraScript>().target = p.transform;
        PlayerScript s = p.GetComponent<PlayerScript>();
        if (s != null) { s.floorTilemap = floorMap; s.decoTilemap = decoMap; s.lavaTile = settings.lavaTile; s.waterTile = settings.waterTile; s.poisonPoolTile = settings.poisonPoolTile; s.spikeTrapTile = settings.spikeTrapTile; }

        for (int i = 1; i < roomCenters.Count; i++)
        {
            int theme = map[roomCenters[i].x, roomCenters[i].y];
            if (theme == 7)
            {
                for (int j = 0; j < 2; j++)
                {
                    Instantiate(healthPotionPrefab, new Vector3(roomCenters[i].x + j, roomCenters[i].y, -1), Quaternion.identity);
                    Instantiate(speedBallPrefab, new Vector3(roomCenters[i].x - j, roomCenters[i].y - 1, -1), Quaternion.identity);
                }
                continue;
            }

            GameObject prefab = (theme == 4) ? enemyPrefabs[0] : (theme == 2) ? enemyPrefabs[1] : (theme == 5) ? enemyPrefabs[2] : enemyPrefabs[0];
            int count = 1 + (level / 2);
            for (int j = 0; j < count; j++)
            {
                Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
                Instantiate(prefab, new Vector3(roomCenters[i].x, roomCenters[i].y, -1f) + offset, Quaternion.identity);
            }
        }
    }

    void ClearDungeon()
    {
        floorMap.ClearAllTiles(); wallMap.ClearAllTiles(); decoMap.ClearAllTiles();
        foreach (var e in GameObject.FindGameObjectsWithTag("Enemy")) DestroyImmediate(e);
        foreach (var i in GameObject.FindGameObjectsWithTag("Item")) DestroyImmediate(i);
        if (GameObject.FindWithTag("Player")) DestroyImmediate(GameObject.FindWithTag("Player"));
    }
}