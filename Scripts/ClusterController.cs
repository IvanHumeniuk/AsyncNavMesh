using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClusterController : MonoBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private GameObject dynamicObstaclePrefab;
    [SerializeField]
    private GameObject botPrefab;
    [SerializeField]
    private Transform targetObj;

    [SerializeField]
    private List<Transform> playerSpawnPoints = new List<Transform>();
    [SerializeField]
    private List<Transform> dynamicObstaclesSpawnPoints = new List<Transform>();
    [SerializeField]
    private List<Transform> targetPoints = new List<Transform>();
    [SerializeField]
    private List<Transform> botSpawnPoints = new List<Transform>();
    [SerializeField]
    private List<RouteData> dynamicObstaclesRoutes = new List<RouteData>();
    [SerializeField]
    private List<RouteData> botsRoutes = new List<RouteData>();

    [SerializeField]
    private int botsPerSpawnPoint = 5;

    [SerializeField]
    private float changeTargetPointInterval = 5f;
    private float lastTargetPointChangeTime = 0f;
    private int targetPointIndex = 0;

    private List<DynamicObstacleController> dynamicObstacles = new List<DynamicObstacleController>();
    private List<PlayerController> players = new List<PlayerController>();
    private List<BotController> bots = new List<BotController>();

    public void Initialize(int index)
    {
        targetObj.position = targetPoints[targetPointIndex].position;
        lastTargetPointChangeTime = Time.time;

        //Spawn players
        for (int i = 0; i < playerSpawnPoints.Count; i++)
        {
            GameObject player = Instantiate(playerPrefab, playerSpawnPoints[i].position, Quaternion.identity, transform.root);
            players.Add(player.GetComponent<PlayerController>());

            players[i].SetGraphName(index);
            players[i].MoveToPoint(targetObj);
        }

        //Spawn dynamic obstacles
        for(int i = 0; i < dynamicObstaclesSpawnPoints.Count; i++)
        {
            GameObject dynObs = Instantiate(dynamicObstaclePrefab, dynamicObstaclesSpawnPoints[i].position, Quaternion.identity, transform.root);
            dynamicObstacles.Add(dynObs.GetComponent<DynamicObstacleController>());

            dynamicObstacles[i].SetGraphMask(index);
            dynamicObstacles[i].SetRoute(dynamicObstaclesRoutes[i].waypoints);
        }

        //Spawn bots
        for(int i = 0; i < botSpawnPoints.Count; i++)
        {
            for (int j = 0; j < botsPerSpawnPoint; j++)
            {
                BotController bot = Instantiate(botPrefab, botSpawnPoints[i].position, Quaternion.identity, transform.root).GetComponent<BotController>();
                bots.Add(bot);

                bot.SetGraphName(index);
                bot.SetRoute(j + 1, botsRoutes[i].waypoints);
            }
        }
    }

    private void Update()
    {
        if(Time.time > lastTargetPointChangeTime + changeTargetPointInterval)
        {
            lastTargetPointChangeTime = Time.time;
            targetPointIndex++;

            if (targetPointIndex >= targetPoints.Count)
                targetPointIndex = 0;

            targetObj.position = targetPoints[targetPointIndex].position;

            for (int i = 0; i < players.Count; i++)
            {
                players[i].MoveToPoint(targetObj);
            }
        }
    }
}

[Serializable]
public class RouteData
{
    public List<Transform> waypoints = new List<Transform>();
}