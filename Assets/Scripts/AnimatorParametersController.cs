using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

//���� �Լ��� AnimatorEvent�� �����ϱ� ���� �Լ�

public class AnimatorParametersController : MonoBehaviour
{

    PlayerController thePlayerController;

    private void Start()
    {
        thePlayerController = GetComponentInParent<PlayerController>();
    }

    private void ChangeDirection()
    {
        float _dirX = Input.GetAxisRaw("Horizontal");
        if(_dirX != 0) thePlayerController.transform.localScale = new Vector3(_dirX, thePlayerController.transform.localScale.y, thePlayerController.transform.localScale.z);

    }

}
