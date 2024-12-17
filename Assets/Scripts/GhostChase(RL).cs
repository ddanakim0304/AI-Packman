using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;

public class GhostChaseRL : Agent
{
    [SerializeField] private Transform pacman;
    private float previousDistance = 0f;
    private float currentDistance;

    public override void OnEpisodeBegin()
    {
        transform.position = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(pacman.position);
        sensor.AddObservation(transform.position);
    }

    private Dictionary<int, Vector3> actionDict = new Dictionary<int, Vector3>{
        { 0, new Vector3(0, 0, 0)},
        { 1, new Vector3(0, 1, 0) },
        { 2, new Vector3(0, -1, 0) },
        { 3, new Vector3(1, 0, 0) },
        { 4, new Vector3(-1, 0, 0) }
    };

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];
        float speed = 2f;
        
        transform.position += actionDict[action] * speed * Time.deltaTime;

        // Update current distance
        currentDistance = Vector3.Distance(pacman.position, transform.position);

        // Compare distances and set rewards
        if (previousDistance == 0)
        {
            previousDistance = currentDistance;
        }
        else
        {
            if (currentDistance < previousDistance)
            {
                SetReward(0.1f);
            }
            else
            {
                SetReward(-0.1f);
            }
            previousDistance = currentDistance;
        }
    }
    



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SetReward(1f);
            EndEpisode();
        }
        else if (other.CompareTag("Obstacle"))
        {
            SetReward(-1f);
            EndEpisode();
        }
    }
}
