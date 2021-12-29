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
            UpdateStatistics();
            DOTween.To(() => DeleteTrackList.alpha, x => DeleteTrackList.alpha = x, 0f, 0.6f).SetEase(Ease.InOutCirc);
            DOVirtual.DelayedCall(0.6f, () => DeleteTrackList.gameObject.SetActive(false));
        }
    }
    
    // * 콜백은 연결 해놓았음.
    // TODO: 모달 창 만들고, 뜨는 거 만들어서 적용하기...
    // ! 레벨 추가 버튼 위치 못 찾음....
    public void OpenAddLevelModal(Transform parent, Board.Button button) {
        // ? 현재 목록에 존재하는 레벨은 제외!
    }
    public void OpenDeleteLevelModal(Board.Button button, byte lv) {
        // ? 이건 그냥 경고창
    }
    public void OpenAddFloorModal(Transform parent, Board.Button button, byte lv) {
        // * 이거 보드 타입이 레벨이 아니면, 이름도 기입하게 해야하는데 어카지..
        // * 또 이거 이름 수정할 수 있게 해줘야 할 거 아니야...
    }
    public void OpenAddTrackToFloorModal(Transform parent, Board.Button button, byte lv, byte floorIndex) {
        // * 퀵 기입 코드 재활용
    }
    public void OpenDeleteFloorModal(Board.Button button, byte lv, byte floorIndex) {
        // ? 이건 그냥 경고창
    }
}
