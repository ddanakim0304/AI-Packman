using UnityEngine;

public class GhostChase : GhostBehavior
{

    private void OnDisable()
    {
        this.ghost.scatter.Enable();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        Node node = other.GetComponent<Node>();

        if (node != null && this.enabled && !this.ghost.frightened.enabled)
        {
            Vector2 direction = Vector2.zero;
            float minDistance = float.MaxValue;

            foreach (Vector2 availableDirection in node.availableDirections)
            {
                Vector3 newPosition = this.transform.position + new Vector3(availableDirection.x, availableDirection.y, 0);
                float distance = (this.ghost.pacman.position - newPosition).sqrMagnitude;

                if (distance < minDistance)
                {
                    minDistance = distance;
                    direction = availableDirection;
                }
            }

            this.ghost.movement.SetDirection(direction);
        }

    }
}
