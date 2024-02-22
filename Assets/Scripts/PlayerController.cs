using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
/*
 * animation croush 수정해야됨 
 * 
 */


public partial class PlayerController : Unit
{
    [SerializeField] private Animator anim; //Warrior의 Animator를 바꾸기 위한 serializedField

    //이동 중 collider변환 용(0: 기본 1: 이동중)
    private Vector2[] ColliderOffsets = new Vector2[2];
    private Vector2[] ColliderSizes = new Vector2[2];

    
    private bool notInputAttack; //플레이어의 공격입력을 제한하는 변수
    private bool isDash; //대쉬중이면 순간무적 (개발 예정)


    [SerializeField] private float dashDelay; //대쉬를 사용하기 위해 필요한 쿨타임
    [SerializeField] private float dashSpeed; //대쉬 스피드 변수
    private float currentDashCalculate; //대쉬 쿨타임 계산을 위한 변수

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

        currentDashCalculate = 0; // 쿨타임 계산 변수 초기화

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
        anim.SetTrigger("Attack");
    }

    public void ClearState()
    {
        isMoving = false; //움직임 초기화
        isAttacking = false; //공격중 변수 초기화
        notInputAttack = false; //막타 공격중 초기화
    }

    public void ChangeisAttacking(bool _flag)
    {
        isAttacking = true;
    }

    public void ChangeLastNormalAttack(bool _flag)
    {
        notInputAttack = _flag;
    }

    public void FinishDash()
    {
        isDash = false;
    }



    

    

    /*
    
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
    }*/


    /*
    private void ChangeToNextMove()
    {
        currentAction[0] = currentAction[1];
        currentAction[1] = PlayerAction.Idle;
    }
    */

}
