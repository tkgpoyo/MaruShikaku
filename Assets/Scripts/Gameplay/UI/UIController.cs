using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private StartLabelUI _startLabelUI;

    private bool _a = false;
    private float _e = 0f;

    void Start()
    {
    }

    void Update()
    {
        _e += Time.deltaTime;
        if (_e > 10)
        {
            _e = 0;
            _a = false;
        }
        if (_a) return;
        _a = true;
        StartCoroutine(_startLabelUI.Play());
    }
}
