using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class WallGenerator
{
    public static void CreateWalls(HashSet<Vector2Int> floorPositions, TileMapVisualizer tileMapVisualizer)
    {
        var basicWallPos = FindWallsInDirections(floorPositions, Direction2D.cardinalDirectionsList);
        var cornerWallPositions = FindWallsInDirections(floorPositions, Direction2D.diagonalDirectionsList);
        CreateBasicWalls(tileMapVisualizer, basicWallPos,floorPositions);
        CreateCornerWalls(tileMapVisualizer, cornerWallPositions, floorPositions);
    }

    private static void CreateCornerWalls(TileMapVisualizer tileMapVisualizer, HashSet<Vector2Int> cornerWallPositions, HashSet<Vector2Int> floorPositions)
    {
        foreach(var position in cornerWallPositions)
        {
            string neighboursBinaryType = "";
            foreach(var direction in Direction2D.eightDirectionsList)
            {
                var neighbourPosition = position + direction;
                if(floorPositions.Contains(neighbourPosition))
                {
                    neighboursBinaryType += "1";
                }
                else
                {
                    neighboursBinaryType += "0";
                }
            }
            tileMapVisualizer.PaintSingleCornerWall(position, neighboursBinaryType);
        }
    }

    private static void CreateBasicWalls(TileMapVisualizer tileMapVisualizer, HashSet<Vector2Int> basicWallPos, HashSet<Vector2Int> floorPositions)
    {
        foreach (var position in basicWallPos)
        {
            string neighboursBinaryType = "";
            foreach(var direction in Direction2D.cardinalDirectionsList)
            {
                var neighbourPosition = position + direction;
                if(floorPositions.Contains(neighbourPosition))
                {
                    neighboursBinaryType += "1";
                }
                else
                {
                    neighboursBinaryType += "0";
                }
            }
            tileMapVisualizer.PaintSingleBasicWall(position,neighboursBinaryType);
        }
    }

    private static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> floorPositions, List<Vector2Int> directionList)
    {
        HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();
        foreach (var position in floorPositions)
        {
            foreach( var direction in directionList)
            {
                var neighbourPosition = position + direction;
                if(floorPositions.Contains(neighbourPosition) == false)
                {
                    wallPositions.Add(neighbourPosition);
                }
            }
        }
        return wallPositions;
    }
}
