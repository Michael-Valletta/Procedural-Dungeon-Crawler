using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SimpleRandomWalkDungeonGenerator : AbstractDungeonGen
{

    [SerializeField] private SimpleRandomWalkSO randomWalkParameters;


    protected override void RunProceduralGeneration()
    {
        HashSet<Vector2Int> floorPositions = RunRandomWalk();
        tileMapVisualizer.Clear();
        tileMapVisualizer.PaintFloorTiles(floorPositions);
        WallGenerator.CreateWalls(floorPositions,tileMapVisualizer);
    }

    private HashSet<Vector2Int> RunRandomWalk()
    {
        var currentPosition = startPosition;
        HashSet<Vector2Int> floorPos = new HashSet<Vector2Int>();

        for (int i = 0; i < randomWalkParameters.iterations; i++)
        {
            var path = ProceduralGenerationAlgorithms.SimpleRandomWalk(currentPosition, randomWalkParameters.walkLength);
            floorPos.UnionWith(path);
            if (randomWalkParameters.startRandomlyEachIteration)
            {
                currentPosition = floorPos.ElementAt(Random.Range(0, floorPos.Count));
            }
        }
        return floorPos;
    }

}
