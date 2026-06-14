using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using System.Runtime.CompilerServices;
using Unity.Properties;

namespace MaruSikaku.Editor.Data
{
    /// <summary>
    /// StageDesignWindowのデータソースを表すクラスです．
    /// </summary>
    public class StageDesignDataSource : INotifyBindablePropertyChanged
    {
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        public StageDesignDataSource()
        {
            StageData = new();
            EditContext = new();
        }

        /// <summary>ステージデータの表示用データ</summary>
        [CreateProperty]
        public StageDisplayData StageData
        {
            get => _stageData;
            set
            {
                if (_stageData == value) { return; }
                if (_stageData != null) {
                    _stageData.propertyChanged -= OnStageDataChanged;
                }
                _stageData = value;
                if (_stageData != null) {
                    _stageData.propertyChanged += OnStageDataChanged;
                }
                Notify();
            }
        }
        private StageDisplayData _stageData;

        /// <summary>ステージデータのJSONファイルパス</summary>
        [CreateProperty]
        public string JsonPath
        {
            get => _jsonPath;
            set
            {
                if (_jsonPath == value) { return; }
                _jsonPath = value;
                Notify();
            }
        }
        private string _jsonPath;

        [CreateProperty]
        public StageEditContext EditContext
        {
            get => _editContext;
            private set {
                if (_editContext == value) { return; }
                if (_editContext != null) {
                    _editContext.propertyChanged -= OnEditContextChanged;
                }
                _editContext = value;
                if (_editContext != null) {
                    _editContext.propertyChanged += OnEditContextChanged;
                }
                Notify();
            }
        }
        private StageEditContext _editContext;

        private void OnStageDataChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            NotifyChild(nameof(StageData), e);
        }

        private void OnEditContextChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            NotifyChild(nameof(EditContext), e);
        }

        /// <summary>
        /// 子要素の変更通知を行います． 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotifyChild(string parentName, BindablePropertyChangedEventArgs e)
        {
            var path = $"{parentName}.{e.propertyName}";

            propertyChanged?.Invoke(
                this,
                new BindablePropertyChangedEventArgs(path)
            );
        }

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