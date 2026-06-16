using MaruSikaku.Editor.Data;
using MaruSikaku.Stage;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MaruSikaku.Editor.Custom
{
    public class StageObjectPropertyView : VisualElement
    {
        private StageObject _currentObject;

        public StageObjectPropertyView()
        {
            AddToClassList("stage-object-property-view");
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
                if (_editContext != null)
                {
                    _editContext.propertyChanged -= OnEditContextChanged;
                }
                _editContext = value;
                if (_editContext != null)
                {
                    _editContext.propertyChanged += OnEditContextChanged;
                }
                Rebuild();
            }
        }
        private StageEditContext _editContext = new();

        private void Rebuild()
        {
            Clear();        // 子要素を全削除
            UnbindCurrentObject();

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

        private void BuildCommonFields(StageObject stageObject)
        {
            Add(new Label(stageObject.Type.ToString()));

            var idField = new IntegerField("ID");
            idField.SetValueWithoutNotify(stageObject.Id);
            idField.SetEnabled(false);
            Add(idField);

            var posXField = new IntegerField("Pos X");
            posXField.SetValueWithoutNotify(stageObject.Pos.x);
            posXField.RegisterValueChangedCallback(e =>
            {
                stageObject.Pos = ClampPos(new (e.newValue, stageObject.Pos.y));
            });
            Add(posXField);

            var posYField = new IntegerField("Pos Y");
            posYField.SetValueWithoutNotify(stageObject.Pos.y);
            posYField.RegisterValueChangedCallback(e =>
            {
                stageObject.Pos = ClampPos(new (stageObject.Pos.x, e.newValue));
            });
            Add(posYField);
        }

        private void BuildSpecificTypeFields(StageObject stageObject)
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
            switchIdField.RegisterValueChangedCallback(evt =>
            {
                wall.SwitchId = evt.newValue;
            });
            Add(switchIdField);
        }

        private Vector2Int ClampPos(Vector2Int pos)
        {
            var x = Mathf.Clamp(pos.x, 0, StageData.SizeX - 1);
            var y = Mathf.Clamp(pos.y, 0, StageData.SizeY - 1);
            return new (x, y);
        }

        private void UnbindCurrentObject()
        {
            if (_currentObject == null) { return; }
            _currentObject = null;
        }

        private void OnEditContextChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            switch (e.propertyName)
            {
                case nameof(EditContext.SelectedCell):
                    Rebuild();
                    break;
            }
        }
    }
}