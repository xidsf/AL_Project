using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackAction : MonoBehaviour
{
    [SerializeField] BanditController bandit;

    private void AttackArea()
    {
        bandit.AttackArea();
    }

    private void CancelIsAttacking()
    {
        bandit.CancelIsAttacking();
    }

}
