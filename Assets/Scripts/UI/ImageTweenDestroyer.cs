using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ImageTweenDestroyer : MonoBehaviour
{
    public Tween Tween;
    public Image Image;
    private void OnDisable() {
        Tween?.Kill();
    }
    private void OnDestroy() {
        Tween?.Kill();
        if(Image.sprite != null) {
            if(Image.sprite.texture != null)
                Destroy(Image.sprite.texture);
            Destroy(Image.sprite);
        }
    }
}
