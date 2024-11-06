using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Level currLevel;

    private void Start()
    {
        currLevel = GameObject.FindGameObjectWithTag("Level").GetComponent<Level>();
    }

    public Vector2 GetCurrLevelBoundingBox()
    {
        return currLevel.GetBoundingBox();
    }
}
