using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomUtil : MonoBehaviour
{
    private RandomUtil() { }

    public static Vector2 GetRandomPositionInBox(Vector2 center, Vector2 size)
    {
        float x = Random.Range(-size.x / 2, size.x / 2);
        float y = Random.Range(-size.y / 2, size.y / 2);
        return center + new Vector2(x, y);
    }

    public static Vector2 GetRandomPositionInBox(Vector2 center, Vector2 size, LayerMask obstacleMask, float minDistanceToObstacle)
    {
        Vector2 position = Vector2.zero;
        do
        {
            position = GetRandomPositionInBox(center, size);
        } while (CheckPositionForColliders(position, obstacleMask, minDistanceToObstacle));

        return position;
    }

    public static Vector2 GetRandomPositionInDonut(Vector2 center, float innerRadius, float outerRadius)
    {
        float radius = Random.Range(innerRadius, outerRadius);
        return center + radius * DirectionUtil.GetRandomDir();
    }

    public static Vector2 GetRandomPositionInDonut(Vector2 center, float innerRadius, float outerRadius, LayerMask obstacleMask, float minDistanceToObstacle)
    {
        Vector2 position = Vector2.zero;
        do
        {
            position = GetRandomPositionInDonut(center, innerRadius, outerRadius);
        } while (CheckPositionForColliders(position, obstacleMask, minDistanceToObstacle));

        return position;
    }

    private static bool CheckPositionForColliders(Vector2 position, LayerMask obstacleMask, float minDistanceToObstacle)
    {
        return Physics2D.OverlapCircle(position, minDistanceToObstacle, obstacleMask) != null;
    }
}
