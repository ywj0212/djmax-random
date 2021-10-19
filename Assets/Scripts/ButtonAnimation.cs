using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ButtonAnimation : MonoBehaviour
{
    [SerializeField] private float moveAmount;
    [SerializeField] private Toggle toggle;
    private Vector2 moveVector;
    private Vector2 innerVector;

    private RectTransform maskRect;
    private RectTransform onRect;
    private void Start() {
        maskRect = (RectTransform)transform.GetChild(0).GetChild(0);
        onRect = (RectTransform)transform.GetChild(0).GetChild(0).GetChild(0);

        moveVector = new Vector2(-moveAmount, moveAmount);
        innerVector = new Vector2(moveAmount * Mathf.Sqrt(2), 0);

        if(toggle == null)
            toggle = GetComponent<Toggle>();
        
        Animate(toggle.isOn);
    }

    public void Animate(bool b) {
        if(b) {
            maskRect.DOLocalMove(Vector2.zero, 0.3f).SetEase(Ease.OutQuad);
            onRect.DOLocalMove(Vector2.zero, 0.3f).SetEase(Ease.OutQuad);
        }
        else {
            maskRect.DOLocalMove(moveVector, 0.3f).SetEase(Ease.OutQuad);
            onRect.DOLocalMove(innerVector, 0.3f).SetEase(Ease.OutQuad);
        }
    }
}