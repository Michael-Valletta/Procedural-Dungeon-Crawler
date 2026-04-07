using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class TileMapVisualizer : MonoBehaviour
{
    [SerializeField] private Tilemap floorTileMap, wallTileMap;

    [SerializeField] private TileBase floorTile, wallTop, wallSideRight, WallSideLeft, wallBottom, wallFull;

    public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions) 
    {
        PaintTiles(floorPositions,floorTileMap,floorTile);
    }

    private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tile)
    {
        foreach (var position in positions)
        {
            PaintSingleTile(tilemap, tile, position);
        }
    }

    private void PaintSingleTile(Tilemap tilemap, TileBase tile, Vector2Int position)
    {
        var tilePosition = tilemap.WorldToCell((Vector3Int)position);
        tilemap.SetTile(tilePosition, tile);
    }

    public void Clear()
    {
        floorTileMap.ClearAllTiles();
        wallTileMap.ClearAllTiles();
    }

    internal void PaintSingleBasicWall(Vector2Int position,string binaryType)
    {
        int typeAsInt = Convert.ToInt32(binaryType, 2);
        TileBase tile = null;
        if (WallTypes.wallTop.Contains(typeAsInt))
        {
            tile = wallTop;
        }
        else if (WallTypes.wallSideRight.Contains(typeAsInt))
        {
            tile = wallSideRight;
        }
        else if (WallTypes.wallSideLeft.Contains(typeAsInt))
        {
            tile = WallSideLeft;
        }
        else if (WallTypes.wallBottom.Contains(typeAsInt))
        {
            tile = wallBottom;
        }
        else if (WallTypes.wallFull.Contains(typeAsInt))
        {
            tile = wallFull;
        }
        if (tile != null)
        {
            PaintSingleTile(wallTileMap, tile, position);
        }

    }

    internal void PaintSingleCornerWall(Vector2Int position, string neighboursBinaryType)
    {
        throw new NotImplementedException();
    }
}
