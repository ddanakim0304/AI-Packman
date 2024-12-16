using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class GhostRLAgent : Agent
{
    public Transform pacman;
    private Node currentNode;

    private Vector2 chosenDirection;
    private int stepCount;
    private const int maxStepsPerEpisode = 100;

    // Display training iteration in UI
    [SerializeField] private TMPro.TextMeshProUGUI iterationText;
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
        // Reset ghost to its initial position
        transform.position = transform.position;

        // Set Pac-Man's position (fixed for this phase)
        pacman.position = new Vector3(5, 5, 0);

        // Reset variables
        chosenDirection = Vector2.zero;
        currentNode = null;
        stepCount = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Observe Pac-Man's relative position
        Vector2 relativePosition = (Vector2)pacman.position - (Vector2)transform.position;
        sensor.AddObservation(relativePosition.normalized);

        // Observe available directions from the current node
        if (currentNode != null)
        {
            foreach (Vector2 dir in currentNode.availableDirections)
            {
                sensor.AddObservation(dir.normalized);
            }
        }
        else
        {
            // If no current node, add zero observations for consistency
            for (int i = 0; i < 4; i++)
            {
                sensor.AddObservation(Vector2.zero);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Get the action (direction index) from the RL model
        int action = actions.DiscreteActions[0];

        // Map action to a direction and move the ghost
        if (currentNode != null && action < currentNode.availableDirections.Count)
        {
            chosenDirection = currentNode.availableDirections[action];
        }
        else
        {
            chosenDirection = Vector2.zero;
        }

        // Move ghost
        if (chosenDirection != Vector2.zero)
        {
            transform.position += (Vector3)chosenDirection;
        }

        // Reward and termination logic
        stepCount++;
        float distanceToPacman = Vector2.Distance(transform.position, pacman.position);

        // Reward for moving closer
        AddReward(-distanceToPacman * 0.01f);

        // End episode if ghost reaches Pac-Man
        if (distanceToPacman < 1.0f)
        {
            AddReward(1.0f); // Big reward for catching Pac-Man
            EndEpisode();
        }

        // End episode if maximum steps are reached
        if (stepCount >= maxStepsPerEpisode)
        {
            AddReward(-1.0f); // Penalty for failing to reach Pac-Man
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
