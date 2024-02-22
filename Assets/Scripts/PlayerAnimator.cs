using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

//�ִϸ��̼� �ڵ尡 ����� �� ���� ���� �ڵ� ����

public partial class PlayerController : Unit
{
    [SerializeField] AnimatorController playerAnimatorController; //�÷��̾� �ִϸ��̼� ��ȯ�� ���� �ൿ�� �����ϱ� ���� ����

    //private Dictionary<string, float> ClipsNameLengthsInfo = new Dictionary<string, float>(); //?? ���� ���óx
    //private IEnumerator processingCoroutine; //�������� �ڷ�ƾ stopcoroutine�ϱ� ���� ���� // animEvent�� ��ü

    private bool isClearedInIdle; //Idle��ȯ / �ѹ� ���� �ʱ�ȭ ��Ű�� �ٸ� �׼��� �� ������ �ʱ�ȭX

    /*
    private void ClipsDictionaryInitialize()
    {
        for (int i = 0; i < playerAnimatorController.animationClips.Length; i++)
        {
            ClipsNameLengthsInfo.Add(playerAnimatorController.animationClips[i].name, playerAnimatorController.animationClips[i].length);
        }
    }
    */
    private void AnimCheck() //�Ķ���͸� �̿��� �Ŵϸ��̼� ���� �Լ�
    {
        anim.SetBool("Run", isMoving);
        anim.SetBool("Rise", isRising);
        anim.SetBool("Fall", isFalling);
        anim.SetBool("Ground", isGround);
    }

    private void ChangeAnimationParameter()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !isClearedInIdle)
        {
            ClearAnimationParameter();
            isClearedInIdle = true;
        }
        else
        {
            isClearedInIdle = false;
        }
        if(anim.GetCurrentAnimatorStateInfo(0).IsName("Attack1") || anim.GetCurrentAnimatorStateInfo(0).IsName("Attack2"))
        {
            isAttacking = true;
            //TryChangeDirection(); --> animationEvent�� �׳� ���ѵ� ���� ��ȯ Ÿ�̹��� ���׹�����
        }
        else if(anim.GetCurrentAnimatorStateInfo(0).IsName("Attack3"))
        {
            //TryChangeDirection(); --> animationEvent�� �׳� ���ѵ� ���� ��ȯ Ÿ�̹��� ���׹�����
            if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.8)
            {
                notInputAttack = false;
            }
            else
            {
                notInputAttack = true;
            }
        }
        else if(anim.GetCurrentAnimatorStateInfo(0).IsName("Dash"))
        {
            ClearAnimationParameter();
            notInputAttack = true;
            StartCoroutine(DashCoroutine());
        }
        
    }

    private void ClearAnimationParameter()
    {
        isAttacking = false; //������ ���� �ʱ�ȭ
        notInputAttack = false; //��Ÿ ������ �ʱ�ȭ
    }

    IEnumerator DashCoroutine()
    {
        yield return new WaitForSeconds(0.6f - anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
        isDash = false;
        notInputAttack = false;
        yield return new WaitForSeconds(1f - anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
    }

    /*  animEvent�� ��ü
    private void TryChangeDirection()
    {
        float _dirX = Input.GetAxisRaw("Horizontal");
        if (transform.localScale.x != _dirX && _dirX != 0 && !isChangedDirection)
        {
            isChangedDirection = true;
            if (processingCoroutine != null)
            {
                StopCoroutine(processingCoroutine);
            }
            processingCoroutine = ChangeAttackDirectionCoroutine(_dirX);
            StartCoroutine(processingCoroutine);
        }
        else if(transform.localScale.x == _dirX && isChangedDirection && _dirX != 0)
        {
            isChangedDirection = false;
            if (processingCoroutine != null)
            {
                StopCoroutine(processingCoroutine);
            }
        }
    }
    */


    /*  animEvent�� ��ü
    IEnumerator ChangeAttackDirectionCoroutine(float _dirX)
    {
        yield return new WaitForSeconds(0.85f - anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
        transform.localScale = new Vector3(_dirX, transform.localScale.y, transform.localScale.z);
        isChangedDirection = false;
    }
    */






}
