using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DirectionUtil
{
    //help method to rotate a vector by a given degree
    public static Vector2 Rotate(Vector2 v, float degrees) {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);
        
        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }

    public static Vector2 GetRandomDir()
    {
        float angle = Random.Range(0, 360);
        return Rotate(Vector2.up, angle);
    }
}
