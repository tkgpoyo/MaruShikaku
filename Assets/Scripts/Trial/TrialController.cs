using System.Collections.Generic;
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
        TestHashSetRemove();
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

    /// <summary>
    /// <see cref="HashSet{T}.Remove(T)"/>の<see cref="HashSet{T}.Remove(T)"/>のテスト
    /// </summary>
    private void TestHashSetRemove()
    {
        // HashSetにないオブジェクトをRemoveしたときに例外が投げられる？（投げられなさそうではあるが）
        //↑例外が投げられない
        var set = new HashSet<GameObject>();
        var exist1 = new GameObject();
        var exist2 = new GameObject();
        var exist3 = new GameObject();
        set.Add(exist1);
        set.Add(exist2);
        set.Add(exist3);

        var nonexist1 = new GameObject();

        set.Remove(exist1);
        set.Remove(nonexist1);
        set.Remove(exist2);
        set.Remove(exist1);
    }
}
