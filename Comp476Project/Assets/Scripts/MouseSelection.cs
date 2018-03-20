using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class used for rectangle mouse selection.
/// </summary>
public class MouseSelection : MonoBehaviour
{
    /// <summary>
    /// Determines if the rectangle is being drawn.
    /// </summary>
    /// <author>Tarik</author>
    bool isSelecting = false;

    /// <summary>
    /// The mouse position on screen when the left click is pressed.
    /// </summary>
    /// <author>Tarik</author>
    Vector3 mousePosition1;

    /// <summary>
    ///The white texture.
    /// </summary>
    /// <author>Tarik</author>
    static Texture2D _whiteTexture;

    /// <summary>
    /// Property determining a white texture.
    /// </summary>
    /// <author>Tarik</author>
    public static Texture2D WhiteTexture
    {
        get
        {
            if (_whiteTexture == null)
            {
                _whiteTexture = new Texture2D(1, 1);
                _whiteTexture.SetPixel(0, 0, Color.white);
                _whiteTexture.Apply();
            }

            return _whiteTexture;
        }
    }

    /// <summary>
    /// The Unity Update method.
    /// </summary>
    /// <author>Tarik</author>
    void Update()
    {
        // If we press the left mouse button, save mouse location and begin selection
        if (Input.GetMouseButtonDown(0))
        {
            isSelecting = true;
            mousePosition1 = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isSelecting = false;
        }
    }

    /// <summary>
    /// The Unity OnGUI method.
    /// </summary>
    /// <author>Tarik</author>
    void OnGUI()
    {
        if (isSelecting)
        {
            // Create a rect from both mouse positions.
            var rect = GetScreenRectangle(mousePosition1, Input.mousePosition);
            DrawRectangle(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            DrawBorderRectangle(rect, 2, new Color(0.8f, 0.8f, 0.95f));
        }
    }

    /// <summary>
    /// Draws the rectanlge on the screen.
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="color"></param>
    /// <author>Tarik</author>
    public static void DrawRectangle(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, WhiteTexture);
        GUI.color = Color.white;
    }

    /// <summary>
    /// Draws the border of the rectangle.
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="thickness"></param>
    /// <param name="color"></param>
    /// <author>Tarik</author>
    public static void DrawBorderRectangle(Rect rect, float thickness, Color color)
    {
        // Top
        DrawRectangle(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        // Left
        DrawRectangle(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        // Right
        DrawRectangle(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        // Bottom
        DrawRectangle(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
    }

    /// <summary>
    /// Converts the rectangle coordinates to screen coordinates.
    /// </summary>
    /// <param name="screenPosition1">A corner of the screen.</param>
    /// <param name="screenPosition2">The ther corner of the screen.</param>
    /// <returns></returns>
    /// <author>Tarik</author>
    public static Rect GetScreenRectangle(Vector3 screenPosition1, Vector3 screenPosition2)
    {
        // Move origin from bottom left to top left
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;
        // Calculate corners
        var topLeft = Vector3.Min(screenPosition1, screenPosition2);
        var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
        // Create Rect
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }

    /// <summary>
    /// Retrieves the bounds of the camera's viewport.
    /// </summary>
    /// <param name="camera">The scene's camera</param>
    /// <param name="screenPosition1">A corner of the screen</param>
    /// <param name="screenPosition2">The ther corner of the screen</param>
    /// <returns>The ViewPort's bounds</returns>
    /// <author>Tarik</author>
    public static Bounds GetViewportBounds(Camera camera, Vector3 screenPosition1, Vector3 screenPosition2)
    {
        var v1 = Camera.main.ScreenToViewportPoint(screenPosition1);
        var v2 = Camera.main.ScreenToViewportPoint(screenPosition2);
        var min = Vector3.Min(v1, v2);
        var max = Vector3.Max(v1, v2);
        min.z = camera.nearClipPlane;
        max.z = camera.farClipPlane;

        var bounds = new Bounds();
        bounds.SetMinMax(min, max);
        return bounds;
    }

    /// <summary>
    /// Determines if a particular GameObject is within the rectangle bounds.
    /// </summary>
    /// <param name="gameObject">The GameObject used to verify.</param>
    /// <returns>Bool value that determines if the gameobject is within the rectangle bounds.</returns>
    /// <author>Tarik</author>
    public bool IsWithinSelectionBounds(GameObject gameObject)
    {
        if (!isSelecting)
        {
            return false;
        }

        var camera = Camera.main;
        var viewportBounds = GetViewportBounds(camera, mousePosition1, Input.mousePosition);

        return viewportBounds.Contains(camera.WorldToViewportPoint(gameObject.transform.position));
    }
}
