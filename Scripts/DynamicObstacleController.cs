using Pathfinding;
using System.Collections.Generic;
using UnityEngine;

public class DynamicObstacleController : MonoBehaviour
{
    private NavmeshCut navmeshCut;

    private List<Transform> waypoints = new List<Transform>();
    private int waypointIndex = 0;

    [SerializeField]
    private float moveSpeed = 5f;
    [SerializeField]
    private float distanceToChangeWayPoint = 1f;

    private void Awake()
    {
        navmeshCut = GetComponent<NavmeshCut>();
    }

    public void SetGraphMask(int index)
    {
        navmeshCut.ChangeGraphMask(GraphMask.FromGraphName(index.ToString()));
    }

    public void SetRoute(List<Transform> route)
    {
        waypoints.AddRange(route);
    }

    private void FixedUpdate()
    {
        if (waypoints.Count == 0)
            return;

        transform.position = Vector3.MoveTowards(transform.position, waypoints[waypointIndex].position, moveSpeed * Time.fixedDeltaTime);

        float distance = Vector3.Distance(transform.position, waypoints[waypointIndex].position);

        if(distance <= distanceToChangeWayPoint)
        {
            waypointIndex++;
            if (waypointIndex >= waypoints.Count)
                waypointIndex = 0;
        }
    }
}