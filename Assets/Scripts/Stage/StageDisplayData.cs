using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using Unity.Properties;
using System.Runtime.CompilerServices;

namespace MaruSikaku.Stage
{
    public class StageDisplayData : INotifyBindablePropertyChanged
    {
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        /// <summary>丸キャラクターの初期位置</summary>
        public Vector2Int MaruStart
        {
            get => _maruStart;
            set
            {
                if (_maruStart == value) { return; }
                _maruStart = value;
                Notify(nameof(MaruStartX));
                Notify(nameof(MaruStartY));
            }
        }
        private Vector2Int _maruStart;

        /// <summary>四角キャラクターの初期位置</summary>
        public Vector2Int SikakuStart
        {
            get => _sikakuStart;
            set
            {
                if (_sikakuStart == value) { return; }
                _sikakuStart = value;
                Notify(nameof(SikakuStartX));
                Notify(nameof(SikakuStartY));
            }
        }
        private Vector2Int _sikakuStart;

        #region 画面表示用プロパティ
        /// <summary>丸キャラクターの初期位置のX座標</summary>
        [CreateProperty]
        public int MaruStartX
        {
            get => _maruStart.x;
            set
            {
                if (_maruStart.x == value) { return; }
                _maruStart.x = value;
                Notify();
            }
        }
        /// <summary>丸キャラクターの初期位置のY座標</summary>
        [CreateProperty]
        public int MaruStartY
        {
            get => _maruStart.y;
            set
            {
                if (_maruStart.y == value) { return; }
                _maruStart.y = value;
                Notify();
            }
        }

        /// <summary>四角キャラクターの初期位置のX座標</summary>
        [CreateProperty]
        public int SikakuStartX
        {
            get => _sikakuStart.x;
            set
            {
                if (_sikakuStart.x == value) { return; }
                _sikakuStart.x = value;
                Notify();
            }
        }
        /// <summary>四角キャラクターの初期位置のY座標</summary>
        [CreateProperty]
        public int SikakuStartY
        {
            get => _sikakuStart.y;
            set
            {
                if (_sikakuStart.y == value) { return; }
                _sikakuStart.y = value;
                Notify();
            }
        }

        #endregion 画面表示用プロパティ

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