using MaruSikaku.Editor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace MaruSikaku.Editor.Custom
{
    public class StageGridView : VisualElement
    {
        private const float CELL_DEFAULT_PIXEL = 20f;
        private static readonly Color BACKGROUND_COLOR = Color.blue;
        private static readonly Color GRID_COLOR = Color.black;
        private static readonly Color HOVER_COLOR = new(1, 1, 1, 0.4f);

        private StageDisplayData _data;
        private StageEditContext _editContext;
        private float _cellPixel => CELL_DEFAULT_PIXEL * _editContext?.ZoomRate ?? 1f;

        public StageGridView(StageDisplayData stageData, StageEditContext editContext)
        {
            _data = stageData ?? throw new ArgumentNullException(nameof(stageData));
            _editContext = editContext ?? throw new ArgumentNullException(nameof(editContext));
            AddToClassList("stage-grid-view");

            _data.propertyChanged += OnStageDataChanged;

            UpdateView();

            RegisterCallback<ClickEvent>(OnMouseClick);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);

            generateVisualContent += OnGenerateVisualContent;
        }

        private void OnStageDataChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            switch (e.propertyName) {
                case nameof(StageDisplayData.SizeX):
                case nameof(StageDisplayData.SizeY):
                    UpdateView();
                    break;
            }
        }

        private void OnMouseClick(ClickEvent e)
        {
        }

        private void OnPointerMove(PointerMoveEvent e)
        {
            var cell = PointerToCell(e.localPosition);
            if (!IsInsideStage(cell)) {                         // ステージ外にマウスがある場合
                _editContext.HoverCell = null;
                return;
            }
            if (_editContext.HoverCell == cell) { return; }     // 以前のhover中のセルと現在のhover中のセルが同じ場合，変更がないためhover中のセルを更新せず抜ける

            _editContext.HoverCell = cell;                      // hover中のセルに設定
            MarkDirtyRepaint();                                 // 再描画
        }

        private void OnPointerLeave(PointerLeaveEvent e)
        {
            _editContext.HoverCell = null;
            MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext context)
        {
            var painter = context.painter2D;

            DrawBackground(painter);
            DrawGrid(painter);
            DrawHoverCell(painter);
        }

        private void UpdateView()
        {
            style.width = _data.SizeX * _cellPixel;
            style.height = _data.SizeY * _cellPixel;

            MarkDirtyRepaint();
        }

        private void DrawBackground(Painter2D painter) 
        {
            painter.fillColor = BACKGROUND_COLOR;

            painter.BeginPath();
            painter.Rect(contentRect);
            painter.ClosePath();

            painter.Fill();
        }

        private void DrawGrid(Painter2D painter)
        {
            painter.BeginPath();
            painter.strokeColor = GRID_COLOR;

            // 外枠をなぞる
            painter.Rect(contentRect);              // Gridの外枠をなぞる

            // Gridを描画
            for (int x = 1; x < _data.SizeX; x++) {
                var px = x * _cellPixel;
                painter.MoveTo(new(px, contentRect.yMin));
                painter.LineTo(new(px, contentRect.yMax));
            }
            for (int y = 1; y < _data.SizeY; y++) {
                var py = y * _cellPixel;
                painter.MoveTo(new(contentRect.xMin, py));
                painter.LineTo(new(contentRect.xMax, py));
            }

            painter.Stroke();                       // Gridの外枠とGrid線を同時に描画
        }

        private void DrawHoverCell(Painter2D painter)
        {
            if (_editContext.HoverCell == null) { return; }

            painter.fillColor = HOVER_COLOR;
            painter.BeginPath();

            var hoverCell = (Vector2Int)_editContext.HoverCell;     // マウス移動イベントで上書きされる恐れがあるため，マウスが乗っているセルの場所を保存
            var hoverRect = new Rect() { 
                xMin = hoverCell.x * _cellPixel, xMax = (hoverCell.x + 1) * _cellPixel,
                yMin = hoverCell.y * _cellPixel, yMax = (hoverCell.y + 1) * _cellPixel
            };                                                      // マウスが乗っているセルのRect
            painter.Rect(hoverRect);

            painter.Fill();
        }

        private Vector2Int PointerToCell(Vector2 position)
        {
            var x = Mathf.FloorToInt(position.x / _cellPixel);
            var y = Mathf.FloorToInt(position.y / _cellPixel);
            return new(x, y);
        }

        /// <summary>
        /// セルがステージの中にあるかどうかを返します．
        /// </summary>
        /// <param name="cell">セル</param>
        private bool IsInsideStage(Vector2Int cell)
        {
            return _data != null &&
                0 <= cell.x && cell.x < _data.SizeX && 
                0 <= cell.y && cell.y < _data.SizeY;
        }
    }

    /// <summary>
    /// <see cref="Painter2D"/>の拡張メソッド用クラスです．
    /// </summary>
    public static class Painter2DExtension
    {
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
