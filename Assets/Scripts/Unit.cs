using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

/*
 * 함수 상속시
 * DamageIndicator 프리팹 추가해야함
 * 나머진 인스펙터창에서 하나씩 추가
 * 애니메이션 시간정보 필요시 ClipsDictionaryInitialize()를 start함수에 넣으면 사용 가능
 */

abstract public class Unit : MonoBehaviour
{
    [Header ("UnitStatus")]
    [SerializeField] protected float __currentHP; //현재 HP
    [SerializeField] protected int __maxHP; //최대 체력
    [SerializeField] protected float __attackPoint; //공격력
    [SerializeField] protected float __defencePoint; //방어력
    [SerializeField] protected float __jumpForce; //점프력
    [SerializeField] protected float __limitFallingVelocity = 20f; //떨어지는 속도 제한
    [SerializeField] protected float __ImmuneTime; //대쉬하거나 피격시 무적 시간
    protected float currentImmuneTime; //무적시간 계산을 위한 변수

    [Header("UnitSpeed")]
    [SerializeField] protected float __moveSpeed; //이동 속도
    [SerializeField] protected float __recoveryTime; //복구 속도


    [Header("DamageIndicator")]
    [SerializeField] protected GameObject damageIndicatorPrefab;

    protected Dictionary<string, float> UnitAnimationClipInfo = new Dictionary<string, float>(); //공격종류와 공격 별 걸리는 시간을 저장한 dictionary
    
    protected BoxCollider2D myCollider; //본인 collider
    protected Rigidbody2D myRigid; //본인 rigidbody
    
    protected bool isMoving; //움직임 채크
    protected bool isGround; //공중에 있는지 확인하기 위한 변수
    protected bool isFalling; //떨어지고 있는지 확인하기 위한 변수
    protected bool isRising; //올라가고 있는지 확인
    protected bool isAttacking; //공격중 채크
    protected bool isBlocked; //플레이어가 벽으로 이동하면 벽을 뚫는 현상 수정을 위한 불값
    protected bool canMove; //움직임이 가능인지 확인용 상태 변수
    protected bool isImmune; //무적상태 변수
    public bool isDead; //사망 확인 변수

    protected float _lastPos; //움직임 채크를 위한 이전 위치 저장 변수

    protected RaycastHit2D[] _hit = new RaycastHit2D[3]; //raycast정보 저장을 위한 변수

    abstract protected void Move();
    abstract protected void Attack();
    abstract protected void Death();

    
    

    protected virtual void CalculateImmuneTime()
    {
        if(isImmune)
        {
            if(currentImmuneTime > 0)
            {
                currentImmuneTime -= Time.deltaTime;
            }
            else
            {
                isImmune = false;
            }
        }
    }

    

    public void ChangeMyHealth(float _change)
    {
        if (isImmune) return;
        float _tempHP;
        _tempHP = __currentHP;
        float _randgap = 0.3f;
        Vector3 randomPosition = new Vector3(UnityEngine.Random.Range((float)transform.position.x - _randgap, (float)transform.position.x + _randgap),
            UnityEngine.Random.Range((float)transform.position.y - _randgap, (float)transform.position.y + _randgap),
            UnityEngine.Random.Range((float)transform.position.z - _randgap, (float)transform.position.z + _randgap));
        GameObject clone = Instantiate(damageIndicatorPrefab, randomPosition, Quaternion.identity);
        if (_change < 0)
        {
            //Debug.Log(Mathf.Ceil(Mathf.Clamp(_change + __defencePoint, _change, 0)));
            clone.GetComponent<DamageIndicator>().ShowDamageIndicator(Mathf.Ceil(Mathf.Clamp(_change + __defencePoint, _change, 0)*10) *0.1f, Color.red);
            __currentHP = Mathf.Ceil((__currentHP + Mathf.Clamp(_change + __defencePoint, _change, 0)) * 10) / 10f;
        }
        else if(_change > 0)
        {
            clone.GetComponent<DamageIndicator>().ShowDamageIndicator(__currentHP - Mathf.Clamp(__currentHP + _change, -__maxHP, __maxHP), Color.green);

            __currentHP = Mathf.Clamp(__currentHP + _change, -__maxHP, __maxHP);
            
        }
        else
        {
            clone.GetComponent<DamageIndicator>().ShowDamageIndicator(0, Color.white);
        }
        if(__currentHP <= 0 && !isDead)
        {
            Death();
        }
        else if (__currentHP < _tempHP)
        {
            isImmune = true;
            currentImmuneTime = __ImmuneTime;
            AfterDamaged();
        }

    }

    protected virtual void AfterDamaged()
    {

    }

    public float GetAttackPoint()
    {
        return __attackPoint;
    }

    protected virtual void MoveCheck()
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

    protected float CalculateDamage(float _coefficient)
    {
        return __attackPoint * _coefficient;
    }

}
