using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeEnemyHealthIndicator : MonoBehaviour
{
    private RectTransform myRectTransform;
    private BanditController banditController;

    private void Start()
    {
        myRectTransform = GetComponent<RectTransform>();
        banditController = GetComponentInParent<BanditController>();
    }

    void Update()
    {
        myRectTransform.localScale = banditController.transform.localScale;
    }
}
