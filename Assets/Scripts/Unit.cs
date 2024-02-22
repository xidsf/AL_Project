using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Unit : MonoBehaviour
{

    [SerializeField] protected int __currentHP; //���� HP
    [SerializeField] protected int __maxHP; //�ִ� ü��
    [SerializeField] protected float __attackPoint; //���ݷ�
    [SerializeField] protected float __defencePoint; //����
    [SerializeField] protected float __moveSpeed; //�̵� �ӵ�
    [SerializeField] protected float __jumpForce; //������
    [SerializeField] protected float __limitFallingVelocity = 20f; //�������� �ӵ� ����

    [SerializeField] protected float __attackSpeed; //���� �ӵ�
    [SerializeField] protected float __defenceSpeed; //��� �ӵ�
    [SerializeField] protected float __recoveryTime; //��� �ӵ�

    protected bool isMoving; //������ äũ
    protected bool isGround; //���߿� �ִ��� Ȯ���ϱ� ���� ����
    protected bool isFalling; //�������� �ִ��� Ȯ���ϱ� ���� ����
    protected bool isRising; //�ö󰡰� �ִ��� Ȯ��
    protected bool isAttacking; //������ äũ
    protected bool isImmune; //���� �鿪���� Ȯ�� ����

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
