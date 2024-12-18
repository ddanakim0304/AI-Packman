using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using Unity.VisualScripting;

public class GhostChaseRL : Agent, IGhostChase
{

    public Ghost ghost { get; private set; }
    public float duration;


    public bool IsEnabled => enabled;
    private float previousDistance;
    private Dictionary<int, Vector2> actionDict = new Dictionary<int, Vector2>{
        { 0, Vector2.zero },
        { 1, Vector2.up },
        { 2, Vector2.down },
        { 3, Vector2.right },
        { 4, Vector2.left }
    };

    protected override void OnDisable()
    {
        base.OnDisable();
        ghost.scatter.Enable();
    }

    public override void OnEpisodeBegin()
    {
        // Reset ghost position if needed during training
        previousDistance = Vector2.Distance(
            (Vector2)ghost.pacman.position, 
            (Vector2)transform.position
        );
        this.ghost = GetComponent<Ghost>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((Vector2)ghost.pacman.position);
        sensor.AddObservation((Vector2)transform.position);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Only use ML actions when not handling node movement
        if (!ghost.frightened.enabled)
        {
            int action = actions.DiscreteActions[0];
            Vector2 mlDirection = actionDict[action];
            
            if (mlDirection != Vector2.zero)
            {
                ghost.movement.SetDirection(mlDirection);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Node node = other.GetComponent<Node>();

        if (node != null && enabled && !ghost.frightened.enabled)
        {
            Vector2 targetDirection = ((Vector2)ghost.pacman.position - (Vector2)transform.position).normalized;
            Vector2 bestDirection = Vector2.zero;
            float maxDot = -1f;

            // Get ML prediction
            RequestDecision();
            
            // Fallback to traditional logic if ML doesn't provide valid direction
            foreach (Vector2 availableDirection in node.availableDirections)
            {
                float dot = Vector2.Dot(targetDirection, availableDirection);
                if (dot > maxDot)
                {
                    maxDot = dot;
                    bestDirection = availableDirection;
                }
            }

            if (bestDirection != Vector2.zero)
            {
                ghost.movement.SetDirection(bestDirection);
            }
            else
            {
                ghost.movement.SetDirection(node.availableDirections[0]);
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Implement manual control for testing if needed
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;
    }

        public void Enable()
    {
        enabled = true;
    }

    public void Disable()
    {
        enabled = false;
        OnDisable();
    }
}