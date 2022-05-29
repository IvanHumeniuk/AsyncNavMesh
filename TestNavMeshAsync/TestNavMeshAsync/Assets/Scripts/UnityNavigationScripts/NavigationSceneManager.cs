using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationSceneManager : MonoBehaviour
{
    public GameObject prefab;
    public Vector3 offset;
    public List<NavigationSceneDataController> sceneDataControllers = new List<NavigationSceneDataController>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKeyDown(KeyCode.I))
		{
            NavigationSceneDataController sceneDataController = Instantiate(prefab).GetComponent<NavigationSceneDataController>();
            sceneDataControllers.Add(sceneDataController);

            sceneDataController.Initialize();

            sceneDataController.transform.position = offset * sceneDataControllers.Count;
        }
    }
}
