using System.Collections.Generic;
using System.Linq;
using MaruSikaku.Gameplay.Players;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace MaruSikaku.Gameplay.Stages.Gimmicks
{
    /// <summary>
    /// スイッチを表すクラス
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PressureSwitch : MonoBehaviour
    {
        /// <summary>プレスアニメーション時のパラメータ</summary>
        private const string PARAM_PRESS = "Press";

        /// <summary>開閉可能な壁</summary>
        [SerializeField, ReadOnly] private List<OpenableWall> _openableWalls;
        /// <summary>
        /// 判定除外するレイヤー
        /// </summary>
        [SerializeField] private LayerMask _excludeLayer;

        /// <summary>Animator</summary>
        private Animator _anim;
        ///// <summary>乗っているプレイヤー一覧</summary>
        //private HashSet<PlayerController> _pressingPlayers = new();
        /// <summary>乗っているオブジェクト一覧</summary>
        private HashSet<GameObject> _pressingObjects = new();

        void Awake()
        {
            _anim = GetComponentInChildren<Animator>();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (IsExcludeCollider(other)) { return; }   // 除外する接触なら処理しない
            if (!_pressingObjects.Any())                // 他に誰もスイッチに乗っていない場合
            {
                _anim.SetBool(PARAM_PRESS, true);       // スイッチを起動

                foreach (var wall in _openableWalls)    // 開閉可能な壁全てに対して
                {
                    wall.Open();                        // 壁を開ける
                }
            }
            _pressingObjects.Add(other.gameObject);     // 乗っているオブジェクト一覧に追加
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (IsExcludeCollider(other)) { return; }   // 除外する接触なら処理しない

            //↓Trial Scene の HashSetテストで，別に要素がなくても例外は投げられないことを確認
            //if (_pressingObjects.Contains(other.gameObject))    // 乗っているオブジェクトだった場合（常にtrueと考えられるが）
            //{
                //_pressingObjects.Remove(other.gameObject);      // 乗っているオブジェクト一覧から削除
            //}
            _pressingObjects.Remove(other.gameObject);  // 乗っているオブジェクト一覧から削除

            if (!_pressingObjects.Any())                // スイッチ上に何も無くなった場合
            {
                _anim.SetBool(PARAM_PRESS, false);      // スイッチをOff

                foreach (var wall in _openableWalls)    // 開閉可能な壁全てに対して
                {
                    wall.Close();                       // 壁を閉じる
                }
            }
        }

        /// <summary>
        /// 壁オブジェクトを登録します．
        /// </summary>
        /// <param name="wall"></param>
        public void RegisterWall(OpenableWall wall)
        {
            Assert.IsNotNull<OpenableWall>(wall);
            _openableWalls.Add(wall);
        }

        /// <summary>
        /// 除外する接触かどうかを判定します．
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        private bool IsExcludeCollider(Collider2D other)
        {
            return other == null ||                                         // 相手がnullか
                   other.gameObject == null ||                              // 相手のgameObjectがnullか
                   ((1 << other.gameObject.layer) & _excludeLayer) != 0;    // 除外レイヤーに属するなら，接触は除外
        }
    }
}