using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirix.DMRV;
using DG.Tweening;
using TMPro;

using Ookii.Dialogs;

#if !UNITY_WEBGL

// * 모든 GameObject.SetActive(bool state)를 하고 Dotween 걸어서 UI 모션 자연스럽게 처리해주면 너무 좋을 것 같은디..
// * 이거까지 다 구현하고 출시하자.
public partial class BoardManager : MonoBehaviour
{
#region Inspector
    [Header("Editor")]
    [SerializeField] private Image          EditToggleImage;
    [SerializeField] private Sprite         EditSprite;
    [SerializeField] private Sprite         SaveSprite;
    [SerializeField] private CanvasGroup    DeleteTrackList;
    [SerializeField] private Button         AddLevelButton;
    [Space]
    [SerializeField] private GameObject     BoardCriteriaPanel;
    [SerializeField] private Toggle         BoardCriteriaToggle;
    [SerializeField] private Image          BoardCriteriaToggleImage;
    [SerializeField] private ToggleGroup    BoardCriteriaToggleGroup;
    [SerializeField] private Toggle         BoardCriteriaPP;
    [SerializeField] private Toggle         BoardCriteriaMC;
    [SerializeField] private TMP_InputField BoardCriteriaRate;
    [Space]
    [SerializeField] private GameObject     ModalBackDrop;
    [SerializeField] private GameObject     ModalDuplicateName;
    [SerializeField] private GameObject     ModalNewBoard;
    [SerializeField] private GameObject     ModalDeletionConfirm;
    [SerializeField] private Button         DeletionConfirmButton;
    [SerializeField] private GameObject     ModalNewLevel;
    [SerializeField] private TMP_Dropdown   NewLevelDropdown;
    [SerializeField] private GameObject     ModalNewTrack;
    [SerializeField] private Transform      NewTrackSearchListParent;
    [SerializeField] private TMP_InputField NewTrackSearchFieid;
#endregion
    [Space] // ! TESTING
    [SerializeField] private RectTransform          ImageExportCanvasRect;
    [SerializeField] private Camera                 ImageExportRenderCamera;
    [SerializeField] private RenderTexture Test;
    private bool isEditing = false;

    public void ShowBoard(int index) { // TODO

    }
    public void DuplicateBoard(int index) { // TODO

    }
    public void DeleteBoard(int index) { // TODO

    }
    public void ExportBoard() { // TODO

    }
    public void ImportBoard() { // TODO

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

    public void ReorderablePick(UnityEngine.UI.Extensions.ReorderableList.ReorderableListEventStruct e) {
        A_ReorderableList RD = e.FromList.GetComponent<A_ReorderableList>();
        if(CurrentBoard.Board.CategoryType == Board.Ctgr.Custom)
            BoardReorderEvent?.Invoke(null);
        else
            BoardReorderEvent?.Invoke(RD.Lv);

        CurrentBoard.Board.Buttons[(int)Manager.BoardButton].Lv.Find(l => l.Lv == RD.Lv).Floor[RD.Floor].Tracks.RemoveAt(e.FromIndex);
    }
    public void ReorderableDrop(UnityEngine.UI.Extensions.ReorderableList.ReorderableListEventStruct e) {
        A_ReorderableList RD = e.ToList.GetComponent<A_ReorderableList>();
        BoardReorderEvent?.Invoke(null);

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
        BoardEditEvent?.Invoke(b);
        AddLevelButton.gameObject.SetActive(b && CurrentBoard.Board.CategoryType == Board.Ctgr.Level);
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
    
    public void OpenAddLevelModal() {
        ModalBackDrop.SetActive(true);
        ModalNewLevel.SetActive(true);
        NewLevelDropdown.ClearOptions();
        
        List<string> Options = new List<string>(15);
        for(byte i = 1; i <= 15; i++)
            if(CurrentBoard.Board.Buttons[(int)Manager.BoardButton].Lv.FindAll(l => l.Lv == i).Count <= 0)
                Options.Add(i.ToString());

        NewLevelDropdown.AddOptions(Options);
    }
    public void CloseAddLevelModal() {
        ModalBackDrop.SetActive(false);
        ModalNewLevel.SetActive(false);
    }
    public void AddLevel() {
        if(Byte.TryParse(NewLevelDropdown.options[NewLevelDropdown.value].text, out byte l)) {
            var Lv = CurrentBoard.Board.Buttons[(int)Manager.BoardButton].Lv;

            if(Lv.FindAll(t => t.Lv == l).Count() > 0) return;
            
            Board.ButtonData.LvData lDat = new Board.ButtonData.LvData() { Lv = l };
            Lv.Add(lDat);
            Lv = Lv.OrderBy(t => t.Lv).ToList();

            A_LevelDivider LDUI = CreateLevelDivider(Manager.AchievementUI.ScrollViewport, l);
            LDUI.transform.SetSiblingIndex(Lv.Count - Lv.IndexOf(lDat) - 1);
            LDUI.FilterCheckEmpty(isFilter);
            LDUI.SetEditMode(isEditing);
            
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)LDUI.transform);
            foreach(RectTransform t in Manager.AchievementUI.ScrollViewport) LayoutRebuilder.ForceRebuildLayoutImmediate(t);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Manager.AchievementUI.ScrollViewport);
        }

        CloseAddLevelModal();
    }

    public void AddFloor(Transform parent, byte Lv) {
        Board.ButtonData.LvData.FloorData fDat = new Board.ButtonData.LvData.FloorData() { Tracks = new List<ushort>() };

        var LvData = CurrentBoard.Board.Buttons[(int)Manager.BoardButton].Lv.First(l => l.Lv == Lv);
        byte index = (byte)(LvData.Floor.Count);
        LvData.Floor.Add(fDat);

        switch(Manager.BoardViewMode) {
            case Manager.ViewMode.List:
                A_Floor FL = CreateFloorList(fDat, parent, Lv, index);
                FL.transform.SetSiblingIndex(0);
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)FL.transform);
                break;
            
            case Manager.ViewMode.Grid:
                A_Floor FG = CreateFloorGrid(fDat, parent, Lv, index);
                FG.transform.SetSiblingIndex(0);
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)FG.transform);
                break;
        }

        foreach(RectTransform t in parent) LayoutRebuilder.ForceRebuildLayoutImmediate(t);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)parent);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)parent.parent);
        foreach(RectTransform t in Manager.AchievementUI.ScrollViewport) LayoutRebuilder.ForceRebuildLayoutImmediate(t);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Manager.AchievementUI.ScrollViewport);
    }

    private Transform AddTrackParent;
    private byte AddTrackLv;
    private int AddTrackFloorIndex;
    public void OpenAddTrackToFloorModal(Transform parent, byte Lv, int floorIndex) {
        foreach(Transform t in NewTrackSearchListParent) Destroy(t.gameObject);
        NewTrackSearchFieid.text = string.Empty;

        AddTrackParent = parent;
        AddTrackLv = Lv;
        AddTrackFloorIndex = floorIndex;

        ModalBackdrop.SetActive(true);
        ModalNewTrack.SetActive(true);
    }
    public void CloseAddTrackToFloorModal() {
        foreach(Transform t in NewTrackSearchListParent) Destroy(t.gameObject);

        ModalBackdrop.SetActive(false);
        ModalNewTrack.SetActive(false);
    }
    public void AddTrackSearch(string query) {
        foreach(Transform t in NewTrackSearchListParent) Destroy(t.gameObject);
        if(string.IsNullOrWhiteSpace(query)) return;
        var tracks = SystemFileIO.MainData.TrackTable.Where(t => {
            if(t.Value.Lv != AddTrackLv) return false;
            if(CurrentBoard.Board.ButtonType == Board.Type.Seperated && t.Value.Bt != Manager.ButtonString[(int)Manager.BoardButton]) return false;

            MainData.SongInfo SongData = SystemFileIO.GetSongData(t.Value.SongIndex);
            string match = $"{SongData.Name} {t.Value.Bt} {t.Value.Diff}";
            return match.Contains(query, StringComparison.OrdinalIgnoreCase);
        }).Take(16);
        foreach(var ti in tracks) {
            CreateFloorListTrack(NewTrackSearchListParent, AddTrackLv, ti.Key, checkInfoOpened: false, openInfoOnClick: () => {
                AddTrackToFloor(ti.Key, AddTrackParent, AddTrackLv, AddTrackFloorIndex);
                CloseAddTrackToFloorModal();
            });
        }
    }
    public void AddTrackToFloor(ushort index, Transform parent, byte Lv, int floorIndex) {
        MainData.TrackInfo TrackData = SystemFileIO.GetTrackData(index);
        MainData.SongInfo SongData = SystemFileIO.GetSongData(TrackData.SongIndex);
        Achievement AchievementData = SystemFileIO.GetAchievementSave(index);

        CurrentBoard.Board.Buttons[(int)Manager.BoardButton].Lv.First(l => l.Lv == Lv).Floor[floorIndex].Tracks.Add(index);

        switch(Manager.BoardViewMode) {
            case Manager.ViewMode.List:
                CreateFloorListTrack(parent, Lv, index);
                break;
            
            case Manager.ViewMode.Grid:
                CreateFloorGridTrack(parent, Lv, index);
                break;
        }

        CloseAddTrackToFloorModal();
        AchievementTrackIndexes.Add(index);

        foreach(RectTransform t in AddTrackParent) LayoutRebuilder.ForceRebuildLayoutImmediate(t);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)AddTrackParent);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)AddTrackParent.parent);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)AddTrackParent.parent.parent);
        foreach(RectTransform t in Manager.AchievementUI.ScrollViewport) LayoutRebuilder.ForceRebuildLayoutImmediate(t);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Manager.AchievementUI.ScrollViewport);
    }

    public void OpenDeleteLevelModal(byte Lv) {
        OpenDeletionAlert();
        DeletionConfirmButton.onClick.AddListener(() => DeleteLevel(Lv));
    }
    public void DeleteLevel(byte Lv) {
        List <Board.ButtonData.LvData> LvData = CurrentBoard.Board.Buttons[(int)Manager.BoardButton].Lv;
        int index = LvData.FindIndex(l => l.Lv == Lv);
        Destroy(Manager.AchievementUI.ScrollViewport.GetChild(LvData.Count - index - 1).gameObject);
        LvData.RemoveAt(index);

        CloseDeletionAlert();
    }

    public void OpenDeleteFloorModal(Transform parent, byte Lv, int floorIndex) {
        OpenDeletionAlert();
        DeletionConfirmButton.onClick.AddListener(() => DeleteFloor(parent, Lv, floorIndex));
    }
    public void DeleteFloor(Transform parent, byte Lv, int floorIndex) {
        List<Board.ButtonData.LvData.FloorData> FloorData = CurrentBoard.Board.Buttons[(int)Manager.BoardButton].Lv.First(l => l.Lv == Lv).Floor;
        Destroy(parent.GetChild(FloorData.Count - floorIndex - 1).gameObject);
        FloorData.RemoveAt(floorIndex);

        CloseDeletionAlert();
    }

    public void OpenDeletionAlert() {
        DeletionConfirmButton.onClick.RemoveAllListeners();

        ModalBackdrop.SetActive(true);
        ModalDeletionConfirm.SetActive(true);
    }
    public void CloseDeletionAlert() {
        ModalBackdrop.SetActive(false);
        ModalDeletionConfirm.SetActive(false);
    }
}

#endif