using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("開始時UI関連")]
    [SerializeField] private GameObject _startParent;

    void Start()
    {
        Initialize();
    }

    public IEnumerator PlayStartUI()
    {
        _startParent.SetActive(true);
        var startLabel = _startParent.GetComponentInChildren<StartLabelUI>();
        yield return StartCoroutine(startLabel.Play());     // 開始アニメーションを再生
        yield return new WaitForSeconds(0.25f);             // 少しだけ背景を残す
        _startParent.SetActive(false);
    }

    private void Initialize()
    {
        _startParent.SetActive(false);
    }
}
