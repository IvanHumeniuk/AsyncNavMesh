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
        public int id;

        public Transform target;
        public Vector3[] path;

        private float lastPathTime;
        public float findPathRate;

        private Vector3 movementDirection;
        public float speed = 2;
        public bool isReady;

        private int frameCounter;
        private NavMeshQuery query;

        private NativeArray<PolygonId> result;
        private NativeArray<PathQueryStatus> jobStatus;
        private NativeArray<int> straightPathLength;

        private void Awake()
        {
            isReady = false;
        }

        // Start is called before the first frame update
        void Start()
        {
            path = new Vector3[0];
           /* query = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.Persistent, 1000);
            result = new NativeArray<PolygonId>(100, Allocator.Persistent);
            jobStatus = new NativeArray<PathQueryStatus>(1, Allocator.Persistent);
            straightPathLength = new NativeArray<int>(1, Allocator.Persistent);*/
        }

        private void OnDestroy()
        {
         /*   query.Dispose();
            result.Dispose();
            jobStatus.Dispose();
            straightPathLength.Dispose();*/
        }

        public void Initialize(int id, string name)
        {
            this.id = id;
            isReady = true;
            transform.parent.gameObject.name = name;
            gameObject.name = name;
        }


        // Update is called once per frame
        void FixedUpdate()
        {
            if (isReady == false)
                return;

            frameCounter++;

            if (frameCounter % 3 == 0)
                transform.position += movementDirection * speed * Time.fixedDeltaTime;

            if (Time.time < lastPathTime + findPathRate)
                return;

            movementDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f));

            lastPathTime = Time.time;

            //CalculatePath();
        }

        private void OnDrawGizmos()
        {
            if (isReady == false)
                return;

            Gizmos.color = Color.red;

            if (path != null && path.Length > 0)
            {
                for (int i = 0; i < path.Length - 1; i++)
                {
                    Gizmos.DrawLine(path[i], path[i + 1]);
                }
            }
        }

        private void CalculatePath()
        {
            if (TryFindPath(transform.position, target.position, 1, -1, out Vector3[] newPath))
            {
                path = newPath;
                //Debug.Log($"SUCCESS");
            }
        }

        public PathQueryStatus GetNavigationQuerry(NavMeshQuery navMeshQuery, out int pathLength, out Vector3 startPosition, out Vector3 finishPosition)
        {
            // navMeshQuery = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.TempJob, 1000);

            pathLength = 1;
            startPosition = transform.position;
            finishPosition = target.position;

            int querryFindPatIterations = 1024;

            NavMeshLocation from;
            NavMeshLocation to;
            PathQueryStatus status = PathQueryStatus.Failure;

            try
            {
                from = navMeshQuery.MapLocation(startPosition, Vector3.one, 0);
                to = navMeshQuery.MapLocation(finishPosition, Vector3.one, 0);

                status = navMeshQuery.BeginFindPath(from, to);
            }
            catch (Exception e)
            {
                status = PathQueryStatus.Failure;
                return status;
            }

            for (int i = 0; i < querryFindPatIterations; i++)
            {
                switch (status)
                {
                    case PathQueryStatus.InProgress:
                        {
                            status = navMeshQuery.UpdateFindPath(querryFindPatIterations, out int currentIterations);
                        }
                        break;
                    case PathQueryStatus.Success:
                        {
                            status = navMeshQuery.EndFindPath(out pathLength);
                            return status;
                        }
                    default:
                        //Debug.Log($"{gameObject.name} Nav navMeshQuery failed with the status: {status}");
                        break;
                }
            }

            return status;
        }

        public void UpdatePath(ref NativeArray<NavMeshLocation> straightPath, int length)
        {
            path = new Vector3[length];
            for (int i = 0; i < length; i++)
            {
                path[i] = straightPath[i].position;
            }
        }

        bool TryFindPath(Vector3 start, Vector3 end, float agentRadius, int areas, out Vector3[] path)
        {
            const int maxPathLength = 100;

            NavMeshLocation from;
            NavMeshLocation to;
            PathQueryStatus status = PathQueryStatus.Failure;

            try
            {
                from = query.MapLocation(start, Vector3.one, 0);
                to = query.MapLocation(end, Vector3.one, 0);

                status = query.BeginFindPath(from, to);
            }
            catch (Exception e)
            {
                status = PathQueryStatus.Failure;
                path = default;
                return false;
            }

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

                        try
                        {
                            //int straightPathCount = 0;
                            //var pathStatus = PathUtils.FindStraightPath(query, start, end, result, pathLength,
                            //ref straightPath, ref straightPathFlags, ref vertexSide, ref straightPathCount, maxPathLength);

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

                            JobHandle job = straightPathCalculation.Schedule();
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
                            Debug.Log($"{gameObject.name} Nav query failed with the status: {status}  {pathStatus}  {pathLength}");
                            return false;
                        }
                        finally
                        {
                            straightPath.Dispose();
                            straightPathFlags.Dispose();
                            vertexSide.Dispose();
                        }

                    case PathQueryStatus.Failure:
                        path = default;
                        Debug.Log($"Nav query failed with the status: {status}");
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