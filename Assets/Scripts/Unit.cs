using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Unit : MonoBehaviour
{
    [Header ("UnitStatus")]
    [SerializeField] protected int __currentHP; //���� HP
    [SerializeField] protected int __maxHP; //�ִ� ü��
    [SerializeField] protected float __attackPoint; //���ݷ�
    [SerializeField] protected float __defencePoint; //����
    [SerializeField] protected float __jumpForce; //������
    [SerializeField] protected float __limitFallingVelocity = 20f; //�������� �ӵ� ����

    [Header("UnitSpeed")]
    [SerializeField] protected float __moveSpeed; //�̵� �ӵ�
    [SerializeField] protected float __attackSpeed; //���� �ӵ�
    [SerializeField] protected float __recoveryTime; //���� �ӵ�

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

    public void CheckPlayerBlocked(bool _flag) //ĳ���� ���չ����� colliderȮ��
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
