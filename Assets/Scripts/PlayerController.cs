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
    enum PlayerAction : int
    {
        Idle = -1, normalAttack_1 = 0, normalAttack_2 = 1
    }


    [SerializeField] private Animator anim; //Warrior의 Animator를 바꾸기 위한 serializedField
    private BoxCollider2D myCollider; //본인 collider
    private Rigidbody2D myRigid; //본인 rigidbody

    //이동 중 collider변환 용(0: 기본 1: 이동중)
    private Vector2[] ColliderOffsets = new Vector2[2];
    private Vector2[] ColliderSizes = new Vector2[2];

    private float _lastPos; //움직임 채크를 위한 이전 위치 저장 변수
    private RaycastHit2D[] _hit; //raycast정보 저장을 위한 변수

    [SerializeField] private bool isBlocked;

    private const int maxAttackingAction = 2; //추가타의 갯수 저장용 변수
    private delegate IEnumerator AttackCoroutine(); //2타 이후 코루틴을 순차적으로 적용하기 위한 델리게이트
    private PlayerAction[] currentAction; //현재 적용중인 행동과 다음 행동 선입력을 저장하기 위한 2차원 변수 
    


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


        currentAction = new PlayerAction[2]; //currentAction 초기화
        currentAction[0] = PlayerAction.Idle;
        currentAction[1] = PlayerAction.Idle;
    }

    // Update is called once per frame
    void Update()
    {
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

    private void AnimCheck() //파라미터를 이용한 매니매이션 변경 함수
    {
        
        
        anim.SetBool("Run", isMoving);
        anim.SetBool("Rise", isRising);
        anim.SetBool("Fall", isFalling);
        anim.SetBool("Ground", isGround);
        anim.SetInteger("Attack", (int)currentAction[0]);
        
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
        if (Input.GetKeyDown(KeyCode.Z) && !isRising && !isFalling && isGround && !isAttacking)
        {
            StartCoroutine(Attack1Coroutine());
            currentAction[0] = PlayerAction.normalAttack_1;
        }
        else if (Input.GetKeyDown(KeyCode.Z) && isAttacking) //공격 중 추가 입력하면 다음 행동 예약
        {
            if (currentAction[1] == PlayerAction.Idle)
            {
                currentAction[1] = currentAction[0] + 1;
                if ((int)currentAction[1] > maxAttackingAction - 1)
                {
                    currentAction[1] = PlayerAction.Idle;
                    return;
                }
            }
        }
    }


    IEnumerator Attack1Coroutine()
    {
        isAttacking = true;
        yield return new WaitForSeconds(0.8f); //공격 1타 애니메이션 실행 시간 (바꾸면 수정 해야됨)
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
        yield return new WaitForSeconds(0.4f);//공격 2타 애니메이션 실행 시간 (바꾸면 수정 해야됨)
        ChangeToNextMove();
        isAttacking = false;
    }

    public override void ChangeMyHealth(int _change)
    {
        __currentHP += _change;
    }

    private void ChangeToNextMove()
    {
        currentAction[0] = currentAction[1];
        currentAction[1] = PlayerAction.Idle;
    }

    protected override void Defence()
    {
    }

    protected override void Attack()
    {
        
    }
}
