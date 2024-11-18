using UnityEngine;

public class GhostChase : GhostBehavior
{
    private void OnDisable()
    {
        ghost.scatter.Enable();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Node node = other.GetComponent<Node>();

        if (node != null && enabled && !ghost.frightened.enabled)
        {
            // Get direction to pacman
            Vector2 targetDirection = ((Vector2)ghost.pacman.position - (Vector2)transform.position).normalized;
            Vector2 bestDirection = Vector2.zero;
            float maxDot = -1f;

            foreach (Vector2 availableDirection in node.availableDirections)
            {
                // Compare direction similarity using dot product
                float dot = Vector2.Dot(targetDirection, availableDirection);
                if (dot > maxDot)
                {
                    maxDot = dot;
                    bestDirection = availableDirection;
                }
            }

            if (bestDirection != Vector2.zero)
            {
                ghost.movement.SetDirection(bestDirection);
            }
            else
            {
                // Fallback to first available direction
                ghost.movement.SetDirection(node.availableDirections[0]);
            }
        }
    }
}