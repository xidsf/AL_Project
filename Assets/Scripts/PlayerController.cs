using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Animations;
using UnityEditor;
using UnityEngine.Rendering;


public partial class PlayerController : Unit
{
    enum PlayerAction
    {
        Idle = 0, Fall, Jump, Rise,  Run, Dash, Attack1, Attack2, Attack3
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

    private Collider2D[] attackedUnits;//���� ���� ��� ���ֵ��� �����ϱ� ���� �迭
    private LayerMask EnemyLayer; //���� �����ϰ� �ϱ� ���� layermask
    [SerializeField] private Vector2 attackArea; //���� ����
    [SerializeField] private Vector2 attackAreaCenter;
    private IEnumerator[] processingCoroutine; //�������� �ڷ�ƾ stopcoroutine�ϱ� ���� ����

    void Start()
    {
        _lastPos = transform.position.x; //������ Ȯ���� ���� �ʱⰪ

        ClipsDictionaryInitialize();

        myRigid = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<BoxCollider2D>(); //�ʿ��� ������Ʈ

        ColliderOffsets[0] = myCollider.offset;
        ColliderSizes[0] = myCollider.size;
        ColliderOffsets[1] = new Vector2(0.0508289337f, -0.369088709f);
        ColliderSizes[1] = new Vector2(1.08665276f, 1.79312909f); //�÷��̾ �޸��� �ʿ��� colliderũ��

        currentDashCalculate = 0; // ��Ÿ�� ��� ���� �ʱ�ȭ
        processingCoroutine = new IEnumerator[2];
        EnemyLayer = LayerMask.GetMask("Enemy");
    }

    // Update is called once per frame
    void Update()
    {
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
        //1Ÿ: 8������ �� 5 ������
        //2Ÿ: 4 ������ �� 1 ������
        //3Ÿ: 9 ������ �� 2 ������
        string _actionName = null; //���� ���� ���� ���� ���������� ���� �̸�
        float _attackTime = 0; //���� Ÿ�̹�
        Unit _tempUnit; //���ݴ��� ���ֵ� ������

        for (int i = 0; i < (1f / Time.deltaTime); i++)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack1"))
            {
                _actionName = "Attack1";
                _attackTime = 5f / 8f;
                attackAreaCenter = Vector2.right;
                attackArea = new Vector2(2.4f, 2.4f);
                break;
            }
            else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack2"))
            {
                _actionName = "Attack2";
                _attackTime = 1f / 4f;
                attackAreaCenter = new Vector2(0.5f, 0.1f);
                attackArea = new Vector2(2.35f,2.8f);
                break;
            }
            else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack3"))
            {
                _actionName = "Attack3";
                _attackTime = 2f / 9f;
                attackAreaCenter = Vector2.right;
                attackArea = new Vector2(3f, 2.4f);
                break;
            }
            yield return null;
        }
        if(_actionName != null)
        {
            yield return new WaitForSeconds(ClipsNameLengthsInfo[_actionName] * Mathf.Clamp(_attackTime - anim.GetCurrentAnimatorStateInfo(0).normalizedTime, 0, _attackTime));
            if (isDash)
            {
                yield break;
            }
            attackedUnits = Physics2D.OverlapBoxAll(transform.position + new Vector3(attackAreaCenter.x, attackAreaCenter.y, 0) * transform.localScale.x, attackArea, 0, EnemyLayer);
            
            foreach (var unit in attackedUnits)
            {
                _tempUnit = unit.GetComponent<Unit>();
                if (_tempUnit != null)
                {
                    _tempUnit.ChangeMyHealth(-(int)__attackPoint);
                }
            }
            yield return new WaitForSeconds(ClipsNameLengthsInfo[_actionName] * (1f - anim.GetCurrentAnimatorStateInfo(0).normalizedTime));
            
        }
        processingCoroutine[0] = null;
        if (processingCoroutine[1] != null)
        {
            processingCoroutine[0] = processingCoroutine[1];
            processingCoroutine[1] = null;
            StartCoroutine(processingCoroutine[0]);
        }
    }



    /*
    
    IEnumerator Attack1Coroutine()
    {
        isAttacking = true;
        yield return new WaitForSeconds(0.8f);
        if (currentAction[1] != PlayerAction.Idle)
        {
            if (currentAction[1] == PlayerAction.normalAttack_2)
            {
                ChangeToNextMove();
                StartCoroutine(Attack2Coroutine());
            }
        }
        else
        {
            isAttacking = false;
            currentAction[0] = currentAction[1];
            currentAction[1] = PlayerAction.Idle;
        }
    }


    IEnumerator Attack2Coroutine()
    {
        isAttacking = true;
        yield return new WaitForSeconds(0.4f);
        ChangeToNextMove();
        isAttacking = false;
    }*/


    /*
    private void ChangeToNextMove()
    {
        currentAction[0] = currentAction[1];
        currentAction[1] = PlayerAction.Idle;
    }
    */

}
