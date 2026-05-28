using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using MaruSikaku.Stage;
using System.Runtime.CompilerServices;
using Unity.Properties;

namespace MaruSikaku.Editor
{
    public class StageDesignDataSource : INotifyBindablePropertyChanged
    {
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        /// <summary>ステージデータの表示用データ</summary>
        [CreateProperty]
        public StageDisplayData StageData
        {
            get => _stageData;
            set
            {
                if (_stageData == value) { return; }
                _stageData = value;
            }
        }
        private StageDisplayData _stageData = new();

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