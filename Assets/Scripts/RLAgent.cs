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
    private int stepsStuck = 0;
    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(0f, -3.5f, -1);
        previousDistance = 0f;
        stepsStuck = 0;

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
        float speed = 20f;

        // Move the ghost
        Vector3 moveDirection = actionDict[action] * speed * Time.deltaTime;

        Vector3 newPosition = new Vector3(
            transform.localPosition.x + moveDirection.x,
            transform.localPosition.y + moveDirection.y,
            -1
        );
        

        transform.localPosition = newPosition;

        currentDistance = Vector2.Distance(new Vector2(pacman.localPosition.x, pacman.localPosition.y), 
                                        new Vector2(transform.localPosition.x, transform.localPosition.y));

        AddReward(-0.01f);
        float proximityReward = 0.1f / (currentDistance + 0.1f);
        AddReward(proximityReward);

        if (Vector3.Distance(transform.localPosition, newPosition) < 0.01f)
        {
            stepsStuck++;
        }
        else
        {
            stepsStuck = 0;
        }

        if (stepsStuck >= 50)
        {
            AddReward(-20f);
            EndEpisode();
            Debug.Log("Agent stuck for 50 steps. Ending Episode");
        }

        if (currentDistance < 1.0f)
        {
            SetReward(100f);
            EndEpisode();
            Debug.Log("Pac-Man caught! Ending Episode");
        }

        previousDistance = currentDistance;
    }
}