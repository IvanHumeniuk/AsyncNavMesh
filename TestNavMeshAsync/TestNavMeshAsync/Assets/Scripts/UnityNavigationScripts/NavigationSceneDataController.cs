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

	private Vector3 pathStart;
	private Vector3 pathEnd;

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
		
		int neededChunksCountForPlayers = Mathf.CeilToInt((float)playersCount / NavMeshQueryDataChunk.Capacity);
		int jobsCount = Mathf.CeilToInt((float)neededChunksCountForPlayers / SessionPathesCaclulationJob.Capacity);

		int chunksCount = jobsCount * SessionPathesCaclulationJob.Capacity;

		//Debug.Log($"Chunks {chunksCount}");

		NavMeshQueryDataChunk[] queryDataChunks = new NavMeshQueryDataChunk[chunksCount];

		// Prepare caharacters data

		int chunkIterator = 0;

		for (int i = 0; i < playersCount; i++)
		{
			NavMeshQuery query = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.TempJob, 1000);
			var queryStatus = players[i].GetNavigationQuerry(query, out int pathLength, out pathStart, out pathEnd);
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
					straightPath = new NativeArray<NavMeshLocation>(1, Allocator.TempJob),
					straightPathLength = new NativeArray<int>(1, Allocator.TempJob),
					status = new NativeArray<PathQueryStatus>(1, Allocator.TempJob)
				});

				query.Dispose();
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
					startPos = pathStart,
					endPos = pathEnd,
					queryPathResult = queryPathResult,
					pathLength = pathLength,
					maxPathLength = maxPathLength,
					straightPath = new NativeArray<NavMeshLocation>(pathLength, Allocator.TempJob),
					straightPathLength = new NativeArray<int>(1, Allocator.TempJob),
					status = new NativeArray<PathQueryStatus>(1, Allocator.TempJob)
				});
			}

			if (queryDataChunks[chunkIterator].Length >= NavMeshQueryDataChunk.Capacity)
				chunkIterator++;
		}

		// fill last not assigned chunk items with empty data
		if (chunkIterator < queryDataChunks.Length)
		{
			int emptyChunkItemsCount = NavMeshQueryDataChunk.Capacity - queryDataChunks[chunkIterator].Length;
			for (int i = 0; i < emptyChunkItemsCount; i++)
			{
				//Debug.Log($"LAST: chunk[{chunkIterator} / {chunksCount}] {i} / {emptyChunkItemsCount} ");

				queryDataChunks[chunkIterator].Add(new NavMeshQueryDataContainer()
				{
					characterID = -1,
					query = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.TempJob, 1000),
					startPos = default,
					endPos = default,
					queryPathResult = new NativeArray<PolygonId>(1, Allocator.TempJob),
					pathLength = 0,
					maxPathLength = default,
					straightPath = new NativeArray<NavMeshLocation>(1, Allocator.TempJob),
					straightPathLength = new NativeArray<int>(1, Allocator.TempJob),
					status = new NativeArray<PathQueryStatus>(1, Allocator.TempJob)
				});
			}

			// fill all left chunks with empty data
			int chunksCountNeedToBeFilled = chunksCount - chunkIterator;
			chunkIterator++;

			for (int i = 0; i < chunksCountNeedToBeFilled - 1; i++)
			{
				for (int j = 0; j < NavMeshQueryDataChunk.Capacity; j++)
				{
					//Debug.Log($"OTHER: chunk[{chunkIterator} / {chunksCount}] {j} / {NavMeshQueryDataChunk.Capacity} ");

					queryDataChunks[chunkIterator].Add(new NavMeshQueryDataContainer()
					{
						characterID = -1,
						query = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.TempJob, 1000),
						startPos = default,
						endPos = default,
						queryPathResult = new NativeArray<PolygonId>(1, Allocator.TempJob),
						pathLength = 0,
						maxPathLength = default,
						straightPath = new NativeArray<NavMeshLocation>(1, Allocator.TempJob),
						straightPathLength = new NativeArray<int>(1, Allocator.TempJob),
						status = new NativeArray<PathQueryStatus>(1, Allocator.TempJob)
					});
				}

				chunkIterator++;
			}

			//Debug.Log($"Pl{playersCount} Em{emptyChunkItemsCount}  Ch{chunksCount}  Jo{jobsCount}  Fi{chunksCountNeedToBeFilled}");
		}
		//JOBS

		// Fill job data
		SessionPathesCaclulationJob pathesCaclulationJob = new SessionPathesCaclulationJob();
		for (int i = 0; i < SessionPathesCaclulationJob.Capacity; i++)
		{
			pathesCaclulationJob.Assign(i, ref queryDataChunks[i]);
		}

		JobHandle handle = pathesCaclulationJob.Schedule(SessionPathesCaclulationJob.Capacity, SessionPathesCaclulationJob.Capacity);

		for (int i = 1; i < jobsCount; i++)
		{
			SessionPathesCaclulationJob pathCalculation = new SessionPathesCaclulationJob();

			for (int jobChunkIndex = 0; jobChunkIndex < SessionPathesCaclulationJob.Capacity; jobChunkIndex++)
			{
				int queryChunkIndex = i * SessionPathesCaclulationJob.Capacity + jobChunkIndex;

				pathCalculation.Assign(jobChunkIndex, ref queryDataChunks[queryChunkIndex]);
			}

			handle = pathCalculation.Schedule(SessionPathesCaclulationJob.Capacity, SessionPathesCaclulationJob.Capacity, handle);
		}

		handle.Complete();

		// Handle job result
		// Clear data

		for (int i = 0; i < chunksCount; i++)
		{
			int chunkLength = queryDataChunks[i].Length;

			for (int j = 0; j < chunkLength; j++)
			{
				if (queryDataChunks[i][j].status[0] == PathQueryStatus.Success)
				{
					int index = i * chunkLength + j;

					var path = queryDataChunks[i][j].straightPath;
					players[index].UpdatePath(ref path, queryDataChunks[i][j].straightPathLength[0]);
				}

				queryDataChunks[i][j].query.Dispose();

				if(queryDataChunks[i][j].queryPathResult.IsCreated)
					queryDataChunks[i][j].queryPathResult.Dispose();

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
