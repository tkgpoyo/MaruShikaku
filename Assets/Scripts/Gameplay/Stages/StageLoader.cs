using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MaruSikaku.Gameplay.Players;
using MaruSikaku.Gameplay.Stages.Gimmicks;
using MaruSikaku.Stage;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Tilemaps;

namespace MaruSikaku.Gameplay
{
    public class StageLoader : MonoBehaviour
    {
        /// <summary>地面のタイル</summary>
        [SerializeField] private TileBase _groundTile;
        /// <summary>丸キャラクターのPrefab</summary>
        [SerializeField] private GameObject _maruPrefab;
        /// <summary>四角キャラクターのPrefab</summary>
        [SerializeField] private GameObject _sikakuPrefab;
        /// <summary>壊れるブロックのPrefab</summary>
        [SerializeField] private GameObject _fragilePrefab;
        /// <summary>動くブロックのPrefab</summary>
        [SerializeField] private GameObject _pressablePrefab;
        /// <summary>スイッチのPrefab</summary>
        [SerializeField] private GameObject _switchPrefab;
        /// <summary>壁のPrefab</summary>
        [SerializeField] private GameObject _wallPrefab;
        /// <summary>バネのPrefab</summary>
        [SerializeField] private GameObject _springPrefab;

        private GameObject _stageRoot;
        private GameObject _playerRoot;
        private GameObject _gridRoot;
        private GameObject _groundRoot;
        private GameObject _fragileParent;
        private GameObject _movableParent;
        private GameObject _springParent;
        private GameObject _switchParent;
        private GameObject _wallParent;
        private Tilemap _tilemap;

        public void LoadStage(string jsonPath, out PlayerController[] players)
        {
            if (string.IsNullOrEmpty(jsonPath) || !File.Exists(jsonPath))       // JSONファイルパスが存在しない場合
            {
                throw new ArgumentException($"指定されたステージファイルのJSONパスが存在しません({jsonPath})．");
            }

            var json = File.ReadAllText(jsonPath);                              // jsonテキストを取得
            var stageSaveData = JsonUtility.FromJson<StageSaveData>(json);      // ステージ保存情報をロード
            // ヒエラルキー構成を構築
            BuildHierarchy();
            // キャラクターを初期配置
            var maru = Instantiate(_maruPrefab, GetActualPos(stageSaveData.MaruInitPos), Quaternion.identity, _playerRoot.transform);           // 丸キャラクターの生成
            var sikaku = Instantiate(_sikakuPrefab, GetActualPos(stageSaveData.SikakuInitPos), Quaternion.identity, _playerRoot.transform);     // 四角キャラクターの生成
            players = new PlayerController[]
            {
                maru.GetComponent<PlayerController>(),
                sikaku.GetComponent<PlayerController>(),
            };                                                                                              // プレイヤー情報を取得
            // 地面を生成
            InstantiateGround(stageSaveData);
            // ステージオブジェクトを生成
            var stageObjMap = new Dictionary<int, (EStageObjectType, StageObjectSaveData, GameObject)>();   // IDとGameObjectとの対応表
            foreach (var stageObj in stageSaveData.StageObjects)
            {
                var gameObject = InstantiateStageObject(stageObj);                                          // ステージオブジェクトのGameObjectを取得
                stageObjMap.Add(stageObj.Id, (stageObj.Type, stageObj, gameObject));                        // IDとGameObjectとの対応表に登録
            }
            // ステージオブジェクト間の関係を設定
            BuildStageObjectRelation(stageObjMap);
        }

        /// <summary>
        /// ヒエラルキー構造を構築します．
        /// </summary>
        private void BuildHierarchy()
        {
            // オブジェクトが残っている場合，削除する
            // Trialシーンの Destroy テストによって，親オブジェクト(_stageRoot, _playerRoot)が Destroy されれば子オブジェクトが自動的に Destroy される
            if (_stageRoot != null)                 // ステージの親オブジェクトが残っている場合は
            {
                DestroyImmediate(_stageRoot);       // 即座にDestroy
            }
            if (_playerRoot != null)                // プレイヤーの親オブジェクトが残っている場合は
            {
                DestroyImmediate(_playerRoot);      // 即座にDestroy
            }

            // ヒエラルキー構成を作成
            _stageRoot = new GameObject("Stage");                   // ステージの親オブジェクトを作成
            _playerRoot = new GameObject("Player");                 // プレイヤーの親オブジェクトを作成
            _gridRoot = new GameObject("Grid");                     // グリッドのオブジェクト
            _gridRoot.transform.parent = _stageRoot.transform;      // ステージの親オブジェクトの子オブジェクトとする
            _gridRoot.AddComponent<Grid>();
            _groundRoot = new GameObject("Ground");                 // 地面関連の親となるオブジェクト
            _groundRoot.transform.parent = _gridRoot.transform;     // グリッドのオブジェクトの子オブジェクトとする
            _tilemap = _groundRoot.AddComponent<Tilemap>();         // Tilemapを取得
            _groundRoot.AddComponent<TilemapRenderer>();            // Rendererをアタッチ
            _fragileParent = new GameObject("Fragile");             // 壊れるブロックの親オブジェクト
            _fragileParent.transform.parent = _stageRoot.transform; // ステージの親オブジェクトの子オブジェクトとする
        }

        private void InstantiateGround(StageSaveData stageData)
        {
            var groundCells = new HashSet<Vector2Int>();
            foreach (var terrain in stageData.TerrainCells)
            {
                if (terrain.Type != ETerrainType.Ground) { continue; }

                groundCells.Add(terrain.Pos);
                _tilemap.SetTile(new Vector3Int(terrain.Pos.x, terrain.Pos.y, 0), _groundTile);
            }

            _groundRoot.layer = LayerMask.NameToLayer(MaruSikakuConsts.GROUND_LAYER_NAME);
            foreach (var contour in BuildGroundContours(groundCells))
            {
                var edgeCollider = _groundRoot.AddComponent<EdgeCollider2D>();
                edgeCollider.points = contour;
            }
        }

        /// <summary>
        /// 地面の輪郭を構築します．
        /// </summary>
        /// <param name="groundCells">地面の座標集合</param>
        /// <returns>地面の輪郭</returns>
        private static List<Vector2[]> BuildGroundContours(HashSet<Vector2Int> groundCells)
        {
            var boundary = new HashSet<(Vector2Int Start, Vector2Int End)>();
            foreach (var cell in groundCells)
            {
                var bottomLeft = cell;
                var topLeft = cell + Vector2Int.up;
                var topRight = cell + Vector2Int.one;
                var bottomRight = cell + Vector2Int.right;

                /* 辺を追加していく
                   外周を時計回りに沿うように辺を追加する
                   囲まれた内部に関しては反時計回りになる
                 */
                if (!groundCells.Contains(cell + Vector2Int.left))  { boundary.Add((bottomLeft,  topLeft)); }       // 左に地面がなければ左の辺を追加
                if (!groundCells.Contains(cell + Vector2Int.up))    { boundary.Add((topLeft,     topRight)); }      // 上に地面がなければ上の辺を追加
                if (!groundCells.Contains(cell + Vector2Int.right)) { boundary.Add((topRight,    bottomRight)); }   // 右に地面がなければ右の辺を追加
                if (!groundCells.Contains(cell + Vector2Int.down))  { boundary.Add((bottomRight, bottomLeft)); }    // 下に地面がなければ下の辺を追加
            }

            // 上で集めた辺を合併させていく
            var remaining = new HashSet<(Vector2Int Start, Vector2Int End)>(boundary);      // チェックする辺
            var contours = new List<Vector2[]>();                                           // 輪郭
            foreach (var first in boundary)
            {
                if (!remaining.Contains(first)) { continue; }

                var vertices = new List<Vector2Int>();
                var current = first;
                do
                {
                    remaining.Remove(current);
                    vertices.Add(current.Start);
                    var direction = current.End - current.Start;

                    var right = new Vector2Int(direction.y, -direction.x);
                    var left = new Vector2Int(-direction.y, direction.x);
                    var next = current.End + right;
                    if (!boundary.Contains((current.End, next)))
                    {
                        next = current.End + direction;
                        if (!boundary.Contains((current.End, next)))
                        {
                            next = current.End + left;
                        }
                    }
                    current = (current.End, next);
                }
                while (current != first);

                // 同一直線上の中間頂点だけを除き，角と閉じた輪郭を維持する．
                var points = new List<Vector2>();
                for (int i = 0; i < vertices.Count; i++)
                {
                    var previous = vertices[(i + vertices.Count - 1) % vertices.Count];
                    var vertex = vertices[i];
                    var next = vertices[(i + 1) % vertices.Count];
                    if (vertex - previous != next - vertex)
                    {
                        points.Add(new Vector2(vertex.x, vertex.y));
                    }
                }
                points.Add(points[0]);
                contours.Add(points.ToArray());
            }
            return contours;
        }

        private GameObject InstantiateStageObject(StageObjectSaveData stageObj)
        {
            var prefab = SelectPrefab(stageObj.Type);       // Prefab取得
            var gameObj = Instantiate(prefab, GetActualPos(stageObj.Pos), Quaternion.identity, _stageRoot.transform);   // ステージオブジェクト生成
            if (stageObj.Type is EStageObjectType.Wall)     // 壁の場合
            {
                // 壁の長さの設定が必要
                var wallController = gameObj.GetComponent<OpenableWall>();
                wallController.YLength = stageObj.YLength;
            }
            return gameObj;
        }

        private GameObject SelectPrefab(EStageObjectType type)
        {
            return type switch
            {
                EStageObjectType.Fragile => _fragilePrefab,
                EStageObjectType.Movable => _pressablePrefab,
                EStageObjectType.Spring => _springPrefab,
                EStageObjectType.Switch => _switchPrefab,
                EStageObjectType.Wall => _wallPrefab,
                _ => throw new NotImplementedException($"型{nameof(EStageObjectType)}の値{type}は未実装です．")
            };
        }

        /// <summary>
        /// ステージオブジェクト間の関係を構築します．
        /// 例：スイッチと対応する壁の設定
        /// </summary>
        private void BuildStageObjectRelation(Dictionary<int, (EStageObjectType type, StageObjectSaveData saveData, GameObject obj)> stageObjMap)
        {
            // スイッチと対応する壁の設定
            {
                foreach (var (type, saveData, obj) in stageObjMap.Where(m => m.Value.type is EStageObjectType.Wall).Select(w => w.Value))    // 全ての壁オブジェクトを見る
                {
                    if (!stageObjMap.TryGetValue(saveData.SwitchId, out var switchInfo) ||
                        switchInfo.type is not EStageObjectType.Switch)                         // 壁に対応するスイッチが存在しない場合
                    {
                        throw new Exception($"壁(ID:{saveData.Id})に対応するスイッチ(ID:{saveData.SwitchId})が存在しません．");
                    }

                    var switchController = switchInfo.obj.GetComponent<PressureSwitch>();       // スイッチのControllerを取得
                    switchController.RegisterWall(obj.GetComponent<OpenableWall>());   // 壁を登録
                }
            }
        }

        private Vector2 GetActualPos(Vector2Int pos)
        {
            return new Vector2(pos.x, pos.y) + TilemapOrigin;
        }

        private Vector2 TilemapOrigin => _tilemap.GetCellCenterWorld(Vector3Int.zero);
    }
}