using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using MaruSikaku.Editor.Custom;
using MaruSikaku.Editor.Data;
using MaruSikaku.Stage;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Codice.CM.Common.Merge;
using System;

namespace MaruSikaku.Editor
{
    public class StageDesignWindow : EditorWindow
    {
        /// <summary>StageDesignEditor用のファイルがあるディレクトリパス</summary>
        private const string DIRECTORY_PATH = @"Assets/Editor/";
        /// <summary>StageDesignEditor用のUSSファイル</summary>
        private const string USS_FILE_NAME = @"StageDesignUSS.uss";
        /// <summary>StageDesignEditor用のUXMLファイル</summary>
        private const string UXML_FILE_NAME = @"StageDesignUXML.uxml";

        /// <summary>ステージデザイン画面のデータソース</summary>
        private StageDesignDataSource _dataSource = new();
        private StageGridView _gridView;
        private StageObjectPropertyView _propertyView;

        #region ボタン一覧
        private Button _selectToolButton;
        private Button _eraseToolButton;
        private Button _groundToolButton;
        private Button _springToolButton;
        private Button _fragileToolButton;
        private Button _movableToolButton;
        private Button _switchToolButton;
        private Button _wallToolButton;
        private Button _maruStartToolButton;
        private Button _sikakuStartToolButton;
        #endregion ボタン一覧

        [MenuItem("Tools/StageDesign")]
        public static void Open()
        {
            GetWindow<StageDesignWindow>("StageDesign");    // Windowを生成
        }

        void CreateGUI()
        {
            _gridView = new();
            _gridView.Data = _dataSource.StageData;
            _gridView.EditContext = _dataSource.EditContext;

            _propertyView = new();
            _propertyView.Data = _dataSource.StageData;
            _propertyView.EditContext = _dataSource.EditContext;

            // UXML, USSをロード
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(DIRECTORY_PATH, UXML_FILE_NAME));
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(DIRECTORY_PATH, USS_FILE_NAME));
            uxml.CloneTree(rootVisualElement);          // UXMLから画面を作成
            rootVisualElement.styleSheets.Add(uss);     // USSを追加

            // データソースの変更時イベント
            _dataSource.propertyChanged += OnDataSourcePropertyChanged;

            // データソースの設定
            rootVisualElement.Q<VisualElement>("stageEditorRoot").dataSource = _dataSource;
            rootVisualElement.Q<ScrollView>("gridScrollView").Add(_gridView);
            rootVisualElement.Q<VisualElement>("objectPropertyContainer").Add(_propertyView);

            // header部分のボタン処理イベントの登録
            rootVisualElement.Q<Button>("browseButton").clicked += OnBrowse;
            rootVisualElement.Q<Button>("saveButton").clicked += OnSave;
            rootVisualElement.Q<Button>("saveAsButton").clicked += OnSaveAs;
            // tool部分のボタン処理イベントの登録
            _selectToolButton = rootVisualElement.Q<Button>("selectToolButton");
            _eraseToolButton = rootVisualElement.Q<Button>("eraseToolButton");
            _groundToolButton = rootVisualElement.Q<Button>("groundToolButton");
            _springToolButton = rootVisualElement.Q<Button>("springToolButton");
            _fragileToolButton = rootVisualElement.Q<Button>("fragileToolButton");
            _movableToolButton = rootVisualElement.Q<Button>("movableToolButton");
            _switchToolButton = rootVisualElement.Q<Button>("switchToolButton");
            _wallToolButton = rootVisualElement.Q<Button>("wallToolButton");
            _maruStartToolButton = rootVisualElement.Q<Button>("maruStartToolButton");
            _sikakuStartToolButton = rootVisualElement.Q<Button>("sikakuStartToolButton");
            _selectToolButton.clicked += () => { OnChangeTool(EStageEditMode.Select); };
            _eraseToolButton.clicked += () => { OnChangeTool(EStageEditMode.Erase); };
            _groundToolButton.clicked += () => { OnChangeTool(EStageEditMode.Ground); };
            _springToolButton.clicked += () => { OnChangeTool(EStageEditMode.Spring); };
            _fragileToolButton.clicked += () => { OnChangeTool(EStageEditMode.Fragile); };
            _movableToolButton.clicked += () => { OnChangeTool(EStageEditMode.Movable); };
            _switchToolButton.clicked += () => { OnChangeTool(EStageEditMode.Switch); };
            _wallToolButton.clicked += () => { OnChangeTool(EStageEditMode.Wall); };
            _maruStartToolButton.clicked += () => { OnChangeTool(EStageEditMode.MaruStart); };
            _sikakuStartToolButton.clicked += () => { OnChangeTool(EStageEditMode.SikakuStart); };

            // 最初に選択されている Tool Button を Highlight にする
            HighlightCurrentToolButton(_dataSource.EditContext.EditMode);
        }

        private void OnBrowse()
        {
            var path = EditorUtility.OpenFilePanel("JSONファイルを選択", Application.dataPath, "json"); // JSONファイルを取得
            if (string.IsNullOrWhiteSpace(path)) { return; }

            _dataSource.JsonPath = path;                        // ファイルパスを設定

            // StageDataのロード
            var json = File.ReadAllText(path);
            var stageSaveData = JsonUtility.FromJson<StageSaveData>(json);
            var stageDisplayData = StageDataConverter.FromStageSaveData(stageSaveData);
            _dataSource.StageData = stageDisplayData;
        }

        private void OnSave()
        {
            string path;
            if (string.IsNullOrEmpty(_dataSource.JsonPath))
            {
                path = EditorUtility.SaveFilePanel("JSONファイル保存先を選択", Application.dataPath, "stage", "json");   // JSONファイル保存先を選択
                if (string.IsNullOrWhiteSpace(path)) { return; }
            }
            else
            {
                path = _dataSource.JsonPath;
            }

            var json = EditorJsonUtility.ToJson(StageDataConverter.ToStageSaveData(_dataSource.StageData), true);

            File.WriteAllText(path, json);
        }

        private void OnSaveAs()
        {
            string defaultFolderPath;
            if (string.IsNullOrWhiteSpace(_dataSource.JsonPath))
            {
                defaultFolderPath = Application.dataPath;
            }
            else
            {
                defaultFolderPath = Path.GetDirectoryName(_dataSource.JsonPath);
            }

            var path = EditorUtility.SaveFilePanel("JSONファイル保存先を選択", defaultFolderPath, "stage", "json");
            if (string.IsNullOrWhiteSpace(path)) { return; }

            var json = EditorJsonUtility.ToJson(StageDataConverter.ToStageSaveData(_dataSource.StageData), true);

            File.WriteAllText(path, json);

            _dataSource.JsonPath = path;
        }

        private void OnChangeTool(EStageEditMode editMode)
        {
            _dataSource.EditContext.EditMode = editMode;

            // Tool Button の色をリセット
            foreach (var toolButton in GetAllToolButtons())
            {
                toolButton.style.backgroundColor = StyleKeyword.Null;   // ボタンの色をリセット
            }
            // 選択されたボタンをHighlight状態にする
            HighlightCurrentToolButton(editMode);
        }

        private void OnDataSourcePropertyChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            switch (e.propertyName)
            {
                case nameof(StageDesignDataSource.StageData):
                    _gridView.Data = _dataSource.StageData;
                    _propertyView.Data = _dataSource.StageData;
                    break;
                case nameof(StageDesignDataSource.EditContext):
                    _gridView.EditContext = _dataSource.EditContext; 
                    _propertyView.EditContext = _dataSource.EditContext;
                    break;
            }
        }

        private Button[] GetAllToolButtons()
        {
            return new Button[]
            {
                _selectToolButton,
                _eraseToolButton,
                _groundToolButton,
                _springToolButton,
                _fragileToolButton,
                _movableToolButton,
                _switchToolButton,
                _wallToolButton,
                _maruStartToolButton,
                _sikakuStartToolButton
            };
        }

        private Button GetToolButton(EStageEditMode mode)
        {
            return mode switch
            {
                EStageEditMode.Select => _selectToolButton,
                EStageEditMode.Erase => _eraseToolButton,
                EStageEditMode.Ground => _groundToolButton,
                EStageEditMode.Spring => _springToolButton,
                EStageEditMode.Fragile => _fragileToolButton,
                EStageEditMode.Movable => _movableToolButton,
                EStageEditMode.Switch => _switchToolButton,
                EStageEditMode.Wall => _wallToolButton,
                EStageEditMode.MaruStart => _maruStartToolButton,
                EStageEditMode.SikakuStart => _sikakuStartToolButton,
                _ => throw new NotImplementedException($"{mode}は未実装です．")
            };
        }

        private void HighlightCurrentToolButton(EStageEditMode mode)
        {
            var highlightColor = Color.Lerp(Color.white, Color.black, 0.45f);
            var button = GetToolButton(mode);
            button.style.backgroundColor = highlightColor;
        }
    }
}