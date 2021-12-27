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

    /// <summary>
    /// Network Behaviour �����O���Ѩƥ�
    /// �s�u�ΩT�wFPS
    /// </summary>
    public override void FixedUpdateNetwork()
    {
        //Runner �s�u���澹
        //Expired()�O�_���
        //Despawn()�R��
        //Object �s�u����
        //Runner.Deltatime �s�u���@�����ƹ�
        //�p�G �p�ɾ� ���(���s) �N�R�� ���s�u����
        if (life.Expired(Runner)) Runner.Despawn(Object);
        else transform.Translate(0, 0, speed * Runner.DeltaTime);
    }
    #endregion
}
