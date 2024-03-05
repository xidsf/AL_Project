using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class PlayerController : Unit
{
    public struct PlayerAttackActionInfo
    {
        public PlayerAttackActionInfo(string _Name = "None", bool _KnockDown = false,  Vector2 _center = default(Vector2), Vector2 _area = default(Vector2), float _coefficient = 1f)
        {
            actionName = _Name;
            isKnockDown = _KnockDown;
            attackAreaCenter = _center;
            attackArea = _area;
            attackCoefficient = _coefficient;
        }
        public string actionName; //���� ���� ���� ���� ���������� ���� �̸�
        public bool isKnockDown; //�˴ٿ� ���� ���� ����
        public Vector2 attackArea; //���� ����
        public Vector2 attackAreaCenter; //������ �߽���ǥ
        public float attackCoefficient; //������ ���
    }

    
    [Header("PlayerAnimator")]
    [SerializeField] private Animator anim; //Warrior�� Animator�� �ٲٱ� ���� serializedField

    //�̵� �� collider��ȯ ��(0: �⺻ 1: �̵���)
    private Vector2[] ColliderOffsets = new Vector2[2];
    private Vector2[] ColliderSizes = new Vector2[2];


    private bool notInputAttack; //�÷��̾��� �����Է��� �����ϴ� ����
    private bool isDash; //�뽬���̸� �������� (���� ����)

    [Header("PlayerDash")]
    [SerializeField] private float dashDelay; //�뽬�� ����ϱ� ���� �ʿ��� ��Ÿ��
    [SerializeField] private float dashSpeed; //�뽬 ���ǵ� ����
    private float currentDashCalculate; //�뽬 ��Ÿ�� ����� ���� ����

    
    //�� ������ serializedField����
    public readonly PlayerAttackActionInfo Attack1 = 
        new PlayerAttackActionInfo("Attack1", false, Vector2.right, new Vector2(2.4f, 2.4f), 1);
    public readonly PlayerAttackActionInfo Attack2 = 
        new PlayerAttackActionInfo("Attack2", false, new Vector2(0.5f, 0.1f), new Vector2(2.35f, 2.8f), 0.8f);
    public readonly PlayerAttackActionInfo Attack3 = 
        new PlayerAttackActionInfo("Attack3", false, Vector2.right, new Vector2(3f, 2.4f), 1.3f);
    public readonly PlayerAttackActionInfo Dash_Attack = 
        new PlayerAttackActionInfo("Dash-Attack", true, Vector2.right, new Vector2(3f, 2.4f), 1.5f);

    
    void Start()
    {
        _lastPos = transform.position.x; //������ Ȯ���� ���� �ʱⰪ


        myRigid = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<BoxCollider2D>(); //�ʿ��� ������Ʈ

        ColliderOffsets[0] = myCollider.offset;
        ColliderSizes[0] = myCollider.size;
        ColliderOffsets[1] = new Vector2(0.0508289337f, -0.369088709f);
        ColliderSizes[1] = new Vector2(1.08665276f, 1.79312909f); //�÷��̾ �޸��� �ʿ��� colliderũ��

        currentDashCalculate = 0; // ��Ÿ�� ��� ���� �ʱ�ȭ
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) SceneManager.LoadScene("Title");

        if(!isDead)
        {
            CalculateImmuneTime();
            TryJump();
            CheckAirPramameter();
            TryDash();
            ChangeAnimationParameter();
            TryAttack();
        }

        AnimCheck();

        TryGroundCheck();
        CalculateDashDelay();
    }

    private void FixedUpdate()
    {
        RestrictVelocity();
        if(!isDead)
        {
            TryMove();
            MoveCheck();
            ChangeCollider();
        }
        
    }



    private void TryMove()
    {
        if(!isAttacking && !isDash && !anim.GetCurrentAnimatorStateInfo(0).IsName("Dash-Attack") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Hurt"))
        {
            Move();
        }
        else if (isDash)
        {
            Dash();
        }
    }

    protected override void Move()
    {
        
        float _positionX = Input.GetAxisRaw("Horizontal");
        if( _positionX != 0 )
        {
            transform.localScale = new Vector3(_positionX, transform.localScale.y, transform.localScale.z);
        }

        if (!isBlocked)
        {
            _positionX = _positionX * __moveSpeed * Time.deltaTime;
            transform.position = new Vector2(transform.position.x + _positionX, transform.position.y);
        }
    }

    private void Dash()
    {
        float _positionX = Input.GetAxisRaw("Horizontal");
        float _dir;
        if( _positionX != 0 )
        {
            transform.localScale = new Vector3(_positionX, transform.localScale.y, transform.localScale.z);
            _dir = _positionX;
        }
        else
        {
            if (transform.localScale.x < 0) _dir = -1;
            else _dir = 1;
        }

        if (!isBlocked)
        {
            _dir = _dir * dashSpeed * Time.deltaTime;
            transform.position = new Vector2(transform.position.x + _dir, transform.position.y);
        }

    }


    private void CalculateDashDelay()
    {
        if(currentDashCalculate > 0)
        {
            currentDashCalculate -= Time.deltaTime;
        }
    }

    private void TryDash()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Hurt")) return;
        if(Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (currentDashCalculate <= 0)
            {
                anim.SetTrigger("Dash");
                isDash = true;
                notInputAttack = true;
                currentDashCalculate = dashDelay;
            }
        }
    }
    

    private void RestrictVelocity() //���� �ӵ� ����
    {
        if(myRigid.velocity.y < -__limitFallingVelocity)
        {
            myRigid.velocity = new Vector2(myRigid.velocity.x, -__limitFallingVelocity);
        }
    }

    private void ChangeCollider() //�����̴� ���� collider��ȯ�� �Լ�
    {
        if(isMoving)
        {
            myCollider.offset = ColliderOffsets[1];
            myCollider.size = ColliderSizes[1];
        }
        else
        {
            myCollider.offset = ColliderOffsets[0];
            myCollider.size = ColliderSizes[0];
        }
    }

    private void TryJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround && !isAttacking)
        {
            Jump();
        }
    }

    private void Jump()
    {
        myRigid.velocity += new Vector2(myRigid.velocity.x, __jumpForce);
    }

    private void CheckAirPramameter() //���� �ִϸ��̼Ǻ�ȯ�� ���� �Ķ���Ͱ� ���� �Լ�
    {
        float _gap = 1f;
        if(myRigid.velocity.y > _gap)
        {
            isRising = true;
        }
        else
        {
            isRising = false;
        }
        if(myRigid.velocity.y < -_gap)
        {
            isFalling = true;
        }
        else
        {
            isFalling = false;
        }
    }

    private void TryAttack() //���� �õ�
    {
        if (Input.GetKeyDown(KeyCode.Z) && !isRising && !isFalling && isGround && !notInputAttack)
        {
            Attack();
        }
    }

    protected override void Attack()
    {
        anim.SetTrigger("Attack");
        UnitAnimationActionController.UnitDamageCalculate = new UnitAnimationActionController.UnitDamageCalculateDelegate(CalculateDamage);
    }

    public void CheckPlayerBlocked(bool _flag) //ĳ���� ���չ����� colliderȮ��
    {
        isBlocked = _flag;
    }

    protected override void AfterDamaged()
    {
        notInputAttack = true;
        anim.SetTrigger("Hurt");
        isImmune = true;
        isDash = false;
        currentImmuneTime = __ImmuneTime;

    }

    protected override void CalculateImmuneTime()
    {
        base.CalculateImmuneTime();
        if(!isImmune)
        {
            notInputAttack = false;
        }
    }

    protected override void Death()
    {
        isDead = true;
        isImmune = true;
        anim.SetTrigger("Death");
        myCollider.offset = new Vector2(0.05f, -1.2f);
        myCollider.size = new Vector2(1.07f, 0.1f);
        StartCoroutine(DeathCoroutine());
    }

    IEnumerator DeathCoroutine()
    {
        yield return new WaitForSeconds(3f);
        transform.position = new Vector3(-6f, 2f, 0);
        __currentHP = __maxHP;
        isImmune = false;
        isDead = false;
        myCollider.offset = new Vector2(0.05f, -0.28f);
        myCollider.size = new Vector2(1.07f, 1.98f);
        anim.SetTrigger("Recover");
    }


    /*
    private void OnDrawGizmos()
    {
        PlayerAttackActionInfo _applyedPlayerAttack = new PlayerAttackActionInfo();

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack1"))
        {
            _applyedPlayerAttack = thePlayerController.Attack1;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack2"))
        {
            _applyedPlayerAttack = thePlayerController.Attack2;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack3"))
        {
            _applyedPlayerAttack = thePlayerController.Attack3;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Dash-Attack"))
        {
            _applyedPlayerAttack = thePlayerController.Dash_Attack;
        }
        if (_applyedPlayerAttack.actionName != "None")
        {
            Gizmos.DrawWireCube(thePlayerController.transform.position + new Vector3(_applyedPlayerAttack.attackAreaCenter.x, _applyedPlayerAttack.attackAreaCenter.y, 0) * transform.localScale.x, _applyedPlayerAttack.attackArea);
        }
    }

    */
}
