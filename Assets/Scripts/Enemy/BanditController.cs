using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BanditController : Unit
{
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        TryGroundCheck();
    }

    private void FixedUpdate()
    {
        MoveCheck();
    }
}
