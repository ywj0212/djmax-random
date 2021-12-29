using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using TMPro;
using DG.Tweening;

public class A_FloorGrid : MonoBehaviour
{
    public TextMeshProUGUI Title;
    public Transform GridParent;
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
    public void FilterCheckEmpty() {
        gameObject.SetActive(GridParent.ChildCountActive() != 0);
    }

    public void FoldToggle() {
        isFolded ^= true;
        GridParent.gameObject.SetActive(!isFolded);

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
        foreach(RectTransform t in GridParent) LayoutRebuilder.ForceRebuildLayoutImmediate(t);
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)GridParent);
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent);
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent.parent);
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent.parent.parent);
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent.parent.parent.parent);
    }
}
