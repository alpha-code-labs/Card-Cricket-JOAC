using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class UIConnectionLine : MonoBehaviour
{
    private NodeButtonScript fromNode;
    private NodeButtonScript toNode;
    private RectTransform lineRect;

    void Awake()
    {
        lineRect = GetComponent<RectTransform>();
    }

    public void SetConnection(NodeButtonScript from, NodeButtonScript to)
    {
        fromNode = from;
        toNode = to;
        UpdateLinePosition();
    }

    void Update()
    {
        // Update line position if nodes move
        if (fromNode != null && toNode != null)
        {
            UpdateLinePosition();
        }
    }

    private void UpdateLinePosition()
    {
        Vector2 startPos = fromNode.GetComponent<RectTransform>().anchoredPosition;
        Vector2 endPos = toNode.GetComponent<RectTransform>().anchoredPosition;

        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude;

        // Position and scale the line
        lineRect.anchoredPosition = startPos + direction * 0.5f;
        lineRect.sizeDelta = new Vector2(distance, 3f);

        // Rotate to point from start to end
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        lineRect.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}