using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeButtonScript : MonoBehaviour
{
    public static ConnectionManager connectionManager;
    public List<NodeButtonScript> connectedNodes = new List<NodeButtonScript>();

    public void DrawLineFrom(NodeButtonScript originNode)
    {
        // Add to connections if not already connected
        if (!connectedNodes.Contains(originNode))
        {
            connectedNodes.Add(originNode);
            originNode.connectedNodes.Add(this);

            // Create visual connection
            connectionManager.CreateConnection(originNode, this);
        }

        Debug.Log($"Drawing line from {originNode.gameObject.name} to {gameObject.name}");
    }

    public void RemoveConnection(NodeButtonScript targetNode)
    {
        if (connectedNodes.Contains(targetNode))
        {
            connectedNodes.Remove(targetNode);
            targetNode.connectedNodes.Remove(this);

            // Remove visual connection
            string connectionId = $"{GetInstanceID()}-{targetNode.GetInstanceID()}";
            connectionManager.RemoveConnection(connectionId);
        }
    }
    public List<FlagToNodeConnection> flagConnections = new List<FlagToNodeConnection>();

    void Start()
    {
        foreach (var flagConnection in flagConnections)
        {
            if (FlagManager.Instance.GetFlag(flagConnection.flagName))
            {
                DrawLineFrom(flagConnection.targetNode);
            }
        }

    }
}
[Serializable]
public class FlagToNodeConnection
{
    public string flagName;
    public NodeButtonScript targetNode;

    public FlagToNodeConnection(string name, NodeButtonScript node)
    {
        flagName = name;
        targetNode = node;
    }
}