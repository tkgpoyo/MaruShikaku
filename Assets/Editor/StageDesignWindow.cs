using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using MaruSikaku.Editor.Custom;
using MaruSikaku.Editor.Data;
using MaruSikaku.Stage;
using System.Runtime.InteropServices;
using System.ComponentModel;

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

            // header部分のボタン処理イベントの登録
            rootVisualElement.Q<Button>("browseButton").clicked += OnBrowse;
            rootVisualElement.Q<Button>("saveButton").clicked += OnSave;
            rootVisualElement.Q<Button>("saveAsButton").clicked += OnSaveAs;
            // tool部分のボタン処理イベントの登録
            rootVisualElement.Q<Button>("selectToolButton").clicked += () => { OnChangeTool(EStageEditMode.Select); };
            rootVisualElement.Q<Button>("eraseToolButton").clicked += () => { OnChangeTool(EStageEditMode.Erase); };
            rootVisualElement.Q<Button>("groundToolButton").clicked += () => { OnChangeTool(EStageEditMode.Ground); };
            rootVisualElement.Q<Button>("springToolButton").clicked += () => { OnChangeTool(EStageEditMode.Spring); };
            rootVisualElement.Q<Button>("fragileToolButton").clicked += () => { OnChangeTool(EStageEditMode.Fragile); };
            rootVisualElement.Q<Button>("movableToolButton").clicked += () => { OnChangeTool(EStageEditMode.Movable); };
            rootVisualElement.Q<Button>("switchToolButton").clicked += () => { OnChangeTool(EStageEditMode.Switch); };
            rootVisualElement.Q<Button>("wallToolButton").clicked += () => { OnChangeTool(EStageEditMode.Wall); };
            rootVisualElement.Q<Button>("maruStartToolButton").clicked += () => { OnChangeTool(EStageEditMode.MaruStart); };
            rootVisualElement.Q<Button>("sikakuStartToolButton").clicked += () => { OnChangeTool(EStageEditMode.SikakuStart); };
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
        }

        private void OnDataSourcePropertyChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            switch (e.propertyName)
            {
                case nameof(StageDesignDataSource.StageData):
                    _gridView.Data = _dataSource.StageData;
                    break;
                case nameof(StageDesignDataSource.EditContext):
                    _gridView.EditContext = _dataSource.EditContext;
                    break;
            }
        }
    }
}