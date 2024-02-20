using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PassableGroundEnable : MonoBehaviour
{
    //private TilemapCollider2D _tilemapColider2D;
    private CompositeCollider2D _compositeCollider2D;

    private void Start()
    {
        //_tilemapColider2D = GetComponent<TilemapCollider2D>();
        _compositeCollider2D = GetComponent<CompositeCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("GroundChecker"))
        {
            _compositeCollider2D.isTrigger = false;
            transform.tag = "Ground";
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        _compositeCollider2D.isTrigger = true;
        transform.tag = "passableGround";
    }

}
