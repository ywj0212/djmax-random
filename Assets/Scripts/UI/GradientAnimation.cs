using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GradientAnimation : MonoBehaviour
{
    [SerializeField] private float dist;
    [SerializeField] private Image enabledImage;
    private RectTransform maskRect;
    private RectTransform onRect;
    private Sequence seq;

    private void Start() {
        maskRect = (RectTransform)transform;
        onRect = (RectTransform)transform.GetChild(0);

        seq = DOTween.Sequence()
        .SetAutoKill(false)
        .Append(maskRect.DOLocalMove(new Vector2(dist, 0), 1f))
        .Join(onRect.DOLocalMove(new Vector2(-dist / 1.414f, -dist / 1.414f), 1f))
        .InsertCallback(0.5f, () => { enabledImage.DOFade(1, 0f); })
        .OnStart(() => {
            maskRect.anchoredPosition = new Vector2(-dist, 0);
            onRect.anchoredPosition = new Vector2(dist / 1.414f, dist / 1.414f);
            enabledImage.DOFade(0, 0f);
        });
    }

    public void Animation(bool b) {
        if(b)
            seq.Restart();
        else
            enabledImage.DOFade(0, 0f);
    }
}
