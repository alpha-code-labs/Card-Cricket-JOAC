using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BARGradientManger : MonoBehaviour
{
    [SerializeField] Image BAR;
    [SerializeField] Image MARKER;
    [SerializeField] private Texture2D gradientTexture;
    [SerializeField] public List<Color> mColorList;
    [ContextMenu("Fill Gradient")]
    public void UpdateGradient()
    {
        int width = 1;
        float height = mColorList.Count;
        gradientTexture = new Texture2D(width, (int)height);
        gradientTexture.filterMode = FilterMode.Trilinear;
        BAR.sprite = Sprite.Create(gradientTexture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));

        for (int y = 0; y < height; y++)
        {
            Color color = mColorList[y];
            for (int x = 0; x < width; x++)
            {
                gradientTexture.SetPixel(x, y, color);
            }
        }
        gradientTexture.Apply();
    }
}
