using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BanditController : Unit
{
    [Header("BanditField")]
    [SerializeField] private TextMeshProUGUI _HPText; //HPǥ�ø� ���� �ؽ�Ʈ
    [SerializeField] private Material TransparentMaterial; //���� ������������ ���׸���
    private Material originMaterial; //���� ������̴� ���׸���

    private bool isDead; //�� ��ȯ��Ȱ�� ���� ����

    private Animator anim;
    private SpriteRenderer mySprite;
    private void Start()
    {
        myRigid = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<BoxCollider2D>(); //�ʿ��� ������Ʈ
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
