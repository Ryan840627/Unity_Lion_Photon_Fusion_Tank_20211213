using UnityEngine;
using Fusion;

public class Bullet : NetworkBehaviour
{
    #region 欄位
    [Header("子彈移動速度"), Range(0, 1000)]
    public float speed = 7.5f;
    [Header("消失子彈時間"), Range(0, 10)]
    public float lifeTime = 5f;
    #endregion

    #region 屬性
    // Networked 連線用屬性資料
    /// <summary>
    /// 存活計時器
    /// </summary>
    [Networked]
    private TickTimer life { get; set; }
    #endregion

    #region 方法
    /// <summary>
    /// 初始資料
    /// </summary>
    public void Init()
    {
        //存活計時器 = 計時器.從秒數建立(連線執行器，存活時間)
        life = TickTimer.CreateFromSeconds(Runner, lifeTime);
    }
    #endregion
}
