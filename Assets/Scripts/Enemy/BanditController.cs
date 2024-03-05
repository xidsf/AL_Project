using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public class BanditController : Unit
{
    [Header("BanditField")]
    [SerializeField] private TextMeshProUGUI _HPText; //HPǥ�ø� ���� �ؽ�Ʈ
    [SerializeField] private Material TransparentMaterial; //���� ������������ ���׸���
    [SerializeField] private GameObject Holder; //���� �¿� �ٲٱ� ���� ���� ������Ʈ Ȧ��
    private Material originMaterial; //���� ������̴� ���׸���
    private PlayerController player;

    
    [SerializeField] private bool isCongnizingPlayer; //���� �÷��̾� �ν��� �ߴ��� �˱����� ����
    private bool isInEnemyArea = true; //bandit�� ���� ���� �ȿ� �ִ��� Ȯ��
    [HideInInspector] public bool isInRestArea = true; //�޽� ���� ���� �ִ��� Ȯ��
    [HideInInspector] public bool isInTryAttackArea = false;

    private bool isRightBlocked;
    private bool isLeftBlocked; //���� �÷��̾ ���� �ڷε� �̵� �����ϱ� ������ ���ʿ� �ݶ��̴� ����

    private RestArea RestArea; //�޽� ������ �޾ƿ��� ����

    [Header("CombatStatus")]
    [SerializeField] private float combatIdleSpeed; //�������� �� �̵��ӵ�
    [SerializeField] private float combatToIdleTime; //�����߿��� �Ϲ� ���·� ���ƿ��� �ð�
    [SerializeField] private float combatDistance; //������ ���� ���̸� ���ݾ� �������� �ƴϸ� ���ݾ� �ٰ���
    [SerializeField] private float AttackCoolTime; //���� ��Ÿ��
    private const float combatDistanceArea = 0.05f; //���� ���� ���ϸ� ��� �ε�ε�Ÿ�
    private float currentToIdleTime;
    private float currentAttackCoolTime = 2f;
    private float moveDirection;
    private float currentLocalScale;
    

    [SerializeField] private Animator anim; //�ʿ��� ������Ʈ
    [SerializeField] private SpriteRenderer mySprite; //���� ��������Ʈ

    private void Start()
    {
        player = FindObjectOfType<PlayerController>(); //�÷��̾�� 1��ۿ� ����
        RestArea = GetComponentInParent<CheckArea>().GetComponentInChildren<RestArea>();
        myRigid = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<BoxCollider2D>(); //�ʿ��� ������Ʈ

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
        PlayerController _temp; //�÷��̾� ������ �� ��� ��� ����
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
