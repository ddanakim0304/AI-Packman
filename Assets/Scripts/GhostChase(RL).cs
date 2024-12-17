using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;

public class GhostChaseRL : Agent
{
    [SerializeField] private Transform pacman;
    [SerializeField] private List<Transform> spawnPoints;
    private float previousDistance = 0f;
    private float currentDistance;

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(0f, -3.5f, -1);
        previousDistance = 0f;

        // Randomly place Pac-Man at one of the spawn points
        if (spawnPoints != null && spawnPoints.Count > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Count);
            pacman.localPosition = spawnPoints[randomIndex].localPosition;
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
        transform.localPosition += actionDict[action] * speed * Time.deltaTime;

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
            SetReward(100f); // Large reward for catching Pac-Man
            EndEpisode();
            Debug.Log("Pac-Man caught! Ending Episode");
        }

        previousDistance = currentDistance;

    }
}