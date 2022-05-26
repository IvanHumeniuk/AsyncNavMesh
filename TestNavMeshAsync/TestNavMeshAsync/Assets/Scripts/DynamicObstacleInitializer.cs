using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicObstacleInitializer : MonoBehaviour
{
    private NavmeshCut navmeshCut;

    private void Awake()
    {
        navmeshCut = GetComponent<NavmeshCut>();

        //Should be set on Awake due to correct initialization
        ChangeGraphMask();
    }

    private void Start()
    {
       // ChangeGraphMask();
    }

    private void ChangeGraphMask()
    {
        navmeshCut.ChangeGraphMask(GraphMask.FromGraphName("MAIN"));
    }
}