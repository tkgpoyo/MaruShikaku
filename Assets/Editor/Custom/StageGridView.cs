using Codice.Client.Common.GameUI;
using MaruSikaku.Editor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using MaruSikaku.Stage;

namespace MaruSikaku.Editor.Custom
{
    public class StageGridView : VisualElement
    {
        private const float CELL_DEFAULT_PIXEL = 20f;
        private static readonly Color BACKGROUND_COLOR = Color.softBlue;
        private static readonly Color GRID_COLOR = Color.black;
        private static readonly Color GRID_SELECTED_COLOR = Color.red;
        private static readonly Color HOVER_COLOR = new(1, 1, 1, 0.35f);

        private float _cellPixel => CELL_DEFAULT_PIXEL * EditContext?.ZoomRate ?? 1f;
        private int _nextId = 0;

        public StageGridView()
        {
            AddToClassList("stage-grid-view");

            UpdateView();

            RegisterCallback<ClickEvent>(OnMouseClick);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);

            generateVisualContent += OnGenerateVisualContent;
        }

        public StageDisplayData Data
        {
            get => _data;
            set
            {
                if (_data == value) { return; }
                if (_data != null)
                {
                    _data.propertyChanged -= OnStageDataChanged;
                }
                _data = value;
                if (_data != null)
                {
                    _data.propertyChanged += OnStageDataChanged;

                    _nextId = _data.StageObjects.Count <= 0 ? 0 : _data.StageObjects.Max(stageObject => stageObject.Id + 1);
                }

                MarkDirtyRepaint();
            }
        }
        private StageDisplayData _data = new();
        
        public StageEditContext EditContext
        {
            get => _editContext;
            set
            {
                if (_editContext == value) { return; }
                if (_editContext != null)
                {
                    _editContext.propertyChanged -= OnEditContextChanged;
                }
                _editContext = value;
                if (_editContext != null)
                {
                    _editContext.propertyChanged += OnEditContextChanged;
                }

                MarkDirtyRepaint();
            }
        }
        private StageEditContext _editContext = new();

        private void OnStageDataChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            switch (e.propertyName) {
                case nameof(StageDisplayData.SizeX):
                case nameof(StageDisplayData.SizeY):
                    UpdateView();
                    MarkDirtyRepaint();
                    break;
                case nameof(StageDisplayData.TerrainCells):
                case nameof(StageDisplayData.StageObjects):
                    MarkDirtyRepaint();
                    break;
            }
        }

        private void OnEditContextChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            switch (e.propertyName)
            {
                case nameof(StageEditContext.HoverCell):
                case nameof(StageEditContext.SelectedCell):
                    MarkDirtyRepaint();
                    break;
            }
        }

        private void OnMouseClick(ClickEvent e)
        {
            if (EditContext.HoverCell == null) { return; }
            var hoverCell = (Vector2Int)EditContext.HoverCell;

            switch (EditContext.EditMode)
            {
                case EStageEditMode.Select:
                    EditContext.SelectedCell = hoverCell;
                    break;
                case EStageEditMode.Erase:
                    if (Data.TerrainDic.ContainsKey(hoverCell))
                    {
                        Data.RemoveTerrainCell(Data.TerrainDic[hoverCell]);
                    }
                    if (Data.StageObjectDic.ContainsKey(hoverCell))
                    {
                        Data.RemoveStageObject(Data.StageObjectDic[hoverCell]);
                    }
                    break;
                case EStageEditMode.Ground:
                    if (Data.TerrainDic.ContainsKey(hoverCell) || Data.StageObjectDic.ContainsKey(hoverCell)) { return; }
                    var ground = new StageTerrainCell(hoverCell, ETerrainType.Ground);
                    Data.AddTerrainCell(ground);
                    break;
                case EStageEditMode.Spring:
                case EStageEditMode.Fragile:
                case EStageEditMode.Movable:
                case EStageEditMode.Switch:
                case EStageEditMode.Wall:
                    if (Data.TerrainDic.ContainsKey(hoverCell) || Data.StageObjectDic.ContainsKey(hoverCell)) { return; }
                    var stageObject = InstantiateStageObject(hoverCell, EditContext.EditMode);
                    Data.AddStageObject(stageObject);
                    EditContext.SelectedCell = hoverCell;
                    break;
                case EStageEditMode.MaruStart:
                case EStageEditMode.SikakuStart:
                    break;
            }

            StageObject InstantiateStageObject(Vector2Int pos, EStageEditMode mode)
            {
                return mode switch
                {
                    EStageEditMode.Spring => new SpringObject(GetNextId(), pos),
                    EStageEditMode.Fragile => new FragileObject(GetNextId(), pos),
                    EStageEditMode.Movable => new MovableObject(GetNextId(), pos),
                    EStageEditMode.Switch => new SwitchObject(GetNextId(), pos),
                    EStageEditMode.Wall => new WallObject(GetNextId(), pos),
                    _ => throw new NotImplementedException($"{mode}は未対応です．")
                };
            }
        }

        private void OnPointerMove(PointerMoveEvent e)
        {
            var cell = PointerToCell(e.localPosition);
            if (!IsInsideStage(cell)) {                         // ステージ外にマウスがある場合
                EditContext.HoverCell = null;
                return;
            }
            if (EditContext.HoverCell == cell) { return; }     // 以前のhover中のセルと現在のhover中のセルが同じ場合，変更がないためhover中のセルを更新せず抜ける

            EditContext.HoverCell = cell;                      // hover中のセルに設定
        }

        private void OnPointerLeave(PointerLeaveEvent e)
        {
            EditContext.HoverCell = null;
        }

        private void OnGenerateVisualContent(MeshGenerationContext context)
        {
            var painter = context.painter2D;

            DrawBackground(painter);
            DrawGrid(painter);
            DrawHoverCell(painter);
            DrawTerrainCells(painter);
            DrawStageObjects(painter);
            DrawSelectedCell(painter);
        }

        private void UpdateView()
        {
            style.width = Data.SizeX * _cellPixel;
            style.height = Data.SizeY * _cellPixel;
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
            for (int x = 1; x < Data.SizeX; x++) {
                var px = x * _cellPixel;
                painter.MoveTo(new(px, contentRect.yMin));
                painter.LineTo(new(px, contentRect.yMax));
            }
            for (int y = 1; y < Data.SizeY; y++) {
                var py = y * _cellPixel;
                painter.MoveTo(new(contentRect.xMin, py));
                painter.LineTo(new(contentRect.xMax, py));
            }

            painter.Stroke();                       // Gridの外枠とGrid線を同時に描画
        }

        private void DrawHoverCell(Painter2D painter)
        {
            if (EditContext.HoverCell == null) { return; }

            painter.fillColor = HOVER_COLOR;
            painter.BeginPath();

            var hoverCell = (Vector2Int)EditContext.HoverCell;     // マウス移動イベントで上書きされる恐れがあるため，マウスが乗っているセルの場所を保存
            var hoverRect = new Rect() { 
                xMin = hoverCell.x * _cellPixel, xMax = (hoverCell.x + 1) * _cellPixel,
                yMin = hoverCell.y * _cellPixel, yMax = (hoverCell.y + 1) * _cellPixel
            };                                                      // マウスが乗っているセルのRect
            painter.Rect(hoverRect);

            painter.Fill();
        }

        private void DrawTerrainCells(Painter2D painter)
        {
            foreach (var terrainCell in Data.TerrainCells)
            {
                if (!IsInsideStage(terrainCell.Pos)) { continue; }
                DrawTerrainCell(painter, terrainCell);
            }

            void DrawTerrainCell(Painter2D painter, StageTerrainCell terrainCell)
            {
                painter.BeginPath();
                switch (terrainCell.Type)
                {
                    case ETerrainType.Ground:
                        painter.fillColor = Color.black;
                        break;
                }
                painter.Rect(CellToRect(terrainCell.Pos));
                painter.Fill();
            }
        }

        private void DrawStageObjects(Painter2D painter)
        {
            foreach (var stageObject in Data.StageObjects)
            {
                if (!IsInsideStage(stageObject.Pos)) { continue; }
                DrawStageObject(painter, stageObject);
            }

            void DrawStageObject(Painter2D painter, StageObject stageObject)
            {
                var rect = CellToRect(stageObject.Pos);
                switch (stageObject.Type)
                {
                    case EStageObjectType.Fragile:
                        painter.BeginPath();
                        // ブロックの四角部分を描画
                        painter.fillColor = Color.gray;
                        painter.strokeColor = Color.black;
                        painter.Rect(rect);
                        painter.Fill();
                        painter.Stroke();
                        painter.ClosePath();
                        // ブロックのヒビ部分を描画
                        painter.BeginPath();
                        painter.fillColor = Color.black;
                        painter.MoveTo(CellToPixel(stageObject.Pos, x: 0.4f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.3f, y: 0.5f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.6f));
                        painter.Fill();
                        painter.ClosePath();
                        break;
                    case EStageObjectType.Movable:
                        painter.BeginPath();
                        painter.fillColor = Color.darkGreen;
                        painter.strokeColor = Color.black;
                        painter.Rect(CellToRect(stageObject.Pos));
                        painter.Fill();
                        painter.Stroke();
                        break;
                    case EStageObjectType.Spring:
                        // バネの土台部分
                        var springBase1TopY = rect.yMin;
                        var springBase1BottomY = CellToPixel(stageObject.Pos, y: 0.2f).y;
                        var springBase2TopY = CellToPixel(stageObject.Pos, y: 0.8f).y;
                        var springBase2BottomY = rect.yMax;
                        painter.BeginPath();
                        painter.strokeColor = Color.black;
                        painter.fillColor = Color.Lerp(Color.gray, Color.black, 0.6f);
                        painter.Rect(new Rect()
                        {
                            xMin = rect.xMin,
                            xMax = rect.xMax,
                            yMin = springBase1TopY,
                            yMax = springBase1BottomY
                        });
                        painter.Rect(new Rect()
                        {
                            xMin = rect.xMin,
                            xMax = rect.xMax,
                            yMin = springBase2TopY,
                            yMax = springBase2BottomY
                        });
                        painter.Fill();
                        painter.Stroke();
                        painter.ClosePath();
                        // バネの縮む部分
                        painter.BeginPath();
                        painter.fillColor = Color.black;
                        painter.strokeColor = Color.black;
                        painter.MoveTo(CellToPixel(stageObject.Pos, x: 0.6f, y: 0.2f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.7f, y: 0.35f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.6f, y: 0.5f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.7f, y: 0.65f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.6f, y: 0.8f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.7f, y: 0.8f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.8f, y: 0.65f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.7f, y: 0.5f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.8f, y: 0.35f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.7f, y: 0.2f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.6f, y: 0.2f));

                        painter.MoveTo(CellToPixel(stageObject.Pos, x: 0.4f, y: 0.2f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.3f, y: 0.35f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.4f, y: 0.5f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.3f, y: 0.65f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.4f, y: 0.8f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.3f, y: 0.8f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.2f, y: 0.65f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.3f, y: 0.5f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.2f, y: 0.35f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.3f, y: 0.2f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.4f, y: 0.2f));

                        painter.Fill();
                        painter.Stroke();
                        break;
                    case EStageObjectType.Switch:
                        // 押す部分
                        painter.BeginPath();
                        painter.fillColor = Color.yellow;
                        painter.strokeColor = Color.black;
                        painter.MoveTo(CellToPixel(stageObject.Pos, x: 0.2f, y: 0.8f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.3f, y: 0.5f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.7f, y: 0.5f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.8f, y: 0.8f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.2f, y: 0.8f));
                        painter.Fill();
                        painter.Stroke();
                        painter.ClosePath();
                        // 土台部分
                        painter.BeginPath();
                        painter.fillColor = Color.Lerp(Color.gray, Color.black, 0.6f);
                        painter.strokeColor = Color.black;
                        var switchBaseTopY = CellToPixel(stageObject.Pos, y: 0.8f).y;
                        var switchBaseBottomY = rect.yMax;
                        painter.Rect(new()
                        {
                            xMin = rect.xMin,
                            xMax = rect.xMax,
                            yMin = switchBaseTopY,
                            yMax = switchBaseBottomY
                        });
                        painter.Fill();
                        painter.Stroke();
                        painter.ClosePath();
                        break;
                    case EStageObjectType.Wall:
                        painter.BeginPath();
                        painter.fillColor = Color.yellow;
                        painter.strokeColor = Color.black;
                        painter.MoveTo(CellToPixel(stageObject.Pos));
                        painter.LineTo(CellToPixel(stageObject.Pos, y: 0.5f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.3f, y: 0.5f));
                        painter.LineTo(CellToPixel(stageObject.Pos, x: 0.3f));
                        painter.LineTo(CellToPixel(stageObject.Pos));
                        painter.Fill();
                        painter.Stroke();
                        painter.ClosePath();
                        break;
                }
            }
        }

        private void DrawSelectedCell(Painter2D painter)
        {
            // 選択されているセルを赤い枠で描画
            if ( EditContext.SelectedCell != null)
            {
                painter.BeginPath();
                painter.strokeColor = GRID_SELECTED_COLOR;
                painter.lineWidth = 2;
                painter.Rect(CellToRect((Vector2Int)EditContext.SelectedCell));

                painter.Stroke();
            }
            
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
            return Data != null &&
                0 <= cell.x && cell.x < Data.SizeX && 
                0 <= cell.y && cell.y < Data.SizeY;
        }
        /// <summary>
        /// セルから長方形へと変換します．
        /// </summary>
        /// <param name="cellPos">セル</param>
        /// <returns>セルの長方形</returns>
        private Rect CellToRect(Vector2Int cellPos)
        {
            return new Rect()
            {
                xMin = cellPos.x * _cellPixel,
                xMax = (cellPos.x + 1) * _cellPixel,
                yMin = cellPos.y * _cellPixel,
                yMax = (cellPos.y + 1) * _cellPixel
            };
        }

        /// <summary>
        /// セル座標からPixel座標へ変換を行います．
        /// </summary>
        /// <param name="cell">セル</param>
        /// <param name="x">セル内のx位置(0~1)</param>
        /// <param name="y">セル内のy位置(0~1)</param>
        /// <returns>Pixel座標</returns>
        private Vector2 CellToPixel(Vector2 cell, float x = 0, float y = 0)
        {
            x = Mathf.Clamp(x, 0, 1);
            y = Mathf.Clamp(y, 0, 1);

            return new ((cell.x + x) * _cellPixel, (cell.y + y) * _cellPixel);
        }

        /// <summary>
        /// 次のステージオブジェクトIDを取得します．
        /// </summary>
        /// <returns>ステージオブジェクトID</returns>
        private int GetNextId()
        {
            return _nextId++;
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
