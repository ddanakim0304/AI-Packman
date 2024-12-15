using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GhostChaseQLearning : GhostBehavior, IGhostChase 
{
    private float learningRate = 0.1f;
    private float discountFactor = 0.9f;
    private float explorationRate = 0.2f;
    private float updateInterval = 1f;
    private Node currentNode;

    public bool IsEnabled => enabled;
    private Node[] allNodes;
    private Dictionary<int, Dictionary<Vector2, float>> qTable;

    private void OnDisable()
    {
        ghost.scatter.Enable();
    }
    private void Start()
    {
        allNodes = NodeManager.Instance.allNodes.ToArray();
        InitializeQTable();
        StartCoroutine(UpdateBehavior());
    }

        private IEnumerator UpdateBehavior()
    {
        while (enabled)
        {
            yield return new WaitForSeconds(updateInterval);
            
            if (!ghost.frightened.enabled)
            {
                currentNode = FindNearestNode(transform.position);
                if (currentNode != null)
                {
                    Vector2 direction = ChooseAction(currentNode);
                    UpdateQValue(currentNode, direction);
                    ghost.movement.SetDirection(direction);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        Node node = other.GetComponent<Node>();
        
        if (node != null && enabled && !ghost.frightened.enabled)
        {
            currentNode = node;
            Vector2 direction = ChooseAction(node);
            UpdateQValue(node, direction);
            ghost.movement.SetDirection(direction);
        }
    }

    private void InitializeQTable()
    {
        qTable = new Dictionary<int, Dictionary<Vector2, float>>();
        foreach (Node node in allNodes)
        {
            Dictionary<Vector2, float> actions = new Dictionary<Vector2, float>();
            foreach (Vector2 direction in node.availableDirections)
            {
                actions[direction] = 0f;
            }
            qTable[node.GetInstanceID()] = actions;
        }
    }

    private Vector2 ChooseAction(Node node)
    {
        if (Random.value < explorationRate)
        {
            // Exploration: random valid direction
            int randomIndex = Random.Range(0, node.availableDirections.Count);
            return node.availableDirections[randomIndex];
        }
        else
        {
            // Exploitation: best known action
            return GetBestAction(node);
        }
    }

    private Vector2 GetBestAction(Node node)
    {
        Dictionary<Vector2, float> stateActions = qTable[node.GetInstanceID()];
        Vector2 bestAction = node.availableDirections[0];
        float bestValue = float.MinValue;

        foreach (var kvp in stateActions)
        {
            if (kvp.Value > bestValue)
            {
                bestValue = kvp.Value;
                bestAction = kvp.Key;
            }
        }

        return bestAction;
    }

    private void UpdateQValue(Node currentNode, Vector2 action)
    {
        float reward = CalculateReward();
        float currentQ = qTable[currentNode.GetInstanceID()][action];

        // Get the next node based on chosen action
        Vector2 nextPosition = (Vector2)currentNode.transform.position + action;
        Node nextNode = FindNearestNode(nextPosition);

        float maxNextQ = 0f;
        if (nextNode != null)
        {
            // Get max Q-value for next state
            maxNextQ = qTable[nextNode.GetInstanceID()].Values.Max();
        }

        // Q-learning update formula
        float newQ = currentQ + learningRate * (reward + discountFactor * maxNextQ - currentQ);
        qTable[currentNode.GetInstanceID()][action] = newQ;
    }

    private float CalculateReward()
    {
        float distanceToPacman = Vector2.Distance(transform.position, ghost.pacman.position);
        float baseReward = 10.0f / (distanceToPacman + 1.0f); // Increased base reward

        Vector2 directionToPacman = ((Vector2)ghost.pacman.position - (Vector2)transform.position).normalized;
        float alignment = Vector2.Dot(ghost.movement.direction, directionToPacman);
        
        // Penalize getting too close to other ghosts
        float ghostPenalty = 0f;
        foreach (Ghost otherGhost in FindObjectsOfType<Ghost>())
        {
            if (otherGhost != ghost)
            {
                float ghostDist = Vector2.Distance(transform.position, otherGhost.transform.position);
                if (ghostDist < 2f)
                {
                    ghostPenalty += 1f / (ghostDist + 0.1f);
                }
            }
        }

        return (baseReward * (1 + alignment)) - ghostPenalty;
    }

    private Node FindNearestNode(Vector2 position)
    {
        float minDist = float.MaxValue;
        Node nearest = null;

        foreach (Node node in allNodes)
        {
            float dist = Vector2.Distance(position, node.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = node;
            }
        }

        return nearest;
    }
}