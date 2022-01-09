using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ImageTweenDestroyer : MonoBehaviour
{
    public Tween Tween;
    public Image Image;
    public Sprite Sprite;
    public Texture2D Texture;

    public void Dispose() {
        OnDestroy();
    }
    private void OnDestroy() {
        Tween?.Kill();
        
        if(Sprite != null) Destroy(Sprite);
        if(Texture != null) Destroy(Texture);
    }
}
