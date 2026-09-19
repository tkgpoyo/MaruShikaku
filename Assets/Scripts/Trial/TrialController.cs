using UnityEngine;

public class TrialController : MonoBehaviour
{
    #region Destroyテスト関連
    [Header("Destroyテスト関連")]
    [SerializeField]
    private GameObject _parent_destroy_test;
    [SerializeField]
    private GameObject[] _children_destroy_test;
    #endregion (Destroyテスト関連)

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 親オブジェクトがDestroyされた時の子オブジェクトの挙動を確認するためのテストイベントハンドラ
    /// </summary>
    public void OnDestroyButton()
    {
        Destroy(_parent_destroy_test);
    }
}
