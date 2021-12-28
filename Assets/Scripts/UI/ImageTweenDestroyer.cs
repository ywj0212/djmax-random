using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ImageTweenDestroyer : MonoBehaviour
{
    public Tween Tween;
    private void OnDisable() {
        Tween?.Kill();
    }
    private void OnDestroy() {
        Tween?.Kill();
    }
}
