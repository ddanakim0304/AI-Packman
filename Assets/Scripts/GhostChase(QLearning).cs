using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GhostChaseQLearning : GhostBehavior, IGhostChase
{
    [Header("Q-Learning Hyperparameters")]
    [SerializeField] private float alpha = 0.1f;    // Learning rate
    [SerializeField] private float gamma = 0.9f;    // Discount factor
    [SerializeField] private float epsilon = 0.3f;  // Epsilon for epsilon-greedy
    [SerializeField] private float epsilonMin = 0.1f;
    [SerializeField] private float epsilonDecay = 0.999f; // Decay epsilon each step

    public bool IsEnabled => enabled;

    // Q-learning storage
    private Dictionary<(int dx, int dy), Dictionary<Vector2, float>> Q 
        = new Dictionary<(int dx, int dy), Dictionary<Vector2, float>>();

    private (int dx, int dy) previousState;
    private Vector2 previousAction;
    private bool hasPreviousState = false;
    private List<Vector2> previousActions;

    private void OnDisable()
    {
        ghost.scatter.Enable();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Node node = other.GetComponent<Node>();
        if (node == null || !enabled || ghost.frightened.enabled)
        {
            return;
        }
    
        Vector2 ghostPos = node.transform.position;
        Vector2 pacmanPos = ghost.pacman.position;
    
        int dx = Mathf.RoundToInt(pacmanPos.x - ghostPos.x);
        int dy = Mathf.RoundToInt(pacmanPos.y - ghostPos.y);
        (int dx, int dy) currentState = (dx, dy);
    
        // Initialize Q-table for current state if it doesn't exist
        if (!Q.ContainsKey(currentState))
        {
            Q[currentState] = new Dictionary<Vector2, float>();
        }
    
        List<Vector2> actions = node.availableDirections;
    
        // Initialize Q-values for all available actions
        foreach (Vector2 action in actions)
        {
            Vector2 roundedAction = RoundDirection(action);
            if (!Q[currentState].ContainsKey(roundedAction))
            {
                Q[currentState][roundedAction] = 0f;
            }
        }
    
        // Update Q-values for previous state-action pair if exists
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
    
            float r = CalculateReward(previousState, currentState);
            float maxQNext = Q[currentState].Values.Max();
            
            Q[previousState][roundedPrevAction] = Q[previousState][roundedPrevAction] + 
                alpha * (r + gamma * maxQNext - Q[previousState][roundedPrevAction]);
            
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
            chosenAction = RoundDirection(actions[0]);
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

    private float CalculateReward((int dx, int dy) prev, (int dx, int dy) current)
    {
        float reward = -0.1f;
        int prevDist = Mathf.Abs(prev.dx) + Mathf.Abs(prev.dy);
        int currDist = Mathf.Abs(current.dx) + Mathf.Abs(current.dy);

        if (currDist < prevDist)
        {
            reward += 0.5f;
        }

        if (current.dx == 0 && current.dy == 0)
        {
            reward = 100f;
        }

        return reward;
    }

    private Vector2 RoundDirection(Vector2 dir)
    {
        return new Vector2(Mathf.Round(dir.x), Mathf.Round(dir.y));
    }
}
