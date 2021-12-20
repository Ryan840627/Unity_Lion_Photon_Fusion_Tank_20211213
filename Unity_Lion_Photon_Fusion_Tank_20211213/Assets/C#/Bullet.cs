using UnityEngine;
using Fusion;

public class Bullet : NetworkBehaviour
{
    #region ���
    [Header("�l�u���ʳt��"), Range(0, 1000)]
    public float speed = 7.5f;
    [Header("�����l�u�ɶ�"), Range(0, 10)]
    public float lifeTime = 5f;
    #endregion

    #region �ݩ�
    // Networked �s�u���ݩʸ��
    /// <summary>
    /// �s���p�ɾ�
    /// </summary>
    [Networked]
    private TickTimer life { get; set; }
    #endregion

    #region ��k
    /// <summary>
    /// ��l���
    /// </summary>
    public void Init()
    {
        //�s���p�ɾ� = �p�ɾ�.�q��ƫإ�(�s�u���澹�A�s���ɶ�)
        life = TickTimer.CreateFromSeconds(Runner, lifeTime);
    }
    #endregion
}
