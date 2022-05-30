using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;

namespace UnityNavigationResearch
{
    public class PlayerController : MonoBehaviour
    {
        public Transform target;
        public Vector3[] path;

        private float lastPathTime;
        public float findPathRate;

        public bool isReady;

        // Start is called before the first frame update
        void Start()
        {
            path = new Vector3[0];
        }



        // Update is called once per frame
        void Update()
        {
            if (isReady == false)
                return;

            if (path.Length > 0)
            {
                for (int i = 0; i < path.Length - 1; i++)
                {
                    Debug.DrawLine(path[i], path[i + 1], Color.yellow);
                }
            }

            if (Time.time < lastPathTime + findPathRate)
                return;

            lastPathTime = Time.time;

            CalculatePath();
        }

		private void CalculatePath()
		{
            if(TryFindPath(transform.position, target.position, 1, -1, out path))
			{
				//Debug.Log($"SUCCESS");
			}
        }

        bool TryFindPath(Vector3 start, Vector3 end, float agentRadius, int areas, out Vector3[] path)
        {
            const int maxPathLength = 100;

            using (var result = new NativeArray<PolygonId>(100, Allocator.TempJob))
            using (var query = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.TempJob, 100))
            {
                var from = query.MapLocation(start, Vector3.one * 10, 0);
                var to = query.MapLocation(end, Vector3.one * 10, 0);

                var status = query.BeginFindPath(from, to, areas);
                int maxIterations = 1024;

                while (true)
                {
                    switch (status)
                    {
                        case PathQueryStatus.InProgress:
                            status = query.UpdateFindPath(maxIterations, out int currentIterations);
                            break;

                        case PathQueryStatus.Success:

                            var finalStatus = query.EndFindPath(out int pathLength);
                            var pathResult = query.GetPathResult(result);
                            var straightPath = new NativeArray<NavMeshLocation>(pathLength, Allocator.TempJob);
                            var straightPathFlags = new NativeArray<StraightPathFlags>(pathLength, Allocator.TempJob);
                            var vertexSide = new NativeArray<float>(pathLength, Allocator.TempJob);
                            var jobStatus = new NativeArray<PathQueryStatus>(1, Allocator.TempJob);
                            var straightPathLength = new NativeArray<int>(1, Allocator.TempJob);

                            try
                            {
                                 //int straightPathCount = 0;
                                 //var pathStatus = PathUtils.FindStraightPath(query, start, end, result, pathLength, ref straightPath, ref straightPathFlags, ref vertexSide, ref straightPathCount, maxPathLength);

                                StraightPathCalculationJob straightPathCalculation = new StraightPathCalculationJob()
                                {
                                    query = query,
                                    startPos = start,
                                    endPos = end,
                                    path = result,
                                    pathSize = pathLength,
                                    straightPath = straightPath,
                                    vertexSide = vertexSide,
                                    maxStraightPath = maxPathLength,
                                    straightPathFlags = straightPathFlags,
                                    status = jobStatus,
                                    straightPathLength = straightPathLength
                                };

                                //pathData = straightPathCalculation;
                                //return true;

                                JobHandle job = straightPathCalculation.Schedule(1, 1);
                                job.Complete();
                               
                                int straightPathCount = straightPathCalculation.straightPathLength[0];
                                PathQueryStatus pathStatus = straightPathCalculation.status[0];
                                straightPath = straightPathCalculation.straightPath;
                               

                                if (pathStatus == PathQueryStatus.Success)
                                {
                                    path = new Vector3[straightPathCount];
                                    for (int i = 0; i < straightPathCount; i++)
                                    {
                                        path[i] = straightPath[i].position;
                                    }
                                    return true;
                                }

                                path = default;
                                Debug.Log($"Nav query failed with the status: {status}  {pathStatus}  {pathLength}");
                                return false;
                            }
                            finally
                            {
                                jobStatus.Dispose();
                                straightPathLength.Dispose();
                                straightPath.Dispose();
                                straightPathFlags.Dispose();
                                vertexSide.Dispose();
                            }

                        case PathQueryStatus.Failure:
                            path = default;
                            return false;

                        default:
                            Debug.Log($"Nav query failed with the status: {status}");
                            path = default;
                            return false;
                    }
                }
            }
        }
    }
}