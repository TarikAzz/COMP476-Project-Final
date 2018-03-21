using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof (FieldOfView))]
public class FieldofViewEditor : Editor {

    void OnSceneGUI()
    {
        FieldOfView fov = (FieldOfView) target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fov.transform.position,Vector3.up,Vector3.forward,360,fov.ViewRadius);
        Vector3 viewAngleA = fov.DirectionFromAngle(-fov.ViewAngle / 2, false);
        Vector3 viewAngleB = fov.DirectionFromAngle(fov.ViewAngle / 2, false);
        Handles.DrawLine(fov.transform.position, fov.transform.position+viewAngleA*fov.ViewRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleB * fov.ViewRadius);

        Handles.color = Color.red;
        foreach (Transform visibleTarget in fov.visibleTargets)
        {
            Handles.DrawLine(fov.transform.position,visibleTarget.position);
        }
    }
		
	
}
