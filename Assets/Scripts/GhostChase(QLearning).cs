using UnityEngine;
using System.Collections.Generic;

public class GhostChaseQLearning : GhostBehavior, IGhostChase
{
    [Header("Q-Learning Hyperparameters")]
    [SerializeField] private float alpha = 0.1f;    // Learning rate
    [SerializeField] private float gamma = 0.9f;    // Discount factor
    [SerializeField] private float epsilon = 1.0f;  // Epsilon for epsilon-greedy
    [SerializeField] private float epsilonMin = 0.1f;
    [SerializeField] private float epsilonDecay = 0.999f; // Decay epsilon each step

    public bool IsEnabled => enabled;

    // Q-learning storage
    private Dictionary<(int dx, int dy), Dictionary<Vector2, float>> Q 
        = new Dictionary<(int dx, int dy), Dictionary<Vector2, float>>();

    private (int dx, int dy) previousState;
    private Vector2 previousAction;
    private bool hasPreviousState = false;
    private List<Vector2> previousActions; // store previous available actions
    
    private void OnDisable()
    {
        // When this behavior is disabled, switch to scatter
        ghost.scatter.Enable();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Node node = other.GetComponent<Node>();
        if (node == null || !enabled || ghost.frightened.enabled)
        {
            // If there's no node or the ghost is frightened, skip Q-learning decision
            return;
        }

        // Current positions
        Vector2 ghostPos = node.transform.position;
        Vector2 pacmanPos = ghost.pacman.position;

        // Compute relative position (dx, dy)
        int dx = Mathf.RoundToInt(pacmanPos.x - ghostPos.x);
        int dy = Mathf.RoundToInt(pacmanPos.y - ghostPos.y);
        (int dx, int dy) currentState = (dx, dy);

        List<Vector2> actions = node.availableDirections;

        // Initialize Q-values for currentState if needed
        if (!Q.ContainsKey(currentState))
        {
            Q[currentState] = new Dictionary<Vector2, float>();
            foreach (var a in actions)
            {
                // Round directions to avoid floating-point issues if needed
                Vector2 dir = new Vector2(Mathf.Round(a.x), Mathf.Round(a.y));
                if (!Q[currentState].ContainsKey(dir))
                    Q[currentState][dir] = 0f;
            }
        }

        // Only update Q if we have a previous state and action
        if (hasPreviousState)
        {
            // Ensure Q-values for previousState
            if (!Q.ContainsKey(previousState))
            {
                // Initialize using previousActions
                Q[previousState] = new Dictionary<Vector2, float>();
                foreach (var a in previousActions)
                {
                    Vector2 dir = new Vector2(Mathf.Round(a.x), Mathf.Round(a.y));
                    if (!Q[previousState].ContainsKey(dir))
                        Q[previousState][dir] = 0f;
                }
            }

            // Ensure Q-value for previousAction
            if (!Q[previousState].ContainsKey(previousAction))
            {
                Q[previousState][previousAction] = 0f;
            }

            // Calculate reward for transition
            float r = CalculateReward(previousState, currentState);

            // Compute max Q for next state
            float maxQNext = float.NegativeInfinity;
            foreach (var a in Q[currentState].Keys)
            {
                if (Q[currentState][a] > maxQNext)
                    maxQNext = Q[currentState][a];
            }

            // Q-update
            float oldQ = Q[previousState][previousAction];
            Q[previousState][previousAction] = oldQ + alpha * (r + gamma * maxQNext - oldQ);

            // Decay epsilon over time to reduce exploration
            epsilon = Mathf.Max(epsilonMin, epsilon * epsilonDecay);
        }

        if (actions.Count == 0)
        {
            // If no directions are available, choose a random direction
            ghost.movement.SetDirection(actions[0]);
            return;
        }

        // Select an action in currentState using epsilon-greedy
        Vector2 chosenAction;
        if (Random.value < epsilon)
        {
            // Exploration
            chosenAction = actions[Random.Range(0, actions.Count)];
        }
        else
        {
            // Exploitation: pick action with max Q-value
            float bestQ = float.NegativeInfinity;
            chosenAction = actions[0];
            foreach (var a in actions)
            {
                Vector2 dir = new Vector2(Mathf.Round(a.x), Mathf.Round(a.y));
                if (Q[currentState][dir] > bestQ)
                {
                    bestQ = Q[currentState][dir];
                    chosenAction = dir;
                }
            }
        }

        // Move the ghost in the chosen direction
        ghost.movement.SetDirection(chosenAction);

        // Store current as previous for next update
        previousState = currentState;
        previousAction = chosenAction;
        previousActions = actions; // store actions for next time
        hasPreviousState = true;
    }

    private float CalculateReward((int dx, int dy) prev, (int dx, int dy) current)
    {
        // Base step cost
        float reward = -0.1f;

        int prevDist = Mathf.Abs(prev.dx) + Mathf.Abs(prev.dy);
        int currDist = Mathf.Abs(current.dx) + Mathf.Abs(current.dy);

        // Reward getting closer to Pac-Man
        if (currDist < prevDist)
        {
            reward += 0.5f;
        }

        // If caught Pac-Man
        if (current.dx == 0 && current.dy == 0)
        {
            reward = 100f; // Large positive reward for success
        }

        return reward;
    }

}
