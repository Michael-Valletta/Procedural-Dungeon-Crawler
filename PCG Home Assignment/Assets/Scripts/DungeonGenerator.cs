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
    public GameObject enemyPrefab;

    private int[,] map;
    private List<Vector2Int> roomCenters = new List<Vector2Int>();

    [ContextMenu("Generate Dungeon")]
    public void Generate()
    {
        ClearDungeon();
        map = new int[settings.width, settings.height];
        roomCenters.Clear();
        System.Random rng = new System.Random(settings.seed);

        for (int i = 0; i < settings.roomCount; i++)
        {
            int rw = rng.Next(settings.minRoomSize, settings.maxRoomSize);
            int rh = rng.Next(settings.minRoomSize, settings.maxRoomSize);
            int rx = rng.Next(3, settings.width - rw - 3);
            int ry = rng.Next(3, settings.height - rh - 3);

            if (CanPlaceRoom(rx, ry, rw, rh))
            {
                int theme = (i % 3 == 1) ? 2 : (i % 3 == 2) ? 4 : 1;
                CarveRoom(rx, ry, rw, rh, theme);
                roomCenters.Add(new Vector2Int(rx + rw / 2, ry + rh / 2));
            }
        }

        for (int i = 0; i < roomCenters.Count - 1; i++)
            CreateWideCorridor(roomCenters[i], roomCenters[i + 1]);

        RenderDungeon(rng);
        SpawnAndAutoLink();
    }

    void RenderDungeon(System.Random rng)
    {
        for (int x = 0; x < settings.width; x++)
        {
            for (int y = 0; y < settings.height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                int type = map[x, y];

                if (type > 0)
                {
                    TileBase fTile = (type == 2) ? settings.waterTile : (type == 4) ? settings.mossyFloorTile : settings.floorTile;
                    if (type == 1 && rng.NextDouble() < 0.05f) floorMap.SetTile(pos, settings.lavaTile);
                    else floorMap.SetTile(pos, fTile);

                    if (rng.NextDouble() < 0.03f && type != 2)
                    {
                        decoMap.SetTile(pos, (rng.Next(0, 2) == 0) ? settings.chestTile : settings.goldTile);
                        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(0.6f, 0.6f, 1f));
                        decoMap.SetTransformMatrix(pos, matrix);
                    }
                }
                else if (HasNeighbor(x, y, out int theme))
                {
                    wallMap.SetTile(pos, GetThemedWall(x, y, theme));

                    if (IsWalkable(x, y - 1) && rng.NextDouble() < 0.15f)
                    {
                        decoMap.SetTile(pos, settings.torchTile);
                        Matrix4x4 torchMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(0.6f, 0.6f, 1f));
                        decoMap.SetTransformMatrix(pos, torchMatrix);
                    }
                }
            }
        }
    }

    TileBase GetThemedWall(int x, int y, int theme)
    {
        bool up = IsWalkable(x, y + 1);
        bool down = IsWalkable(x, y - 1);
        bool left = IsWalkable(x - 1, y);
        bool right = IsWalkable(x + 1, y);
        bool upLeft = IsWalkable(x - 1, y + 1);
        bool upRight = IsWalkable(x + 1, y + 1);

        if (theme == 2)
        {
            if (up) return settings.waterWallBottom;
            if (down) return settings.waterWallTop;
            if (left) return settings.waterWallRight;
            if (right) return settings.waterWallLeft;
            return settings.waterWallTop;
        }
        if (theme == 4)
        {
            if (up) return settings.mossWallBottom;
            if (down) return settings.mossWallTop;
            if (left) return settings.mossWallRight;
            if (right) return settings.mossWallLeft;
            return settings.mossWallTop;
        }

        if (up) return settings.wallBottom;
        if (down) return settings.wallTop;
        if (left) return settings.wallRight;
        if (right) return settings.wallLeft;
        if (upLeft || upRight) return settings.wallTile;

        return settings.wallTile;
    }

    bool IsWalkable(int x, int y) => x >= 0 && x < settings.width && y >= 0 && y < settings.height && map[x, y] > 0;

    void CreateWideCorridor(Vector2Int s, Vector2Int e)
    {
        Vector2Int c = s;
        while (c.x != e.x) { SetC(c.x, c.y); SetC(c.x, c.y + 1); c.x += (e.x > c.x) ? 1 : -1; }
        while (c.y != e.y) { SetC(c.x, c.y); SetC(c.x + 1, c.y); c.y += (e.y > c.y) ? 1 : -1; }
    }

    void SetC(int x, int y) { if (map[x, y] == 0) map[x, y] = 1; }

    bool CanPlaceRoom(int x, int y, int w, int h)
    {
        for (int i = x - 2; i < x + w + 2; i++)
            for (int j = y - 2; j < y + h + 2; j++)
                if (map[i, j] != 0) return false;
        return true;
    }

    void CarveRoom(int x, int y, int w, int h, int t)
    {
        for (int i = x; i < x + w; i++)
            for (int j = y; j < y + h; j++) map[i, j] = t;
    }

    bool HasNeighbor(int x, int y, out int t)
    {
        t = 1;
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
                if (IsWalkable(x + i, y + j)) { t = map[x + i, y + j]; return true; }
        return false;
    }

    void SpawnAndAutoLink()
    {
        Vector3 pos = new Vector3(roomCenters[0].x, roomCenters[0].y, -1f);
        GameObject p = Instantiate(playerPrefab, pos, Quaternion.identity);
        Camera.main.GetComponent<CameraScript>().target = p.transform;
        PlayerScript s = p.GetComponent<PlayerScript>();
        if (s != null)
        {
            s.floorTilemap = floorMap;
            s.lavaTile = settings.lavaTile;
            s.waterTile = settings.waterTile;
        }
        for (int i = 1; i < roomCenters.Count; i++)
            Instantiate(enemyPrefab, new Vector3(roomCenters[i].x, roomCenters[i].y, -1f), Quaternion.identity);
    }

    void ClearDungeon()
    {
        floorMap.ClearAllTiles(); wallMap.ClearAllTiles(); decoMap.ClearAllTiles();
        foreach (var e in GameObject.FindGameObjectsWithTag("Enemy")) DestroyImmediate(e);
        if (GameObject.FindWithTag("Player")) DestroyImmediate(GameObject.FindWithTag("Player"));
    }
}