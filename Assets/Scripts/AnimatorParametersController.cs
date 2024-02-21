using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

//���� �Լ��� AnimatorEvent�� �����ϱ� ���� �Լ�

public class AnimatorParametersController : MonoBehaviour
{
    [SerializeField] PlayerController thePlayerController;

    private void CallPlayerStateClear()
    {
        thePlayerController.ClearState(); //�����Ӱ� ������ �ʱ�ȭ
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
