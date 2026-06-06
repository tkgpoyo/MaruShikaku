using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using Unity.Properties;
using System.Runtime.CompilerServices;
using System.Text;
using System.Security.Policy;

namespace MaruSikaku.Editor.Data
{
    /// <summary>
    /// 編集モードを表す列挙型です．
    /// </summary>
    public enum EStageEditMode
    {
        /// <summary>選択モード</summary>
        Select,
        /// <summary>削除モード</summary>
        Erase,
        /// <summary>地面ブロック配置モード</summary>
        Ground,
        /// <summary>バネ配置モード</summary>
        Spring,
        /// <summary>壊れるブロック配置モード</summary>
        Fragile,
        /// <summary>動くブロック配置モード</summary>
        Movable,
        /// <summary>スイッチ配置モード</summary>
        Switch,
        /// <summary>壁配置モード</summary>
        Wall,
        /// <summary>丸キャラクター初期位置配置モード</summary>
        MaruStart,
        /// <summary>四角キャラクター初期位置配置モード</summary>
        SikakuStart,
    }

    public class StageEditContext : INotifyBindablePropertyChanged
    {
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        public StageEditContext()
        {
            EditMode = EStageEditMode.Select;
            ZoomRate = 1f;
        }

        public Vector2Int? HoverCell
        {
            get => _hoverCell;
            set
            {
                if (_hoverCell == value) { return; }

                _hoverCell = value;

                // HoverCellTextを変更
                var sb = new StringBuilder();
                if (_hoverCell == null)
                {
                    sb.Append("Cell: -, -");
                }
                else
                {
                    var cell = (Vector2Int)_hoverCell;
                    sb.Append("Cell: ").Append(cell.x).Append(", ").Append(cell.y);
                }

                HoverCellText = sb.ToString();
                Notify();
            }
        }
        private Vector2Int? _hoverCell;

        public Vector2Int? SelectedCell
        {
            get => _selectedCell;
            set
            {
                if (_selectedCell == value) { return; }
                _selectedCell = value;
                Notify();
            }
        }
        private Vector2Int? _selectedCell = null;

        [CreateProperty]
        public EStageEditMode EditMode
        {
            get => _editMode;
            set
            {
                if (_editMode == value) { return; }

                _editMode = value;
                Notify();
            }
        }
        private EStageEditMode _editMode;

        [CreateProperty]
        public string HoverCellText
        {
            get => _hoverCellText;
            set
            {
                if (_hoverCellText == value) { return; }

                _hoverCellText = value;
                Notify();
            }
        }
        private string _hoverCellText;

        [CreateProperty]
        public float ZoomRate
        {
            get => _zoomRate;
            set
            {
                if (Mathf.Approximately(_zoomRate, value)) { return; }

                _zoomRate = Mathf.Clamp(value, 0.01f, 2f);
                Notify();
            }
        }
        private float _zoomRate;

        /// <summary>
        /// 変更通知を行います． 
        /// </summary>
        /// <param name="property">プロパティ名</param>
        private void Notify([CallerMemberName] string property = "")
        {
            propertyChanged?.Invoke(this, new(property));
        }
    }
}