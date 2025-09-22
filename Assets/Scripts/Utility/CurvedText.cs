using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class CurvedText : MonoBehaviour
{
    [Header("Curve Settings")]
    [SerializeField] private AnimationCurve curveShape = AnimationCurve.EaseInOut(0, 0, 1, 0);
    [SerializeField] private float curveStrength = 30f;
    [SerializeField] private bool updateInRealtime = false;
    
    [Header("Arc Settings")]
    [SerializeField] private bool useArcMode = false;
    [SerializeField] private float arcRadius = 200f;
    [SerializeField] private float arcDegrees = 180f;
    
    private TextMeshProUGUI textComponent;
    private bool hasTextChanged;
    
    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
        {
            Debug.LogError("CurvedText: No TextMeshProUGUI component found!");
            enabled = false;
        }
    }
    
    void OnEnable()
    {
        // Subscribe to text change events
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
        UpdateCurvedText();
    }
    
    void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        
        // Reset text to original state when disabled
        if (textComponent != null)
        {
            textComponent.ForceMeshUpdate();
        }
    }
    
    void OnTextChanged(Object obj)
    {
        if (obj == textComponent)
        {
            hasTextChanged = true;
        }
    }
    
    void LateUpdate()
    {
        if (updateInRealtime || hasTextChanged)
        {
            UpdateCurvedText();
            hasTextChanged = false;
        }
    }
    
    [ContextMenu("Apply Curve")]
    public void UpdateCurvedText()
    {
        if (textComponent == null) return;
        
        // Force mesh update to get latest character positions
        textComponent.ForceMeshUpdate(true);
        
        TMP_TextInfo textInfo = textComponent.textInfo;
        
        // Don't process if no characters
        if (textInfo.characterCount == 0) return;
        
        if (useArcMode)
        {
            ApplyArcCurve(textInfo);
        }
        else
        {
            ApplyVerticalCurve(textInfo);
        }
        
        // Update the mesh with new vertex positions
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
            if (meshInfo.mesh == null) continue;
            
            meshInfo.mesh.vertices = meshInfo.vertices;
            textComponent.UpdateGeometry(meshInfo.mesh, i);
        }
    }
    
    private void ApplyVerticalCurve(TMP_TextInfo textInfo)
    {
        float boundsMinX = textComponent.bounds.min.x;
        float boundsMaxX = textComponent.bounds.max.x;
        float textWidth = boundsMaxX - boundsMinX;
        
        if (textWidth <= 0) return;
        
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            
            // Skip invisible characters
            if (!charInfo.isVisible) continue;
            
            int vertexIndex = charInfo.vertexIndex;
            int materialIndex = charInfo.materialReferenceIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;
            
            // Calculate character's horizontal position in text (0 to 1)
            Vector3 charMidpoint = (vertices[vertexIndex + 1] + vertices[vertexIndex + 2]) / 2f;
            float normalizedX = (charMidpoint.x - boundsMinX) / textWidth;
            normalizedX = Mathf.Clamp01(normalizedX);
            
            // Get curve value at this position
            float curveValue = curveShape.Evaluate(normalizedX) * curveStrength;
            
            // Apply vertical offset to all 4 vertices of the character
            for (int j = 0; j < 4; j++)
            {
                vertices[vertexIndex + j].y += curveValue;
            }
        }
    }
    
    private void ApplyArcCurve(TMP_TextInfo textInfo)
    {
        float totalAngle = arcDegrees * Mathf.Deg2Rad;
        float startAngle = -totalAngle / 2f;
        
        // Calculate angle step based on text bounds
        float boundsMinX = textComponent.bounds.min.x;
        float boundsMaxX = textComponent.bounds.max.x;
        float textWidth = boundsMaxX - boundsMinX;
        
        if (textWidth <= 0) return;
        
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            
            if (!charInfo.isVisible) continue;
            
            int vertexIndex = charInfo.vertexIndex;
            int materialIndex = charInfo.materialReferenceIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;
            
            // Get character center position
            Vector3 charCenter = Vector3.zero;
            for (int j = 0; j < 4; j++)
            {
                charCenter += vertices[vertexIndex + j];
            }
            charCenter /= 4f;
            
            // Calculate normalized position
            float normalizedX = (charCenter.x - boundsMinX) / textWidth;
            normalizedX = Mathf.Clamp01(normalizedX);
            
            // Calculate angle for this character
            float angle = startAngle + (normalizedX * totalAngle);
            
            // Calculate position on arc
            float x = Mathf.Sin(angle) * arcRadius;
            float y = Mathf.Cos(angle) * arcRadius - arcRadius;
            
            // Calculate rotation
            float rotationAngle = -angle * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, rotationAngle);
            
            // Apply transformation to each vertex
            for (int j = 0; j < 4; j++)
            {
                Vector3 vertex = vertices[vertexIndex + j];
                
                // Center vertex around character center
                vertex -= charCenter;
                
                // Apply rotation
                vertex = rotation * vertex;
                
                // Apply position on arc
                vertex.x += x;
                vertex.y += y;
                
                // Store transformed vertex
                vertices[vertexIndex + j] = vertex + charCenter;
            }
        }
    }
    
    // Public methods for runtime control
    public void SetCurveStrength(float strength)
    {
        curveStrength = strength;
        UpdateCurvedText();
    }
    
    public void SetArcRadius(float radius)
    {
        arcRadius = radius;
        UpdateCurvedText();
    }
    
    public void SetArcDegrees(float degrees)
    {
        arcDegrees = degrees;
        UpdateCurvedText();
    }
    
    public void ToggleArcMode(bool useArc)
    {
        useArcMode = useArc;
        UpdateCurvedText();
    }
    
    // Reset to default values
    [ContextMenu("Reset Curve")]
    public void ResetCurve()
    {
        curveShape = AnimationCurve.EaseInOut(0, 0, 1, 0);
        curveStrength = 30f;
        arcRadius = 200f;
        arcDegrees = 180f;
        UpdateCurvedText();
    }
    
#if UNITY_EDITOR
    // Update in editor when values change
    void OnValidate()
    {
        if (textComponent != null && Application.isPlaying)
        {
            UpdateCurvedText();
        }
    }
#endif
}