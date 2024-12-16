using UnityEngine;

public class GhostChaseRL : GhostBehavior, IGhostChase
{
    public GhostRLAgent rlAgent;
    public bool IsEnabled => enabled;

    private void OnDisable()
    {
        ghost.scatter.Enable();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Node node = other.GetComponent<Node>();

        if (node != null && enabled && !ghost.frightened.enabled)
        {
            // Pass current node information to the RL agent
            rlAgent.SetCurrentNode(node);

            // Get direction decision from the RL agent
            Vector2 chosenDirection = rlAgent.GetChosenDirection();

            if (chosenDirection != Vector2.zero)
            {
                ghost.movement.SetDirection(chosenDirection);
            }
            else
            {
                // Fallback to the first available direction
                ghost.movement.SetDirection(node.availableDirections[0]);
            }
        }
    }
}