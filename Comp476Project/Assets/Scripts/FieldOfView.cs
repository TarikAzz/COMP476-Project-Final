using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// A character or object with a field of view
/// </summary>
public class FieldOfView : MonoBehaviour
{
    /// <summary>
    /// The maximum distance that the chracter can see
    ///  </summary>
    public float ViewRadius;

    /// <summary>
    /// The angle viewable by the character
    /// </summary>
    [Range(0,360)]
    public float ViewAngle;

    /// <summary>
    /// Layer mask for characters
    /// </summary>
    public LayerMask targetMask;
    
    /// <summary>
    /// Layer mask for obstacles in the scene
    /// </summary>
    public LayerMask obstacleMask;

    /// <summary>
    /// A list of targets currently within the field of view
    /// </summary>
    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();

    /// <summary>
    /// The resolution for the field of view visualization
    /// Determines how many rays are fired to render the cone of vision
    /// </summary>
    public float meshResolution;

    /// <summary>
    /// The mesh filter for the field of view visualization
    /// </summary>
    public MeshFilter viewMeshFilter;

    /// <summary>
    /// The mesh for the field of view visualization
    /// </summary>
    private Mesh viewMesh;

    /// <summary>
    /// The maximum distance between objects used when determining edges for the field of view visualization
    /// </summary>
    public float edgeDistanceThreshold;

    /// <summary>
    /// The maximum number of iterations run to narrow down the position of edges for the field of view visualization
    /// </summary>
    public int edgeResolveIterations;

    /// <summary>
    /// The playermanager object which owns the character
    /// </summary>
    public PlayerManager owner;

    void Start()
    {
        owner = transform.root.GetComponent<PlayerManager>();

        if (owner.Kind == PlayerManager.PlayerKind.Infiltrator)
        {
            Destroy(viewMeshFilter.gameObject);
            Destroy(this);   
        }

        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
        // Check for visible targets with delay
        StartCoroutine("FindTargetsWithDelay",.2f);
    }


    void LateUpdate()
    {
        DrawFieldOfView();
        foreach (Transform target in visibleTargets)
        {
            // Draw a line in the editor to each visible target
            Debug.DrawLine(transform.position, target.position, Color.red);
        }
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        // Find all potential targets within the view radius
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, ViewRadius, targetMask);
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            // Check if the target is within the characters view angle
            if (Vector3.Angle(transform.forward, directionToTarget) < ViewAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                // Check if any obstacles are between the character and the target
                if (!Physics.Raycast(transform.position,directionToTarget,distanceToTarget,obstacleMask))
                {
                    Character targetCharacter = target.GetComponent<Character>();
                    
                    if (targetCharacter!=null)
                    {
                        //Check if the target has the same owner as the character TODO: Check this before other conditions
                        if (targetCharacter.PlayerManager != owner)
                        {
                            visibleTargets.Add(target);
                        }
                    }
                    
                }
                
            }
        }

    }

    /// <summary>
    /// Takes an angle and returns an equivalent vector3. 
    /// </summary>
    /// <param name="angleInDegrees">The input angle</param>
    /// <param name="angleIsGlobal">True if the angle is global</param>
    /// <returns></returns>
    public Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            // If the input angle is not global, convert it to global
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    /// <summary>
    /// Draw a mesh displaying the characters field of view
    /// </summary>
    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(ViewAngle* meshResolution);
        float stepAngleSize = ViewAngle / stepCount;

        List<Vector3> viewPoints = new List<Vector3>();

        ViewCastInfo oldViewCast = new ViewCastInfo();
        for (int i = 0; i <= stepCount; i++)
        {

            float angle = transform.eulerAngles.y - ViewAngle / 2 + i * stepAngleSize;
            //Debug.DrawLine(transform.position, transform.position+DirectionFromAngle(angle,true) *ViewRadius,Color.red);
            ViewCastInfo newViewCast = ViewCast(angle);


            if (i > 0)
            {
                // If the distance between length of previous cast and current cast is greater than the edge distance threshold,
                // more rays are needed to pinpoint the location of obstacle edges
                bool edgeDistanceThresholdExceeded = Mathf.Abs(oldViewCast.distance - newViewCast.distance) >
                                                     edgeDistanceThreshold;

                // If the previous cast hit an object and the new cast did not, OR if both casts hit edge distance threshold 
                // was exceeded, call Find Edge to pinpoint the location of the edge for visualization
                // 
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDistanceThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }

            viewPoints.Add(newViewCast.endPoint);

            oldViewCast = newViewCast;

        }

        // Update the visualization mesh vertices and triangle indices
        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount-2)*3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
           
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
}

    /// <summary>
    /// Perform a raycast and return a ViewCastInfo struct  with the results
    /// </summary>
    /// <param name="globalAngle">The angle to cast a ray</param>
    /// <returns></returns>
    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 direction = DirectionFromAngle(globalAngle, true);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, ViewRadius, obstacleMask))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + direction * ViewRadius, hit.distance, globalAngle);
        }
    }

    /// <summary>
    /// Struct containing results of a raycast
    /// </summary>
    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 endPoint;
        public float distance;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _endPoint, float _distance, float _angle)
        {
            hit = _hit;
            endPoint = _endPoint;
            distance = _distance;
            angle = _angle;
        }
    }


    /// <summary>
    /// Narrow down the location of an edge between two raycasts
    /// </summary>
    /// <param name="minViewCast">The first raycast result</param>
    /// <param name="maxViewCast">The second raycast result</param>
    /// <returns></returns>
    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;
        
        for (int i = 0; i < edgeResolveIterations; i++)
        {
            // Get the angle of the midpoint between the angles of the two rays
            float angle = (minAngle + maxAngle) / 2;
            // Cast a ray in the midpoint angle
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDistanceThresholdExceeded = Mathf.Abs(minViewCast.distance - newViewCast.distance) >
                                                    edgeDistanceThreshold;
            // If the the min viewcast hit and so did the new viewcast, the new viewcast becomes the new min viewcast
            if (newViewCast.hit == minViewCast.hit && !edgeDistanceThresholdExceeded)
            {
                minAngle = angle;
                minPoint = minViewCast.endPoint;
            }
            else
            // Else the new viewcast becomes the new max viewcast
            {
                maxAngle = angle;
                maxPoint = maxViewCast.endPoint;
            }
        }

        //After the maximum number of iterations, return the edge information
        return new EdgeInfo(minPoint,maxPoint);
        

    }

    /// <summary>
    /// A struct containing information about an edge between two viewcasts for field of view visualization
    /// </summary>
    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointb)
        {
            pointA = _pointA;
            pointB = _pointb;
        }
    }


}
