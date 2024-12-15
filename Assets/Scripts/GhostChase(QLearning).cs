using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GhostChaseQLearning : GhostBehavior, IGhostChase
{
    private float learningRate = 0.1f;
    private float discountFactor = 0.9f;
    private float explorationRate = 0.1f;
    public bool IsEnabled => enabled;

    // Cached nodes
    private Node[] allNodes;
    // Q-table: State(Node) -> Action(Direction) -> Q-value
    private Dictionary<int, Dictionary<Vector2, float>> qTable = new Dictionary<int, Dictionary<Vector2, float>>();

    private void OnDisable()
    {
        ghost.scatter.Enable();
    }

    private void Start()
    {
        // Cache all nodes once at startup
        allNodes = NodeManager.Instance.allNodes.ToArray();
        InitializeQTable();

        // Start the coroutine for periodic updates
        StartCoroutine(QUpdatesEveryThreeSeconds());
    }

    private void InitializeQTable()
    {
        foreach (Node node in allNodes)
        {
            Dictionary<Vector2, float> actions = new Dictionary<Vector2, float>();
            foreach (Vector2 direction in node.availableDirections)
            {
                actions[direction] = 0f;
            }
            qTable[node.GetInstanceID()] = actions;
        }
    }

    private IEnumerator QUpdatesEveryThreeSeconds()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);

            // Perform Q-value update every 3 seconds if conditions allow
            if (enabled && !ghost.frightened.enabled)
            {
                Node currentNode = GetCurrentNode();
                if (currentNode != null)
                {
                    // Choose an action using epsilon-greedy policy
                    Vector2 chosenDirection = ChooseAction(currentNode);
                    ghost.movement.SetDirection(chosenDirection);

                    // Update Q-values based on the chosen action
                    UpdateQValue(currentNode, chosenDirection);
                }
            }
        }
    }

    private Node GetCurrentNode()
    {
        // Find the nearest node to the ghost's current position
        Node nearestNode = null;
        float minDistance = float.MaxValue;
        Vector2 currentPosition = transform.position;

        foreach (Node node in allNodes)
        {
            float distance = Vector2.Distance(currentPosition, node.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestNode = node;
            }
        }

        return nearestNode;
    }

    private Vector2 ChooseAction(Node node)
    {
        // Epsilon-greedy policy
        if (Random.value < explorationRate)
        {
            // Exploration: random action
            int randomIndex = Random.Range(0, node.availableDirections.Count);
            return node.availableDirections[randomIndex];
        }
        else
        {
            // Exploitation: best known action
            return GetBestAction(node);
        }
    }

    private Vector2 GetBestAction(Node node)
    {
        Dictionary<Vector2, float> stateActions = qTable[node.GetInstanceID()];
        Vector2 bestAction = Vector2.zero;
        float bestValue = float.MinValue;

        foreach (var kvp in stateActions)
        {
            if (kvp.Value > bestValue)
            {
                bestValue = kvp.Value;
                bestAction = kvp.Key;
            }
        }

        return bestAction;
    }

    private void UpdateQValue(Node currentNode, Vector2 action)
    {
        // Calculate reward based on current state
        float reward = CalculateReward();

        // Current Q-value
        float currentQ = qTable[currentNode.GetInstanceID()][action];

        // Estimate the value of the next state
        float nextStateValue = EstimateNextStateValue(currentNode, action);

        // Q-learning update
        float newQ = currentQ + learningRate * (reward + discountFactor * nextStateValue - currentQ);
        qTable[currentNode.GetInstanceID()][action] = newQ;
    }

    private float CalculateReward()
    {
        // Higher reward when closer to Pacman
        float distanceToPacman = Vector2.Distance(transform.position, ghost.pacman.position);
        return 1.0f / (distanceToPacman + 1.0f);
    }

    private float EstimateNextStateValue(Node currentNode, Vector2 action)
    {
        // Predict the next position
        Vector2 nextPosition = (Vector2)currentNode.transform.position + action;
        
        // Find the nearest node to that next position using cached nodes
        Node nearestNode = null;
        float minDistance = float.MaxValue;

        foreach (Node node in allNodes)
        {
            float distance = Vector2.Distance(node.transform.position, nextPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestNode = node;
            }
        }

        if (nearestNode != null && qTable.ContainsKey(nearestNode.GetInstanceID()))
        {
            return qTable[nearestNode.GetInstanceID()].Values.Max();
        }

        return 0f; // If no next node found, return 0
    }
}
