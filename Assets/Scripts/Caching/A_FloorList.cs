using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using TMPro;
using DG.Tweening;

public class A_FloorList : MonoBehaviour
{
    public TextMeshProUGUI Title;
    public Transform ListParent;
    public ReorderableList ReorderableList;
    [Space]
    [SerializeField] private Transform FoldIndicator;
    private bool isFolded = false;

    public void FoldToggle() {
        isFolded ^= true;
        ListParent.gameObject.SetActive(!isFolded);

        if(isFolded)
            FoldIndicator.DORotate(new Vector3(0, 0, 180), 0.3f).SetEase(Ease.InOutCirc);
        else
            FoldIndicator.DORotate(Vector3.zero, 0.3f).SetEase(Ease.InOutCirc);
        
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)ListParent);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent.parent);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent.parent.parent);
    }

    public void OnReorder() {
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)ListParent);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent.parent);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent.parent.parent);
    }
}
