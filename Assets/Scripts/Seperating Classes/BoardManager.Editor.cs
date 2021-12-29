using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Mirix.DMRV;
using DG.Tweening;

public partial class BoardManager : MonoBehaviour
{
    [Header("Editor")]
    [SerializeField] private UnityEngine.UI.Image   EditToggleImage;
    [SerializeField] private Sprite                 EditSprite;
    [SerializeField] private Sprite                 SaveSprite;
    [SerializeField] private GameObject             NewTrackModal;
    [SerializeField] private CanvasGroup            DeleteTrackList;
    private bool isEditing = false;

    public void ReorderablePick(ReorderableList.ReorderableListEventStruct e) {
        A_ReorderableList RD = e.FromList.GetComponent<A_ReorderableList>();
        if(CurrentBoard.Board.CategoryType == Board.Ctgr.Custom)
            BoardReorderEvent.Invoke(null);
        else
            BoardReorderEvent.Invoke(RD.Lv);

        CurrentBoard.Board.Buttons[(int)Manager.BoardButton].Lv.Find(l => l.Lv == RD.Lv).Floor[RD.Floor].Tracks.RemoveAt(e.FromIndex);
    }
    public void ReorderableDrop(ReorderableList.ReorderableListEventStruct e) {
        A_ReorderableList RD = e.ToList.GetComponent<A_ReorderableList>();
        BoardReorderEvent.Invoke(null);

        ushort Index = e.DroppedObject.GetComponent<BoardTrack>().Index;
        CurrentBoard.Board.Buttons[(int)Manager.BoardButton].Lv.Find(l => l.Lv == RD.Lv).Floor[RD.Floor].Tracks.Insert(e.ToIndex, Index);
    }

    public void SetEditMode(bool b) {
        if(b) {
            isEditing = true;
            EditToggleImage.sprite = SaveSprite;
            BoardEditEvent.Invoke(true);
            DeleteTrackList.gameObject.SetActive(true);
            DOTween.To(() => DeleteTrackList.alpha, x => DeleteTrackList.alpha = x, 1f, 0.6f).SetEase(Ease.InOutCirc);
        }
        else {
            isEditing = false;
            EditToggleImage.sprite = EditSprite;
            BoardEditEvent.Invoke(false);
            DOTween.To(() => DeleteTrackList.alpha, x => DeleteTrackList.alpha = x, 0f, 0.6f).SetEase(Ease.InOutCirc);
            DOVirtual.DelayedCall(0.6f, () => DeleteTrackList.gameObject.SetActive(false));
        }
    }
    
    // * 콜백은 연결 해놓았음.
    // TODO: 모달 창 만들고, 뜨는 거 만들어서 적용하기...
    public void OpenAddLevelModal(Transform parent) {
        
    }
    public void OpenDeleteLevelModal(byte lv) {
        
    }
    public void OpenAddFloorModal(Transform parent, byte lv) {

    }
    public void OpenAddTrackToFloorModal(Transform parent, byte lv, byte floorIndex) {
        
    }
    public void OpenDeleteFloorModal(byte lv, byte floorIndex) {

    }
}
