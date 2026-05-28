using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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

        [MenuItem("Tools/StageDesign")]
        public static void Open()
        {
            GetWindow<StageDesignWindow>("StageDesign");    // Windowを生成
        }

        void CreateGUI()
        {
            // UXML, USSをロード
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(DIRECTORY_PATH, UXML_FILE_NAME));
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(DIRECTORY_PATH, USS_FILE_NAME));
            uxml.CloneTree(rootVisualElement);          // UXMLから画面を作成
            rootVisualElement.styleSheets.Add(uss);     // USSを追加

            // データソースの設定
            rootVisualElement.Q<VisualElement>("stageEditorRoot").dataSource = _dataSource;
            // header部分のボタン処理イベントの登録
            rootVisualElement.Q<Button>("browseButton").clicked += OnBrowse;
        }

        private void OnBrowse()
        {
            var path = EditorUtility.OpenFilePanel("JSONファイルを選択", Application.dataPath, "json"); // JSONファイルを取得
            if (string.IsNullOrWhiteSpace(path)) { return; }

            _dataSource.JsonPath = path;                        // ファイルパスを設定
        }
    }
}