using MaruSikaku.Editor.Data;
using MaruSikaku.Stage;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MaruSikaku.Editor.Custom
{
    public class StageObjectPropertyView : VisualElement
    {
        private StageObjectDisplayData _currentObject;
        private readonly List<Action> _fieldEventUnbinders = new();
        private bool _isEditContextBound;
        private bool _isStageDataBound;

        public StageObjectPropertyView()
        {
            AddToClassList("stage-object-property-view");
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            Rebuild();
        }

        public StageDisplayData StageData
        {
            get => _stageData;
            set
            {
                if (_stageData == value) { return; }
                UnbindStageData();
                _stageData = value;
                BindStageData();
                Rebuild();
            }
        }
        private StageDisplayData _stageData = new();

        public StageEditContext EditContext
        {
            get => _editContext;
            set
            {
                if (_editContext == value) { return; }
                UnbindEditContext();
                _editContext = value;
                BindEditContext();
                Rebuild();
            }
        }
        private StageEditContext _editContext = new();

        private void Rebuild()
        {
            UnbindCurrentObject();
            Clear();        // 子要素を全削除

            if (StageData == null || EditContext == null)
            {
                Add(new Label("No data"));
                return;
            }

            if (_editContext.SelectedCell == null)
            {
                Add(new Label("No cell selected"));
                return;
            }

            var selectedCell = (Vector2Int)EditContext.SelectedCell;

            if (!StageData.TryGetStageObject(selectedCell, out var selectedObject))
            {
                Add(new Label("No object selected"));
                return;
            }

            _currentObject = selectedObject;
            BuildCommonFields(selectedObject);
            BuildSpecificTypeFields(selectedObject);
        }

        private void BuildCommonFields(StageObjectDisplayData stageObject)
        {
            Add(new Label(stageObject.Type.ToString()));

            var idField = new IntegerField("ID");
            idField.SetValueWithoutNotify(stageObject.Id);
            idField.SetEnabled(false);
            Add(idField);

            var posXField = new IntegerField("Pos X");
            posXField.SetValueWithoutNotify(stageObject.Pos.x);
            posXField.isDelayed = true;
            EventCallback<ChangeEvent<int>> onPosXChanged = e =>
            {
                var newPos = new Vector2Int(e.newValue, stageObject.Pos.y); // 移動先の座標
                if (!StageData.TryMoveObject(stageObject, newPos))          // 移動できない場合
                {
                    posXField.SetValueWithoutNotify(stageObject.Pos.x);     // 元の値に戻す
                    return;
                }

                EditContext.SelectedCell = newPos;                          // 選択セルを変更
            };
            posXField.RegisterValueChangedCallback(onPosXChanged);
            _fieldEventUnbinders.Add(() => posXField.UnregisterValueChangedCallback(onPosXChanged));
            Add(posXField);

            var posYField = new IntegerField("Pos Y");
            posYField.SetValueWithoutNotify(stageObject.Pos.y);
            posYField.isDelayed = true;
            EventCallback<ChangeEvent<int>> onPosYChanged = e =>
            {
                var newPos = new Vector2Int(stageObject.Pos.x, e.newValue); // 移動先の座標
                if (!StageData.TryMoveObject(stageObject, newPos))          // 移動できない場合
                {
                    posYField.SetValueWithoutNotify(stageObject.Pos.y);     // 元の値に戻す
                    return;
                }

                EditContext.SelectedCell = newPos;                          // 選択セルを変更
            };
            posYField.RegisterValueChangedCallback(onPosYChanged);
            _fieldEventUnbinders.Add(() => posYField.UnregisterValueChangedCallback(onPosYChanged));
            Add(posYField);
        }

        private void BuildSpecificTypeFields(StageObjectDisplayData stageObject)
        {
            switch (stageObject.Type)
            {
                case EStageObjectType.Wall:
                    BuildWallFields((WallObjectDisplayData)stageObject);
                    break;
            }
        }

        private void BuildWallFields(WallObjectDisplayData wall)
        {
            var switchIdField = new IntegerField("Switch ID");
            switchIdField.SetValueWithoutNotify(wall.SwitchId);
            switchIdField.isDelayed = true;
            EventCallback<ChangeEvent<int>> onSwitchIdChanged = evt =>
            {
                wall.SwitchId = evt.newValue;
            };
            switchIdField.RegisterValueChangedCallback(onSwitchIdChanged);
            _fieldEventUnbinders.Add(() => switchIdField.UnregisterValueChangedCallback(onSwitchIdChanged));
            Add(switchIdField);

            var lengthField = new IntegerField("Length");
            lengthField.SetValueWithoutNotify(wall.YLength);
            lengthField.isDelayed = true;
            EventCallback<ChangeEvent<int>> onLengthChanged = evt =>
            {
                var targetLength = evt.newValue;                            // 目標の長さ
                if (!StageData.TryChangeWallLength(wall, targetLength))     // 長さを変更できない場合
                {
                    lengthField.SetValueWithoutNotify(wall.YLength);        // 元の値に戻す
                }
            };
            lengthField.RegisterValueChangedCallback(onLengthChanged);
            _fieldEventUnbinders.Add(() => lengthField.UnregisterValueChangedCallback(onLengthChanged));
            Add(lengthField);
        }

        private void UnbindCurrentObject()
        {
            foreach (var unbind in _fieldEventUnbinders)
            {
                unbind();
            }
            _fieldEventUnbinders.Clear();
            _currentObject = null;
        }

        private void BindEditContext()
        {
            if (_isEditContextBound || _editContext == null || panel == null) { return; }
            _editContext.propertyChanged += OnEditContextChanged;
            _isEditContextBound = true;
        }

        private void UnbindEditContext()
        {
            if (!_isEditContextBound || _editContext == null) { return; }
            _editContext.propertyChanged -= OnEditContextChanged;
            _isEditContextBound = false;
        }

        private void BindStageData()
        {
            if (_isStageDataBound || _stageData == null || panel == null) { return; }
            _stageData.propertyChanged += OnStageDataChanged;
            _isStageDataBound = true;
        }

        private void UnbindStageData()
        {
            if (!_isStageDataBound || _stageData == null) { return; }
            _stageData.propertyChanged -= OnStageDataChanged;
            _isStageDataBound = false;
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            BindEditContext();
            Rebuild();
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            UnbindEditContext();
            UnbindCurrentObject();
        }

        /// <summary>
        /// 編集状態が変更された時のイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEditContextChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            switch (e.propertyName)
            {
                case nameof(StageEditContext.SelectedCell):     // 選択中のセルが変更された場合
                    Rebuild();                                  // 再ビルド
                    break;
            }
        }

        /// <summary>
        /// ステージ情報が変更された時のイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStageDataChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            switch (e.propertyName)
            {
                case nameof(StageDisplayData.StageObjects):     // オブジェクトが変更された場合
                    Rebuild();                                  // 再ビルド
                    break;
            }
        }
    }
}
