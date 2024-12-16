using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GhostChaseQLearning : GhostBehavior, IGhostChase
{
    [Header("Q-Learning Hyperparameters")]
    [SerializeField] private float learningRate = 0.1f;     // Learning rate
    [SerializeField] private float discountFactor = 0.9f;     // Discount factor
    [SerializeField] private float epsilon = 0.5f;   // Epsilon for epsilon-greedy
    [SerializeField] private float epsilonMin = 0.1f;
    [SerializeField] private float epsilonDecay = 0.999f; // Decay epsilon each step
    public bool IsEnabled => enabled;

    private Node previousState;
    private Vector2 previousAction;
    private bool hasPreviousState = false;

    private Vector2 pacmanPos;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            DebugPrintQTable();
        }
    }


    public void DebugPrintQTable()
    {
        Debug.Log("----- Q-Learning Table -----");
        foreach (var state in Q)
        {
            string stateInfo = $"State (Node: {state.Key.node.name}, Region: {state.Key.pacmanRegion}): ";
            foreach (var action in state.Value)
            {
                stateInfo += $"Action {action.Key} -> Q: {action.Value:F2}, ";
            }
            Debug.Log(stateInfo.TrimEnd(',', ' '));
        }
        Debug.Log("----------------------------");
    }

    private void OnDisable()
    {
        ghost.scatter.Enable();
    }

    // Pacman's relative directions
    private readonly Vector2[] directionVectors = new[] {
        new Vector2(-1, 1),  // Top-left
        new Vector2(0, 1),   // Top
        new Vector2(1, 1),   // Top-right
        new Vector2(-1, 0),  // Left
        new Vector2(1, 0),   // Right 
        new Vector2(-1, -1), // Bottom-left
        new Vector2(0, -1),  // Bottom
        new Vector2(1, -1)   // Bottom-right
    };

    private struct QState
    {
        public Node node;
        public int pacmanRegion;

        public QState(Node node, int region)
        {
            this.node = node;
            this.pacmanRegion = region;
        }
    }

    // Q-learning storage
    private Dictionary<QState, Dictionary<Vector2, float>> Q = 
    new Dictionary<QState, Dictionary<Vector2, float>>();

    // Determine Pac-Man's relative region
    private int GetPacmanRegion(Vector2 ghostPos, Vector2 pacmanPos)
    {
        Vector2 direction = (pacmanPos - ghostPos).normalized;
        float maxDot = float.MinValue;
        int currentRegion = 0;
        
        for (int i = 0; i < directionVectors.Length; i++)
        {
            float dot = Vector2.Dot(direction, directionVectors[i].normalized);
            if (dot > maxDot)
            {
                maxDot = dot;
                currentRegion = i;
            }
        }
        return currentRegion;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled || ghost.frightened.enabled)
        {
            return;
        }

        Node node = other.GetComponent<Node>();
        // If there's no node, ignore
        if (node == null)
        {
            return;
        }
        Vector2 ghostPos = node.transform.position;
        
        int region = GetPacmanRegion(ghostPos, pacmanPos);
        QState currentState = new QState(node, region);

        // If no available directions, remain still
        List<Vector2> actions = node.availableDirections;
        if (actions == null || actions.Count == 0)
        {
            ghost.movement.SetDirection(Vector2.zero);
            return;
        }

        // Remove the opposite of the previous action to prevent immediate backtracking
        if (hasPreviousState && previousAction != Vector2.zero)
        {
            Vector2 oppositeDirection = -previousAction;
            actions = actions.Where(a => RoundDirection(a) != RoundDirection(oppositeDirection)).ToList();
            
            if (actions.Count == 0)
            {
                // Stay still to avoid back-and-forth
                ghost.movement.SetDirection(Vector2.zero);
                return;
            }
        }

        // Initialize Q-table for current state if it doesn't exist
        if (!Q.ContainsKey(currentState))
        {
            Q[currentState] = new Dictionary<Vector2, float>();
        }

        // Initialize Q-values for all available actions
        foreach (Vector2 action in actions)
        {
            Vector2 roundedAction = RoundDirection(action);
            if (!Q[currentState].ContainsKey(roundedAction))
            {
                Q[currentState][roundedAction] = 0f;
            }
        }

        // Update Q-values for the previous state-action pair if exists
        if (hasPreviousState)
        {
            QState prevState = new QState(previousState, GetPacmanRegion(previousState.transform.position, pacmanPos));
            if (!Q.ContainsKey(prevState))
            {
                Q[prevState] = new Dictionary<Vector2, float>();
                Q[prevState][previousAction] = 0f;
            }

            Vector2 roundedPrevAction = RoundDirection(previousAction);
            if (!Q[prevState].ContainsKey(roundedPrevAction))
            {
                Q[prevState][roundedPrevAction] = 0f;
            }

            float r = CalculateReward(previousState, currentState.node, pacmanPos, ghostPos);
            float maxQNext = Q[currentState].Values.Max();

            // Q-learning update formula for Q(s, a)
            Q[prevState][roundedPrevAction] = Q[prevState][roundedPrevAction] +
                learningRate * (r + discountFactor * maxQNext - Q[prevState][roundedPrevAction]);

            epsilon = Mathf.Max(epsilonMin, epsilon * epsilonDecay);
        }

        // Choose next action (epsilon-greedy)
        Vector2 chosenAction;
        if (Random.value < epsilon)
        {
            // Exploration: random action
            chosenAction = RoundDirection(actions[Random.Range(0, actions.Count)]);
        }
        else
        {
            // Exploitation: best action
            chosenAction = Vector2.zero;
            float bestQ = float.NegativeInfinity;
            foreach (var action in actions)
            {
                Vector2 roundedAction = RoundDirection(action);
                float qVal = Q[currentState][roundedAction];
                if (qVal > bestQ)
                {
                    bestQ = qVal;
                    chosenAction = roundedAction;
                }
            }
        }

        ghost.movement.SetDirection(chosenAction);
        previousState = node;
        previousAction = chosenAction;
        hasPreviousState = true;
    }

    private float CalculateReward(Node prev, Node current, Vector2 pacmanPos, Vector2 ghostPos)
    {
        // Example: reward for moving closer to predicted Pac-Man position
        float prevDist = Vector2.Distance(prev.transform.position, pacmanPos);
        float currDist = Vector2.Distance(current.transform.position, pacmanPos);

        float reward = -0.5f;  // Base negative reward to encourage efficient movement
        if (currDist < prevDist)
        {
            reward += 5.0f;  // Reward for getting closer
        }
        else if (currDist > prevDist)
        {
            reward -= 2.0f;  // Penalty for getting farther
        }
        return reward;
    }

    private Vector2 RoundDirection(Vector2 dir)
    {
        return new Vector2(Mathf.Round(dir.x), Mathf.Round(dir.y));
    }
}