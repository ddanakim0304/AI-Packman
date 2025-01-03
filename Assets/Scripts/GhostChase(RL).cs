using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using TMPro; 

public class GhostChaseRL : Agent
{
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private Transform pacman;
    [SerializeField] private List<Transform> spawnPoints;
    private float previousDistance = 0f;
    private float currentDistance;
    [SerializeField] private int pacmanCaughtCounter = 0;
    private float episodeTimer = 0f;


    private void Update()
    {
        if (counterText != null)
        {
            counterText.text = $"Count: {pacmanCaughtCounter}";
        }

        // Update timer and check for timeout
        episodeTimer += Time.deltaTime;
        if (episodeTimer >= 40f)
        {
            EndEpisode();
        }
    }

    public override void OnEpisodeBegin()
    {
        episodeTimer = 0f;
        transform.localPosition = new Vector3(0f, -3.5f, -1);
        previousDistance = 0f;

        // Randomly place Pac-Man at one of the spawn points
        if (spawnPoints != null && spawnPoints.Count > 0)
        {
            // Reset the ghost's position
            int randomIndex1 = Random.Range(0, spawnPoints.Count);
            Vector3 ghostPosition = spawnPoints[randomIndex1].localPosition;
            transform.localPosition = new Vector3(ghostPosition.x, ghostPosition.y, -1); // Constant Z

            
            int randomIndex2 = Random.Range(0, spawnPoints.Count);
            Vector3 pacmanPosition = spawnPoints[randomIndex2].localPosition;
            pacman.localPosition = new Vector3(pacmanPosition.x, pacmanPosition.y, -5);
        }

        
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(pacman.localPosition);
        sensor.AddObservation(transform.localPosition);
    }

    private Dictionary<int, Vector3> actionDict = new Dictionary<int, Vector3>{
        { 0, Vector3.zero },
        { 1, Vector3.up },
        { 2, Vector2.down },
        { 3, Vector2.right },
        { 4, Vector2.left }
    };

        public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];
        float speed = 20f;

        // Move the ghost
        Vector3 moveDirection = actionDict[action] * speed * Time.deltaTime;

        Vector3 proposedPosition = new Vector3(
            transform.localPosition.x + moveDirection.x,
            transform.localPosition.y + moveDirection.y,
            -1
        );

        transform.localPosition = proposedPosition;

        currentDistance = Vector2.Distance(new Vector2(pacman.localPosition.x, pacman.localPosition.y), 
                                        new Vector2(transform.localPosition.x, transform.localPosition.y));

        if (currentDistance < 1.0f)
        {
            pacmanCaughtCounter++;
            EndEpisode();
        }
        previousDistance = currentDistance;
    }


}