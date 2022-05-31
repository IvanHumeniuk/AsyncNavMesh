using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class NavigationSceneManager : MonoBehaviour
{
    public GameObject prefab;

    public float recalculationPathRate;
    private float lastPathUpdateRate;

    [Header("Spawn navigations settings")]

    public Vector3 offset;

    [Range(1, 100)] public int instancesPerXAxis;
    [Range(1, 100)] public int instancesPerZAxis;

    private int height;
    private int z;

    public int count;
    public float spawnRate;
    private float lastSpawnTime;
    public bool performSpawn;

    private NativeArray<SessionPathesCaclulationJob> sessionPathDatas;

    [Space(10)]
    public List<NavigationSceneDataController> sceneDataControllers = new List<NavigationSceneDataController>();
	private void Start()
	{
        performSpawn = true;
    }

	// Update is called once per frame
	void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            performSpawn = !performSpawn;

        if (performSpawn == false)
            return;

        if (sceneDataControllers.Count >= count)
            return;

        if (Time.time < lastSpawnTime + spawnRate)
            return;

        NavigationSceneDataController sceneDataController = Instantiate(prefab).GetComponent<NavigationSceneDataController>();

        sceneDataController.Initialize();

        Vector3 position = Vector3.zero;

        if (sceneDataControllers.Count > 0 && sceneDataControllers.Count % instancesPerXAxis == 0)
        {
            z++;
            if (z % instancesPerZAxis == 0)
                height++;
        }

        position.x = offset.x * (sceneDataControllers.Count % instancesPerXAxis);
        position.y = offset.y * height;
        position.z = offset.z * (z % instancesPerZAxis);

        sceneDataController.transform.position = position;

        sceneDataControllers.Add(sceneDataController);
        lastSpawnTime = Time.time;
    }

    private void FixedUpdate()
    {
        /*if (Time.time < lastPathUpdateRate + recalculationPathRate)
            return;

        if (sceneDataControllers.Count == 0)
            return;

        sessionPathDatas = new NativeArray<SessionPathDataContainer>(sceneDataControllers.Count, Allocator.TempJob);

        for (int i = 0; i < sceneDataControllers.Count; i++)
        {
            sessionPathDatas[i] = sceneDataControllers[i].CollectSessionPathData();
        }

        /*ClusterPathDataCalculationJob pathDataCalculationJob = new ClusterPathDataCalculationJob()
        {
            sessionPathDataContainers = sessionPathDatas
        };

        JobHandle job = pathDataCalculationJob.Schedule(sessionPathDatas.Length, 20);
        job.Complete();

		for (int i = 0; i < pathDataCalculationJob.sessionPathDataContainers.Length; i++)
		{
            int sessionID = pathDataCalculationJob.sessionPathDataContainers[i].sessionID;
            sceneDataControllers[sessionID].HandlePathCalculations(pathDataCalculationJob.sessionPathDataContainers[i].characterPathDataContainers);
        }

        lastPathUpdateRate = Time.time;
        sessionPathDatas.Dispose();*/
    }
}
