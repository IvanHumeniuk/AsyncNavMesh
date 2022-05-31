using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;
using UnityNavigationResearch;

public class NavigationSceneDataController : MonoBehaviour
{
    [SerializeField] private NavMeshSurface surface;
    [SerializeField] private List<PlayerController> players;

	public int batchCount = 32;

    private float lastPathTime;
    public float findPathRate = 1;

	public bool isReady;

	private IEnumerator Start()
	{
		yield return null;
		isReady = true;
	}

	[ContextMenu("init")]
    public void Initialize()
	{
        players.Clear();
        players.AddRange(GetComponentsInChildren<PlayerController>());
		for (int i = 0; i < players.Count; i++)
		{
            players[i].Initialize(i, $"Player: {i}");
		}
    }

	private void FixedUpdate()
	{
		if (isReady == false)
			return;

        if (Time.time < lastPathTime + findPathRate)
            return; 

        lastPathTime = Time.time;

		int playersCount = players.Count;

		if (playersCount == 0)
			return;

		int maxPathLength = 100;
		int chunksCount = Mathf.CeilToInt((float)playersCount / NavMeshQueryDataChunk.MaxCapacity);
		//Debug.Log($"Chunks {chunksCount}");

		NavMeshQueryDataChunk[] queryDataChunks = new NavMeshQueryDataChunk[chunksCount];

		// Prepare caharacters data

		//NativeArray<PolygonId> queryPathResult = new NativeArray<PolygonId>(100, Allocator.TempJob);

		int chunkIterator = 0;

		for (int i = 0; i < playersCount; i++)
		{
			var queryStatus = players[i].GetNavigationQuerry(out NavMeshQuery query, out int pathLength, out Vector3 start, out Vector3 finish);
			if (queryStatus != PathQueryStatus.Success)
			{
				//Debug.Log($"{gameObject.name}  {players[i].gameObject.name} FAILED {queryStatus}");

				// fill with empty data. Jobs watn it
				queryDataChunks[chunkIterator].Add(new NavMeshQueryDataContainer()
				{
					characterID = -1,
					query = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.TempJob, 1000),
					startPos = default,
					endPos = default,
					queryPathResult = new NativeArray<PolygonId>(1, Allocator.TempJob),
					pathLength = 0,
					maxPathLength = default,
					straightPathFlags = new NativeArray<StraightPathFlags>(1, Allocator.TempJob),
					vertexSide = new NativeArray<float>(1, Allocator.TempJob),
					straightPath = new NativeArray<NavMeshLocation>(1, Allocator.TempJob),
					straightPathLength = new NativeArray<int>(1, Allocator.TempJob),
					status = new NativeArray<PathQueryStatus>(1, Allocator.TempJob)
				});
			}
			else
			{
				NativeArray<PolygonId> queryPathResult = new NativeArray<PolygonId>(100, Allocator.TempJob);
				query.GetPathResult(queryPathResult);

				//Debug.Log($"{chunkIterator}  {queryDataChunks[chunkIterator].Length} {NavMeshQueryDataChunk.MaxCapacity}");
				queryDataChunks[chunkIterator].Add(new NavMeshQueryDataContainer()
				{
					characterID = players[i].id,
					query = query,
					startPos = start,
					endPos = finish,
					queryPathResult = queryPathResult,
					pathLength = pathLength,
					maxPathLength = maxPathLength,
					straightPathFlags = new NativeArray<StraightPathFlags>(pathLength, Allocator.TempJob),
					vertexSide = new NativeArray<float>(pathLength, Allocator.TempJob),
					straightPath = new NativeArray<NavMeshLocation>(pathLength, Allocator.TempJob),
					straightPathLength = new NativeArray<int>(1, Allocator.TempJob),
					status = new NativeArray<PathQueryStatus>(1, Allocator.TempJob)
				});
			}

			if (queryDataChunks[chunkIterator].Length >= NavMeshQueryDataChunk.MaxCapacity)
				chunkIterator++;
		}

		// fill not assigned chunk items with empty data
		int emptyChunkItemsCount = chunksCount * NavMeshQueryDataChunk.MaxCapacity - playersCount;
		for (int i = NavMeshQueryDataChunk.MaxCapacity - emptyChunkItemsCount; i < NavMeshQueryDataChunk.MaxCapacity; i++)
		{
			queryDataChunks[chunkIterator].Add(new NavMeshQueryDataContainer()
			{
				characterID = -1,
				query = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.TempJob, 1000),
				startPos = default,
				endPos = default,
				queryPathResult = new NativeArray<PolygonId>(1, Allocator.TempJob),
				pathLength = 0,
				maxPathLength = default,
				straightPathFlags = new NativeArray<StraightPathFlags>(1, Allocator.TempJob),
				vertexSide = new NativeArray<float>(1, Allocator.TempJob),
				straightPath = new NativeArray<NavMeshLocation>(1, Allocator.TempJob),
				straightPathLength = new NativeArray<int>(1, Allocator.TempJob),
				status = new NativeArray<PathQueryStatus>(1, Allocator.TempJob)
			});
		}

		// JOB
		SessionPathesCaclulationJob pathesCaclulationJob = new SessionPathesCaclulationJob();
		for (int i = 0; i < chunksCount; i++)
		{
			switch (i)
			{
				case 0:
					{
						pathesCaclulationJob.chunk0 = queryDataChunks[0];
						break;
					}
				case 1:
					{
						pathesCaclulationJob.chunk1 = queryDataChunks[1];
						break;
					}
				case 2:
					{
						pathesCaclulationJob.chunk2 = queryDataChunks[2];
						break;
					}
				case 3:
					{
						pathesCaclulationJob.chunk3 = queryDataChunks[3];
						break;
					}
				default:
					return;
			}
		}

		JobHandle job = pathesCaclulationJob.Schedule();
		job.Complete();
		
		// Handle job result



		// Clear data

		for (int i = 0; i < chunksCount; i++)
		{
			for (int j = 0; j < queryDataChunks[i].Length; j++)
			{
				if(queryDataChunks[i][j].status[0] == PathQueryStatus.Success)
				{
					var path = queryDataChunks[i][j].straightPath;
					players[j + i * j].UpdatePath(ref path, queryDataChunks[i][j].straightPathLength[0]);
				}

				queryDataChunks[i][j].query.Dispose();

				if(queryDataChunks[i][j].straightPathFlags.IsCreated)
					queryDataChunks[i][j].straightPathFlags.Dispose();
				
				if(queryDataChunks[i][j].queryPathResult.IsCreated)
					queryDataChunks[i][j].queryPathResult.Dispose();

				if(queryDataChunks[i][j].vertexSide.IsCreated)
					queryDataChunks[i][j].vertexSide.Dispose();

				if(queryDataChunks[i][j].straightPath.IsCreated)
					queryDataChunks[i][j].straightPath.Dispose();

				if(queryDataChunks[i][j].straightPathLength.IsCreated)
					queryDataChunks[i][j].straightPathLength.Dispose();

				if(queryDataChunks[i][j].status.IsCreated)
					queryDataChunks[i][j].status.Dispose();
			}
		}
	}
}
