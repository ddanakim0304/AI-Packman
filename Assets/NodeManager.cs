using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
    private static NodeManager instance;
    public static NodeManager Instance => instance;
    
    public List<Node> allNodes { get; private set; }

    private void Awake()
    {
        instance = this;
        // Find all nodes in the scene at startup
        allNodes = new List<Node>(FindObjectsOfType<Node>());
    }
}