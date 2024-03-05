using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIndicate : MonoBehaviour
{
    [SerializeField] BanditController bandit;
    RaycastHit2D raycastHit;

    private void OnTriggerStay2D(Collider2D collision)
    {
        CheckPlayer(collision);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckPlayer(collision);
    }

    private void CheckPlayer(Collider2D collision)
    {
        if (!bandit.GetIsCongnizingPlayer())
        {
            if (collision != null)
            {
                if (collision.CompareTag("Player"))
                {
                    if (collision.GetComponent<PlayerController>().isDead) return;
                    Vector3 N_Vector = (collision.gameObject.transform.position - bandit.gameObject.transform.position).normalized;
                    raycastHit = Physics2D.Raycast(bandit.transform.position, N_Vector, 5f);
                    Debug.DrawRay(bandit.transform.position, N_Vector * 5f);
                    if (raycastHit.collider != null)
                    {
                        bandit.PlayerCongnize();
                    }

                }
            }
        }
    }
}
