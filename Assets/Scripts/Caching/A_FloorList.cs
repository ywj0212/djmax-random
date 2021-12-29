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
    public Button DeleteFloorButton;
    public Button NewTrackButton;
    private bool isFolded = false;
    
    public void SetEditMode(bool state) {
        DeleteFloorButton.gameObject.SetActive(state);
        NewTrackButton.gameObject.SetActive(state);
        ReorderableList.SetState(state);
    }
    public void FilterCheckEmpty(bool isFilter) {
        gameObject.SetActive(!isFilter || ListParent.ChildCountActive() != 0);
    }

    public void FoldToggle() {
        isFolded ^= true;
        ListParent.gameObject.SetActive(!isFolded);

        if(isFolded)
            FoldIndicator.DORotate(new Vector3(0, 0, 180), 0.3f).SetEase(Ease.InOutCirc);
        else
            FoldIndicator.DORotate(Vector3.zero, 0.3f).SetEase(Ease.InOutCirc);
        
        StartCoroutine(RebuildLayout());
    }
    public void OnReorder() {
        StartCoroutine(RebuildLayout());
    }
    private IEnumerator RebuildLayout() {
        foreach(RectTransform t in ListParent) LayoutRebuilder.ForceRebuildLayoutImmediate(t);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)ListParent);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent);
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent.parent);
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent.parent.parent);
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent.parent.parent.parent);
        yield break;
    }
}
