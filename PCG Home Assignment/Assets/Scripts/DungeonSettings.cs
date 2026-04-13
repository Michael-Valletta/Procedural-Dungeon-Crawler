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
    public TileBase waterTile;
    public TileBase lavaTile;
    public float noiseScale = 0.1f;
    [Range(0, 1)] public float lavaFrequency = 0.05f;

    [Header("Wall Directions")]
    public TileBase wallTop;
    public TileBase wallBottom;
    public TileBase wallLeft;
    public TileBase wallRight;

    [Header("Themed Walls (Water)")]
    public TileBase waterWallTop;
    public TileBase waterWallBottom;
    public TileBase waterWallLeft;
    public TileBase waterWallRight;
    public TileBase waterWallInnerCorner;
    public TileBase waterWallOuterCorner;

    [Header("Themed Walls (Moss)")]
    public TileBase mossWallTop;
    public TileBase mossWallBottom;
    public TileBase mossWallLeft;
    public TileBase mossWallRight;

    [Header("Decorations")]
    public TileBase torchTile;
    public TileBase chestTile;
    public TileBase goldTile;
    [Range(0, 1)] public float decorationChance = 0.05f;
}