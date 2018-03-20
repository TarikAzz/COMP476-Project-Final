using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A walkable object
/// </summary>
public class Walkable : MonoBehaviour
{
    #region Constants

    /// <summary>
    /// The threshold at which a slope is considered a wall
    /// </summary>
    private const float AngularThreshold = 45f;

    #endregion

    /// <summary>
    /// Identifies the surface as a wall or not. Walls are not walkable
    /// </summary>
    /// <param name="normal">The surface normal</param>
    /// <returns>Whether the surface is considered a wall or not</returns>
    public bool IsWall(Vector3 normal)
    {
        var angle = Vector2.Angle(
            Vector2.up, 
            new Vector2(normal.x, normal.y)
        );
        
        return angle < AngularThreshold;
    }
}
