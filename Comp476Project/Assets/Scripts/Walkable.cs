using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A walkable object
/// </summary>
public class Walkable : MonoBehaviour
{
    #region Enum

    /// <summary>
    /// The different kinds a walkabe surface can be
    /// </summary>
    public enum WalkableKind
    {
        Neutral,
        Start,
        Goal
    }

    #endregion

    #region Constants

    /// <summary>
    /// The threshold at which a slope is considered a wall
    /// </summary>
    private const float AngularThreshold = 45f;

    #endregion

    #region Public variables

    /// <summary>
    /// The walkable surface's kind
    /// </summary>
    public WalkableKind Kind;

    #endregion

    /// <summary>
    /// Handles collision with characters
    /// </summary>
    /// <param name="collision">The collision information</param>
    void OnCollisionEnter(Collision collision)
    {
        var character = collision.gameObject.GetComponent<Character>();

        if (Kind == WalkableKind.Neutral || character == null || !character.Owner.isLocalPlayer)
        {
            return;
        }
        
        Debug.Log("Character entered a surface of the " + Kind + " kind");
    }

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
