using UnityEngine;
using UnityEngine.UI;
using Fusion;

/// <summary>
/// 坦克控制器
/// 前後左右移動
/// 選轉塔與發射子彈
/// </summary>
public class PlayerControl : NetworkBehaviour
{
    #region 欄位
    [Header("移動速度"), Range(0, 1000)]
    public float speed = 7.5f;
    [Header("發射子彈間隔"), Range(0, 1.5f)]
    public float intervalFire = 0.35f;
    [Header("子彈物件")]
    public Bullet bullet;
    [Header("子彈生成位置")]
    public Transform pointFire;
    [Header("砲塔")]
    public Transform traTower;

    private InputField inputMassage;
    private Text textAllMessage;
    /// <summary>
    /// 連線角色控制器
    /// </summary>
    private NetworkCharacterController ncc;
    #endregion
    #region 事件
    private void Awake()
    {
        ncc = GetComponent<NetworkCharacterController>();
        inputMassage = GameObject.Find("聊天輸入區域").GetComponent<InputField>();
        textAllMessage = GameObject.Find("聊天訊息").GetComponent<Text>();
        inputMassage.onEndEdit.AddListener((string message) => { InputMessage(message); });
    }
    private void OnCollisionEnter(Collision collision)
    {
        //如果 碰到物件 名字內含有 子彈 就刪除物件
        if (collision.gameObject.name.Contains("子彈"))
        {
            Destroy(gameObject);
        }
    }
    #endregion
    #region 屬性
    /// <summary>
    /// 開槍間隔計時器
    /// </summary>
    public TickTimer inverval { get; set; }
    #endregion

    #region  方法
    /// <summary>
    /// Fusion 固定更新事件 約等於 Unity FixedUpdate
    /// </summary>
    public override void FixedUpdateNetwork()
    {
        Move();
        Fire();
    }
    /// <summary>
    /// 移動
    /// </summary>
    private void Move()
    {
        //如果 有 輸入資料
        if (GetInput(out NetworkInputData dataInput))
        {
            //連線角色控制器.移動(速度*方向*連線一偵時間)
            ncc.Move(speed * dataInput.direction * Runner.DeltaTime);

            // 取得滑鼠座標 ，並將Y 指定與砲塔一樣的高度避免砲塔歪掉
            Vector3 positionMouse = dataInput.positionMouse;
            positionMouse.y = traTower.position.y;
            //砲塔 的 前方軸向 = 滑鼠 - 坦克(向量)
            traTower.forward = positionMouse - transform.position;
        }
    }

    private void Fire()
    {
        if (GetInput(out NetworkInputData dataInput))                                   //如果 玩家有輸入資料
        {
            if (inverval.ExpiredOrNotRunning(Runner))                                    //如果 開槍間隔計時器 過期或者沒有在執行
            {
                if (dataInput.inputFire)                                                  //如果 輸入資料是開槍左鍵
                {
                    inverval = TickTimer.CreateFromSeconds(Runner, intervalFire);         //建立計時器 
                    Runner.Spawn(bullet,                                                  // 連線.生成 (連線物件，座標，角度，輸入權限，匿名函式，(執行器，)生成物件 => {})
                        pointFire.position, 
                        pointFire.rotation, 
                        Object.InputAuthority,     
                        (runner, objectSpawn) => objectSpawn.GetComponent<Bullet>().Init());
                }
            }
        }
    }
    /// <summary>
    /// 輸入訊息與同步訊息
    /// </summary>
    /// <param name="message">輸入資料</param>
    private void InputMessage(string message)
    {
        if (Object.HasInputAuthority)
        {
            RPC_SendMassage(message);
        }
    }

    [Rpc(RpcSources.InputAuthority,RpcTargets.All)]
    private void RPC_SendMassage(string message,RpcInfo info = default)
    {
        textAllMessage.text += message+"\n";
    }
    #endregion

}

