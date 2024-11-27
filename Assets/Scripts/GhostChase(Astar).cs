using UnityEngine;
using System.Collections.Generic;

public class GhostChaseAstar : GhostBehavior, IGhostChase
{
    private Node currentNode;
    private float predictionTime = 0.5f;
    private float predictionUpdateInterval = 5f;
    private float lastPredictionTime;
    private Vector2 currentPredictedTarget;

    private void OnDisable()
    {
        // Enable scatter behavior when this behavior is disabled
        ghost.scatter.Enable();
    }
    private void Start()
    {
        lastPredictionTime = Time.time;
        currentPredictedTarget = PredictTargetPosition();
        Debug.Log("Initial predicted target: " + currentPredictedTarget);
    }

    private void Update()
    {
        // Update the predicted target position at regular intervals
        if (Time.time - lastPredictionTime >= predictionUpdateInterval)
        {
            currentPredictedTarget = PredictTargetPosition();
            lastPredictionTime = Time.time;
            Debug.Log("Updated predicted target: " + currentPredictedTarget);
        }
    }


    private Vector2 PredictTargetPosition()
    {
        // Predict Pacman's future position based on its current direction and speed
        Vector2 pacmanPos = ghost.pacman.position;
        Vector2 pacmanDirection = ghost.pacman.GetComponent<Movement>().direction;
        Vector2 predictedPosition = pacmanPos + (pacmanDirection * ghost.movement.speed * predictionTime);
        Debug.Log("Predicted position: " + predictedPosition);
        return predictedPosition;
    }

    private List<Node> FindPath(Vector2 targetPosition)
    {
        Debug.Log("Finding path to target position: " + targetPosition);
        var openSet = new List<Node>();
        var closedSet = new HashSet<Node>();
        var nodeCosts = new Dictionary<Node, NodeInfo>();

        openSet.Add(currentNode);
        nodeCosts[currentNode] = new NodeInfo(0, Vector2.Distance(currentNode.transform.position, targetPosition));

        while (openSet.Count > 0)
        {
            Node current = GetLowestFCostNode(openSet, nodeCosts);
            Debug.Log("Current node: " + current.transform.position);

            // Check if the target position is reached

            if (Vector2.Distance(current.transform.position, targetPosition) < 0.5f)
            {
                Debug.Log("Path found to target position.");
                return ReconstructPath(nodeCosts, current);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            // Evaluate neighboring nodes

            foreach (Vector2 direction in current.availableDirections)
            {
                Vector2 nextPosition = (Vector2)current.transform.position + direction;
                RaycastHit2D hit = Physics2D.Raycast(current.transform.position, direction, 1f);

                if (hit.collider != null)
                {
                    Node neighbor = hit.collider.GetComponent<Node>();
                    if (neighbor != null && !closedSet.Contains(neighbor))
                    {
                        float newGCost = nodeCosts[current].gCost + 1;

                        // Update node costs if a better path is found
                        if (!nodeCosts.ContainsKey(neighbor) || newGCost < nodeCosts[neighbor].gCost)
                        {
                            nodeCosts[neighbor] = new NodeInfo(
                                newGCost,
                                Vector2.Distance(neighbor.transform.position, targetPosition),
                                current
                            );

                            if (!openSet.Contains(neighbor))
                            {
                                openSet.Add(neighbor);
                            }
                        }
                    }
                }
            }
        }

        Debug.Log("No path found to target position.");
        return null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Node node = other.GetComponent<Node>();

        if (node != null && enabled && !ghost.frightened.enabled)
        {
            currentNode = node;
            Debug.Log("Entered node: " + node.transform.position);
            List<Node> path = FindPath(currentPredictedTarget);

            if (path != null && path.Count > 1)
            {
                Vector2 nextNodePosition = path[1].transform.position;
                Vector2 direction = (nextNodePosition - (Vector2)transform.position).normalized;

                direction.x = Mathf.Round(direction.x);
                direction.y = Mathf.Round(direction.y);

                if (IsValidDirection(node, direction))
                {
                    Debug.Log("Setting direction: " + direction);
                    ghost.movement.SetDirection(direction);
                }
                else
                {
                    Debug.Log("Invalid direction: " + direction + ". Choosing best available direction.");
                    ChooseBestAvailableDirection(node);
                }
            }
            else
            {
                Debug.Log("No valid path found. Choosing best available direction.");
                ChooseBestAvailableDirection(node);
            }
        }
    }

    // Check if the direction is valid based on available directions
    private bool IsValidDirection(Node node, Vector2 direction)
    {
        return node.availableDirections.Contains(direction);
    }

    private void ChooseBestAvailableDirection(Node node)
    {
        Vector2 targetDir = ((Vector2)ghost.pacman.position - (Vector2)transform.position).normalized;
        float bestScore = float.MinValue;
        Vector2 bestDirection = node.availableDirections[0];

        // Choose the best direction based on the target direction
        foreach (Vector2 availableDir in node.availableDirections)
        {
            if (availableDir == -ghost.movement.direction && node.availableDirections.Count > 1)
            {
                continue;
            }

            float score = Vector2.Dot(availableDir, targetDir);
            if (score > bestScore)
            {
                bestScore = score;
                bestDirection = availableDir;
            }
        }

        Debug.Log("Best available direction: " + bestDirection);
        ghost.movement.SetDirection(bestDirection);
    }

    private Node GetLowestFCostNode(List<Node> nodes, Dictionary<Node, NodeInfo> nodeCosts)
    {
        // Get the node with the lowest fCost
        Node lowest = nodes[0];
        foreach (Node node in nodes)
        {
            if (nodeCosts[node].fCost < nodeCosts[lowest].fCost)
            {
                lowest = node;
            }
        }
        return lowest;
    }

    private List<Node> ReconstructPath(Dictionary<Node, NodeInfo> nodeCosts, Node current)
    {
        // Reconstruct the path from the target node to the start node
        var path = new List<Node> { current };
        while (nodeCosts[current].parent != null)
        {
            current = nodeCosts[current].parent;
            path.Insert(0, current);
        }
        Debug.Log("Reconstructed path: " + string.Join(" -> ", path));
        return path;
    }

    public bool IsEnabled => enabled;

    private class NodeInfo
    {
        public float gCost;
        public float hCost;
        public float fCost => gCost + hCost;
        public Node parent;

        public NodeInfo(float g, float h, Node p = null)
        {
            gCost = g;
            hCost = h;
            parent = p;
        }
    }
}