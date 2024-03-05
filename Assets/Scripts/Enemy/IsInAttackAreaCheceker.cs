using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsInAttackAreaCheceker : MonoBehaviour
{
    [SerializeField] private BanditController bandit;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision != null)
        {
            if (collision.CompareTag("Player")) bandit.isInTryAttackArea = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.CompareTag("Player")) bandit.isInTryAttackArea = false;
        }
    }
}
