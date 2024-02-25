using System.Collections;
using TMPro;
using UnityEngine;


public partial class PlayerController : Unit
{
    public struct PlayerAttackActionInfo
    {
        public PlayerAttackActionInfo(string _Name = "None", float _time = 0,  Vector2 _center = default(Vector2), Vector2 _area = default(Vector2), float _coefficient = 1f)
        {
            actionName = _Name;
            attackTime = _time;
            attackAreaCenter = _center;
            attackArea = _area;
            attackCoefficient = _coefficient;
        }
        public string actionName; //공격 별로 공격 적용 시점구별을 위한 이름
        public float attackTime; //어택 타이밍
        public Vector2 attackArea; //공격 범위
        public Vector2 attackAreaCenter; //공격의 중심좌표
        public float attackCoefficient; //데미지 계수
    }

    [Header("PlayerAnimator")]
    [SerializeField] private Animator anim; //Warrior의 Animator를 바꾸기 위한 serializedField

    //이동 중 collider변환 용(0: 기본 1: 이동중)
    private Vector2[] ColliderOffsets = new Vector2[2];
    private Vector2[] ColliderSizes = new Vector2[2];


    private bool notInputAttack; //플레이어의 공격입력을 제한하는 변수
    private bool isDash; //대쉬중이면 순간무적 (개발 예정)

    [Header("PlayerDash")]
    [SerializeField] private float dashDelay; //대쉬를 사용하기 위해 필요한 쿨타임
    [SerializeField] private float dashSpeed; //대쉬 스피드 변수
    private float currentDashCalculate; //대쉬 쿨타임 계산을 위한 변수

    private Collider2D[] attackedUnits;//공격 당한 모든 유닛들을 저장하기 위한 배열
    private LayerMask EnemyLayer; //적만 공격하게 하기 위한 layermask
    private IEnumerator[] processingCoroutine; //진행중인 코루틴 stopcoroutine하기 위해 저장
    //걍 볼려고 serializedField넣음
    [SerializeField] readonly PlayerAttackActionInfo Attack1 = 
        new PlayerAttackActionInfo("Attack1", 5f / 8f, Vector2.right, new Vector2(2.4f, 2.4f), 1);
    [SerializeField] readonly PlayerAttackActionInfo Attack2 = 
        new PlayerAttackActionInfo("Attack2", 1f / 4f, new Vector2(0.5f, 0.1f), new Vector2(2.35f, 2.8f), 0.8f);
    [SerializeField] readonly PlayerAttackActionInfo Attack3 = 
        new PlayerAttackActionInfo("Attack3", 2f / 9f, Vector2.right, new Vector2(3f, 2.4f), 1.3f);
    [SerializeField] readonly PlayerAttackActionInfo Dash_Attack = 
        new PlayerAttackActionInfo("Dash-Attack", 3f / 11f, Vector2.right, new Vector2(3f, 2.4f), 1.5f);

    
    void Start()
    {
        _lastPos = transform.position.x; //움직인 확인을 위한 초기값

        ClipsDictionaryInitialize();

        myRigid = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<BoxCollider2D>(); //필요한 컴포넌트

        ColliderOffsets[0] = myCollider.offset;
        ColliderSizes[0] = myCollider.size;
        ColliderOffsets[1] = new Vector2(0.0508289337f, -0.369088709f);
        ColliderSizes[1] = new Vector2(1.08665276f, 1.79312909f); //플레이어가 달릴때 필요한 collider크기

        currentDashCalculate = 0; // 쿨타임 계산 변수 초기화
        processingCoroutine = new IEnumerator[2];
        EnemyLayer = LayerMask.GetMask("Enemy");

        
    }

    // Update is called once per frame
    void Update()
    {
        CalculateImmuneTime();
        TryGroundCheck();
        TryJump();
        CheckAirPramameter();
        CalculateDashDelay();
        TryDash();
        ChangeAnimationParameter();
        TryAttack();
        AnimCheck();
    }

    private void FixedUpdate()
    {
        RestrictVelocity();
        TryMove();
        MoveCheck();
        ChangeCollider();
    }

    private void TryMove()
    {
        if(!isAttacking && !isDash)
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
        if(!isBlocked)
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
        if(Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (currentDashCalculate <= 0)
            {
                anim.SetTrigger("Dash");
                isDash = true;
                notInputAttack = true;
                if(processingCoroutine[0] != null)
                {
                    StopCoroutine(processingCoroutine[0]);
                    processingCoroutine[0] = null;
                }
                if(processingCoroutine[1] != null)
                {
                    StopCoroutine(processingCoroutine[1]);
                    processingCoroutine[1] = null;
                }
                currentDashCalculate = dashDelay;
            }
        }
    }
    

    private void RestrictVelocity() //낙하 속도 제한
    {
        if(myRigid.velocity.y < -__limitFallingVelocity)
        {
            myRigid.velocity = new Vector2(myRigid.velocity.x, -__limitFallingVelocity);
        }
    }

    private void ChangeCollider() //움직이는 동안 collider변환용 함수
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

    private void CheckAirPramameter() //공중 애니매이션변환을 위한 파라미터값 변경 함수
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

    private void TryAttack() //공격 시도
    {
        if (Input.GetKeyDown(KeyCode.Z) && !isRising && !isFalling && isGround && !notInputAttack)
        {
            Attack();
        }
    }

    

    protected override void Attack()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Dash") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= dashEndTime) return;
        anim.SetTrigger("Attack");
        if (processingCoroutine[0] == null)
        {
            processingCoroutine[0] = AttackCoroutine();
            StartCoroutine(processingCoroutine[0]);
        }
        else if (processingCoroutine[1] == null && isAttacking)
        {
            processingCoroutine[1] = AttackCoroutine();
        }
    }

    
    IEnumerator AttackCoroutine()
    {
        //1타: 8프레임 중 5 프레임
        //2타: 4 프레임 중 1 프레임
        //3타: 9 프레임 중 2 프레임
        PlayerAttackActionInfo _applyedPlayerAttack = new PlayerAttackActionInfo();
        Unit _tempUnit; //공격당한 유닛들 데미지

        for (int i = 0; i < (1f / Time.deltaTime); i++)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack1"))
            {
                _applyedPlayerAttack = Attack1;
                break;
            }
            else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack2"))
            {
                _applyedPlayerAttack = Attack2;
                break;
            }
            else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack3"))
            {
                _applyedPlayerAttack = Attack3;
                break;
            }
            else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Dash-Attack"))
            {
                _applyedPlayerAttack = Dash_Attack;
                StopCoroutine(_savedDashCoroutine);
                StartCoroutine(DashAttackCoroutine());
                break;
            }
            yield return null;
        }
        
        if(_applyedPlayerAttack.actionName != "None")
        {
            yield return new WaitForSeconds(UnitAnimationClipInfo[_applyedPlayerAttack.actionName] * Mathf.Clamp(_applyedPlayerAttack.attackTime - anim.GetCurrentAnimatorStateInfo(0).normalizedTime, 0, _applyedPlayerAttack.attackTime));
            if (isDash && !anim.GetCurrentAnimatorStateInfo(0).IsName("Dash-Attack"))
            {
                yield break;
            }
            attackedUnits = Physics2D.OverlapBoxAll(transform.position + new Vector3(_applyedPlayerAttack.attackAreaCenter.x, _applyedPlayerAttack.attackAreaCenter.y, 0) * transform.localScale.x, _applyedPlayerAttack.attackArea, 0, EnemyLayer);
            
            foreach (var unit in attackedUnits)
            {
                _tempUnit = unit.GetComponent<Unit>();
                if (_tempUnit != null)
                {
                    _tempUnit.ChangeMyHealth((Mathf.Ceil(-__attackPoint * _applyedPlayerAttack.attackCoefficient * 10)) / 10f);
                }
            }
            yield return new WaitForSeconds(UnitAnimationClipInfo[_applyedPlayerAttack.actionName] * (1f - anim.GetCurrentAnimatorStateInfo(0).normalizedTime));
            
        }
        processingCoroutine[0] = null;
        if (processingCoroutine[1] != null)
        {
            processingCoroutine[0] = processingCoroutine[1];
            processingCoroutine[1] = null;
            StartCoroutine(processingCoroutine[0]);
        }
    }

    IEnumerator DashAttackCoroutine()
    {
        float _temp = UnitAnimationClipInfo["Dash-Attack"] * (1 - (dashEndTime)) - anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
        if (_temp > 0)
        {
            yield return new WaitForSeconds(_temp);
        }
        isDash = false;
    }

    protected override void Death()
    {
        
    }

    /*
    private void OnDrawGizmos()
    {
        PlayerAttackActionInfo _applyedPlayerAttack = new PlayerAttackActionInfo();

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack1"))
        {
            _applyedPlayerAttack = Attack1;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack2"))
        {
            _applyedPlayerAttack = Attack2;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack3"))
        {
            _applyedPlayerAttack = Attack3;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Dash-Attack"))
        {
            _applyedPlayerAttack = Dash_Attack;
        }
        if(_applyedPlayerAttack.actionName != "None")
        {
            Gizmos.DrawWireCube(transform.position + new Vector3(_applyedPlayerAttack.attackAreaCenter.x, _applyedPlayerAttack.attackAreaCenter.y, 0) * transform.localScale.x, _applyedPlayerAttack.attackArea);
        }
    }

    */
}
