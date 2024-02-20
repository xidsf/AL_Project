using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
/*
 * animation croush 수정해야됨 
 * 
 */


public class PlayerController : Unit
{
    [SerializeField] private Animator anim; //Warrior의 Animator를 바꾸기 위한 serializedField
    private BoxCollider2D myCollider; //본인 collider
    private Rigidbody2D myRigid; //본인 rigidbody

    //이동 중 collider변환 용(0: 기본 1: 이동중)
    private Vector2[] ColliderOffsets = new Vector2[2];
    private Vector2[] ColliderSizes = new Vector2[2];

    private float _lastPos; //움직임 채크를 위한 이전 위치 저장 변수
    private RaycastHit2D[] _hit; //raycast정보 저장을 위한 변수

    [SerializeField] private bool isBlocked;

    private bool[] processingAttack; //추가타 확인 및 변환을 위한 변수
    private const int maxSequenceAttacking = 2; //추가타의 갯수 저장용 변수
    private delegate IEnumerator AttackCoroutine(); //2타 이후 코루틴을 순차적으로 적용하기 위한 델리게이트
    private int[] currentAttackChecker; //현재 적용중인 공격이 몇 번째인지 저장하기 위한 변수

    void Start()
    {
        isAttacking = false;

        _lastPos = transform.position.x; //움직인 확인을 위한 초기값

        myRigid = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<BoxCollider2D>(); //필요한 컴포넌트

        ColliderOffsets[0] = myCollider.offset;
        ColliderSizes[0] = myCollider.size;
        ColliderOffsets[1] = new Vector2(0.0508289337f, -0.369088709f);
        ColliderSizes[1] = new Vector2(1.08665276f, 1.79312909f); //플레이어가 달릴때 필요한 collider크기

        _hit = new RaycastHit2D[3]; //발 밑으로 ray 3개를 쏘고 정보를 받을 배열


        currentAttackChecker = new int[2]; //currentAttack 초기화
        processingAttack = new bool[maxSequenceAttacking]; //진행 중인 attacking모션
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(isAttacking + " " + processingAttack[0] + " " + processingAttack[1]);
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

    private void AnimCheck() //파라미터를 이용한 매니매이션 변경 함수
    {
        
        
        anim.SetBool("Run", isMoving);
        anim.SetBool("Rise", isRising);
        anim.SetBool("Fall", isFalling);
        anim.SetBool("Ground", isGround);
        anim.SetInteger("Attack", currentAttackChecker[0]);
        
    }

    public void CheckPlayerBlocked(bool _flag) //캐릭터 벽뚫방지용 collider확인
    {
        isBlocked = _flag;
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

    

    private void TryAttack() //공격 1타 시도
    {
        AttackCoroutine[] attackingCoroutine = 
            { Attack1Coroutine, Attack2Coroutine };
        

        if (Input.GetKeyDown(KeyCode.Z) && !isRising && !isFalling && isGround && !isAttacking)
        {
            StartCoroutine(attackingCoroutine[0]());
            currentAttackChecker[0]++;
        }
        else if (Input.GetKeyDown(KeyCode.Z) && isAttacking)
        {
            if (currentAttackChecker[1] == 0)
            {
                currentAttackChecker[1] = currentAttackChecker[0] + 1;
                if (currentAttackChecker[1] > maxSequenceAttacking)
                {
                    currentAttackChecker[1] = 0;
                }
                StartCoroutine(attackingCoroutine[currentAttackChecker[1] - 1]()); // <-- 수정 필요
            }
            
        }
    }


    IEnumerator Attack1Coroutine()
    {
        isAttacking = true;
        processingAttack[0] = true;
        yield return new WaitForSeconds(1f);
        isAttacking = false;
        processingAttack[0] = false;
        currentAttackChecker[0] = currentAttackChecker[1];
        currentAttackChecker[1] = 0;
        
    }


    IEnumerator Attack2Coroutine()
    {
        
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack1"))
        {
            yield return new WaitForSeconds(1f - anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
        }
        isAttacking = true;
        processingAttack[1] = true;
        yield return new WaitForSeconds(0.5f);
        currentAttackChecker[0] = currentAttackChecker[1];
        currentAttackChecker[1] = 0;
        isAttacking = false;
        processingAttack[1] = false;
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
