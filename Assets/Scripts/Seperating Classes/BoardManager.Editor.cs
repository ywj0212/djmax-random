using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Mirix.DMRV;
using DG.Tweening;
using TMPro;

using Ookii.Dialogs;

public partial class BoardManager : MonoBehaviour
{
    [Header("Editor")]
    [SerializeField] private UnityEngine.UI.Image   EditToggleImage;
    [SerializeField] private Sprite                 EditSprite;
    [SerializeField] private Sprite                 SaveSprite;
    [SerializeField] private CanvasGroup            DeleteTrackList;
    [SerializeField] private Button                 AddLevelButton;
    [Space]
    [SerializeField] private GameObject             BoardCriteriaPanel;
    [SerializeField] private Toggle                 BoardCriteriaToggle;
    [SerializeField] private Image                  BoardCriteriaToggleImage;
    [SerializeField] private ToggleGroup            BoardCriteriaToggleGroup;
    [SerializeField] private Toggle                 BoardCriteriaPP;
    [SerializeField] private Toggle                 BoardCriteriaMC;
    [SerializeField] private TMP_InputField         BoardCriteriaRate;
    [Space]
    [SerializeField] private GameObject             NewTrackModal;
    [SerializeField] private GameObject             ModalDuplicateName;
    [SerializeField] private GameObject             ModalNewBoard;
    [SerializeField] private GameObject             ModalDeletionConfirm;
    [SerializeField] private Button                 DeletionConfirmButton;
    [SerializeField] private GameObject             NewLevelModal;
    [SerializeField] private TMP_Dropdown           LevelDropdown;
    [Space]
    [SerializeField] private RectTransform          ImageExportCanvasRect;
    [SerializeField] private Camera                 ImageExportRenderCamera;
    [SerializeField] private RenderTexture Test;
    private bool isEditing = false;

    public void ShowBoard(int index) {

    }
    public void DuplicateBoard(int index) {

    }
    public void DeleteBoard(int index) {

    }
    public void ExportBoard() {

    }
    public void ImportBoard() {

    }

    private void Start() {

    }
    public IEnumerator ExportBoardImage() { // TODO 동적 생성 체크
        // int height = Mathf.CeilToInt(ImageExportCanvasRect.sizeDelta.y);
        // RenderTexture Render = new RenderTexture(850, height, 16, RenderTextureFormat.ARGB32);
        // Render.Create();
        // ImageExportRenderCamera.targetTexture = Render;
        
        // do something ...

        // Destroy(Render);
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForEndOfFrame();

        print($"{Test.width}, {Test.height}");
        print($"{ImageExportRenderCamera.scaledPixelWidth}, {ImageExportRenderCamera.scaledPixelHeight}");
        RenderTexture.active = Test;
        Texture2D tex = new Texture2D(Test.width, Test.height);
        tex.ReadPixels(new Rect(0, 0, Test.width, Test.height), 0, 0);
        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();

        VistaSaveFileDialog SaveFileDialog = new VistaSaveFileDialog();
        SaveFileDialog.Title = "Export Board File";
        SaveFileDialog.Filter = "PNG Image |*.png";
        var Result = SaveFileDialog.ShowDialog();
        if(Result == System.Windows.Forms.DialogResult.OK) {
            System.IO.File.WriteAllBytes(SaveFileDialog.FileName, bytes);
        }
    }

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

    private Tween DeleteTrackTween;
    public void SetEditMode(bool b) {
        if(b) {
            EditToggleImage.sprite = SaveSprite;
            DeleteTrackTween?.Kill();
            DeleteTrackList.gameObject.SetActive(true);
            DeleteTrackTween = DOTween.To(() => DeleteTrackList.alpha, x => DeleteTrackList.alpha = x, 1f, 0.6f).SetEase(Ease.InOutCirc);
        }
        else {
            EditToggleImage.sprite = EditSprite;
            DeleteTrackTween?.Kill();
            DeleteTrackTween = DOTween.To(() => DeleteTrackList.alpha, x => DeleteTrackList.alpha = x, 0f, 0.6f).SetEase(Ease.InOutCirc);
            DOVirtual.DelayedCall(0.6f, () => DeleteTrackList.gameObject.SetActive(false));
            UpdateStatistics();
        }
        BoardEditEvent.Invoke(b);
        AddLevelButton.gameObject.SetActive(b);
        isEditing = b;
        BoardCriteriaPanel.SetActive(b || CurrentBoard.Board.Criteria != null);
        BoardCriteriaPP.gameObject.SetActive(CurrentBoard.Board.Criteria != null && (b || CurrentBoard.Board.Criteria.Crit == Board.CriteriaData.CritType.Perfect));
        BoardCriteriaMC.gameObject.SetActive(CurrentBoard.Board.Criteria != null && (b || CurrentBoard.Board.Criteria.Crit == Board.CriteriaData.CritType.MaxCombo));
        BoardCriteriaRate.gameObject.SetActive(CurrentBoard.Board.Criteria != null && (b || CurrentBoard.Board.Criteria?.Rate != null));
        BoardCriteriaToggle.gameObject.SetActive(b);
        BoardCriteriaRate.interactable = b;

        Canvas.ForceUpdateCanvases();
    }

    public void SetBoardCriteria(bool state) {
        if(state) {
            BoardCriteriaToggleImage.sprite = Manager.AchievementUI.DeleteSprite;
            
            BoardCriteriaPP.gameObject.SetActive(true);
            BoardCriteriaMC.gameObject.SetActive(true);
            BoardCriteriaRate.gameObject.SetActive(true);

            CurrentBoard.Board.Criteria = new Board.CriteriaData();
        }
        else {
            BoardCriteriaToggleImage.sprite = Manager.AchievementUI.CreateSprite;
            
            BoardCriteriaPP.gameObject.SetActive(false);
            BoardCriteriaMC.gameObject.SetActive(false);
            BoardCriteriaRate.gameObject.SetActive(false);

            CurrentBoard.Board.Criteria = null;
        }
    }
    public void SetBoardCriteriaRate(string input) {
        if(float.TryParse(input, out float r)) {
            BoardCriteriaRate.text = string.Format("{0:0.00}", r);
            CurrentBoard.Board.Criteria.Rate = r;
        }
        else { 
            BoardCriteriaRate.text = "";
            CurrentBoard.Board.Criteria.Rate = null;
        }
    }
    public void SetBoardCriteriaState() {
        Board.CriteriaData.CritType state = (Board.CriteriaData.CritType)(Manager.GetSelectedToggleIndex(BoardCriteriaToggleGroup) + 1 ?? 0);
        CurrentBoard.Board.Criteria.Crit = state;
    }
    
    // TODO 여기 밑에서 부터 구현할거 산더미....
    // * 콜백은 연결 해놓았음.
    // TODO: 모달 창 만들고, 뜨는 거 만들어서 적용하기...
    // ! 레벨 추가 버튼 위치 못 찾음....
    public void OpenAddLevelModal(Transform parent, Board.Button button) {
        // ? 현재 목록에 존재하는 레벨은 제외!
        // CurrentBoard.Board.Buttons[(int)Manager.BoardButton].Lv[i].Lv
    }
    public void AddLevel() {

    }

    public void OpenAddFloorModal(Transform parent, Board.Button button, byte lv) {
        // * 이거 보드 타입이 레벨이 아니면, 이름도 기입하게 해야하는데 어카지..
        // * 또 이거 이름 수정할 수 있게 해줘야 할 거 아니야...
    }
    public void AddFloor() {

    }

    public void OpenAddTrackToFloorModal(Transform parent, Board.Button button, byte lv, byte floorIndex) {
        // * 퀵 기입 코드 재활용
    }
    public void AddTrackToFloor() {

    }

    public void OpenDeleteLevelModal(Board.Button button, byte lv) {
        // ? 이건 그냥 경고창
    }
    public void DeleteLevel() {

    }

    public void OpenDeleteFloorModal(Board.Button button, byte lv, byte floorIndex) {
        // ? 이건 그냥 경고창
    }
    public void DeleteFloor() {

    }

    public void OpenDeletionAlert() {

    }
    public void CloseDeletionAlert() {

    }
}
