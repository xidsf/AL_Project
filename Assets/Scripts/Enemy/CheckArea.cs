using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckArea : MonoBehaviour
{
    public bool isPlayerIn;

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision != null)
        {
            if(collision.gameObject.CompareTag("Player"))
            {
                isPlayerIn = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision != null)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                isPlayerIn = true;
            }
        }
    }

}
