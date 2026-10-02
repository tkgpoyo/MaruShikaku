using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private TMP_Text _startLabel;

    private Texture2D _startLabelHideTex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _startLabelHideTex = new Texture2D(100, 100);
        for (int y = 0; y < 100; y++)
        {
            for (int x = 0; x < 100; x++)
            {
                Color c = Color.white;
                if (x > 50)
                {
                    c.a = 0;
                }
                _startLabelHideTex.SetPixel(x, y, c);
            }
        }
        var rect = GetTextRect(_startLabel);
        _startLabelHideTex.Apply();
        _startLabel.fontMaterial.SetTexture("_HideTex", _startLabelHideTex);
        _startLabel.fontMaterial.SetVector("_TextRect", new Vector4(rect.xMin, rect.yMin, rect.width, rect.height));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Rect GetTextRect(TMP_Text text)
    {
        text.ForceMeshUpdate();

        if (text.textInfo.characterCount <= 0) { return Rect.zero; }

        var bottomLeft = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        var topRight = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        var found = false;

        foreach (var charInfo in text.textInfo.characterInfo)
        {
            if (!charInfo.isVisible) { continue; }

            bottomLeft = Vector2.Min(bottomLeft, charInfo.bottomLeft);
            topRight = Vector2.Max(topRight, charInfo.topRight);
            found = true;
        }

        return found ? 
                Rect.MinMaxRect(bottomLeft.x, bottomLeft.y, topRight.x, topRight.y) :
                Rect.zero;
    }
}
