using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.AI;

public struct SessionPathesCaclulationJob : IJobParallelFor
{
	[NativeDisableParallelForRestriction] public NativeArray<int> charactersIDs;
	[NativeDisableParallelForRestriction] public NativeArray<NavMeshQuery> queries;
	[NativeDisableParallelForRestriction] public NativeArray<Vector3> startPositions;
	[NativeDisableParallelForRestriction] public NativeArray<Vector3> endPositions;
	[NativeDisableParallelForRestriction] public NativeArray<PolygonId> path;
	[NativeDisableParallelForRestriction] public NativeArray <int> pathSizes;
	[NativeDisableParallelForRestriction] public NativeArray<int> maxStraightPathes;

	[NativeDisableParallelForRestriction] public NativeHashMap<int, CharacterPathDataContainer> characterPathDataContainers;

	[NativeDisableParallelForRestriction] public NativeArray<NavMeshLocation> straightPath;
	[NativeDisableParallelForRestriction] public NativeArray<StraightPathFlags> straightPathFlags;
	[NativeDisableParallelForRestriction] public NativeArray<float> vertexSide;
	[NativeDisableParallelForRestriction] public NativeArray<int> straightPathLength;
	[NativeDisableParallelForRestriction] public NativeArray<PathQueryStatus> statuses;
	
	public void Execute(int index)
	{
		int pathSize = pathSizes[index];

		if (pathSize == 0)
			return;
		
		int characterID = charactersIDs[index];
		NavMeshQuery query = queries[index];
		Vector3 startPos = startPositions[index];
		Vector3 endPos = endPositions[index];
		int maxStraightPath = maxStraightPathes[0];

		characterPathDataContainers.TryGetValue(characterID, out CharacterPathDataContainer characterData);

		var characterPath = path.GetSubArray(index * maxStraightPath, maxStraightPath);
		var characterStraightPath = straightPath.GetSubArray(characterData.startIndex, characterData.length);
		var characterStraightPathFlags = straightPathFlags.GetSubArray(characterData.startIndex, characterData.length);
		var characterVertexSide = vertexSide.GetSubArray(characterData.startIndex, characterData.length);
		
		int characterPathLength = 0;

		PathQueryStatus pathStatus = PathUtils.FindStraightPath(
			query,
			startPos,
			endPos,
			characterPath,
			pathSize,
			ref characterStraightPath,
			ref characterStraightPathFlags,
			ref characterVertexSide,
			ref characterPathLength,
			maxStraightPath);

		straightPathLength[index] = characterPathLength;
		statuses[index] = pathStatus;
		
		int dataCounter = 0;
		for (int i = characterData.startIndex; i < characterData.length; i++)
		{
			straightPath[i] = characterStraightPath[dataCounter];
			dataCounter++;
		}
	}
}
