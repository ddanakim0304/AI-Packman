using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GhostChaseQLearning : GhostBehavior, IGhostChase
{
    [Header("Q-Learning Hyperparameters")]
    [SerializeField] private float alpha = 0.1f;    // Learning rate
    [SerializeField] private float gamma = 0.9f;    // Discount factor
    [SerializeField] private float epsilon = 0.5f;  // Epsilon for epsilon-greedy
    [SerializeField] private float epsilonMin = 0.1f;
    [SerializeField] private float epsilonDecay = 0.999f; // Decay epsilon each step

    public bool IsEnabled => enabled;

    // Q-learning storage: Q[Node][ActionDirection] = Q-value
    private Dictionary<Node, Dictionary<Vector2, float>> Q 
        = new Dictionary<Node, Dictionary<Vector2, float>>();

    private Node previousState;
    private Vector2 previousAction;
    private bool hasPreviousState = false;

    private void Start()
    {
        // Start periodic logging every 5 seconds
        InvokeRepeating(nameof(DebugPrintQTable), 5f, 5f);
    }

    private void OnDisable()
    {
        ghost.scatter.Enable();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled || ghost.frightened.enabled) return;

        Node node = other.GetComponent<Node>();
        if (node == null) return;

        // The current state is the current node
        Node currentState = node;

        // Initialize Q-values for the current state if needed
        if (!Q.ContainsKey(currentState))
        {
            Q[currentState] = new Dictionary<Vector2, float>();
        }

        List<Vector2> actions = node.availableDirections;
        foreach (Vector2 action in actions)
        {
            Vector2 roundedAction = RoundDirection(action);
            if (!Q[currentState].ContainsKey(roundedAction))
            {
                Q[currentState][roundedAction] = 0f;
            }
        }

        // Update Q-values for the previous state-action pair
        if (hasPreviousState)
        {
            // Ensure previous state exists in Q
            if (!Q.ContainsKey(previousState))
            {
                Q[previousState] = new Dictionary<Vector2, float>();
                Q[previousState][previousAction] = 0f;
            }

            Vector2 roundedPrevAction = RoundDirection(previousAction);
            if (!Q[previousState].ContainsKey(roundedPrevAction))
            {
                Q[previousState][roundedPrevAction] = 0f;
            }

            float r = CalculateReward(previousState, currentState);
            float maxQNext = 0f;
            if (Q.ContainsKey(currentState) && Q[currentState].Count > 0)
            {
                maxQNext = Q[currentState].Values.Max();
            }

            float oldVal = Q[previousState][roundedPrevAction];
            Q[previousState][roundedPrevAction] = oldVal + alpha * (r + gamma * maxQNext - oldVal);

            // Decay epsilon
            epsilon = Mathf.Max(epsilonMin, epsilon * epsilonDecay);
        }

        // Choose the next action (epsilon-greedy)
        Vector2 chosenAction;
        if (Random.value < epsilon)
        {
            // Exploration
            chosenAction = RoundDirection(actions[Random.Range(0, actions.Count)]);
        }
        else
        {
            // Exploitation
            chosenAction = actions[0];
            float bestQ = float.NegativeInfinity;
            foreach (var action in actions)
            {
                Vector2 roundedAction = RoundDirection(action);
                if (Q[currentState][roundedAction] > bestQ)
                {
                    bestQ = Q[currentState][roundedAction];
                    chosenAction = roundedAction;
                }
            }
        }

        ghost.movement.SetDirection(chosenAction);
        previousState = currentState;
        previousAction = chosenAction;
        hasPreviousState = true;
    }

    private float CalculateReward(Node prevNode, Node currentNode)
    {
        // Reward movement that decreases Manhattan distance to Pac-Man
        // and give a large reward if the ghost reaches Pac-Man.
        Vector2 ghostPos = currentNode.transform.position;
        Vector2 pacmanPos = ghost.pacman.position;

        int prevDist = GetManhattanDistance(prevNode.transform.position, pacmanPos);
        int currDist = GetManhattanDistance(ghostPos, pacmanPos);

        float reward = -0.5f; // small penalty per step to encourage efficiency
        if (currDist < prevDist)
        {
            reward += 5.0f; // reward getting closer
        }

        // If ghost catches Pac-Man (assuming this means positions are the same)
        if (Mathf.Approximately(ghostPos.x, pacmanPos.x) && Mathf.Approximately(ghostPos.y, pacmanPos.y))
        {
            reward = 100f;
        }

        return reward;
    }

    private int GetManhattanDistance(Vector2 posA, Vector2 posB)
    {
        return Mathf.Abs(Mathf.RoundToInt(posA.x - posB.x)) + Mathf.Abs(Mathf.RoundToInt(posA.y - posB.y));
    }

    private Vector2 RoundDirection(Vector2 dir)
    {
        return new Vector2(Mathf.Round(dir.x), Mathf.Round(dir.y));
    }

    private void DebugPrintQTable()
    {
        Debug.Log("----- Q-Table (Filtered) -----");
        foreach (var state in Q)
        {
            // Filter states with at least one significant Q-value
            if (state.Value.Values.Any(q => Mathf.Abs(q) > 0.1f))
            {
                string stateInfo = $"State {state.Key.name}: ";
                foreach (var action in state.Value)
                {
                    stateInfo += $"Action {action.Key} -> Q: {action.Value:F2}, ";
                }
                Debug.Log(stateInfo.TrimEnd(',', ' '));
            }
        }
        Debug.Log("-------------------");
    }
}
