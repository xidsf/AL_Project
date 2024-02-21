using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

//여기 함수는 AnimatorEvent를 적용하기 위한 함수

public class AnimatorParametersController : MonoBehaviour
{
    [SerializeField] PlayerController thePlayerController;

    private void CallPlayerStateClear()
    {
        thePlayerController.ClearState(); //움직임과 공격중 초기화
    }

    private void DoNotInputAttackTrue()
    {
        thePlayerController.ChangeLastNormalAttack(true);
    }

    private void DoNotInputAttackFalse()
    {
        thePlayerController.ChangeLastNormalAttack(false);
    }

    private void isAttackingTrue()
    {
        thePlayerController.ChangeisAttacking(true);
    }

    private void FinishDashSpeed()
    {
        thePlayerController.FinishDash();
    }

    private void ChangePlayerDir()
    {
        float _dirX = Input.GetAxisRaw("Horizontal");
        if(_dirX != 0)
        {
            thePlayerController.transform.localScale = new Vector3(_dirX, thePlayerController.transform.localScale.y, thePlayerController.transform.localScale.z);
        }
        
    }

}
