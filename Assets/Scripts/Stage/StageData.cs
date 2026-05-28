using UnityEngine;

namespace MaruSikaku.Stage
{
    /// <summary>ステージデータを表すクラス</summary>
    public class StageData
    {
        /// <summary>ステージサイズ</summary>
        public Vector2Int Size { get; set; } = new(10, 10);
        /// <summary>丸キャラクターの初期位置</summary>
        public Vector2Int MaruInitPos { get; set; } = new(2, 0);
        /// <summary>四角キャラクターの初期位置</summary>
        public Vector2Int SikakuInitPos { get; set; } = new(1, 0);
    }
}