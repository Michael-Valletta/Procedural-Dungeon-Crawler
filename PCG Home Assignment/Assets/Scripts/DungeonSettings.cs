using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class DungeonSettings
{
    public int seed = 12345;
    public int width = 80;
    public int height = 80;
    public int roomCount = 8;
    public int minRoomSize = 5;
    public int maxRoomSize = 10;

    [Header("Base Tiles")]
    public TileBase floorTile;
    public TileBase wallTile; 

    [Header("Variation Tiles")]
    public TileBase mossyFloorTile;
    public TileBase crackedFloorTile;
    public float noiseScale = 0.1f;

    [Header("Wall Directions")]
    public TileBase wallTop;
    public TileBase wallBottom;
    public TileBase wallLeft;
    public TileBase wallRight;
}