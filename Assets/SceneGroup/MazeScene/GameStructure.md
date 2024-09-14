# ゲーム構造とクラスの役割  
  
## MazeGameScene (SingletonMonoBehaviour)  
役割:  
- ゲーム全体の状態管理  
- シーンの遷移制御  
- ゲームのセットアップと初期化  
- ゲームの一時停止/再開  
- 他の主要コンポーネント（MazeManager, PlayerController, EnemyManager, UIManager）の参照を保持  
  
主な機能:  
- ゲームの開始、終了、リスタート  
- ポーズメニューの制御  
- ゲームオーバー/クリア時の処理  
- シーン間のデータ受け渡し  
  
## MazeManager (MonoBehaviour)  
役割:  
- 迷路の状態管理  
- 迷路上の要素（壁、通路、アイテム、敵）の位置情報管理  
- 迷路生成ロジックの実行  
  
主な機能:  
- 迷路データの生成と保持  
- 特定の位置の状態照会（壁か通路か、アイテムの有無など）  
- プレイヤーや敵の移動可能判定  
- 視界計算のための迷路情報提供  
  
## SoundManager (SingletonMonoBehaviour)  
役割:  
- ゲーム内の全ての音声を管理  
- BGMと効果音の再生制御  
  
主な機能:  
- 各種効果音の再生  
- BGMの再生、停止、フェード  
- 音量調整  
- 音声リソースの事前ロードと管理  
  
## PlayerController (MonoBehaviour)  
役割:  
- プレイヤーの状態と行動を管理  
  
主な機能:  
- プレイヤーの移動制御  
- プレイヤーのステータス（MP、視界など）管理  
- アイテムの使用  
- 敵との衝突検出  
  
## EnemyManager (MonoBehaviour)  
役割:  
- 敵キャラクターの生成と管理  
  
主な機能:  
- 敵の生成とAI制御  
- 敵の移動ロジック  
- プレイヤーとの相互作用  
  
## UIManager (MonoBehaviour)  
役割:  
- ゲームUI全体の管理と表示  
  
主な機能:  
- プレイヤーステータスの表示  
- ミニマップの表示  
- メニュー画面の制御  
- ゲームオーバー/クリア画面の表示  
  
## MazeGameStats (SingletonMonoBehaviour)  
役割:  
- ゲームパラメータの管理と提供  
  
主な機能:  
- XMLからのゲームパラメータ読み込み  
- 他のクラスへのパラメータ提供  
  
## クラス間の主要な相互作用  
1. MazeGameSceneが全体の調整役となり、他のコンポーネントを初期化・制御します。  
2. MazeManagerは迷路データを生成・管理し、PlayerControllerとEnemyManagerに移動可能判定を提供します。  
3. PlayerControllerとEnemyManagerは、MazeManagerを介して迷路上の移動と相互作用を行います。  
4. UIManagerはMazeGameScene、PlayerController、EnemyManagerから情報を取得し、画面に表示します。  
5. SoundManagerは各コンポーネントからの要求に応じて音声を再生します。  
6. 全てのクラスは必要に応じてMazeGameStatsからゲームパラメータを取得します。  
  
## 初期化順序  
1. MazeGameStats  
2. SoundManager  
3. MazeGameScene  
4. MazeManager  
5. PlayerController  
6. EnemyManager  
7. UIManager  
  
注意: MazeGameStatsとSoundManagerは他のコンポーネントより先に初期化される必要があります。