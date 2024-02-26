using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;
using Unity.Mathematics;
using static PlayerController;

abstract public class Unit : MonoBehaviour
{
    [Header ("UnitStatus")]
    [SerializeField] protected float __currentHP; //���� HP
    [SerializeField] protected int __maxHP; //�ִ� ü��
    [SerializeField] protected float __attackPoint; //���ݷ�
    [SerializeField] protected float __defencePoint; //����
    [SerializeField] protected float __jumpForce; //������
    [SerializeField] protected float __limitFallingVelocity = 20f; //�������� �ӵ� ����
    [SerializeField] protected float __ImmuneTime; //�뽬�ϰų� �ǰݽ� ���� �ð�
    protected float currentImmuneTime; //�����ð� ����� ���� ����

    [Header("UnitSpeed")]
    [SerializeField] protected float __moveSpeed; //�̵� �ӵ�
    [SerializeField] protected float __attackSpeed; //���� �ӵ�
    [SerializeField] protected float __recoveryTime; //���� �ӵ�

    [Header("UnitAnimatorController")]
    [SerializeField] protected AnimatorController UnitAnimatorController; //��ü �ִϸ��̼� ��ȯ�� ���� ȿ���� �ڷ�ƾ���� �����ϱ� ���� ����

    [Header("DamageIndicator")]
    [SerializeField] protected GameObject damageIndicatorPrefab;

    protected Dictionary<string, float> UnitAnimationClipInfo = new Dictionary<string, float>(); //���������� ���� �� �ɸ��� �ð��� ������ dictionary
    

    protected BoxCollider2D myCollider; //���� collider
    protected Rigidbody2D myRigid; //���� rigidbody
    
    protected bool isMoving; //������ äũ
    protected bool isGround; //���߿� �ִ��� Ȯ���ϱ� ���� ����
    protected bool isFalling; //�������� �ִ��� Ȯ���ϱ� ���� ����
    protected bool isRising; //�ö󰡰� �ִ��� Ȯ��
    protected bool isAttacking; //������ äũ
    protected bool isBlocked; //�÷��̾ ������ �̵��ϸ� ���� �մ� ���� ������ ���� �Ұ�
    protected bool isImmune; //�������� ����

    protected float _lastPos; //������ äũ�� ���� ���� ��ġ ���� ����

    protected RaycastHit2D[] _hit = new RaycastHit2D[3]; //raycast���� ������ ���� ����

    abstract protected void Move();
    abstract protected void Attack();
    abstract protected void Death();

    
    protected void ClipsDictionaryInitialize()
    {
        for (int i = 0; i < UnitAnimatorController.animationClips.Length; i++)
        {
            UnitAnimationClipInfo.Add(UnitAnimatorController.animationClips[i].name, UnitAnimatorController.animationClips[i].length);
            //Debug.Log(UnitAnimatorController.animationClips[i].name + UnitAnimatorController.animationClips[i].length);
        }
    }

    protected virtual void CalculateImmuneTime()
    {
        if(isImmune)
        {
            if(currentImmuneTime > 0)
            {
                currentImmuneTime =- Time.deltaTime;
            }
            if(currentImmuneTime <= 0)
            {
                isImmune = false;
            }
        }
    }

    public void CheckPlayerBlocked(bool _flag) //ĳ���� ���չ����� colliderȮ��
    {
        isBlocked = _flag;
    }

    public virtual void ChangeMyHealth(float _change)
    {
        if (isImmune) return;
        float _tempHP;
        _tempHP = __currentHP;
        if (_change < 0)
        {
            //Debug.Log(Mathf.Ceil((__currentHP + Mathf.Clamp(_change + __defencePoint, _change, 0)) * 10) / 10f);
            float _randgap = 0.3f;
            Vector3 randomPosition = new Vector3(UnityEngine.Random.Range((float)transform.position.x - _randgap, (float)transform.position.x + _randgap), 
                UnityEngine.Random.Range((float)transform.position.y - _randgap, (float)transform.position.y + _randgap), 
                UnityEngine.Random.Range((float)transform.position.z - _randgap, (float)transform.position.z + _randgap));
            GameObject clone = Instantiate(damageIndicatorPrefab, randomPosition, Quaternion.identity);
            clone.GetComponent<DamageIndicator>().ShowDamageIndicator(Mathf.Ceil(Mathf.Clamp(_change + __defencePoint, _change, 0)*10) *0.1f);
            __currentHP = Mathf.Ceil((__currentHP + Mathf.Clamp(_change + __defencePoint, _change, 0)) * 10) / 10f;
        }
        else
        {
            __currentHP = Mathf.Clamp(__currentHP + _change, -__maxHP, __maxHP);
        }
        if (__currentHP < _tempHP)
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

}
