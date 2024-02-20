using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackArea : MonoBehaviour
{
    [SerializeField] private Unit attacker;
    private int _damage;

    private void Update()
    {
        _damage = ((int)attacker.GetAttackPoint());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision != null)
        {
            collision.gameObject.GetComponent<Unit>().ChangeMyHealth(_damage);
        }
    }

}
