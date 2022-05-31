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
        if (Time.time < lastPathTime + findPathRate)
            return; 

        lastPathTime = Time.time;

		int maxPathLength = 100;
		int playersCount = players.Count;

		// Prepare caharacters data

		NativeArray<int> charactersIDs = new NativeArray<int>(playersCount, Allocator.TempJob);
		NativeArray<NavMeshQuery> queries = new NativeArray<NavMeshQuery>(playersCount, Allocator.TempJob);
		NativeArray<Vector3> startPositions = new NativeArray<Vector3>(playersCount, Allocator.TempJob);
		NativeArray<Vector3> endPositions = new NativeArray<Vector3>(playersCount, Allocator.TempJob);
		NativeArray<int> pathSizes = new NativeArray<int>(playersCount, Allocator.TempJob);
		
		NativeArray<PolygonId> path = new NativeArray<PolygonId>(playersCount * maxPathLength, Allocator.TempJob);

		NativeArray<int> maxStraightPathes = new NativeArray<int>(1, Allocator.TempJob);
		maxStraightPathes[0] = maxPathLength;

		NativeArray<int> straightPathLength = new NativeArray<int>(playersCount, Allocator.TempJob); // do not fill
		NativeArray<PathQueryStatus> statuses = new NativeArray<PathQueryStatus>(playersCount, Allocator.TempJob); // do not fill

		NativeHashMap<int, CharacterPathDataContainer> characterPathDataContainers = new NativeHashMap<int, CharacterPathDataContainer>(playersCount, Allocator.TempJob);

		NativeList<NavMeshLocation> straightPath = new NativeList<NavMeshLocation>(Allocator.TempJob);
		NativeList<StraightPathFlags> straightPathFlags = new NativeList<StraightPathFlags>(Allocator.TempJob);
		NativeList<float> vertexSide = new NativeList<float>(Allocator.TempJob);
	
		NavMeshQuery query;
		int pathLength;
		Vector3 start;
		Vector3 finish;

		NativeArray<PolygonId> queryPathResult = new NativeArray<PolygonId>(100, Allocator.TempJob);
		int nativeListLength = 0;

		for (int i = 0; i < playersCount; i++)
		{
			var queryStatus = players[i].GetNavigationQuerry(out query, out pathLength, out start, out finish);
			if (queryStatus != PathQueryStatus.Success)
			{
				Debug.Log($"{gameObject.name}  {players[i].gameObject.name} FAILED {queryStatus}");
				continue;
			}

			charactersIDs[i] = players[i].id;
			queries[i] = query;
			startPositions[i] = start;
			endPositions[i] = finish;
			pathSizes[i] = pathLength;

			query.GetPathResult(queryPathResult);

			int queryResultIndex = 0;
			for (int pathResultIndex = playersCount * i; pathResultIndex < maxPathLength; pathResultIndex++)
			{
				path[pathResultIndex] = queryPathResult[queryResultIndex];
				queryResultIndex++;
			}

			characterPathDataContainers.Add(players[i].id, new CharacterPathDataContainer()
			{
				startIndex = nativeListLength,
				length = pathLength
			});

			straightPath.AddRange(new NativeArray<NavMeshLocation>(pathLength, Allocator.TempJob));
			straightPathFlags.AddRange(new NativeArray<StraightPathFlags>(pathLength, Allocator.TempJob));
			vertexSide.AddRange(new NativeArray<float>(pathLength, Allocator.TempJob));

			nativeListLength += pathLength;
		}

		// JOB
		SessionPathesCaclulationJob pathesCaclulationJob = new SessionPathesCaclulationJob()
		{
			charactersIDs = charactersIDs,
			queries = queries,
			startPositions = startPositions,
			endPositions = endPositions,
			path = path,
			pathSizes = pathSizes,
			maxStraightPathes = maxStraightPathes,
			characterPathDataContainers = characterPathDataContainers,
			straightPath = straightPath,
			straightPathFlags = straightPathFlags,
			vertexSide = vertexSide,
			straightPathLength = straightPathLength,
			statuses = statuses
		};

		JobHandle job = pathesCaclulationJob.Schedule(playersCount, batchCount);
		job.Complete();
		
		// Handle job result

		// Clear data
		charactersIDs.Dispose();
		queries.Dispose();
		startPositions.Dispose();
		endPositions.Dispose();
		path.Dispose();
		pathSizes.Dispose();
		characterPathDataContainers.Dispose();
		straightPath.Dispose();
		straightPathFlags.Dispose();
		vertexSide.Dispose();
		straightPathLength.Dispose();
		statuses.Dispose();

		queryPathResult.Dispose();
	}
}
