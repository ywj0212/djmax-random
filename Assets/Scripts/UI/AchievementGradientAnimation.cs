using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class AchievementGradientAnimation : MonoBehaviour
{
    [SerializeField] private float dist;
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
        .OnStart(() => {
            maskRect.anchoredPosition = new Vector2(-dist, 0);
            onRect.anchoredPosition = new Vector2(dist / 1.414f, dist / 1.414f);
        });
    }

    public void Animation() {
        seq.Restart();
    }
}
