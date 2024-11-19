using UnityEngine;
using System.Collections.Generic;

public class GhostChaseAstar : GhostBehavior, IGhostChase
{
    private Node currentNode;
    private float predictionTime = 0.5f;
    private float predictionUpdateInterval = 3f;
    private float lastPredictionTime;
    private Vector2 currentPredictedTarget;

    private void Start()
    {
        lastPredictionTime = Time.time;
        currentPredictedTarget = PredictTargetPosition();
    }

    private void Update()
    {
        if (Time.time - lastPredictionTime >= predictionUpdateInterval)
        {
            currentPredictedTarget = PredictTargetPosition();
            lastPredictionTime = Time.time;
        }
    }

    private void OnDisable()
    {
        ghost.scatter.Enable();
    }

    private Vector2 PredictTargetPosition()
    {
        // Get pacman's current position and movement direction
        Vector2 pacmanPos = ghost.pacman.position;
        Vector2 pacmanDirection = ghost.pacman.GetComponent<Movement>().direction;
        
        // Predict future position
        return pacmanPos + (pacmanDirection * ghost.movement.speed * predictionTime);
    }

    private List<Node> FindPath(Vector2 targetPosition)
    {
        var openSet = new List<Node>();
        var closedSet = new HashSet<Node>();
        var nodeCosts = new Dictionary<Node, NodeInfo>();

        openSet.Add(currentNode);
        nodeCosts[currentNode] = new NodeInfo(0, Vector2.Distance(currentNode.transform.position, targetPosition));

        while (openSet.Count > 0)
        {
            Node current = GetLowestFCostNode(openSet, nodeCosts);
            
            if (Vector2.Distance(current.transform.position, targetPosition) < 0.5f)
            {
                return ReconstructPath(nodeCosts, current);
            }

            openSet.Remove(current);
            closedSet.Add(current);

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

        return null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Node node = other.GetComponent<Node>();

        if (node != null && enabled && !ghost.frightened.enabled)
        {
            currentNode = node;
            List<Node> path = FindPath(currentPredictedTarget);
            
            if (path != null && path.Count > 1)
            {
                Vector2 nextNodePosition = path[1].transform.position;
                Vector2 direction = (nextNodePosition - (Vector2)transform.position).normalized;
                
                // Round the direction vector to ensure it aligns with grid
                direction.x = Mathf.Round(direction.x);
                direction.y = Mathf.Round(direction.y);
                
                // Validate the direction is available
                if (IsValidDirection(node, direction))
                {
                    ghost.movement.SetDirection(direction);
                }
                else
                {
                    // Choose best available direction
                    ChooseBestAvailableDirection(node);
                }
            }
            else
            {
                ChooseBestAvailableDirection(node);
            }
        }
    }

    private bool IsValidDirection(Node node, Vector2 direction)
    {
        return node.availableDirections.Contains(direction);
    }

    private void ChooseBestAvailableDirection(Node node)
    {
        Vector2 targetDir = ((Vector2)ghost.pacman.position - (Vector2)transform.position).normalized;
        float bestScore = float.MinValue;
        Vector2 bestDirection = node.availableDirections[0];

        foreach (Vector2 availableDir in node.availableDirections)
        {
            // Don't reverse direction unless it's the only option
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

        ghost.movement.SetDirection(bestDirection);
    }

    private Node GetLowestFCostNode(List<Node> nodes, Dictionary<Node, NodeInfo> nodeCosts)
    {
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
        var path = new List<Node> { current };
        while (nodeCosts[current].parent != null)
        {
            current = nodeCosts[current].parent;
            path.Insert(0, current);
        }
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