using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{

    public LayerMask obstacleLayer;
    public List<Vector2> availableDirections { get; private set; }


    private void Start()
    {
        availableDirections = new List<Vector2>();

        GetAvailableDirections(Vector2.up);
        GetAvailableDirections(Vector2.down);
        GetAvailableDirections(Vector2.left);
        GetAvailableDirections(Vector2.right);
    }

    private void GetAvailableDirections(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one * 0.5f, 0f, direction, 1.0f, obstacleLayer);

        if (hit.collider == null)
        {
            availableDirections.Add(direction);
        }
    }
}
