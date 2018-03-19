using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walkable : MonoBehaviour
{
    private const float AngularThreshold = 45f;

    public bool IsWall(Vector3 normal)
    {
        var angle = Vector2.Angle(
            Vector2.up, 
            new Vector2(normal.x, normal.y)
        );
        
        return angle < AngularThreshold;
    }
}
