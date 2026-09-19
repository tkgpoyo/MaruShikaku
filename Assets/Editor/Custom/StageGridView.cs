using MaruSikaku.Editor.Data;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using MaruSikaku.Stage;

namespace MaruSikaku.Editor.Custom
{
    public class StageGridView : VisualElement
    {
        private const int LEFT_MOUSE_BUTTON_MASK = 0x00_00_00_01;
        private const float CELL_DEFAULT_PIXEL = 20f;
        private static readonly Color BACKGROUND_COLOR = Color.softBlue;
        private static readonly Color GRID_COLOR = Color.black;
        private static readonly Color GRID_SELECTED_COLOR = Color.red;
        private static readonly Color HOVER_COLOR = new(1, 1, 1, 0.35f);

        private float _cellPixel => CELL_DEFAULT_PIXEL * EditContext?.ZoomRate ?? 1f;
        private int _nextId = 0;
        private Vector2Int? _lastPaintedCell = null;

        public StageGridView()
        {
            AddToClassList("stage-grid-view");

            UpdateView();

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            RegisterCallback<PointerEnterEvent>(OnPointerEnter);

            generateVisualContent += OnGenerateVisualContent;
        }

        public StageDisplayData StageData
        {
            get => _stageData;
            set
            {
                if (_stageData == value) { return; }
                if (_stageData != null)
                {
                    _stageData.propertyChanged -= OnStageDataChanged;
                }
                _stageData = value;
                if (_stageData != null)
                {
                    _stageData.propertyChanged += OnStageDataChanged;

                    _nextId = _stageData.StageObjects.Count <= 0 ? 0 : _stageData.StageObjects.Max(stageObject => stageObject.Id + 1);
                }

                MarkDirtyRepaint();
            }
        }
        private StageDisplayData _stageData = new();
        
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
            MarkDirtyRepaint();
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

        private void EditStage(Vector2Int pos, EStageEditMode mode)
        {
            switch (mode)
            {
                case EStageEditMode.Select:
                    break;
                case EStageEditMode.Erase:
                    if (StageData.TryGetTerrainCell(pos, out var removeTerrain))
                    {
                        StageData.RemoveTerrainCell(removeTerrain);
                    }
                    if (StageData.TryGetStageObject(pos, out var removeObject))
                    {
                        StageData.RemoveStageObject(removeObject);
                    }
                    break;
                case EStageEditMode.Ground:
                    if (StageData.HasAnyStageElement(pos)) { break; }
                    StageData.AddTerrainCell(new StageTerrainCell(pos, ETerrainType.Ground));
                    break;
                case EStageEditMode.Spring:
                case EStageEditMode.Fragile:
                case EStageEditMode.Movable:
                case EStageEditMode.Switch:
                case EStageEditMode.Wall:
                    if (StageData.HasAnyStageElement(pos)) { break; }
                    StageData.AddStageObject(InstantiateStageObject(pos, EditContext.EditMode));
                    break;
                case EStageEditMode.MaruStart:
                    if (StageData.HasAnyStageElement(pos)) { break; }
                    StageData.MaruStart = pos;
                    break;
                case EStageEditMode.SikakuStart:
                    if (StageData.HasAnyStageElement(pos)) { break; }
                    StageData.SikakuStart = pos;
                    break;
            }

            //↓オブジェクト配置後に選択中セルを変更しないと，オブジェクトプロパティビューがうまく更新されない
            EditContext.SelectedCell = pos;         // いずれのモードにおいても，クリックしたセルを選択状態にする

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

        /// <summary>
        /// ドラッグ配置可能かどうかを判定します．
        /// </summary>
        /// <param name="mode">編集モード</param>
        /// <returns>ドラッグ編集可能かどうか</returns>
        private bool CanDragPaint(EStageEditMode mode) => mode is EStageEditMode.Erase or EStageEditMode.Ground or EStageEditMode.Fragile;

        /// <summary>
        /// 左クリックされているかどうかを判定します．
        /// </summary>
        /// <param name="e">ポインターのイベント</param>
        /// <returns>左クリックされているか</returns>
        private bool IsLeftButtonPressed(IPointerEvent e) => (e.pressedButtons & LEFT_MOUSE_BUTTON_MASK) != 0;

        private void OnPointerDown(PointerDownEvent e)
        {
            if (!IsLeftButtonPressed(e)) { return; } // 左クリックのみ
            e.StopPropagation();

            var cell = PointerToCell(e.localPosition);
            if (!StageData.IsInsideStage(cell)) { return; }

            _lastPaintedCell = cell;

            this.CapturePointer(e.pointerId);

            EditStage(cell, EditContext.EditMode);
        }

        private void OnPointerMove(PointerMoveEvent e)
        {
            e.StopPropagation();

            var cell = PointerToCell(e.localPosition);
            if (!StageData.IsInsideStage(cell)) {       // ステージ外にマウスがある場合
                EditContext.HoverCell = null;
                return;
            }
            if (EditContext.HoverCell != cell)
            {
                EditContext.HoverCell = cell;
            }

            if (!IsLeftButtonPressed(e))
            {
                _lastPaintedCell = null;
                return;
            }

            if (!CanDragPaint(EditContext.EditMode)) { return; }
            if (cell == _lastPaintedCell) { return; }

            _lastPaintedCell = cell;

            EditStage(cell, EditContext.EditMode);
        }

        private void OnPointerUp(PointerUpEvent e)
        {
            e.StopPropagation();

            _lastPaintedCell = null;

            if (this.HasPointerCapture(e.pointerId))
            {
                this.ReleasePointer(e.pointerId);
            }
        }

        private void OnPointerLeave(PointerLeaveEvent e)
        {
            e.StopPropagation();

            EditContext.HoverCell = null;
            _lastPaintedCell = null;

            if (this.HasPointerCapture(e.pointerId))
            {
                this.ReleasePointer(e.pointerId);
            }
        }

        private void OnPointerEnter(PointerEnterEvent e)
        {
            e.StopPropagation();

            var cell = PointerToCell(e.localPosition);

            if (StageData.IsInsideStage(cell))
            {
                EditContext.HoverCell = cell;
            }
        }

        private void OnGenerateVisualContent(MeshGenerationContext context)
        {
            var painter = context.painter2D;

            UpdateView();
            DrawBackground(painter);
            DrawGrid(painter);
            DrawHoverCell(painter);
            DrawTerrainCells(painter);
            DrawStageObjects(painter);
            DrawSelectedCell(painter);
            DrawPlayerStartPos(painter);
            DrawPlayerGoalPos(painter);
        }

        private void UpdateView()
        {
            style.width = StageData.SizeX * _cellPixel;
            style.height = StageData.SizeY * _cellPixel;
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
            for (int x = 1; x < StageData.SizeX; x++) {
                var px = x * _cellPixel;
                painter.MoveTo(new(px, contentRect.yMin));
                painter.LineTo(new(px, contentRect.yMax));
            }
            for (int y = 1; y < StageData.SizeY; y++) {
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
            painter.Rect(CellToRect(hoverCell));

            painter.Fill();
        }

        private void DrawTerrainCells(Painter2D painter)
        {
            foreach (var terrainCell in StageData.TerrainCells)
            {
                if (!StageData.IsInsideStage(terrainCell.Pos)) { continue; }
                DrawTerrainCell(painter, terrainCell);
            }

        }

        private void DrawTerrainCell(Painter2D painter, StageTerrainCell terrainCell)
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

        private void DrawStageObjects(Painter2D painter)
        {
            foreach (var stageObject in StageData.StageObjects)
            {
                if (!StageData.IsInsideStage(stageObject.Pos)) { continue; }
                DrawStageObject(painter, stageObject);
            }

        }

        private void DrawStageObject(Painter2D painter, StageObject stageObject)
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
                    painter.MoveTo(CellToPixel(stageObject.Pos, x: 0.4f, y: 1f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.3f, y: 0.5f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.6f, y: 1f));
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
                    var springBase1BottomY = CellToPixel(stageObject.Pos, y: 0.8f).y;
                    var springBase2TopY = CellToPixel(stageObject.Pos, y: 0.2f).y;
                    var springBase2BottomY = rect.yMax;
                    painter.BeginPath();
                    painter.strokeColor = Color.black;
                    painter.fillColor = Color.Lerp(Color.gray, Color.black, 0.6f);
                    painter.Rect(new Rect()
                    {
                        xMin = rect.xMin,
                        xMax = rect.xMax,
                        yMin = springBase1BottomY,
                        yMax = springBase1TopY
                    });
                    painter.Rect(new Rect()
                    {
                        xMin = rect.xMin,
                        xMax = rect.xMax,
                        yMin = springBase2BottomY,
                        yMax = springBase2TopY
                    });
                    painter.Fill();
                    painter.Stroke();
                    painter.ClosePath();
                    // バネの縮む部分
                    painter.BeginPath();
                    painter.fillColor = Color.black;
                    painter.strokeColor = Color.black;
                    painter.MoveTo(CellToPixel(stageObject.Pos, x: 0.6f, y: 0.8f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.7f, y: 0.65f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.6f, y: 0.5f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.7f, y: 0.35f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.6f, y: 0.2f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.7f, y: 0.2f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.8f, y: 0.35f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.7f, y: 0.5f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.8f, y: 0.85f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.7f, y: 0.8f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.6f, y: 0.8f));

                    painter.MoveTo(CellToPixel(stageObject.Pos, x: 0.4f, y: 0.8f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.3f, y: 0.65f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.4f, y: 0.5f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.3f, y: 0.35f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.4f, y: 0.2f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.3f, y: 0.2f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.2f, y: 0.35f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.3f, y: 0.5f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.2f, y: 0.65f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.3f, y: 0.8f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.4f, y: 0.8f));

                    painter.Fill();
                    painter.Stroke();
                    break;
                case EStageObjectType.Switch:
                    // 押す部分
                    painter.BeginPath();
                    painter.fillColor = Color.yellow;
                    painter.strokeColor = Color.black;
                    painter.MoveTo(CellToPixel(stageObject.Pos, x: 0.2f, y: 0.2f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.3f, y: 0.5f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.7f, y: 0.5f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.8f, y: 0.2f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.2f, y: 0.2f));
                    painter.Fill();
                    painter.Stroke();
                    painter.ClosePath();
                    // 土台部分
                    painter.BeginPath();
                    painter.fillColor = Color.Lerp(Color.gray, Color.black, 0.4f);
                    painter.strokeColor = Color.black;
                    var switchBaseTopY = CellToPixel(stageObject.Pos, y: 0.2f).y;
                    var switchBaseBottomY = rect.yMax;
                    painter.Rect(new()
                    {
                        xMin = rect.xMin,
                        xMax = rect.xMax,
                        yMin = switchBaseBottomY,
                        yMax = switchBaseTopY
                    });
                    painter.Fill();
                    painter.Stroke();
                    painter.ClosePath();
                    break;
                case EStageObjectType.Wall:
                    painter.BeginPath();
                    painter.fillColor = Color.yellow;
                    painter.strokeColor = Color.black;
                    painter.MoveTo(CellToPixel(stageObject.Pos, y: 1));
                    painter.LineTo(CellToPixel(stageObject.Pos, y: 0.5f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.3f, y: 0.5f));
                    painter.LineTo(CellToPixel(stageObject.Pos, x: 0.3f, y: 1));
                    painter.LineTo(CellToPixel(stageObject.Pos, y: 1));
                    painter.Fill();
                    painter.Stroke();
                    painter.ClosePath();
                    break;
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

        private void DrawPlayerStartPos(Painter2D painter)
        {
            if (StageData.IsInsideStage(StageData.MaruStart))
            {
                painter.fillColor = Color.red;
                painter.strokeColor = Color.black;
                painter.lineWidth = 1f;
                painter.BeginPath();
                painter.Arc(CellToPixel(StageData.MaruStart, x: 0.5f, y: 0.5f), _cellPixel * 0.4f, new Angle(0f), new Angle(360f));
                painter.Fill();
                painter.Stroke();
                painter.ClosePath();
            }
            if (StageData.IsInsideStage(StageData.SikakuStart))
            {
                painter.fillColor = Color.blue;
                painter.strokeColor = Color.black;
                painter.lineWidth = 1f;
                painter.BeginPath();
                painter.MoveTo(CellToPixel(StageData.SikakuStart, x: 0.1f, y: 0.1f));
                painter.LineTo(CellToPixel(StageData.SikakuStart, x: 0.9f, y: 0.1f));
                painter.LineTo(CellToPixel(StageData.SikakuStart, x: 0.9f, y: 0.9f));
                painter.LineTo(CellToPixel(StageData.SikakuStart, x: 0.1f, y: 0.9f));
                painter.LineTo(CellToPixel(StageData.SikakuStart, x: 0.1f, y: 0.1f));
                painter.Fill();
                painter.Stroke();
                painter.ClosePath();
            }
        }

        private void DrawPlayerGoalPos(Painter2D painter)
        {
            if (StageData.IsInsideStage(StageData.MaruGoal))
            {
                painter.BeginPath();
                painter.fillColor = Color.red;
                painter.strokeColor = Color.black;
                painter.Rect(CellToRect(StageData.MaruGoal));
                painter.Fill();
                painter.Stroke();
                painter.ClosePath();

                painter.BeginPath();
                painter.strokeColor = Color.black;
                painter.lineWidth = 2;
                painter.Arc(CellToPixel(StageData.MaruGoal, x: 0.5f, y: 0.5f), _cellPixel * 0.3f, new Angle(30), new Angle(330));
                painter.MoveTo(CellToPixel(StageData.MaruGoal, x: 0.5f, y: 0.5f));
                painter.LineTo(CellToPixel(StageData.MaruGoal, x: 0.746f, y: 0.5f));
                painter.LineTo(CellToPixel(StageData.MaruGoal, x: 0.746f, y: 0.2f));
                painter.Stroke();
                painter.ClosePath();
            }
            if (StageData.IsInsideStage(StageData.SikakuGoal))
            {
                painter.BeginPath();
                painter.fillColor = Color.blue;
                painter.strokeColor = Color.black;
                painter.Rect(CellToRect(StageData.SikakuGoal));
                painter.Fill();
                painter.Stroke();
                painter.ClosePath();

                painter.BeginPath();
                painter.strokeColor = Color.black;
                painter.lineWidth = 2;
                painter.Arc(CellToPixel(StageData.SikakuGoal, x: 0.5f, y: 0.5f), _cellPixel * 0.3f, new Angle(30), new Angle(330));
                painter.MoveTo(CellToPixel(StageData.SikakuGoal, x: 0.5f, y: 0.5f));
                painter.LineTo(CellToPixel(StageData.SikakuGoal, x: 0.746f, y: 0.5f));
                painter.LineTo(CellToPixel(StageData.SikakuGoal, x: 0.746f, y: 0.2f));
                painter.Stroke();
                painter.ClosePath();
            }
        }

        private Vector2Int PointerToCell(Vector2 position)
        {
            var x = Mathf.FloorToInt(position.x / _cellPixel);
            var y = StageData.SizeY - Mathf.CeilToInt(position.y / _cellPixel);
            return new(x, y);
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
                yMin = (StageData.SizeY - (cellPos.y + 1)) * _cellPixel,
                yMax = (StageData.SizeY - cellPos.y) * _cellPixel
            };
        }

        /// <summary>
        /// セル座標からPixel座標へ変換を行います．
        /// </summary>
        /// <param name="cell">セル</param>
        /// <param name="x">セル内のx位置(0~1)</param>
        /// <param name="y">セル内のy位置(0~1)</param>
        /// <returns>Pixel座標</returns>
        private Vector2 CellToPixel(Vector2Int cell, float x = 0, float y = 0)
        {
            x = Mathf.Clamp(x, 0, 1);
            y = Mathf.Clamp(y, 0, 1);

            return new ((cell.x + x) * _cellPixel, (StageData.SizeY - (cell.y + y)) * _cellPixel);
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
}
