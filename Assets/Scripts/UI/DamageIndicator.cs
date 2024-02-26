using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageIndicator : MonoBehaviour
{
    
    [SerializeField] private TextMeshProUGUI _textMeshProUGUI;
    private const float _maxSize = 0.7f;
    private const float _minSize = 0.2f;

    public void ShowDamageIndicator(float _damage)
    {
        _textMeshProUGUI.text = _damage.ToString("F1");
        StartCoroutine(DamageColorSizeChange());
    }

    IEnumerator DamageColorSizeChange()
    {
        float _currentFontSize;
        for (int i = 0; i < 60; i++)
        {
            _currentFontSize = _textMeshProUGUI.fontSize;
            _textMeshProUGUI.fontSize = Mathf.Lerp(_currentFontSize, _maxSize, 5f * Time.deltaTime);
            yield return new WaitForSeconds(0.002f);
        }
        StartCoroutine(DamageColorSizeChangeCoroutine());
    }


    IEnumerator DamageColorSizeChangeCoroutine()
    {
        float _currentFontSize;
        Color _currentColor;
        float _changeSpeed = 25f;
        for (int i = 0; i < 80; i++)
        {
            _currentFontSize = _textMeshProUGUI.fontSize;
            _currentColor = _textMeshProUGUI.color;
            _textMeshProUGUI.fontSize = Mathf.Lerp(_currentFontSize, _minSize, _changeSpeed * Time.deltaTime);
            _textMeshProUGUI.color = new Color(Mathf.Lerp(_currentColor.r, 0, _changeSpeed * Time.deltaTime),
                Mathf.Lerp(_currentColor.g, 0, _changeSpeed * Time.deltaTime),
                Mathf.Lerp(_currentColor.b, 0, _changeSpeed * Time.deltaTime), 
                Mathf.Lerp(_currentColor.a, 0, _changeSpeed * Time.deltaTime));
            yield return new WaitForSeconds(0.01f);
        }
        Destroy(this.gameObject);
    }

}
