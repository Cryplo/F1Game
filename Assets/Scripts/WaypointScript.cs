using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointScript : MonoBehaviour
{
    [SerializeField] float minDistanceToReach = 5f;
    [SerializeField] WaypointScript nextWaypoint;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public WaypointScript getNext()
    {
        return nextWaypoint;
    }
    
    private void OnDrawGizmos()
    {
        Vector3 start = transform.position;
        Vector3 end = nextWaypoint.GetComponentInParent<Transform>().position;
        Gizmos.DrawLine(start, end);
    }
}
