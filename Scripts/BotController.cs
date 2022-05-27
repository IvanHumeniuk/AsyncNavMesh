using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotController : MonoBehaviour
{
    private Seeker seeker;
    private AIDestinationSetter aIDestinationSetter;
    private AIPath aiPath;

    [SerializeField]
    private Transform target;

    private string graphName = "0";

    [SerializeField]
    private float recalculatePathInterval = 2f;
    private float lastRecalculateTime = 0f;

    private List<Transform> waypoints = new List<Transform>();
    private int waypointIndex = 0;

    [SerializeField]
    private float distanceToChangeWayPoint = 1f;

    private void Awake()
    {
        seeker = GetComponent<Seeker>();
        aIDestinationSetter = GetComponent<AIDestinationSetter>();
        aiPath = GetComponent<AIPath>();
    }

    public void SetGraphName(int index)
    {
        graphName = index.ToString();
        //Set graphmask
        seeker.graphMask = GraphMask.FromGraphName(graphName);
    }

    public void SetRoute(float speed, List<Transform> route)
    {
        aiPath.maxSpeed = speed;
        waypoints.AddRange(route);
        MoveToPoint(waypoints[waypointIndex]);
    }

    private void MoveToPoint(Transform target)
    {
        aIDestinationSetter.target = target;
        this.target = target;
        CalculatePath();
    }

    private void Update()
    {
        if (waypoints.Count == 0)
            return;

        float distance = Vector3.Distance(transform.position, waypoints[waypointIndex].position);

        if (distance <= distanceToChangeWayPoint)
        {
            waypointIndex++;
            if (waypointIndex >= waypoints.Count)
                waypointIndex = 0;

            MoveToPoint(waypoints[waypointIndex]);
        }

        if (Time.time > lastRecalculateTime + recalculatePathInterval)
        {
            lastRecalculateTime = Time.time;
            CalculatePath();
        }
    }

    private void CalculatePath()
    {
        if (target == null)
            return;

        //Calculate path according to graphmask
        seeker.StartPath(transform.position, target.position, (path) =>
        {
            //Callback on path generated asynchroniously
            //for(int i = 0; i < path.vectorPath.Count; i++)
            //{
            //Debug.Log(path.vectorPath[i]);
            //}
        });

    }
}