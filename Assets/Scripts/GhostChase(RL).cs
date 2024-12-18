using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using Unity.VisualScripting;

public class GhostChaseRL : Agent
{
    [SerializeField] private Transform pacman;
    private float previousDistance = 0f;
    private float currentDistance;
    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(0f, 3.5f, -1);
        previousDistance = 0f;
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

    // Check for obstacles using raycast
    RaycastHit2D hit = Physics2D.Raycast(
        transform.localPosition,
        moveDirection,
        moveDirection.magnitude,
        LayerMask.GetMask("Obstacle")  // Make sure you have an "Obstacle" layer
    );

    // Only move if no obstacle is hit
    if (hit.collider == null)
    {
        transform.localPosition = proposedPosition;
    }
    else
    {
        transform.localPosition = new Vector3(
            hit.point.x,
            hit.point.y,
            -1
        );
}

        currentDistance = Vector2.Distance(new Vector2(pacman.localPosition.x, pacman.localPosition.y), 
                                        new Vector2(transform.localPosition.x, transform.localPosition.y));
        previousDistance = currentDistance;
    }
}