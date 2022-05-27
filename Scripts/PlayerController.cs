using Pathfinding;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Seeker seeker;
    private AIDestinationSetter aIDestinationSetter;

    [SerializeField]
    private Transform target;

    private string graphName = "0";

    [SerializeField]
    private float recalculatePathInterval = 2f;
    private float lastRecalculateTime = 0f;

    private void Awake()
    {
        seeker = GetComponent<Seeker>();
        aIDestinationSetter = GetComponent<AIDestinationSetter>();
    }

    public void SetGraphName(int index)
    {
        graphName = index.ToString();
        //Set graphmask
        seeker.graphMask = GraphMask.FromGraphName(graphName);
    }

    public void MoveToPoint(Transform target)
    {
        aIDestinationSetter.target = target;
        this.target = target;
        CalculatePath();
    }

    private void Update()
    {
        if(Time.time > lastRecalculateTime + recalculatePathInterval)
        {
            lastRecalculateTime = Time.time;
            CalculatePath();
        }
    }

    private void CalculatePath()
    {
        if (target == null)
            return;

        //Calculate path according to graphmask
        seeker.StartPath(transform.position, target.position, (path) =>
        {
            //Callback on path generated asynchroniously
            //for(int i = 0; i < path.vectorPath.Count; i++)
            //{
                //Debug.Log(path.vectorPath[i]);
            //}
        });

    }
}
