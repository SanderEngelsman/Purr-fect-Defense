using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    public Transform[] waypoints;

    public Vector3 GetWaypoint(int index)
    {
        return waypoints[index].position;
    }

    public int GetWaypointCount()
    {
        return waypoints.Length;
    }
}
