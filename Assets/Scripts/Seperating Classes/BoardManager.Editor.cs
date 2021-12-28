using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Mirix.DMRV;

public partial class BoardManager : MonoBehaviour
{
    [Header("Editor")]
    [SerializeField] private UnityEngine.UI.Image   EditToggleImage;
    [SerializeField] private Sprite                 EditSprite;
    [SerializeField] private Sprite                 SaveSprite;
    [SerializeField] private GameObject             NewTrackModal;
    [SerializeField] private GameObject             DeleteTrackList;

    //* FromList.gameObject.GetComponent<\datacontainer\>... 통해서 Level, Floor의 인덱스를 얻고!
    //* ...RemoveAt(FromIndex), ...Insert(ToIndex, \data\) 이용해서 이전!
    public void ReorderablePick(ReorderableList.ReorderableListEventStruct e) {
        A_ReorderableList RD = e.FromList.GetComponent<A_ReorderableList>();
        if(CurrentBoard.Board.CategoryType == Board.Ctgr.Custom)
            BoardReorderEvent.Invoke(null);
        else
            BoardReorderEvent.Invoke(RD.Lv);

        
        print($"Pick ▶ L{RD.Lv} / F{RD.Floor} / {e.FromIndex}");
    }
    public void ReorderableDrop(ReorderableList.ReorderableListEventStruct e) {
        A_ReorderableList RD = e.ToList.GetComponent<A_ReorderableList>();
        BoardReorderEvent.Invoke(null);

        print($"Drop ▶ L{RD.Lv} / F{RD.Floor} / {e.ToIndex}");
    }

    //* Transform.GetSiblingIndex(), Transform.childCount 두 개 써서 뭔가 뭔가 할 수 있을 듯
    //* 집어들면 삭제(RemoveAt(index)), 내려놓으면 추가/서순(Insert(index))
    private BoardInfo Editing;
    public void SetEditMode(bool b) {
        if(b) {
            EditToggleImage.sprite = SaveSprite;
            BoardEditEvent.Invoke(true);
        }
        else {
            EditToggleImage.sprite = EditSprite;
            BoardEditEvent.Invoke(false);
        }
    }
    
    public void RestrictDrop() {

    }
    public void ApplyDrop() {

    }
    
    public void AddLevel() {
        
    }
    public void AddFloor() {

    }
    public void AddTrackToFloor() {
        
    }
}
