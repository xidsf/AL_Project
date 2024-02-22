using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

//애니메이션 코드가 길어질 것 같아 따로 코드 제작

public partial class PlayerController : Unit
{
    [SerializeField] AnimatorController playerAnimatorController; //플레이어 애니메이션 변환에 따른 행동을 제어하기 위한 파일

    //private Dictionary<string, float> ClipsNameLengthsInfo = new Dictionary<string, float>(); //?? 아직 사용처x
    //private IEnumerator processingCoroutine; //진행중인 코루틴 stopcoroutine하기 위해 저장 // animEvent로 대체

    private bool isClearedInIdle; //Idle변환 / 한번 상태 초기화 시키면 다른 액션을 할 때까지 초기화X

    /*
    private void ClipsDictionaryInitialize()
    {
        for (int i = 0; i < playerAnimatorController.animationClips.Length; i++)
        {
            ClipsNameLengthsInfo.Add(playerAnimatorController.animationClips[i].name, playerAnimatorController.animationClips[i].length);
        }
    }
    */
    private void AnimCheck() //파라미터를 이용한 매니매이션 변경 함수
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
            //TryChangeDirection(); --> animationEvent가 그냥 편한듯 방향 전환 타이밍이 뒤죽박죽임
        }
        else if(anim.GetCurrentAnimatorStateInfo(0).IsName("Attack3"))
        {
            //TryChangeDirection(); --> animationEvent가 그냥 편한듯 방향 전환 타이밍이 뒤죽박죽임
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
        isAttacking = false; //공격중 변수 초기화
        notInputAttack = false; //막타 공격중 초기화
    }

    IEnumerator DashCoroutine()
    {
        yield return new WaitForSeconds(0.6f - anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
        isDash = false;
        notInputAttack = false;
        yield return new WaitForSeconds(1f - anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
    }

    /*  animEvent로 대체
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


    /*  animEvent로 대체
    IEnumerator ChangeAttackDirectionCoroutine(float _dirX)
    {
        yield return new WaitForSeconds(0.85f - anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
        transform.localScale = new Vector3(_dirX, transform.localScale.y, transform.localScale.z);
        isChangedDirection = false;
    }
    */






}
