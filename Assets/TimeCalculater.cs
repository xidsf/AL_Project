using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeCalculater : MonoBehaviour
{
    private TextMeshProUGUI Text;
    private float currentTime = 0;
    private BanditController bandit;
    private bool isdead = false;

    void Start()
    {
        Text = GetComponent<TextMeshProUGUI>();
        bandit = FindObjectOfType<BanditController>();
    }

    void Update()
    {
        if(!bandit.isDead && !isdead)
        {
            
            currentTime += Time.deltaTime;
            Text.text = (Mathf.Ceil(currentTime*100f)*0.01f).ToString() + "sec";
        }
        else
        {
            isdead = true;
            
            Text.color = Color.blue;
        }
    }
}
