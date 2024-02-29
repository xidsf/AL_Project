using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using static PlayerController;

//여기 함수는 AnimatorEvent를 적용하기 위한 함수

public class UnitAnimationActionController : MonoBehaviour
{
    public delegate float UnitDamageCalculateDelegate(float __coefficient);
    static public UnitDamageCalculateDelegate UnitDamageCalculate;

    private PlayerController thePlayerController;
    private Animator anim;
    private LayerMask EnemyLayer; //적만 공격하게 하기 위한 layermask

    private void Start()
    {
        thePlayerController = GetComponentInParent<PlayerController>();
        anim = GetComponentInParent<Animator>();
        EnemyLayer = LayerMask.GetMask("Enemy");
    }

    private void ChangeDirection()
    {
        float _dirX = Input.GetAxisRaw("Horizontal");
        if(_dirX != 0) thePlayerController.transform.localScale = new Vector3(_dirX, thePlayerController.transform.localScale.y, thePlayerController.transform.localScale.z);

    }

    private void AttackArea()
    {
        Collider2D[] attackedUnits;//공격 당한 모든 유닛들을 저장하기 위한 배열
        Unit _tempUnit; //공격당한 유닛들

        PlayerAttackActionInfo _applyedPlayerAttack = new PlayerAttackActionInfo();
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack1"))
        {
            _applyedPlayerAttack = thePlayerController.Attack1;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack2"))
        {
            _applyedPlayerAttack = thePlayerController.Attack2;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack3"))
        {
            _applyedPlayerAttack = thePlayerController.Attack3;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Dash-Attack"))
        {
            _applyedPlayerAttack = thePlayerController.Dash_Attack;
        }
        attackedUnits = Physics2D.OverlapBoxAll(thePlayerController.transform.position + new Vector3(_applyedPlayerAttack.attackAreaCenter.x, _applyedPlayerAttack.attackAreaCenter.y, 0) * thePlayerController.transform.localScale.x, _applyedPlayerAttack.attackArea, 0, EnemyLayer);
        
        foreach (var unit in attackedUnits)
        {
            _tempUnit = unit.GetComponent<Unit>();
            if (_tempUnit != null)
            {
                _tempUnit.ChangeMyHealth(-UnitDamageCalculate(_applyedPlayerAttack.attackCoefficient));
            }
        }
    }

    
}
