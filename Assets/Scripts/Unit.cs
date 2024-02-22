using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Unit : MonoBehaviour
{

    [SerializeField] protected int __currentHP; //현재 HP
    [SerializeField] protected int __maxHP; //최대 체력
    [SerializeField] protected float __attackPoint; //공격력
    [SerializeField] protected float __defencePoint; //방어력
    [SerializeField] protected float __moveSpeed; //이동 속도
    [SerializeField] protected float __jumpForce; //점프력
    [SerializeField] protected float __limitFallingVelocity = 20f; //떨어지는 속도 제한

    [SerializeField] protected float __attackSpeed; //공격 속도
    [SerializeField] protected float __defenceSpeed; //방어 속도
    [SerializeField] protected float __recoveryTime; //재생 속도

    protected bool isMoving; //움직임 채크
    protected bool isGround; //공중에 있는지 확인하기 위한 변수
    protected bool isFalling; //떨어지고 있는지 확인하기 위한 변수
    protected bool isRising; //올라가고 있는지 확인
    protected bool isAttacking; //공격중 채크
    protected bool isImmune; //공격 면역인지 확인 변수

    abstract protected void Move();
    abstract protected void Attack();
    abstract protected void Defence();
    
    public void ChangeMyHealth(int _change)
    {
        if(_change < 0)
        {
            __currentHP = __currentHP + _change + (int)__defencePoint;
        }
        else
        {
            __currentHP = __currentHP + _change;
        }

    }

    public float GetAttackPoint()
    {
        return __attackPoint;
    }

}
