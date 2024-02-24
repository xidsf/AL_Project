using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Unit : MonoBehaviour
{
    [Header ("UnitStatus")]
    [SerializeField] protected int __currentHP; //현재 HP
    [SerializeField] protected int __maxHP; //최대 체력
    [SerializeField] protected float __attackPoint; //공격력
    [SerializeField] protected float __defencePoint; //방어력
    [SerializeField] protected float __jumpForce; //점프력
    [SerializeField] protected float __limitFallingVelocity = 20f; //떨어지는 속도 제한

    [Header("UnitSpeed")]
    [SerializeField] protected float __moveSpeed; //이동 속도
    [SerializeField] protected float __attackSpeed; //공격 속도
    [SerializeField] protected float __recoveryTime; //복구 속도

    protected BoxCollider2D myCollider; //본인 collider
    protected Rigidbody2D myRigid; //본인 rigidbody

    protected bool isMoving; //움직임 채크
    protected bool isGround; //공중에 있는지 확인하기 위한 변수
    protected bool isFalling; //떨어지고 있는지 확인하기 위한 변수
    protected bool isRising; //올라가고 있는지 확인
    protected bool isAttacking; //공격중 채크
    protected bool isBlocked; //플레이어가 벽으로 이동하면 벽을 뚫는 현상 수정을 위한 불값
    protected bool isImmune; //무적상태 변수

    protected float _lastPos; //움직임 채크를 위한 이전 위치 저장 변수

    protected RaycastHit2D[] _hit = new RaycastHit2D[3]; //raycast정보 저장을 위한 변수

    abstract protected void Move();
    abstract protected void Attack();

    public void CheckPlayerBlocked(bool _flag) //캐릭터 벽뚫방지용 collider확인
    {
        isBlocked = _flag;
    }

    public void ChangeMyHealth(int _change)
    {
        if (isImmune) return;
        if(_change < 0)
        {
            __currentHP = __currentHP + Mathf.Clamp(_change + (int)__defencePoint, _change, 0);

        }
        else
        {
            __currentHP = Mathf.Clamp(__currentHP + _change, -__maxHP, __maxHP);
        }

    }

    public float GetAttackPoint()
    {
        return __attackPoint;
    }

    protected void MoveCheck()
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

    protected void TryGroundCheck()
    {
        isGround = GroundCheck();
    }

    protected bool GroundCheck()
    {
        float _rayCastLength = 0.05f;
        Vector2 _bottomLeft = new Vector2(myCollider.bounds.center.x - myCollider.bounds.extents.x, myCollider.bounds.center.y - myCollider.bounds.extents.y);
        Vector2 _bottomRight = new Vector2(myCollider.bounds.center.x + myCollider.bounds.extents.x, myCollider.bounds.center.y - myCollider.bounds.extents.y);
        Vector2 _bottomCenter = new Vector2(myCollider.bounds.center.x, myCollider.bounds.center.y - myCollider.bounds.extents.y);
        _hit[0] = Physics2D.Raycast(_bottomCenter, Vector2.down, _rayCastLength, LayerMask.GetMask("Ground"));
        _hit[1] = Physics2D.Raycast(_bottomLeft, Vector2.down, _rayCastLength, LayerMask.GetMask("Ground"));
        _hit[2] = Physics2D.Raycast(_bottomRight, Vector2.down, _rayCastLength, LayerMask.GetMask("Ground"));

        //Debug.DrawRay(_bottomLeft, Vector2.down * _rayCastLength, Color.red);
        //Debug.DrawRay(_bottomRight, Vector2.down * _rayCastLength, Color.red);
        //Debug.DrawRay(_bottomCenter, Vector2.down * _rayCastLength, Color.red);

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

}
