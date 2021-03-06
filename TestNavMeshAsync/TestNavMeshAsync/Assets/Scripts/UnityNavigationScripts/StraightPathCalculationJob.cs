using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.AI;

[BurstCompile]
public struct StraightPathCalculationJob : IJob
{
    [NativeDisableParallelForRestriction] public NavMeshQuery query;
    public Vector3 startPos;
    public Vector3 endPos;
    [NativeDisableParallelForRestriction] public NativeSlice<PolygonId> path;
    public int pathSize;
    public int maxStraightPath;

    // return
    [NativeDisableParallelForRestriction] public NativeArray<NavMeshLocation> straightPath;
    [NativeDisableParallelForRestriction] public NativeArray<StraightPathFlags> straightPathFlags;
    [NativeDisableParallelForRestriction] public NativeArray<float> vertexSide;
    [NativeDisableParallelForRestriction] public NativeArray<int> straightPathLength;
    [NativeDisableParallelForRestriction] public NativeArray<PathQueryStatus> status;
    // 

    public void Execute()
    {
        // check before execution of continue
        if (!query.IsValid(path[0]))
        {
            straightPath[0] = new NavMeshLocation(); // empty terminator
            status[0] = PathQueryStatus.Failure; // | PathQueryStatus.InvalidParam;
            return;
        }

        straightPath[0] = query.CreateLocation(startPos, path[0]);

        straightPathFlags[0] = StraightPathFlags.Start;

        var apexIndex = 0;
        var n = 1;

        if (pathSize > 1)
        {
            var startPolyWorldToLocal = query.PolygonWorldToLocalMatrix(path[0]);

            var apex = startPolyWorldToLocal.MultiplyPoint(startPos);
            var left = new Vector3(0, 0, 0); // Vector3.zero accesses a static readonly which does not work in burst yet
            var right = new Vector3(0, 0, 0);
            var leftIndex = -1;
            var rightIndex = -1;

            for (var i = 1; i <= pathSize; ++i)
            {
                var polyWorldToLocal = query.PolygonWorldToLocalMatrix(path[apexIndex]);

                Vector3 vl, vr;
                if (i == pathSize)
                {
                    vl = vr = polyWorldToLocal.MultiplyPoint(endPos);
                }
                else
                {
                    var success = query.GetPortalPoints(path[i - 1], path[i], out vl, out vr);
                    if (!success)
                    {
                        status[0] = PathQueryStatus.Failure; // | PathQueryStatus.InvalidParam;
                        return;
                    }

                    vl = polyWorldToLocal.MultiplyPoint(vl);
                    vr = polyWorldToLocal.MultiplyPoint(vr);
                }

                vl = vl - apex;
                vr = vr - apex;

                // Ensure left/right ordering
                if (PathUtils.Perp2D(vl, vr) < 0)
                    PathUtils.Swap(ref vl, ref vr);

                // Terminate funnel by turning
                if (PathUtils.Perp2D(left, vr) < 0)
                {
                    var polyLocalToWorld = query.PolygonLocalToWorldMatrix(path[apexIndex]);
                    var termPos = polyLocalToWorld.MultiplyPoint(apex + left);

                    n = PathUtils.RetracePortals(query, apexIndex, leftIndex, path, n, termPos, ref straightPath, ref straightPathFlags, maxStraightPath);
                    if (vertexSide.Length > 0)
                    {
                        vertexSide[n - 1] = -1;
                    }

                    //Debug.Log("LEFT");

                    if (n == maxStraightPath)
                    {
                        straightPathLength[0] = n;
                        status[0] = PathQueryStatus.Success; // | PathQueryStatus.BufferTooSmall;
                        return;
                    }

                    apex = polyWorldToLocal.MultiplyPoint(termPos);
                    left.Set(0, 0, 0);
                    right.Set(0, 0, 0);
                    i = apexIndex = leftIndex;
                    continue;
                }
                if (PathUtils.Perp2D(right, vl) > 0)
                {
                    var polyLocalToWorld = query.PolygonLocalToWorldMatrix(path[apexIndex]);
                    var termPos = polyLocalToWorld.MultiplyPoint(apex + right);

                    n = PathUtils.RetracePortals(query, apexIndex, rightIndex, path, n, termPos, ref straightPath, ref straightPathFlags, maxStraightPath);
                    if (vertexSide.Length > 0)
                    {
                        vertexSide[n - 1] = 1;
                    }

                    //Debug.Log("RIGHT");

                    if (n == maxStraightPath)
                    {
                        straightPathLength[0] = n;
                        status[0] = PathQueryStatus.Success; // | PathQueryStatus.BufferTooSmall;
                        return;
                    }

                    apex = polyWorldToLocal.MultiplyPoint(termPos);
                    left.Set(0, 0, 0);
                    right.Set(0, 0, 0);
                    i = apexIndex = rightIndex;
                    continue;
                }

                // Narrow funnel
                if (PathUtils.Perp2D(left, vl) >= 0)
                {
                    left = vl;
                    leftIndex = i;
                }
                if (PathUtils.Perp2D(right, vr) <= 0)
                {
                    right = vr;
                    rightIndex = i;
                }
            }
        }

        // Remove the the next to last if duplicate point - e.g. start and end positions are the same
        // (in which case we have get a single point)
        if (n > 0 && (straightPath[n - 1].position == endPos))
            n--;

        if (n >= 1)
            n = PathUtils.RetracePortals(query, apexIndex, pathSize - 1, path, n, endPos, ref straightPath, ref straightPathFlags, maxStraightPath);

        if (vertexSide.Length > 0)
        {
            vertexSide[n - 1] = 0;
        }

        if (n == maxStraightPath)
        {
            straightPathLength[0] = n;
            status[0] = PathQueryStatus.Success; // | PathQueryStatus.BufferTooSmall;
            return;
        }

        // Fix flag for final path point
        straightPathFlags[n - 1] = StraightPathFlags.End;

        straightPathLength[0] = n;
        status[0] = PathQueryStatus.Success;
    }

}
