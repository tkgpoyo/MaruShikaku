using System.Collections;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

public class StartLabelUI : TextMeshProUGUI
{
    private const int HIDE_TEX_WIDTH = 200;
    private const int HIDE_TEX_HEIGHT = 200;
    private static Texture2D DEFAULT_HIDE_TEXTURE => DefaultHideTexture();

    [SerializeField, Range(1, 3)] private float _pathTime = 2f;
    [SerializeField] private Vector2[] _path;
    [SerializeField, Range(0, 2)] private float _breakTime = 1f;
    [SerializeField, Range(0.05f, 0.5f)] private float _radius = 0.1f;

    private Texture2D _hideTex;
    private bool _isAnimating = false;

    protected override void Start()
    {
        base.Start();

        ForceMeshUpdate();
        SetDefaultHideTexture();
        var bounds = GetTextBounds(true);
        fontMaterial.SetTexture("_HideTex", _hideTex);
        fontMaterial.SetVector("_TextRect", new Vector4(bounds.min.x, bounds.min.y, bounds.size.x, bounds.size.y));
    }

    public IEnumerator Play()
    {
        if (_path.Length <= 1) { yield break; }
        if (_isAnimating) { yield break; }
        _isAnimating = true;

        SetDefaultHideTexture();

        yield return PaintPath(false);
        yield return new WaitForSeconds(_breakTime);
        yield return PaintPath(true);

        SetDefaultHideTexture();

        _isAnimating = false;

        IEnumerator PaintPath(bool erase)
        {
            var path = new Vector2[_path.Length];
            for (int i = 0; i < path.Length; i++)
            {
                path[i] = new Vector2(_path[i].x * HIDE_TEX_WIDTH, _path[i].y * HIDE_TEX_HEIGHT);
            }

            var totalDistance = 0f;
            var distances = new float[path.Length - 1];
            for (int i = 0; i < path.Length - 1; i++)
            {
                var distance = Vector2.Distance(path[i], path[i+1]);
                totalDistance += distance;
                distances[i] = distance;
            }

            var times = new float[path.Length - 1];
            var secPerDistance = _pathTime / Mathf.Max(totalDistance, 0.001f);
            for (int i = 0; i < times.Length; i++)
            {
                times[i] = distances[i] * secPerDistance;
            }

            var elapsed = 0f;
            var elapsedInPath = 0f;
            var idx = 0;
            var lastIdx = path.Length - 2;
            var radius = _radius * Mathf.Min(HIDE_TEX_WIDTH, HIDE_TEX_HEIGHT);
            while (elapsed < _pathTime)
            {
                if (idx < lastIdx && times[idx] <= elapsedInPath)
                {
                    elapsedInPath -= times[idx];
                    idx++;
                }
                var position = Vector2.Lerp(path[idx], path[idx + 1], elapsedInPath / Mathf.Max(times[idx], 0.001f));
                Paint(position, radius, erase);
                yield return null;
                elapsed += Time.deltaTime;
                elapsedInPath += Time.deltaTime;
            }
            Paint(path.Last(), radius, erase);
        }
    }

    private void Paint(Vector2 center, float radius, bool erase)
    {
        var colors = _hideTex.GetPixels32();
        var color32 = new Color32(255, 255, 255, erase ? (byte)0 : (byte)255);
        for (int y = Mathf.Max(0, Mathf.FloorToInt(center.y - radius)); y < Mathf.Min(HIDE_TEX_HEIGHT, Mathf.FloorToInt(center.y + radius)); y++)
        {
            for (int x = Mathf.Max(0, Mathf.FloorToInt(center.x - radius)); x < Mathf.Min(HIDE_TEX_WIDTH, Mathf.FloorToInt(center.x + radius)); x++)
            {
                if (Vector2.Distance(new (x, y), center) > radius) { continue; }
                colors[y * HIDE_TEX_WIDTH + x] = color32;
            }
        }
        _hideTex.SetPixels32(colors);
        _hideTex.Apply();
    }

    private void SetDefaultHideTexture()
    {
        if (_hideTex == null)
        {
            _hideTex = new Texture2D(HIDE_TEX_WIDTH, HIDE_TEX_HEIGHT);
        }
        _hideTex.CopyPixels(DEFAULT_HIDE_TEXTURE);
    }

    private static Texture2D DefaultHideTexture()
    {
        var tex = new Texture2D(HIDE_TEX_WIDTH, HIDE_TEX_HEIGHT);
        var colorData = Enumerable.Repeat(new byte[] { 255, 255, 255, 0 }, HIDE_TEX_HEIGHT * HIDE_TEX_WIDTH).SelectMany(b => b).ToArray();
        tex.SetPixelData(colorData, 0);
        tex.Apply();
        return tex;
    }
}
