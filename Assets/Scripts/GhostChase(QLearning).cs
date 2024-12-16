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

    // Q-learning storage: Q[Node][ActionDirection] = Q-value
    private Dictionary<Node, Dictionary<Vector2, float>> Q = new Dictionary<Node, Dictionary<Vector2, float>>();

    private Node previousState;
    private Vector2 previousAction;
    private bool hasPreviousState = false;

    private float positionUpdateInterval = 1.0f;
    private float lastPositionUpdateTime = 0f;

    private Vector2 pacmanPos;

    private void Update()
    {
        if (Time.time - lastPositionUpdateTime >= positionUpdateInterval)
        {
            lastPositionUpdateTime = Time.time;

            Vector2 pacmanPos = ghost.pacman.position;
        }
    }


    private void OnDisable()
    {
        ghost.scatter.Enable();
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

        Node currentState = node;

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
            
            // If removing opposite direction leaves no actions, just use all actions again
            // or stay still if you prefer that behavior
            if (actions.Count == 0)
            {
                // Stay still or revert to all actions
                // Here we choose to stay still to avoid back-and-forth.
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
            if (!Q.ContainsKey(previousState))
            {
                Q[previousState] = new Dictionary<Vector2, float>();
                Q[previousState][previousAction] = 0f;
            }

            // Ensure the previous action exists in the Q-table
            Vector2 roundedPrevAction = RoundDirection(previousAction);
            if (!Q[previousState].ContainsKey(roundedPrevAction))
            {
                Q[previousState][roundedPrevAction] = 0f;
            }

            float r = CalculateReward(previousState, currentState, pacmanPos, ghostPos);
            float maxQNext = Q[currentState].Values.Max();

            Q[previousState][roundedPrevAction] = Q[previousState][roundedPrevAction] +
                learningRate * (r + discountFactor * maxQNext - Q[previousState][roundedPrevAction]);

            epsilon = Mathf.Max(epsilonMin, epsilon * epsilonDecay);
        }

        // Choose next action
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
        previousState = currentState;
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

        // If ghost reaches Pac-Man’s predicted position (very unlikely)
        if (Mathf.Approximately(currDist, 0f))
        {
            reward += 100f;
        }

        return reward;
    }

    private Vector2 RoundDirection(Vector2 dir)
    {
        return new Vector2(Mathf.Round(dir.x), Mathf.Round(dir.y));
    }
}
