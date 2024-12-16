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
    
        // Existing reset code
        transform.position = GetRandomNodePosition();
        pacman.position = GetRandomNodePosition();
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
        // Get action index
        int action = actions.DiscreteActions[0];

        // Map action to a direction
        if (currentNode != null && action < currentNode.availableDirections.Count)
        {
            chosenDirection = currentNode.availableDirections[action];
        }
        else
        {
            chosenDirection = Vector2.zero;
        }

        // Reward logic
        float distanceToPacman = Vector2.Distance(transform.position, pacman.position);
        AddReward(-distanceToPacman * 0.01f); // Slight penalty for distance

        if (distanceToPacman < 1.0f)
        {
            AddReward(1.0f); // Big reward for catching Pac-Man
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

    private Vector3 GetRandomNodePosition()
    {
        Node[] nodes = FindObjectsOfType<Node>();
        return nodes[Random.Range(0, nodes.Length)].transform.position;
    }
}
