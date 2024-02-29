using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

//애니메이션 코드가 길어질 것 같아 따로 코드 제작...했는데 별로 안김,,,ㅋ

public partial class PlayerController : Unit
{
    
    private bool isClearedInIdle; //Idle변환 / 한번 상태 초기화 시키면 다른 액션을 할 때까지 초기화X
    
    private const float dashEndTime = 3f / 7f;
    IEnumerator _savedDashCoroutine;

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
        }
        else if(anim.GetCurrentAnimatorStateInfo(0).IsName("Attack3"))
        {
            notInputAttack = true;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Dash-Attack"))
        {
            isAttacking = true;
            notInputAttack = true;
            if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 3f / 11f)
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
        isAttacking = false; //공격중 변수 초기화
        notInputAttack = false; //막타 공격중 초기화
    }

    IEnumerator DashCoroutine()
    {
        yield return new WaitForSeconds(UnitAnimationClipInfo["Dash"] * (1f - dashEndTime));
        isDash = false;
    }
    
}
