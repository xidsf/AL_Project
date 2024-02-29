using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideCheckCollider : MonoBehaviour
{
    private PlayerController player;

    private void Start()
    {
        player = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                player.CheckPlayerBlocked(true);
            }
            Debug.Log(collision.gameObject);
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                player.CheckPlayerBlocked(false);
            }
        }
    }

}
