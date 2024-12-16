using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class GhostRLAgent : Agent
{
    private Node currentNode;
    public Transform pacman;

    private Vector2 chosenDirection;

    [SerializeField] private TMPro.TextMeshProUGUI iterationText; // UI text component
    private int trainingIteration = 0;

    public override void OnEpisodeBegin()
    {
        // Increment iteration counter
        trainingIteration++;
        
        // Update UI text if assigned
        if (iterationText != null)
        {
            iterationText.text = $"Training Iteration: {trainingIteration}";
        }

        chosenDirection = Vector2.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Observe Pac-Man's relative position
        Vector2 relativePosition = (Vector2)pacman.position - (Vector2)transform.position;
        sensor.AddObservation(relativePosition.normalized);

        // Observe available directions at the current node
        foreach (Vector2 dir in currentNode.availableDirections)
        {
            sensor.AddObservation(dir.normalized);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];
        
        // Store previous position for reward calculation
        Vector2 previousPos = transform.position;
        
        // Get new direction
        Vector2 newDirection = Vector2.zero;
        if (currentNode != null && action < currentNode.availableDirections.Count) {
            newDirection = currentNode.availableDirections[action];
            
            // Strong penalty for reversing direction
            if (newDirection == -chosenDirection) {
                AddReward(-1.0f);  // Increased penalty
                return;
            }
            
            chosenDirection = newDirection;
        }

        // Calculate distance to Pacman
        float distanceToPacman = Vector2.Distance(transform.position, pacman.position);
        
        // Reward for getting closer to Pacman
        float reward = previousPos.magnitude - distanceToPacman;
        AddReward(reward);

        // Additional penalty for standing still
        if (newDirection == Vector2.zero) {
            AddReward(-0.5f);
        }

        // Bonus reward for catching Pacman
        if (distanceToPacman < 1.0f) {
            AddReward(5.0f);
            EndEpisode();
        }
    }

    public void SetCurrentNode(Node node)
    {
        currentNode = node;
    }

    public Vector2 GetChosenDirection()
    {
        return chosenDirection;
    }
}