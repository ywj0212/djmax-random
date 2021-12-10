using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class A_LevelDivider : MonoBehaviour
{
    public TextMeshProUGUI Title;
    public LvToggleParent LvToggleParent;
    public Transform FloorParent;
    [Space]
    [SerializeField] private Transform FoldIndicator;
    private bool isFolded = false;

    public void FoldToggle() {
        isFolded ^= true;
        FloorParent.gameObject.SetActive(!isFolded);

        if(isFolded)
            FoldIndicator.DORotate(new Vector3(0, 0, 180), 0.3f).SetEase(Ease.InOutCirc);
        else
            FoldIndicator.DORotate(Vector3.zero, 0.3f).SetEase(Ease.InOutCirc);
        
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)FloorParent);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent);
    }
}

