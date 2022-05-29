using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityNavigationResearch;

public class NavigationSceneDataController : MonoBehaviour
{
    [SerializeField] private NavMeshSurface surface;
    [SerializeField] private List<PlayerController> playerControllers;

    public void Initialize()
	{
        playerControllers.AddRange(GetComponentsInChildren<PlayerController>());
        playerControllers.ForEach(x => x.isReady = true);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
