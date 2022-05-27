using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClustersManager : MonoBehaviour
{
    [SerializeField]
    private Vector3 graphSize = Vector3.zero;
    [SerializeField]
    private LayerMask graphLayerMask;
    [SerializeField]
    private float graphYOffset = 50f;

    [SerializeField]
    private int numberOfClusters = 10;
    [SerializeField]
    private GameObject clusterPrefab;

    [SerializeField]
    private List<ClusterController> clusters = new List<ClusterController>();

    private int graphsCount
    {
        get
        {
            return AstarPath.active.data.graphs.Length;
        }
    }

    private IEnumerator Start()
    {
        for (int i = 0; i < numberOfClusters - 1; i++)
        {
            CreateGraph();
        }

        for(int i = 0; i < numberOfClusters; i++)
        {
            InstantiateCluster(i);
            yield return null;
        }

        AstarPath.active.Scan();
        yield return null;

        //Begin clasters work
        for (int i = 0; i < clusters.Count; i++)
            clusters[i].Initialize(i);
    }

    private void CreateGraph()
    {
        Vector3 previousGraphPosition = (AstarPath.active.data.graphs[graphsCount - 1] as RecastGraph).forcedBoundsCenter;

        RecastGraph rg = AstarPath.active.data.AddGraph(typeof(RecastGraph)) as RecastGraph;

        rg.name = $"{AstarPath.active.data.graphs.Length - 1}";

        rg.forcedBoundsCenter = previousGraphPosition;
        rg.forcedBoundsCenter.y += graphYOffset;

        rg.forcedBoundsSize = graphSize;
        rg.mask = graphLayerMask;
    }

    private void InstantiateCluster(int graphIndex)
    {
        Vector3 previousClusterPosition = (AstarPath.active.data.graphs[graphIndex] as RecastGraph).forcedBoundsCenter;

        ClusterController cluster = Instantiate(clusterPrefab, previousClusterPosition, Quaternion.identity, null).GetComponent<ClusterController>();
        cluster.gameObject.name = $"Cluster {graphIndex}";
        clusters.Add(cluster);
    }
}