using System.Collections.Generic;
using UnityEngine;

public static class ProceduralGenerationAlgorithms
{
    public static HashSet<Vector2Int> SimpleRandomWalk(Vector2Int startPos,int walkLength)
    {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>();
        path.Add(startPos);
        var previouspos = startPos;


        for (int i = 0; i < walkLength; i++)
        {
            var newPos = previouspos + Direction2D.GetRandomCardinalDirection();
            path.Add(newPos);
            previouspos = newPos;
        }
        return path;
    }
}

public static class Direction2D
{
    public static List<Vector2Int> cardinalDirectionsList = new List<Vector2Int>
    {
        new Vector2Int(0,1), // Goes Upwards
        new Vector2Int(1,0), //Goes to the right
        new Vector2Int(0,-1), // Goes downwards
        new Vector2Int(-1,0)   // Goes to the left
    };

    public static Vector2Int GetRandomCardinalDirection()
    {
        return cardinalDirectionsList[Random.Range(0,cardinalDirectionsList.Count)];
    }
}
