using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;

public class RLAgent : Agent
{
    [SerializeField] private Transform pacman;
    [SerializeField] private List<Transform> spawnPoints;
    private float previousDistance = 0f;
    private float currentDistance;

    private Vector3 lastPosition;
    private float stuckThreshold = 0.1f;
    private int stuckCounter = 0;
    private int maxStuckFrames = 50;
    private Vector3 previousMoveDirection;
    private float backAndForthPenalty = -0.5f;
    private float stuckPenalty = -0.3f;
    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(0f, -3.5f, -1);
        previousDistance = 0f;

        lastPosition = transform.localPosition;
        stuckCounter = 0;
        previousMoveDirection = Vector3.zero;

        // Randomly place Pac-Man at one of the spawn points
        if (spawnPoints != null && spawnPoints.Count > 0)
        {
            // Reset the ghost's position
            int randomIndex1 = Random.Range(0, spawnPoints.Count);
            Vector3 ghostPosition = spawnPoints[randomIndex1].localPosition;
            transform.localPosition = new Vector3(ghostPosition.x, ghostPosition.y, -1); // Constant Z

            
            int randomIndex2 = Random.Range(0, spawnPoints.Count);
            pacman.localPosition = spawnPoints[randomIndex2].localPosition;
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
        float speed = 5f;

        // Move the ghost
        Vector3 moveDirection = actionDict[action] * speed * Time.deltaTime;

        transform.localPosition = new Vector3(
            transform.localPosition.x + moveDirection.x,
            transform.localPosition.y + moveDirection.y,
            -1 // Ensure Z remains constant
        );

        // Check for back and forth movement
        if (Vector3.Dot(moveDirection, previousMoveDirection) < 0 && moveDirection.magnitude > 0)
        {
            AddReward(backAndForthPenalty);
            Debug.Log("Back and forth movement detected! Applying penalty.");
        }

        // Check if agent is stuck
        float distanceMoved = Vector3.Distance(transform.localPosition, lastPosition);
        if (distanceMoved < stuckThreshold)
        {
            stuckCounter++;
            if (stuckCounter >= maxStuckFrames)
            {
                AddReward(stuckPenalty);
                Debug.Log("Agent is stuck! Applying penalty.");
                stuckCounter = 0;
            }
        }
        else
        {
            stuckCounter = 0;
        }

        lastPosition = transform.localPosition;

        // Store previous move direction
        previousMoveDirection = moveDirection;


        // Update current distance
        currentDistance = Vector2.Distance(new Vector2(pacman.localPosition.x, pacman.localPosition.y), new Vector2(transform.localPosition.x, transform.localPosition.y));

        // Add step penalty to discourage excessive moves
        AddReward(-0.01f);

        // Reward inversely proportional to distance (closer = higher reward)
        float proximityReward = 1.0f / (currentDistance + 0.1f); // Add small epsilon to avoid division by zero
        AddReward(proximityReward);

        // Success condition: Close enough to catch Pac-Man
        if (currentDistance < 0.5f)
        {
            SetReward(10f); // Large reward for catching Pac-Man
            EndEpisode();
            Debug.Log("Pac-Man caught! Ending Episode");
        }

        previousDistance = currentDistance;

    }
}