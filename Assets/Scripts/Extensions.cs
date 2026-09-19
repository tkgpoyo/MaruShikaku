using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace MaruSikaku
{
    /// <summary>
    /// 拡張メソッド用クラス
    /// </summary>
    public static class MaruSikakuExtensions
    {
        /// <summary>
        /// <see cref="Vector2"/>から<see cref="Vector2Int"/>への変換メソッドです．
        /// </summary>
        /// <param name="vector2"><see cref="Vector2"/>型のインスタンス</param>
        /// <returns><see cref="Vector2Int"/>にしたインスタンス</returns>
        public static Vector2Int ToVec2Int(this Vector2 vector2)
        {
            return new Vector2Int((int)vector2.x, (int)vector2.y);
        }
        /// <summary>
        /// <see cref="Vector2Int"/>から<see cref="Vector2"/>への変換メソッドです．
        /// </summary>
        /// <param name="vec2Int"><see cref="Vector2Int"/>型のインスタンス</param>
        /// <returns><see cref="Vector2"/>にしたインスタンス</returns>
        /// <remarks>暗黙的なキャストを行うだけ</remarks>
        public static Vector2 ToVec2(this Vector2Int vec2Int)
        {
            return vec2Int;
        }

        /// <summary>
        /// 長方形を描画します．
        /// </summary>
        /// <param name="painter"></param>
        /// <param name="rect">長方形</param>
        public static void Rect(this Painter2D painter, Rect rect)
        {
            var rectVertexes = new Vector2[4] { 
                new (rect.xMin, rect.yMin),
                new (rect.xMin, rect.yMax),
                new (rect.xMax, rect.yMax),
                new (rect.xMax, rect.yMin),
            };                                      // 長方形の各頂点の座標
            painter.MoveTo(rectVertexes.Last());    // 長方形の右上の頂点に移動
            foreach (var v in rectVertexes) {
                painter.LineTo(v);                  // 左上→左下→右下→右上 と移動していき，長方形を描画
            }
        }
    }
}