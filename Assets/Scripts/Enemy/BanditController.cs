using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public class BanditController : Unit
{
    [Header("BanditField")]
    [SerializeField] private TextMeshProUGUI _HPText; //HP표시를 위한 텍스트
    [SerializeField] private Material TransparentMaterial; //적이 반투명해지는 메테리얼
    [SerializeField] private GameObject Holder; //적을 좌우 바꾸기 위한 게임 오프젝트 홀더
    private Material originMaterial; //원래 사용중이던 메테리얼
    private PlayerController player;

    
    [SerializeField] private bool isCongnizingPlayer; //적이 플레이어 인식을 했는지 알기위한 변수
    private bool isInEnemyArea = true; //bandit이 공격 영역 안에 있는지 확인
    [HideInInspector] public bool isInRestArea = true; //휴식 지역 내에 있는지 확인
    [HideInInspector] public bool isInTryAttackArea = false;

    private bool isRightBlocked;
    private bool isLeftBlocked; //적은 플레이어를 보며 뒤로도 이동 가능하기 때문에 양쪽에 콜라이더 제작

    private RestArea RestArea; //휴식 지역을 받아오는 변수

    [Header("CombatStatus")]
    [SerializeField] private float combatIdleSpeed; //전투중일 때 이동속도
    [SerializeField] private float combatToIdleTime; //전투중에서 일반 상태로 돌아오는 시간
    [SerializeField] private float combatDistance; //지정한 범위 안이면 조금씩 물러나고 아니면 조금씩 다가감
    [SerializeField] private float AttackCoolTime; //공격 쿨타임
    private const float combatDistanceArea = 0.05f; //범위 지정 안하면 계속 부들부들거림
    private float currentToIdleTime;
    private float currentAttackCoolTime = 2f;
    private float moveDirection;
    private float currentLocalScale;
    

    [SerializeField] private Animator anim; //필요한 컴포넌트
    [SerializeField] private SpriteRenderer mySprite; //원래 스프라이트

    private void Start()
    {
        player = FindObjectOfType<PlayerController>(); //플레이어는 1명밖에 없음
        RestArea = GetComponentInParent<CheckArea>().GetComponentInChildren<RestArea>();
        myRigid = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<BoxCollider2D>(); //필요한 컴포넌트

        originMaterial = mySprite.material;

        currentLocalScale = Holder.transform.localScale.x;
        moveDirection = Holder.transform.localScale.x;

    }

    // Update is called once per frame
    void Update()
    {
        if (!isDead)
        {
            CalculateImmuneTime();
            TryGroundCheck();
            ChangeTextHP();
            ChangeMoveDirection();
            CalculateCongnizeTime();
            CalculateAttackCoolTime();
        }

        AnimCheck();
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        Move();
        MoveCheck();
        
    }


    private void ChangeMoveDirection()
    {
        Vector2 _Player_Bandit_Vecter = player.transform.position - transform.position;
        float _distance;
        float _direction;
        if (isCongnizingPlayer && isInEnemyArea)
        {
            _distance = _Player_Bandit_Vecter.magnitude;
            //Debug.Log(_Player_Bandit_Vecter.x);
            if(_Player_Bandit_Vecter.x > 0)
            {
                _direction = 1;
                currentLocalScale = -1;
            }
            else if (_Player_Bandit_Vecter.x < 0)
            {
                _direction = -1;
                currentLocalScale = 1;
            }
            else
            {
                moveDirection = 0;
                currentLocalScale = Holder.transform.localScale.x;
                return;
            }
            if(currentAttackCoolTime > 0)
            {
                if (_distance > combatDistance + combatDistanceArea) moveDirection = _direction;
                else if (_distance < combatDistance - combatDistanceArea) moveDirection = -_direction;
                else moveDirection = 0;
            }
            else
            {
                moveDirection = _direction;
            }
        }
        else if(!isCongnizingPlayer && !isInRestArea)
        {
            if ((RestArea.transform.position - transform.position).x > 0)
            {
                moveDirection = 1;
                currentLocalScale = -1;
            }
            else if ((RestArea.transform.position - transform.position).x < 0)
            {
                moveDirection = -1;
                currentLocalScale = 1;
            }
            else
            {
                moveDirection = 0;
                currentLocalScale = Holder.transform.localScale.x;
            }
        }
        else
        {
            moveDirection = 0;
        }
    }

    protected override void Move()
    {
        float _move;
        if (isImmune || isAttacking) return;
        if (moveDirection == 0) return;
        if (isInEnemyArea && isCongnizingPlayer && currentAttackCoolTime <= 0)
        {
            _move = moveDirection * __moveSpeed;
        }
        else if (isInEnemyArea && isCongnizingPlayer && currentAttackCoolTime > 0)
        {
            
            _move = moveDirection * combatIdleSpeed;
        }
        else if ((RestArea.transform.position - transform.position).x != 0)
        {
            _move = moveDirection * __moveSpeed * 1.5f;
        }
        else _move = 0;
        if (isRightBlocked && _move > 0) return;
        else if(isLeftBlocked && _move < 0) return;
        transform.position = new Vector2(transform.position.x + _move * Time.deltaTime, transform.position.y);
        
        if (currentLocalScale != 0 && currentLocalScale != Holder.transform.localScale.x)
        {
            Holder.transform.localScale = new Vector3(currentLocalScale, 1, 1);
        }
    }

    public bool CheckRestHeal()
    {
        if (!isCongnizingPlayer && isInRestArea)
        {
            if(__currentHP < __maxHP)
            {
                ChangeMyHealth(5);
                return true;
            }
        }
        return false;
    }

    private void CalculateCongnizeTime()
    {
        if(isCongnizingPlayer && currentToIdleTime > 0)
        {
            currentToIdleTime -= Time.deltaTime;
            if(currentToIdleTime <= 0)
            {
                isCongnizingPlayer = false;
                currentAttackCoolTime = 2f;
            }
        }
    }

    private void CalculateAttackCoolTime()
    {
        if(isCongnizingPlayer && currentAttackCoolTime > 0)
        {
            currentAttackCoolTime -= Time.deltaTime;
        }
        if(currentAttackCoolTime <= 0 && isInTryAttackArea && !isAttacking)
        {
            Attack();
        }
    }

    protected override void Attack()
    {
        anim.SetTrigger("Attack");
        isAttacking = true;
        currentAttackCoolTime = AttackCoolTime;
    }


    protected override void CalculateImmuneTime()
    {
        if (isDead) return;
        base.CalculateImmuneTime();
    }

    protected override void AfterDamaged()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            StartCoroutine(SuperArmorDamagedCoroutine());
            return;
        }
        anim.SetTrigger("Hurt");
        PlayerCongnize();
    }

    IEnumerator SuperArmorDamagedCoroutine()
    {
        mySprite.material = TransparentMaterial;
        yield return new WaitForSeconds(0.1f);
        mySprite.material = originMaterial;
    }

    public void PlayerCongnize()
    {
        isCongnizingPlayer = true;
        currentToIdleTime = combatToIdleTime;

    }

    private void ChangeTextHP()
    {
        _HPText.text = __currentHP.ToString() + " / " + __maxHP.ToString();
    }

    protected override void Death()
    {
        isDead = true;
        isImmune = true;
        anim.SetTrigger("Death");
        StartCoroutine(DeathCoroutine());
    }

    IEnumerator DeathCoroutine()
    {
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(0.2f);
            mySprite.material = TransparentMaterial;
            yield return new WaitForSeconds(0.2f);
            mySprite.material = originMaterial;
        }
        anim.SetTrigger("Recover");
        yield return new WaitForSeconds(UnitAnimationClipInfo["LightBandit_Recover"]);
        __currentHP = __maxHP;
        isImmune = false;
        isDead = false;
    }

    private void AnimCheck()
    {
        anim.SetBool("Grounded", isGround);
        anim.SetBool("Combat", (isCongnizingPlayer && currentAttackCoolTime > 0));
        anim.SetBool("Move", ((isCongnizingPlayer && currentAttackCoolTime <= 0) || isMoving && !isCongnizingPlayer));
    }

    public void CheckEnemyBlocked(string _dir, bool _flag)
    {
        if (_dir == "Right") isRightBlocked = _flag;
        else if (_dir == "Left") isLeftBlocked = _flag;
    }

    public void AttackArea()
    {
        PlayerController _temp; //플레이어 공격이 될 경우 담길 변수
        Collider2D[] _colliders;
        _colliders = Physics2D.OverlapBoxAll(transform.position + Vector3.left * 0.5f * Holder.transform.localScale.x, new Vector2(1.6f, 2f), 0);
        foreach (var unit in _colliders)
        {
            _temp = unit.GetComponent<PlayerController>();
            if (_temp != null)
            {
                _temp.ChangeMyHealth(-CalculateDamage(1));
                StartCoroutine(CheckPlayerDeath(_temp));
            }
        }
        
    }

    IEnumerator CheckPlayerDeath(PlayerController _player)
    {
        yield return new WaitForSeconds(1);
        if (_player.isDead) isCongnizingPlayer = false;
    }

    public void CancelIsAttacking()
    {
        isAttacking = false;
    }

    public bool GetIsCongnizingPlayer()
    {
        return isCongnizingPlayer;
    }

}
