using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.AI;

public struct NavMeshQueryDataContainer
{
    [NativeDisableParallelForRestriction] public int characterID;
    [NativeDisableParallelForRestriction] public NavMeshQuery query;
    [NativeDisableParallelForRestriction] public Vector3 startPos;
    [NativeDisableParallelForRestriction] public Vector3 endPos;
    [NativeDisableParallelForRestriction] public NativeArray<PolygonId> queryPathResult;
    [NativeDisableParallelForRestriction] public int pathLength;
    [NativeDisableParallelForRestriction] public int maxPathLength;

    [NativeDisableParallelForRestriction] public NativeArray<StraightPathFlags> straightPathFlags;
    [NativeDisableParallelForRestriction] public NativeArray<float> vertexSide;
   
    // return
    [NativeDisableParallelForRestriction] public NativeArray<NavMeshLocation> straightPath;
    [NativeDisableParallelForRestriction] public NativeArray<int> straightPathLength;
    [NativeDisableParallelForRestriction] public NativeArray<PathQueryStatus> status;

    public void SwapStraightPath(ref NativeArray<NavMeshLocation> swap)
	{
        if (swap.Length != straightPath.Length)
            return;

		for (int i = 0; i < straightPath.Length; i++)
		{
            straightPath[i] = swap[i];
        }
    }
    
    public void SetStraightPathLength(int length)
	{
        straightPathLength[0] = length;
    }

    public void SetStatus(PathQueryStatus pathStatus)
	{
        status[0] = pathStatus;
    }
}
