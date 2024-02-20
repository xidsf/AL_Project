using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideCheckCollider : MonoBehaviour
{
    private PlayerController myPlayerController;

    private void Start()
    {
        myPlayerController = FindObjectOfType<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                
                myPlayerController.CheckPlayerBlocked(true);
            }
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                
                myPlayerController.CheckPlayerBlocked(false);
            }
        }
    }

}
