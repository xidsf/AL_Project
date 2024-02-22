using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UIElements;
/*
 * animation croush �����ؾߵ� 
 * 
 */


public partial class PlayerController : Unit
{
    [SerializeField] private Animator anim; //Warrior�� Animator�� �ٲٱ� ���� serializedField
    private BoxCollider2D myCollider; //���� collider
    private Rigidbody2D myRigid; //���� rigidbody

    //�̵� �� collider��ȯ ��(0: �⺻ 1: �̵���)
    private Vector2[] ColliderOffsets = new Vector2[2];
    private Vector2[] ColliderSizes = new Vector2[2];

    private float _lastPos; //������ äũ�� ���� ���� ��ġ ���� ����
    private RaycastHit2D[] _hit; //raycast���� ������ ���� ����

    private bool isBlocked; //�÷��̾ ������ �̵��ϸ� ���� �մ� ���� ������ ���� �Ұ�
    [SerializeField] private bool notInputAttack; //�÷��̾��� �����Է��� �����ϴ� ����
    [SerializeField] private bool isDash; //�뽬���̸� �������� (���� ����)

    [SerializeField] private float dashDelay; //�뽬�� ����ϱ� ���� �ʿ��� ��Ÿ��
    [SerializeField] private float dashSpeed; //�뽬 ���ǵ� ����
    private float currentDashCalculate; //�뽬 ��Ÿ�� ����� ���� ����

    

    void Start()
    {
        isAttacking = false;

        _lastPos = transform.position.x; //������ Ȯ���� ���� �ʱⰪ

        myRigid = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<BoxCollider2D>(); //�ʿ��� ������Ʈ

        ColliderOffsets[0] = myCollider.offset;
        ColliderSizes[0] = myCollider.size;
        ColliderOffsets[1] = new Vector2(0.0508289337f, -0.369088709f);
        ColliderSizes[1] = new Vector2(1.08665276f, 1.79312909f); //�÷��̾ �޸��� �ʿ��� colliderũ��

        _hit = new RaycastHit2D[3]; //�� ������ ray 3���� ��� ������ ���� �迭
        currentDashCalculate = 0; // ��Ÿ�� ��� ���� �ʱ�ȭ
        ClipsDictionaryInitialize();
    }

    // Update is called once per frame
    void Update()
    {
        TryGroundCheck();
        TryJump();
        CheckAirPramameter();
        TryAttack();
        CalculateDashDelay();
        TryDash();
        ChangeAnimationParameter();
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

    private void MoveCheck()
    {
        if (MathF.Abs(_lastPos - transform.position.x) >= 0.01f)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
        _lastPos = transform.position.x;
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

    private void TryGroundCheck()
    {
        isGround = GroundCheck();
    }

    private bool GroundCheck()
    {
        
        float _rayCastLength = 0.05f;
        Vector2 _bottomLeft = new Vector2(myCollider.bounds.center.x - myCollider.bounds.extents.x, myCollider.bounds.center.y - myCollider.bounds.extents.y);
        Vector2 _bottomRight = new Vector2(myCollider.bounds.center.x + myCollider.bounds.extents.x, myCollider.bounds.center.y - myCollider.bounds.extents.y);
        Vector2 _bottomCenter = new Vector2(myCollider.bounds.center.x, myCollider.bounds.center.y - myCollider.bounds.extents.y);
        _hit[0] = Physics2D.Raycast(_bottomCenter, Vector2.down, _rayCastLength, LayerMask.GetMask("Ground"));
        _hit[1] = Physics2D.Raycast(_bottomLeft, Vector2.down, _rayCastLength, LayerMask.GetMask("Ground"));
        _hit[2] = Physics2D.Raycast(_bottomRight, Vector2.down, _rayCastLength, LayerMask.GetMask("Ground"));

        Debug.DrawRay(_bottomLeft, Vector2.down * _rayCastLength, Color.red);
        Debug.DrawRay(_bottomRight, Vector2.down * _rayCastLength, Color.red);
        Debug.DrawRay(_bottomCenter, Vector2.down * _rayCastLength, Color.red);

        for (int i = 0; i < 3; i++)
        {
            if (_hit[i].collider != null)
            {
                if (_hit[i].collider.CompareTag("Ground"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void CheckPlayerBlocked(bool _flag) //ĳ���� ���չ����� colliderȮ��
    {
        isBlocked = _flag;
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
        isAttacking = true;
    }
    protected override void Defence()
    {
    }
    

    /*
    
    IEnumerator Attack1Coroutine()
    {
        isAttacking = true;
        yield return new WaitForSeconds(0.8f); //���� 1Ÿ �ִϸ��̼� ���� �ð� (�ٲٸ� ���� �ؾߵ�)
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
        yield return new WaitForSeconds(0.4f);//���� 2Ÿ �ִϸ��̼� ���� �ð� (�ٲٸ� ���� �ؾߵ�)
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
