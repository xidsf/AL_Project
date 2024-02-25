using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

//�ִϸ��̼� �ڵ尡 ����� �� ���� ���� �ڵ� ����...�ߴµ� ���� �ȱ�,,,��

public partial class PlayerController : Unit
{
    
    private bool isClearedInIdle; //Idle��ȯ / �ѹ� ���� �ʱ�ȭ ��Ű�� �ٸ� �׼��� �� ������ �ʱ�ȭX
    
    private const float dashEndTime = 3f / 7f;
    IEnumerator _savedDashCoroutine;

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
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Dash-Attack"))
        {
            isAttacking = true;
            if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.8f)
            {
                notInputAttack = false;
            }
            else
            {
                notInputAttack = true;
            }
            if(anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 3f / 11f)
            {
                isDash = false;
            }
        }
        else if(anim.GetCurrentAnimatorStateInfo(0).IsName("Dash"))
        {
            ClearAnimationParameter();
            _savedDashCoroutine = DashCoroutine();
            StartCoroutine(_savedDashCoroutine);
        }
        
    }

    private void ClearAnimationParameter()
    {
        isAttacking = false; //������ ���� �ʱ�ȭ
        notInputAttack = false; //��Ÿ ������ �ʱ�ȭ
    }

    IEnumerator DashCoroutine()
    {
        yield return new WaitForSeconds(UnitAnimationClipInfo["Dash"] * (1f - dashEndTime));
        isDash = false;
        yield return new WaitForSeconds(1f - anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
    }
    


    /*  animEvent�� ��ü : coroutine�� animationEvent�� ���� �����ϴ°� ������ �𸣰����� AnimationEvent�� ���ؼ� �� ��
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
