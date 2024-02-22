using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

//애니메이션 코드가 길어질 것 같아 따로 코드 제작

public partial class PlayerController : Unit
{
    [SerializeField] AnimatorController playerAnimatorController; //플레이어 애니메이션 변환에 따른 행동을 제어하기 위한 파일

    private Dictionary<string, float> ClipsNameLengthsInfo = new Dictionary<string, float>(); //?? 아직 사용처x
    private IEnumerator processingCoroutine; //진행중인 코루틴 stopcoroutine하기 위해 저장

    private bool isClearedInIdle; //Idle변환
    private float AttackDirection = 1;

    private void ClipsDictionaryInitialize()
    {
        for (int i = 0; i < playerAnimatorController.animationClips.Length; i++)
        {
            ClipsNameLengthsInfo.Add(playerAnimatorController.animationClips[i].name, playerAnimatorController.animationClips[i].length);
        }
    }

    private void AnimCheck() //파라미터를 이용한 매니매이션 변경 함수
    {
        anim.SetBool("Run", isMoving);
        anim.SetBool("Rise", isRising);
        anim.SetBool("Fall", isFalling);
        anim.SetBool("Ground", isGround);
    }

    private void ChangeAnimationParameter()
    {
        int temp = anim.GetCurrentAnimatorStateInfo(0).GetHashCode();
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
            float _tempDir = Input.GetAxisRaw("Horizontal");
            if(AttackDirection != _tempDir && _tempDir != 0)
            {
                AttackDirection = _tempDir;
                ChangeDirection(_tempDir);
            }
            
        }
        else if(anim.GetCurrentAnimatorStateInfo(0).IsName("Attack3"))
        {
            notInputAttack = true;
            if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.8)
            {
                notInputAttack = false;
            }
        }
        else if(anim.GetCurrentAnimatorStateInfo(0).IsName("Dash"))
        {
            ClearAnimationParameter();
            notInputAttack = true;
            StartCoroutine(DashCoroutine());
        }
        
    }

    private void ChangeDirection(float _dirX)
    {
        if (processingCoroutine != null)
        {
            StopCoroutine(processingCoroutine);
        }
        processingCoroutine = ChangeAttackDirection(_dirX);
        StartCoroutine(processingCoroutine);
    }

    private void ClearAnimationParameter()
    {
        isAttacking = false; //공격중 변수 초기화
        notInputAttack = false; //막타 공격중 초기화
    }

    IEnumerator ChangeAttackDirection(float _dirX)
    {
        yield return new WaitForSeconds(1f - anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
        transform.localScale = new Vector3(_dirX, transform.localScale.y, transform.localScale.z);
    }

    IEnumerator DashCoroutine()
    {
        yield return new WaitForSeconds(0.6f - anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
        isDash = false;
        notInputAttack = false;
        yield return new WaitForSeconds(1f - anim.GetCurrentAnimatorStateInfo(0).normalizedTime);

        
    }

    
    
    

}
