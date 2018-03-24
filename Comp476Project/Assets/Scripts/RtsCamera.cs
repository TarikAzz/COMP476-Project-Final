using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the camera movement and functionalities.
/// </summary>
public class RtsCamera : MonoBehaviour
{
    /// <summary>
    /// Toggle value determining if the mouse on edge moves the camera.
    /// </summary>
    public bool canMoveByEdge = true;

    /// <summary>
    /// Toggle value determining if the traditional FOV style zoom occurs.
    /// When toggled off, the camera will physical zoom in.
    /// </summary>
    public bool isFovZoom = true;

    /// <summary>
    /// Value determining the speed of the camera.
    /// </summary>
    public float cameraSpeed;

    /// <summary>
    /// The zoom Speed.
    /// </summary>
    public float zoomSpeed;

    /// <summary>
    /// Determines the horizontal rotation.
    /// </summary>
    public float rotateHorizontal;

    /// <summary>
    /// Determines the horizontal rotation.
    /// </summary>
    public float rotateVertical;
    
    /// <summary>
    /// The min value the camera can pitch.
    /// </summary>
    public float minPitch;

    /// <summary>
    /// The max value the camera can pitch.
    /// </summary>
    public float maxPitch;
    
    /// <summary>
    /// The min FOV zoom value.
    /// </summary>
    public float minFov;

    /// <summary>
    /// The max FOV zoom value.
    /// </summary>
    public float maxFov;

    /// <summary>
    /// The yaw value.
    /// </summary>
    float yaw = 0;

    /// <summary>
    /// The actualy pitch value of the camera.
    /// </summary>
    float pitch = 0;

    /// <summary>
    /// Position on the y plane which keeps the camera at a constant height when isFovZoom is false.
    /// </summary>
    float yPosition;
    
    /// <summary>
    /// The Unity Start method.
    /// </summary>
    void Start()
    {
        yPosition = transform.position.y;
    }

    /// <summary>
    /// The Unity Update method.
    /// </summary>
    void Update()
    {
        // Pan the camera when the mouse hits an edge of the screen.
        if (!Input.GetMouseButton(2) && canMoveByEdge)
        {
            if (Input.mousePosition.y <= 0)
            {
                // transform.position -= transform.forward * cameraSpeed * Time.deltaTime;
                transform.Translate(-(new Vector3(0, cameraSpeed * Time.deltaTime, 0)), Space.Self);
            }

            if (Input.mousePosition.y >= Screen.height)
            {
                //transform.position += transform.forward * cameraSpeed * Time.deltaTime;
                transform.Translate((new Vector3(0, cameraSpeed * Time.deltaTime, 0)), Space.Self);
            }

            if (Input.mousePosition.x <= 0)
            {
                transform.Translate(Vector3.left * cameraSpeed * Time.deltaTime, Space.Self);
            }

            if (Input.mousePosition.x >= Screen.width)
            {
                transform.Translate(Vector3.right * cameraSpeed * Time.deltaTime, Space.Self);
            }
        }

        // Pan the camera either by WASD or arrow keys.
        if (isFovZoom)
        {
            transform.Translate((new Vector3(0, Input.GetAxis("Vertical"), 0) * cameraSpeed * Time.deltaTime), Space.Self);
        }
        else
        {
            transform.position += (transform.forward * cameraSpeed * Time.deltaTime) * Input.GetAxis("Vertical");
        }

        transform.Translate((new Vector3(Input.GetAxis("Horizontal"), 0, 0) * cameraSpeed * Time.deltaTime), Space.Self);
      
        pitch = transform.eulerAngles.x;
        yaw = transform.eulerAngles.y;

        // Use the middle mouse button to rotate the camera and "free look".
        if (Input.GetMouseButton(2)) 
        {
            yaw += rotateHorizontal * Input.GetAxis("Mouse X");
            pitch -= rotateVertical * Input.GetAxis("Mouse Y");

            // Clamping pitch to avoid gimble lock and for possible gameplay reasons
            transform.eulerAngles = new Vector3(Mathf.Clamp(pitch, minPitch, maxPitch), yaw, 0.0f);
        }
        
        // Toggle between two types of zooms.
        if (isFovZoom)
        {
            // Zooming in and out with the camera's FOV.
            float fov = Camera.main.fieldOfView;

            fov += -Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
            fov = Mathf.Clamp(fov, minFov, maxFov);
            Camera.main.fieldOfView = fov;
            
            // Keeps camera at constant height
            transform.position = new Vector3(transform.position.x, yPosition, transform.position.z);
        }
        else
        {
            // Zooming in and out by physically moving the camera.
            transform.Translate(new Vector3(0, 0, Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * 10 * Time.deltaTime), Space.Self);
        }
    }
}
