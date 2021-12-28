using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class A_FloorList : MonoBehaviour
{
    public TextMeshProUGUI Title;
    public Transform ListParent;
    public A_ReorderableList ReorderableList;

    [SerializeField] private Transform FoldIndicator;
    [SerializeField] private Button DeleteFloorButton;
    [SerializeField] private Button NewTrackButton;
    private bool isFolded = false;
    
    public void SetEditMode(bool state) {
        DeleteFloorButton.gameObject.SetActive(state);
        NewTrackButton.gameObject.SetActive(state);
        ReorderableList.SetState(state);
    }
    public void FilterCheckEmpty() {
        if(ListParent.ChildCountActive() == 0) {
            gameObject.SetActive(false);
        }
        else {
            gameObject.SetActive(true);
        }
    }

    public void FoldToggle() {
        isFolded ^= true;
        ListParent.gameObject.SetActive(!isFolded);

        if(isFolded)
            FoldIndicator.DORotate(new Vector3(0, 0, 180), 0.3f).SetEase(Ease.InOutCirc);
        else
            FoldIndicator.DORotate(Vector3.zero, 0.3f).SetEase(Ease.InOutCirc);
        
        RebuildLayout();
    }
    public void OnReorder() {
        DOVirtual.DelayedCall(0.01f, RebuildLayout);
    }
    public void RebuildLayout() {
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)ListParent);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent.parent);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent.parent.parent);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent.parent.parent.parent);
    }
}
