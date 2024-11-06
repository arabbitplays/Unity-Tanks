using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField] private Vector2 boundingBox;

    public Vector2 GetBoundingBox()
    {
        return boundingBox;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector2.zero, boundingBox);
    }
}
