using System.Collections.Generic;
using System.Linq;
using MaruSikaku.Gameplay.Players;
using UnityEngine;

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

        /// <summary>Animator</summary>
        private Animator _anim;
        /// <summary>乗っているプレイヤー一覧</summary>
        private HashSet<PlayerController> _pressingPlayers = new();

        void Awake()
        {
            _anim = GetComponentInChildren<Animator>();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<PlayerController>(out var player)) { return; }   // プレイヤー以外との接触は無視
            if (!_pressingPlayers.Any())                // 他に誰もスイッチに乗っていない場合
            {
                _anim.SetBool(PARAM_PRESS, true);       // スイッチを起動
            }
            _pressingPlayers.Add(player);               // 乗っているプレイヤー一覧に追加
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (!other.TryGetComponent<PlayerController>(out var player)) { return; }   // プレイヤー以外との接触は無視
            if (_pressingPlayers.Contains(player))      // 乗っているプレイヤーだった場合（常にtrueと考えられるが）
            {
                _pressingPlayers.Remove(player);        // 乗っているプレイヤー一覧から削除
            }
            if (!_pressingPlayers.Any())                // スイッチ上に誰もいなくなった場合
            {
                _anim.SetBool(PARAM_PRESS, false);      // スイッチをOff
            }
        }
    }
}