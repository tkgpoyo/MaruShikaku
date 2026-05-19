using UnityEngine;
using MaruSikaku.Gameplay.Players.Inputs;
using MaruSikaku.Gameplay.Players.Visuals;
using UnityEngine.InputSystem;
using System.Collections;

namespace MaruSikaku.Gameplay.Players.Abilities
{
    /// <summary>
    /// 能力の実行段階を表す列挙型
    /// </summary>
    public enum EAbilityPhase
    {
        /// <summary>能力未実行</summary>
        None,
        /// <summary>能力の開始動作段階</summary>
        Anticipation,
        /// <summary>能力のアクション段階</summary>
        Action,
        /// <summary>能力の終了動作段階</summary>
        Recovery
    }

    /// <summary>
    /// プレイヤーの能力を表すクラス
    /// </summary>
    public abstract class PlayerAbility : MonoBehaviour
    {
        /// <summary>実行の優先度</summary>
        public int Order => _order;
        [SerializeField] private int _order = 0;

        /// <summary>プレイヤーの状態</summary>
        protected PlayerContext Context { get; private set; }
        /// <summary>プレイヤーの見た目制御クラス</summary>
        protected PlayerVisual Visual { get; private set; }
        /// <summary>能力の実行段階</summary>
        public EAbilityPhase Phase { get; private set; } = EAbilityPhase.None;
        /// <summary>実行中かどうか</summary>
        public bool IsRunning => Phase is not EAbilityPhase.None;

        /// <summary>能力の実行フロー</summary>
        private Coroutine _flow;

        /// <summary>
        /// 能力の初期化を行います。
        /// </summary>
        /// <remarks>
        /// ゲーム開始時に必ずこのメソッドを呼んでください。
        /// </remarks>
        /// <param name="context">プレイヤー状態</param>
        /// <param name="visual">プレイヤーの見た目制御クラス</param>
        public virtual void Initialize(PlayerContext context, PlayerVisual visual)
        {
            Context = context;
            Visual = visual;
        }

        /// <summary>
        /// <see cref="MonoBehaviour.Update"/> のタイミングで実行されます。
        /// </summary>
        /// <param name="input">入力</param>
        public virtual void Tick(PlayerInputData input)
        {
            if (IsRunning)                      // 能力実行中ならば
            {
                OnTickDuringAbility(input);     // 入力を渡す
            }
        }

        /// <summary>
        /// <see cref="MonoBehaviour.FixedUpdate"/>  のタイミングで実行されます。
        /// </summary>
        /// <remarks>
        /// 能力の開始条件は物理演算による判定が考えられるため、このメソッド内で能力の実行可否を決めています。
        /// </remarks>
        /// <param name="input">入力</param>
        public virtual void FixedTick(PlayerInputData input)
        {
            if (IsRunning)                                      // 能力実行中ならば
            {
                OnFixedTickDuringAbility(input);                // 入力を渡す
            }
            else if (CanStart(input))                           // 能力が実行可能であるならば
            {
                _flow = StartCoroutine(AbilityFlow(input));     // 能力の実行を開始
            }
        }

        /// <summary>
        /// 能力の実行の流れを処理します。
        /// </summary>
        /// <param name="input">プレイヤー入力</param>
        /// <returns></returns>
        private IEnumerator AbilityFlow(PlayerInputData input)
        {
            // 能力前の開始動作実行段階
            Phase = EAbilityPhase.Anticipation;
            OnAnticipationStart(input);
            yield return Anticipation(input);       // 能力の開始動作

            // 能力のアクション段階
            Phase = EAbilityPhase.Action;
            OnActionStart(input);
            yield return Action(input);             // 能力のアクション

            // 能力の終了動作実行段階
            Phase = EAbilityPhase.Recovery;
            OnRecoveryStart(input);
            yield return Recovery(input);           // 能力の終了動作

            Finish();
        }

        /// <summary>
        /// 能力の実行フローが終了する時の処理です。
        /// </summary>
        private void Finish()
        {
            Phase = EAbilityPhase.None;             // 能力未実行にする
            _flow = null;                           // フローをnullにする
            OnFinished();
        }

        /// <summary>
        /// 能力の実行フローが途中でキャンセルされた時の処理です。
        /// </summary>
        protected void Cancel()
        {
            if (_flow != null)                      // 能力の実行フローが続いている場合
            {
                StopCoroutine(_flow);               // 続いているフローを停止
                _flow = null;                       // フローをnullにする
            }
            Phase = EAbilityPhase.None;             // 能力未実行にする
            OnCanceled();
        }

        /// <summary>
        /// 能力が実行中の際の入力処理です。
        /// <see cref="MonoBehaviour.Update"/> に同期します。
        /// </summary>
        /// <param name="input">プレイヤー入力</param>
        protected virtual void OnTickDuringAbility(PlayerInputData input) {}
        /// <summary>
        /// 能力が実行中の際の入力処理です。
        /// <see cref="MonoBehaviour.FixedUpdate"/> に同期します。
        /// </summary>
        /// <param name="input">プレイヤー入力</param>
        protected virtual void OnFixedTickDuringAbility(PlayerInputData input) {}

        /// <summary>
        /// 能力を実行開始できるかどうかを返します。
        /// </summary>
        /// <param name="input">プレイヤー入力</param>
        /// <returns>能力の実行可否</returns>
        protected abstract bool CanStart(PlayerInputData input);
        /// <summary>
        /// 能力の開始動作段階における初期化処理を行います。
        /// </summary>
        /// <param name="input">プレイヤー入力</param>
        protected virtual void OnAnticipationStart(PlayerInputData input) {}
        /// <summary>
        /// 能力の開始動作段階における処理を行います。
        /// </summary>
        /// <param name="input">プレイヤー入力</param>
        /// <returns></returns>
        protected virtual IEnumerator Anticipation(PlayerInputData input) { yield break; }
        /// <summary>
        /// 能力のアクション段階における初期化処理を行います。
        /// </summary>
        /// <param name="input">プレイヤー入力</param>
        protected virtual void OnActionStart(PlayerInputData input) {}
        /// <summary>
        /// 能力のアクション段階における処理を行います。
        /// </summary>
        /// <param name="input">プレイヤー入力</param>
        protected virtual IEnumerator Action(PlayerInputData input) { yield break; }
        /// <summary>
        /// 能力の終了動作段階における初期化処理を行います。
        /// </summary>
        /// <param name="input">プレイヤー入力</param>
        protected virtual void OnRecoveryStart(PlayerInputData input) {}
        /// <summary>
        /// 能力の終了動作段階における処理を行います。
        /// </summary>
        /// <param name="input">プレイヤー入力</param>
        protected virtual IEnumerator Recovery(PlayerInputData input) { yield break; }
        /// <summary>
        /// 能力の実行終了時の処理を行います。
        /// </summary>
        protected virtual void OnFinished() {}
        /// <summary>
        /// 能力の実行キャンセル時の処理を行います。
        /// </summary>
        protected virtual void OnCanceled() {}
    }
}