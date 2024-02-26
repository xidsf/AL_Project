using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BanditController : Unit
{
    [Header("BanditField")]
    [SerializeField] private TextMeshProUGUI _HPText; //HP표시를 위한 텍스트
    [SerializeField] private Material TransparentMaterial; //적이 반투명해지는 메테리얼
    private Material originMaterial; //원래 사용중이던 메테리얼

    private bool isDead; //적 무환부활을 위한 변수

    private Animator anim;
    private SpriteRenderer mySprite;
    private void Start()
    {
        myRigid = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<BoxCollider2D>(); //필요한 컴포넌트
        anim = GetComponentInChildren<Animator>();
        mySprite = GetComponentInChildren<SpriteRenderer>();
        originMaterial = mySprite.material;
        ClipsDictionaryInitialize();
    }

    // Update is called once per frame
    void Update()
    {
        CalculateImmuneTime();
        TryGroundCheck();
        ChangeTextHP();
        DeathCheck();
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

    protected override void CalculateImmuneTime()
    {
        if (isDead) return;
        base.CalculateImmuneTime();
    }

    protected override void AfterDamaged()
    {
        anim.SetTrigger("Hurt");
    }



    private void ChangeTextHP()
    {
        _HPText.text = __currentHP.ToString() + " / " + __maxHP.ToString();
    }

    private void DeathCheck()
    {
        if (isDead) return;
        if(__currentHP <= 0)
        {
            Death();
        }
    }

    protected override void Death()
    {
        isDead = true;
        anim.SetTrigger("Death");
        isImmune = true;
        StartCoroutine(DeathCoroutine());
    }

    IEnumerator DeathCoroutine()
    {
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(0.2f);
            mySprite.material = TransparentMaterial;
            yield return new WaitForSeconds(0.2f);
            mySprite.material = originMaterial;
        }
        anim.SetTrigger("Recover");
        yield return new WaitForSeconds(UnitAnimationClipInfo["LightBandit_Recover"]);
        __currentHP = __maxHP;
        isImmune = false;
        isDead = false;
    }

}
