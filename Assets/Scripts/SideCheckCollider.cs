using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideCheckCollider : MonoBehaviour
{
    private Unit myUnit;

    private void Start()
    {
        myUnit = FindObjectOfType<Unit>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                myUnit.CheckPlayerBlocked(true);
            }
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                myUnit.CheckPlayerBlocked(false);
            }
        }
    }

}
