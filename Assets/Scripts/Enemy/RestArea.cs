using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestArea : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            BanditController bandit = collision.GetComponent<BanditController>();
            bandit.isInRestArea = true;
            
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            BanditController bandit = collision.GetComponent<BanditController>();
            StartCoroutine(HealCoroutine(bandit));

        }

    }

    IEnumerator HealCoroutine(BanditController _bandit)
    {
        while(_bandit.CheckRestHeal())
        {
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.GetComponent<BanditController>().isInRestArea = false;
        }
    }

    

}
