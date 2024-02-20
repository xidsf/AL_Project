using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
/*
 * animation croush �����ؾߵ� 
 * 
 */


public class PlayerController : Unit
{
    enum PlayerAction : int
    {
        Idle = -1, normalAttack_1 = 0, normalAttack_2 = 1
    }


    [SerializeField] private Animator anim; //Warrior�� Animator�� �ٲٱ� ���� serializedField
    private BoxCollider2D myCollider; //���� collider
    private Rigidbody2D myRigid; //���� rigidbody

    //�̵� �� collider��ȯ ��(0: �⺻ 1: �̵���)
    private Vector2[] ColliderOffsets = new Vector2[2];
    private Vector2[] ColliderSizes = new Vector2[2];

    private float _lastPos; //������ äũ�� ���� ���� ��ġ ���� ����
    private RaycastHit2D[] _hit; //raycast���� ������ ���� ����

    [SerializeField] private bool isBlocked;

    private bool[] processingAction; //�߰�Ÿ Ȯ�� �� ��ȯ�� ���� ����
    private const int maxAttackingAction = 2; //�߰�Ÿ�� ���� ����� ����
    private delegate IEnumerator AttackCoroutine(); //2Ÿ ���� �ڷ�ƾ�� ���������� �����ϱ� ���� ��������Ʈ
    private PlayerAction[] currentActionChecker; //���� �������� �ൿ�� ���� �ൿ ���Է��� �����ϱ� ���� 2���� ���� , ���߿� PlayerActionŸ�԰� ȣ��


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


        currentActionChecker = new PlayerAction[2]; //currentAttack �ʱ�ȭ
        currentActionChecker[0] = PlayerAction.Idle;
        currentActionChecker[1] = PlayerAction.Idle;
        processingAction = new bool[maxAttackingAction]; //���� ���� attacking���
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(isAttacking + " " + processingAction[0] + " " + processingAction[1]);
        TryGroundCheck();
        TryJump();
        CheckAirPramameter();
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
        if(!isAttacking)
        {
            Move();
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
        if (Input.GetKeyDown(KeyCode.Space) && isGround)
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

    private void AnimCheck() //�Ķ���͸� �̿��� �Ŵϸ��̼� ���� �Լ�
    {
        
        
        anim.SetBool("Run", isMoving);
        anim.SetBool("Rise", isRising);
        anim.SetBool("Fall", isFalling);
        anim.SetBool("Ground", isGround);
        anim.SetInteger("Attack", (int)currentActionChecker[0]);
        
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

    

    private void TryAttack() //���� 1Ÿ �õ�
    {
        AttackCoroutine[] attackingCoroutine = 
            { Attack1Coroutine, Attack2Coroutine };
        

        if (Input.GetKeyDown(KeyCode.Z) && !isRising && !isFalling && isGround && !isAttacking)
        {
            StartCoroutine(attackingCoroutine[0]());
            currentActionChecker[0]++;
        }
        else if (Input.GetKeyDown(KeyCode.Z) && isAttacking)
        {
            if (currentActionChecker[1] == PlayerAction.Idle)
            {
                currentActionChecker[1] = currentActionChecker[0] + 1;
                if ((int)currentActionChecker[1] > maxAttackingAction - 1)
                {
                    currentActionChecker[1] = PlayerAction.Idle;
                    return;
                }
                StartCoroutine(attackingCoroutine[(int)currentActionChecker[1]]()); 
            }
            
        }
    }


    IEnumerator Attack1Coroutine()
    {
        isAttacking = true;
        processingAction[0] = true;
        yield return new WaitForSeconds(1f);
        isAttacking = false;
        processingAction[0] = false;
        currentActionChecker[0] = currentActionChecker[1];
        currentActionChecker[1] = PlayerAction.Idle;
        
    }


    IEnumerator Attack2Coroutine()
    {
        
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack1"))
        {
            yield return new WaitForSeconds(1f - anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
        }
        isAttacking = true;
        processingAction[1] = true;
        yield return new WaitForSeconds(0.5f);
        currentActionChecker[0] = currentActionChecker[1];
        currentActionChecker[1] = PlayerAction.Idle;
        isAttacking = false;
        processingAction[1] = false;
    }








    public override void ChangeMyHealth(int _change)
    {
        __currentHP += _change;
    }

    protected override void Defence()
    {
    }

    protected override void Attack()
    {
        
    }
}
