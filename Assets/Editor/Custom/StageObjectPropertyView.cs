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
                _stageData = value;
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
                var newPos = new Vector2Int(e.newValue, stageObject.Pos.y);
                if (!StageData.IsInsideStage(newPos) || StageData.HasAnyStageElement(newPos))
                {
                    posXField.SetValueWithoutNotify(stageObject.Pos.x);
                    return;
                }

                stageObject.MoveTo(newPos);
                EditContext.SelectedCell = newPos;
            };
            posXField.RegisterValueChangedCallback(onPosXChanged);
            _fieldEventUnbinders.Add(() => posXField.UnregisterValueChangedCallback(onPosXChanged));
            Add(posXField);

            var posYField = new IntegerField("Pos Y");
            posYField.SetValueWithoutNotify(stageObject.Pos.y);
            posYField.isDelayed = true;
            EventCallback<ChangeEvent<int>> onPosYChanged = e =>
            {
                var newPos = new Vector2Int(stageObject.Pos.x, e.newValue);
                if (!StageData.IsInsideStage(newPos) || StageData.HasAnyStageElement(newPos))
                {
                    posYField.SetValueWithoutNotify(stageObject.Pos.y);
                    return;
                }

                stageObject.MoveTo(newPos);
                EditContext.SelectedCell = newPos;
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
                    BuildWallFields((WallObject)stageObject);
                    break;
            }
        }

        private void BuildWallFields(WallObject wall)
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

        private void OnEditContextChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            switch (e.propertyName)
            {
                case nameof(StageEditContext.SelectedCell):
                    Rebuild();
                    break;
            }
        }
    }
}
