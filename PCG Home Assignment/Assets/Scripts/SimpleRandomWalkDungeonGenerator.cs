using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SimpleRandomWalkDungeonGenerator : MonoBehaviour
{
    [SerializeField] protected Vector2Int startPos = Vector2Int.zero;

    [SerializeField] private int iterations = 10;
    [SerializeField] public int walkLength = 10;
    [SerializeField] public bool startRandIteration = true;


    public void RunProceduralGeneration()
    {
        HashSet<Vector2Int> floorPositions = RunRandomWalk();
        foreach (var position in floorPositions)
        {
            Debug.Log(position);
        }
    }

    private HashSet<Vector2Int> RunRandomWalk()
    {
        var currentPosition = startPos;
        HashSet<Vector2Int> floorPos = new HashSet<Vector2Int>();

        for (int i = 0; i < iterations; i++)
        {
            var path = ProceduralGenerationAlgorithms.SimpleRandomWalk(currentPosition, walkLength);
            floorPos.UnionWith(path);
            if (startRandIteration)
            {
                currentPosition = floorPos.ElementAt(Random.Range(0, floorPos.Count));
            }
        }
        return floorPos;
    }
}
