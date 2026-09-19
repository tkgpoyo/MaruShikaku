# アクションパズルゲーム（制作中）

## 概要
2体のキャラクターを交互に操作し，それぞれの能力を使ってステージ内のギミックを解くアクションパズルゲームです．

## 使用技術
- Unity
- C#
- Input System
- Unity UI Toolkit

## 操作方法
|キー|操作|
|----|----|
|L|操作キャラクターの切り替え|
|J|キャラクターごとのメインアクション|
|K|キャラクターごとのサブアクション|

## キャラクターの切り替え
アクティブなキャラクターを切り替えることができる．
ギミックごとにプレイヤーを切り替えてステージを攻略しよう！

<img width="500" height="300" alt="switch_player" src="https://github.com/user-attachments/assets/ab19deb8-f98d-44c1-bd95-ab28ae1f51f6" />


## キャラクターごとの能力
### まる
- ジャンプ能力(Jキー)
  - ジャンプによって障害物の上を通り越せる！
<img width="500" height="300" alt="jump" src="https://github.com/user-attachments/assets/f040b16b-ae91-4549-b678-e1e17eebba46" />

- ダイブ能力(Kキー/実装予定)
  - 地面に潜り込んで，特定の壁をすり抜ける！

### しかく
- プレス能力(Jキー)
  - 勢いよく降下し，下にある特定のブロックを壊せる！
<img width="500" height="300" alt="press_normal" src="https://github.com/user-attachments/assets/6f41a0ce-c9c7-4f94-b4f4-23b87afb15a7" />
<img width="500" height="300" alt="press" src="https://github.com/user-attachments/assets/1b3b80d7-fbb4-4c8a-b75d-d90d701ef33f" />

- タックル能力(Kキー)
  - 特定の壁を壊したり，重いブロックを動かしたりできる！
<img width="500" height="300" alt="tackle_fail" src="https://github.com/user-attachments/assets/f5d3f7ba-31a8-4402-831c-593ba51cf5d8" />
<img width="500" height="300" alt="tackle_success" src="https://github.com/user-attachments/assets/5dbf3ec0-9267-44ff-9ed0-bfa0baaf5605" />

## ギミック
- スイッチ
  - スイッチ上に片方のプレイヤーが乗っかることで，壁を開けることができる！
<img width="500" height="300" alt="switch" src="https://github.com/user-attachments/assets/caf5bc64-dc0c-44b0-8a4a-7967e0281da7" />


## 開発者支援ツール StageDesignWindow
本ゲームのステージを直観的に制作可能な開発者向けのツール「StageDesignWindow」を制作しました．
以下のようにして使用します．
1. Unity Editor上で `Tools/StageDesignWindow` を選択することで StageDesignWindow を表示できる
2. Newボタンを押下することで，新しくステージJSONファイルを作成できる
3. BrowseボタンやLoadボタンを押下することで，既存のステージJSONファイルを読み込み可能
4. StageDesignWindowでは地形編集ボタン（Ground）やステージオブジェクトボタン（Spring,Fragile,Movable,Switch,Wall）を押すことで，地形やギミックを配置できる
5. ステージオブジェクトはSelectボタンによって選択することでプロパティを編集できる
6. Eraseボタンによって地形やギミックを削除できる
7. まるとしかくそれぞれの初期位置をMaruStart，SikakuStartあるいは画面の右側の数値入力によって変更できる
8. SaveボタンによってJSONファイルを上書きでき，Save AsボタンによってJSONファイルを新規保存できる

StageDesignWindowの画像を以下に載せる．
<img width="500" height="300" alt="EditorWindow_sample" src="https://github.com/user-attachments/assets/fa59cfd0-685c-4b0d-bcb8-0c8fe8f3b855" />


## 実装済み
- キャラクター切り替え
- ジャンプ操作
- プレス能力
- タックル能力
- 能力に連動するギミック
- 操作ごとのアニメーション
- ステージ制作Windowの基本機能

## 実装予定
- ダイブ能力とそれを使用するギミック
- ステージJSONからのステージ構築処理
- UI
- サウンド


## 制作目的

本作品は，ゲームプログラマ志望のポートフォリオ作品として制作しています。． 
プレイヤーアクション，ギミック処理，キャラクター切り替え，Unity Editor拡張によるステージ制作支援など，ゲーム制作に必要な実装を幅広く行うことを目的としています．
