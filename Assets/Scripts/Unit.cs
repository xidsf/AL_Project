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

    abstract protected void Move();
    abstract protected void Attack();
    abstract protected void Defence();
    abstract public void ChangeMyHealth(int _change);
    
    public float GetAttackPoint()
    {
        return __attackPoint;
    }

}
