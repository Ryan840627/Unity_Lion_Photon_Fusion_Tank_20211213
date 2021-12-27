using UnityEngine;
using UnityEngine.UI;
using Fusion;

/// <summary>
/// �Z�J���
/// �e�ᥪ�k����
/// �����P�o�g�l�u
/// </summary>
public class PlayerControl : NetworkBehaviour
{
    #region ���
    [Header("���ʳt��"), Range(0, 1000)]
    public float speed = 7.5f;
    [Header("�o�g�l�u���j"), Range(0, 1.5f)]
    public float intervalFire = 0.35f;
    [Header("�l�u����")]
    public Bullet bullet;
    [Header("�l�u�ͦ���m")]
    public Transform pointFire;
    [Header("����")]
    public Transform traTower;

    private InputField inputMassage;
    private Text textAllMessage;
    /// <summary>
    /// �s�u���ⱱ�
    /// </summary>
    private NetworkCharacterController ncc;
    #endregion
    #region �ƥ�
    private void Awake()
    {
        ncc = GetComponent<NetworkCharacterController>();
        inputMassage = GameObject.Find("��ѿ�J�ϰ�").GetComponent<InputField>();
        textAllMessage = GameObject.Find("��ѰT��").GetComponent<Text>();
        inputMassage.onEndEdit.AddListener((string message) => { InputMessage(message); });
    }
    private void OnCollisionEnter(Collision collision)
    {
        //�p�G �I�쪫�� �W�r���t�� �l�u �N�R������
        if (collision.gameObject.name.Contains("�l�u"))
        {
            Destroy(gameObject);
        }
    }
    #endregion
    #region �ݩ�
    /// <summary>
    /// �}�j���j�p�ɾ�
    /// </summary>
    public TickTimer inverval { get; set; }
    #endregion

    #region  ��k
    /// <summary>
    /// Fusion �T�w��s�ƥ� ������ Unity FixedUpdate
    /// </summary>
    public override void FixedUpdateNetwork()
    {
        Move();
        Fire();
    }
    /// <summary>
    /// ����
    /// </summary>
    private void Move()
    {
        //�p�G �� ��J���
        if (GetInput(out NetworkInputData dataInput))
        {
            //�s�u���ⱱ�.����(�t��*��V*�s�u�@���ɶ�)
            ncc.Move(speed * dataInput.direction * Runner.DeltaTime);

            // ���o�ƹ��y�� �A�ñNY ���w�P����@�˪������קK����n��
            Vector3 positionMouse = dataInput.positionMouse;
            positionMouse.y = traTower.position.y;
            //���� �� �e��b�V = �ƹ� - �Z�J(�V�q)
            traTower.forward = positionMouse - transform.position;
        }
    }

    private void Fire()
    {
        if (GetInput(out NetworkInputData dataInput))                                   //�p�G ���a����J���
        {
            if (inverval.ExpiredOrNotRunning(Runner))                                    //�p�G �}�j���j�p�ɾ� �L���Ϊ̨S���b����
            {
                if (dataInput.inputFire)                                                  //�p�G ��J��ƬO�}�j����
                {
                    inverval = TickTimer.CreateFromSeconds(Runner, intervalFire);         //�إ߭p�ɾ� 
                    Runner.Spawn(bullet,                                                  // �s�u.�ͦ� (�s�u����A�y�СA���סA��J�v���A�ΦW�禡�A(���澹�A)�ͦ����� => {})
                        pointFire.position, 
                        pointFire.rotation, 
                        Object.InputAuthority,     
                        (runner, objectSpawn) => objectSpawn.GetComponent<Bullet>().Init());
                }
            }
        }
    }
    /// <summary>
    /// ��J�T���P�P�B�T��
    /// </summary>
    /// <param name="message">��J���</param>
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

