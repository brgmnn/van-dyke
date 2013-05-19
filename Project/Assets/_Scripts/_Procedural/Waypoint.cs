/* Sam Cox - 2009 - FrictionPointStudios.com 
 */

using UnityEngine;
using System.Collections;

public class Waypoint : MonoBehaviour {

    public Vector3 CurrentPosition
    {
        get
        {
            return transform.position;
        }
    }

    void OnDrawGizmos()
    {        
        Gizmos.DrawIcon(transform.position, "Waypoint.tif");
    }
	/*
    void OnDrawGizmosSelected()
    {
        (transform.parent.GetComponentInChildren(typeof(FlythoughController)) as FlythoughController).DrawGizmoStuff();
    }
    */
}
