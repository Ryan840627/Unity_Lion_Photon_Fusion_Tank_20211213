using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;

namespace Ryan
{
    //INetworkRunnerCallbacks 連線執行器回呼介面，Runner 執行器處理行為後 會回呼此介面的方法
    /// <summary>
    /// 連線基底生成器
    /// </summary>
    public class BasicSpawner : MonoBehaviour,INetworkRunnerCallbacks
    {
        #region 欄位
        [Header("創建與加入房間的欄位")]
        public InputField inputFieldCreateRoom;
        public InputField InputFieldJoinRoom;
        [Header("玩家控制物件 - 連線預置物")]
        public NetworkPrefabRef goPlayer;
        [Header("畫布連線")]
        public GameObject goCanvas;
        [Header("版本文字")]
        public Text textVersion;
        [Header("玩家生成位置陣列")]
        public Transform[] traSpawnPoints;


        /// <summary>
        /// 玩家輸入的房間名稱
        /// </summary>
        private string roomNameInput;
        /// <summary>
        /// 連線執行器
        /// </summary>
        private NetworkRunner runner;
        private string  Version = "Ryan Copyright  2021  |   Version ";

        /// <summary>
        /// 玩家資料集合 : 玩家參考資訊，玩家連線物件
        /// </summary>
        private Dictionary<PlayerRef, NetworkObject> players = new Dictionary<PlayerRef, NetworkObject>();
        #endregion

        #region 方法
        private void Awake()
        {
            textVersion.text = Version + Application.version;
        }
        /// <summary>
        /// 按鈕點擊時呼叫 : 創建房間
        /// </summary>
        public void BtnCreateRoom()
        {
            roomNameInput = inputFieldCreateRoom.text;
            print(inputFieldCreateRoom.text);
            StartGame(GameMode.Host);
        }
        /// <summary>
        /// 按鈕點擊時呼叫 : 加入房間
        /// </summary>
        public void BtnJoinRoom()
        {
            roomNameInput = InputFieldJoinRoom.text;
            print(InputFieldJoinRoom.text);
            StartGame(GameMode.Client);
        }

        //async 非同步處理 : 執行系統時處理連線
        /// <summary>
        /// 開始連線遊戲
        /// </summary>
        /// <param name="mode">連線模式(主機、客戶)</param>
        public async void StartGame(GameMode mode)
        {
            print("<color=yellow>開始連線</color>");
            runner = gameObject.AddComponent<NetworkRunner>();       // 連線執行器 = 添加元件<連線執行器>
            runner.ProvideInput = true;                     // 連線執行器.是否提供輸入 = 是

            //等待連線 : 遊戲連線模式、房間名稱、連線後場景、場景管理器
            await runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName =roomNameInput,
                Scene = SceneManager.GetActiveScene().buildIndex,
                SceneObjectProvider = gameObject.AddComponent<NetworkSceneManagerDefault>()
            }
            );

            print("<color=yellow>連線完成</color>");
            goCanvas.SetActive(false);
        }
        #endregion
        #region Fusion 回呼函式區域
        public void OnConnectedToServer(NetworkRunner runner)
        {
            
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
            
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
            
        }

        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
            
        }

        /// <summary>
        /// 玩家連線輸入行為
        /// </summary>
        /// <param name="runner">連線執行器</param>
        /// <param name="input">輸入資訊</param>
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            NetworkInputData inputData = new NetworkInputData();                        //新增 連線輸入資料 結構(與另一本腳本包含結構的資料名稱相同)

            #region 自訂輸入案件與移動資訊
            if (Input.GetKey(KeyCode.W)) inputData.direction += Vector3.forward;        //W 前
            if (Input.GetKey(KeyCode.S)) inputData.direction += Vector3.back;           //S 後
            if (Input.GetKey(KeyCode.A)) inputData.direction += Vector3.left;           //A 左
            if (Input.GetKey(KeyCode.D)) inputData.direction += Vector3.right;          //D 右

            inputData.inputFire = Input.GetKey(KeyCode.Mouse0);                         //左鍵 發射
            #endregion
            #region 滑鼠座標處理
            inputData.positionMouse = Input.mousePosition;                                      // 取得滑鼠座標
            inputData.positionMouse.z = 63;                                                     //設定 滑鼠座標 Z 軸 - 可以打到3D物件，大於攝影機的Y

            Vector3 mouseToWorld = Camera.main.ScreenToWorldPoint(inputData.positionMouse);     //透過API將滑鼠轉為世界座標
            inputData.positionMouse = mouseToWorld;                                             //儲存轉換後的滑鼠座標
            #endregion
            input.Set(inputData);                                                                // 輸入資訊.設定(連線輸入資料)
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
            
        }

 

        /// <summary>
        /// 當玩家成功加入房間後
        /// </summary>
        /// <param name="runner">連線執行器</param>
        /// <param name="player">玩家資訊</param>
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            // 隨機生成點 = UnityEngine 的隨機範圍(0、生成位置數量)
            int randomSpawnPoint = UnityEngine.Random.Range(0, traSpawnPoints.Length);
            // 連線執行器 生成 (物件、座標、角度、玩家資訊)
            NetworkObject playerNetworkObject = runner.Spawn(goPlayer, traSpawnPoints[randomSpawnPoint].position, Quaternion.identity, player);
            //將玩家參考資訊與玩家連線物件添加到字典集合內
            players.Add(player, playerNetworkObject);
        }

        /// <summary>
        /// 當玩家退出房間後執行
        /// </summary>
        /// <param name="runner"></param>
        /// <param name="player"></param>
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            // 如果 離開的玩家連線物件 存在 就刪除
            if (players.TryGetValue(player,out NetworkObject playerNetworkObject))
            {
                runner.Despawn(playerNetworkObject);                // 連線執行器.取消生成(該玩家連線物件移除)
                players.Remove(player);                             // 玩家集合，移除(該玩家)
            }
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
        {
            
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
            
        }
        #endregion
    }
}

