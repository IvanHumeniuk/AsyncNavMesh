using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstarController : MonoBehaviour
{
    private Seeker seeker;
    private AIPath aiPath;

    [SerializeField]
    private Transform target;

    private Camera mainCamera;

    [SerializeField]
    private string graphName = "MAIN";

    private void Awake()
    {
        seeker = GetComponent<Seeker>();
        aiPath = GetComponent<AIPath>();

        mainCamera = Camera.main;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            CalculatePath();
        }
    }

    private void CalculatePath()
    {
        //Set graphmask
        seeker.graphMask = GraphMask.FromGraphName(graphName);

        //Calculate path according to graphmask
        seeker.StartPath(transform.position, target.position, (path) =>
        {
            //Callback on path generated asynchroniously
            Debug.Log($"PATH GENERATED FOR: {graphName}");
            for(int i = 0; i < path.vectorPath.Count; i++)
            {
                Debug.Log(path.vectorPath[i]);
            }
        });
    }
}
