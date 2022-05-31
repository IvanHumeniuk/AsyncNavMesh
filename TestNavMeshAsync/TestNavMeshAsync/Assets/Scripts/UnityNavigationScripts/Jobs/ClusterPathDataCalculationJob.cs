using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.AI;

public struct ClusterPathDataCalculationJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction] public NavMeshQueryDataChunk chunk;

    public void Execute(int chunkIndex)
    {

    }
}
