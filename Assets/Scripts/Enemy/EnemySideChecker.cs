using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySideChecker : MonoBehaviour
{
    [SerializeField] BanditController banditController;
    [SerializeField] string dir;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Ground"))
        {
            banditController.CheckEnemyBlocked(dir, true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            banditController.CheckEnemyBlocked(dir, false);
        }
    }
}
