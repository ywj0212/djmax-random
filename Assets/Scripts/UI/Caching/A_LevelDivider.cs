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
    public Button DeleteLevelButton;
    public Button NewFloorButton;
    private bool isFolded = false;

    public void SetEditMode(bool state) {
        if(this != null && gameObject != null) {
            DeleteLevelButton?.gameObject.SetActive(state);
            NewFloorButton?.gameObject.SetActive(state);
        }
    }
    public void FilterCheckEmpty(bool isFilter) {
        if(this != null && gameObject != null)
            gameObject.SetActive(!isFilter || FloorParent.ChildCountActive() != 0);
    }

#if !UNITY_WEBGL
    private void OnDestroy() {
        BoardManager.FilterLevelEvent -= new BoardManager.FilterCallback(this.FilterCheckEmpty);
        BoardManager.BoardEditEvent -= new BoardManager.BoardEditCall(this.SetEditMode);
    }
#endif

    public void FoldToggle() {
        isFolded ^= true;
        FloorParent.gameObject.SetActive(!isFolded);

        if(isFolded)
            FoldIndicator.DORotate(new Vector3(0, 0, 180), 0.3f);
        else
            FoldIndicator.DORotate(Vector3.zero, 0.3f);
        
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)FloorParent);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent);
    }
}

