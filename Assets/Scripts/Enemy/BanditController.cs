using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BanditController : Unit
{
    [SerializeField] private TextMeshProUGUI _HPText;

    private void Start()
    {
        myRigid = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<BoxCollider2D>(); //필요한 컴포넌트
    }

    // Update is called once per frame
    void Update()
    {
        TryGroundCheck();
        ChangeTextHP();
    }

    private void FixedUpdate()
    {
        MoveCheck();
    }

    protected override void Move()
    {
        
    }

    protected override void Attack()
    {
        
    }


    private void ChangeTextHP()
    {
        _HPText.text = __currentHP.ToString() + " / " + __maxHP.ToString();
    }

}
