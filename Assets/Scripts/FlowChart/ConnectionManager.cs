using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ConnectionManager : MonoBehaviour
{
    void Awake()
    {
        NodeButtonScript.connectionManager = this;
    }
    [SerializeField] private GameObject linePrefab; // UI Image line prefab
    [SerializeField] private Transform lineContainer; // Parent for organization

    private Dictionary<string, GameObject> activeConnections = new Dictionary<string, GameObject>();

    public void CreateConnection(NodeButtonScript fromNode, NodeButtonScript toNode)
    {
        string connectionId = GetConnectionId(fromNode, toNode);

        // Remove existing connection if it exists
        RemoveConnection(connectionId);

        // Create new line
        GameObject line = Instantiate(linePrefab, lineContainer);
        UIConnectionLine connectionLine = line.GetComponent<UIConnectionLine>();
        connectionLine.SetConnection(fromNode, toNode);

        // Store reference
        activeConnections[connectionId] = line;
    }

    public void RemoveConnection(string connectionId)
    {
        if (activeConnections.TryGetValue(connectionId, out GameObject line))
        {
            Destroy(line);
            activeConnections.Remove(connectionId);
        }
    }

    public void RemoveAllConnections()
    {
        foreach (var connection in activeConnections.Values)
        {
            if (connection != null) Destroy(connection);
        }
        activeConnections.Clear();
    }

    private string GetConnectionId(NodeButtonScript from, NodeButtonScript to)
    {
        // Create unique ID for connection pair
        return $"{from.GetInstanceID()}-{to.GetInstanceID()}";
    }
}