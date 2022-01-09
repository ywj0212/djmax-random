using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Mirix.DMRV;
using DG.Tweening;
using TMPro;

#if !UNITY_WEBGL

public partial class BoardManager : MonoBehaviour
{
#region Inspector
    [Header("Editor")]
    [SerializeField] private Image              EditToggleImage;
    [SerializeField] private Sprite             EditSprite;
    [SerializeField] private Sprite             SaveSprite;
    [SerializeField] private Button             DeleteBoardButton;
    [SerializeField] private CanvasGroup        DeleteTrackList;
    [SerializeField] private Button             AddLevelButton;
    [Space]
    [SerializeField] private TMP_InputField     BoardNameField;
    [SerializeField] private TextMeshProUGUI    BoardTypeField;
    [SerializeField] private GameObject         BoardCriteriaUI;
    [SerializeField] private Toggle             BoardCriteriaToggle;
    [SerializeField] private Image              BoardCriteriaToggleImage;
    [SerializeField] private Button             BoardCriteriaTypeButton;
    [SerializeField] private Image              BoardCriteriaTypeImage;
    [SerializeField] private Sprite[]           BoardCriteriaTypeSprites;
    [SerializeField] private TMP_InputField     BoardCriteriaRate;
    [Space]
    [SerializeField] private GameObject         ModalDuplicateName;
    [SerializeField] private TMP_InputField     DuplicateNameField;
    [SerializeField] private GameObject         ModalNewBoard;
    [SerializeField] private TMP_InputField     NewBoardNameField;
    [SerializeField] private TMP_Dropdown       NewBoardButtonTypeDropDown;
    [SerializeField] private TMP_Dropdown       NewBoardCategoryTypeDropDown;
    [SerializeField] private GameObject         ModalDeletionConfirm;
    [SerializeField] private Button             DeletionConfirmButton;
    [SerializeField] private GameObject         ModalNewLevel;
    [SerializeField] private TMP_Dropdown       NewLevelDropdown;
    [SerializeField] private GameObject         ModalNewTrack;
    [SerializeField] private Transform          NewTrackSearchListParent;
    [SerializeField] private TMP_InputField     NewTrackSearchFieid;
    [SerializeField] private GameObject         ModalBoardControl;
    [SerializeField] private GameObject         ModalExportImage;
    [SerializeField] private TMP_InputField     ExportImageDJNameField;
    [SerializeField] private Toggle             ExportImageShowRecordToggle;
    [SerializeField] private GameObject         LevelDivExportImagePrefab;
    [SerializeField] private GameObject         FloorGridExportImageWithRecordPrefab;
    [SerializeField] private GameObject         FloorGridExportImageWithoutRecordPrefab;
    [SerializeField] private GameObject         TrackExportImageWithRecordPrefab;
    [SerializeField] private GameObject         TrackExportImageWithoutRecordPrefab;
    [SerializeField] private GameObject         LoadingPanel;
    [Space]
    [SerializeField] private RectTransform      ImageExportCanvasRect;
    [SerializeField] private Camera             ImageExportRenderCamera;
    [SerializeField] private RectTransform      ImageExportGenerationParent;
    [SerializeField] private Image              ImageExportBanner;
    [SerializeField] private TextMeshProUGUI    ImageExportHeader;
    [SerializeField] private GameObject         ImageExportStatisticPanel;
    [SerializeField] private TextMeshProUGUI    ImageExportDJName;
    [SerializeField] private TextMeshProUGUI    ImageExportAverages;
    [SerializeField] private TextMeshProUGUI    ImageExportPerfectPlayRatio;
    [SerializeField] private TextMeshProUGUI    ImageExportMaxComboRatio;
    [SerializeField] private TextMeshProUGUI    ImageExportClearRatio;
    [SerializeField] private RectTransform      ImageExportPerfectPlayBarChart;
    [SerializeField] private RectTransform      ImageExportMaxComboBarChart;
    [SerializeField] private RectTransform      ImageExportClearBarChart;
    
#endregion

    private bool isEditing = false;

    public void UpdateBoardDropdown() {
        BoardDropdown.ClearOptions();

        foreach(var b in SystemFileIO.Boards)
            BoardDropdown.options.Add(new TMP_Dropdown.OptionData() { text = b.Name } );

        BoardDropdown.value = SystemFileIO.Boards.IndexOf(CurrentBoard);

        BoardDropdown.RefreshShownValue();
    }
    public void BoardControlModal(bool state) {
        ModalBoardControl.SetActive(state);
    }
    
    public void ExportBoard() {
        FileDialogIO.ExportBoardData(CurrentBoard);

        BoardControlModal(false);
    }
    public void ImportBoard() {
        SystemFileIO.AddBoard(FileDialogIO.ImportBoardData());
        UpdateBoardDropdown();

        BoardControlModal(false);
    }
    
    public void OpenExportImageModal() {
        BoardControlModal(false);

        ModalBackdrop.SetActive(true);
        ModalExportImage.SetActive(true);
    }
    public void CloseExportImageModal() {
        ModalBackdrop.SetActive(false);
        ModalExportImage.SetActive(false);
    }
    public void ExportBoardImage() {
        StartCoroutine(ExportBoardImage(ExportImageDJNameField.text, ExportImageShowRecordToggle.isOn));
    }
    public IEnumerator ExportBoardImage(string djName, bool showRecord) {
        ImageExportCanvasRect.parent.gameObject.SetActive(true);
        ImageExportRenderCamera.gameObject.SetActive(true);
        LoadingPanel.SetActive(true);
        ModalExportImage.SetActive(false);
        yield return null;
        
        ImageExportHeader.text = $"{((CurrentBoard.Board.ButtonType == Board.Type.Seperated) ? Manager.ButtonString[(int)Manager.BoardButton] : "All")} Tunes\n{CurrentBoard.Name}";
        ImageExportDJName.text = djName;
        ushort trackIndex = CurrentBoard.Board.Buttons.First().Lv.Last().Floor.Last().Tracks.First();
        ushort songIndex = SystemFileIO.GetTrackData(trackIndex).SongIndex;

        // * Taken from `void BoardManager(Main).UpdateStatistics()`
        ImageExportStatisticPanel.SetActive(showRecord);
        if(showRecord) {
            int count = AchievementTrackIndexes.Count;
            int perfect = AchievementTrackIndexes.Where(t => (sbyte)SystemFileIO.GetAchievementSave(t).Status >= (sbyte)Achievement.State.Perfect).Count();
            int maxcombo = AchievementTrackIndexes.Where(t => (sbyte)SystemFileIO.GetAchievementSave(t).Status >= (sbyte)Achievement.State.MaxCombo).Count();
            int clear = AchievementTrackIndexes.Where(t => (sbyte)SystemFileIO.GetAchievementSave(t).Status >= (sbyte)Achievement.State.Clear).Count();
            
            var rates = (from t in SystemFileIO.AchievementData.achievements
                        where t.Status != Achievement.State.None && AchievementTrackIndexes.Contains(t.Index)
                        select t.Rate);
            int ratecount = rates.Count();
            float sum = rates.Sum();
                        
            float average = Mathf.Floor((sum / ratecount) * 100f) / 100f;
            
            ImageExportAverages.text = ((ratecount != 0) ? string.Format("{0:0.00}%", average) : "0.00%");
            ImageExportPerfectPlayRatio.text = $"{perfect}/{count}";
            ImageExportMaxComboRatio.text = $"{maxcombo}/{count}";
            ImageExportClearRatio.text = $"{clear}/{count}";
            ImageExportPerfectPlayBarChart.localScale = (count == 0) ? new Vector3(0, 1, 1) : new Vector3((float)perfect/count, 1, 1);
            ImageExportMaxComboBarChart.localScale = (count == 0) ? new Vector3(0, 1, 1) : new Vector3((float)maxcombo/count, 1, 1);
            ImageExportClearBarChart.localScale = (count == 0) ? new Vector3(0, 1, 1) : new Vector3((float)clear/count, 1, 1);
        }

        if(showRecord)
            yield return StartCoroutine(OpenAchievementRoutine(ImageExportGenerationParent, Manager.ViewMode.Grid, LevelDivExportImagePrefab, null, null, FloorGridExportImageWithRecordPrefab, TrackExportImageWithRecordPrefab));
        else
            yield return StartCoroutine(OpenAchievementRoutine(ImageExportGenerationParent, Manager.ViewMode.Grid, LevelDivExportImagePrefab, null, null, FloorGridExportImageWithoutRecordPrefab, TrackExportImageWithoutRecordPrefab));
        
        yield return new WaitUntil(() => SystemFileIO.IsAllWebRequestEnded());

        foreach(RectTransform t in ImageExportCanvasRect) {
            foreach(RectTransform t2 in t) LayoutRebuilder.ForceRebuildLayoutImmediate(t2);
            LayoutRebuilder.ForceRebuildLayoutImmediate(t);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(ImageExportCanvasRect);
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        yield return new WaitForSeconds(2f);
        yield return new WaitForEndOfFrame();
        
        int height = Mathf.FloorToInt(ImageExportCanvasRect.sizeDelta.y);

        RenderTexture Render = new RenderTexture(1200, height, 16, RenderTextureFormat.ARGB32);
        Render.Create();
        ImageExportRenderCamera.targetTexture = Render;

        yield return new WaitForSeconds(1f);
        yield return new WaitForEndOfFrame();
        
        RenderTexture.active = Render;
        Texture2D tex = new Texture2D(Render.width, Render.height);
        tex.ReadPixels(new Rect(0, 0, Render.width, Render.height), 0, 0);
        tex.Apply();
        FileDialogIO.SavePNG(tex);

        ImageExportRenderCamera.targetTexture = null;
        Destroy(Render);
        ModalBackdrop.SetActive(false);
        LoadingPanel.SetActive(false);
        ClearBoard(ImageExportGenerationParent);

        ImageExportCanvasRect.parent.gameObject.SetActive(false);
        ImageExportRenderCamera.gameObject.SetActive(false);
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
            ResetAllFilter();
            EditToggleImage.sprite = SaveSprite;
            DeleteTrackTween?.Kill();
            DeleteTrackList.gameObject.SetActive(true);
            DeleteTrackTween = DOTween.To(() => DeleteTrackList.alpha, x => DeleteTrackList.alpha = x, 1f, 0.6f);
        }
        else {
            EditToggleImage.sprite = EditSprite;
            DeleteTrackTween?.Kill();
            DeleteTrackTween = DOTween.To(() => DeleteTrackList.alpha, x => DeleteTrackList.alpha = x, 0f, 0.6f);
            DOVirtual.DelayedCall(0.6f, () => DeleteTrackList.gameObject.SetActive(false));
            UpdateStatistics();
            SystemFileIO.SaveBoardData();
        }
        isEditing = b;
        BoardEditEvent?.Invoke(b);
        AddLevelButton.gameObject.SetActive(b && CurrentBoard.Board.CategoryType == Board.Ctgr.Level);
        BoardCriteriaUI.SetActive(b || CurrentBoard.Board.Criteria != null);
        BoardCriteriaTypeButton.gameObject.SetActive(CurrentBoard.Board.Criteria != null && (b || CurrentBoard.Board.Criteria.Crit != Board.CriteriaData.CritType.Clear));
        BoardCriteriaRate.gameObject.SetActive(CurrentBoard.Board.Criteria != null && (b || CurrentBoard.Board.Criteria.Rate.HasValue));
        BoardCriteriaToggle.gameObject.SetActive(b);
        BoardCriteriaTypeButton.interactable = b;
        BoardCriteriaRate.interactable = b;
        BoardNameField.interactable = b;

        Canvas.ForceUpdateCanvases();
    }

    public void SetBoardName (string input) {
        CurrentBoard.Name = input;
        DropdownLabel.text = input;

        UpdateBoardDropdown();
    }
    public void SetBoardCriteria(bool state) {
        if(state) {
            BoardCriteriaToggleImage.sprite = Manager.AchievementUI.DeleteSprite;
            CurrentBoard.Board.Criteria = new Board.CriteriaData();
            
            BoardCriteriaTypeButton.gameObject.SetActive(true);
            BoardCriteriaTypeImage.sprite = BoardCriteriaTypeSprites[((int)CurrentBoard.Board.Criteria.Crit)];
            BoardCriteriaRate.gameObject.SetActive(true);
        }
        else {
            BoardCriteriaToggleImage.sprite = Manager.AchievementUI.CreateSprite;
            
            BoardCriteriaTypeButton.gameObject.SetActive(false);
            BoardCriteriaRate.gameObject.SetActive(false);
            BoardCriteriaRate.text = "";

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
        if(CurrentBoard.Board.Criteria == null) return;
        switch(CurrentBoard.Board.Criteria.Crit) {
            case Board.CriteriaData.CritType.Clear: CurrentBoard.Board.Criteria.Crit = Board.CriteriaData.CritType.MaxCombo; break;
            case Board.CriteriaData.CritType.MaxCombo: CurrentBoard.Board.Criteria.Crit = Board.CriteriaData.CritType.Perfect; break;
            case Board.CriteriaData.CritType.Perfect: CurrentBoard.Board.Criteria.Crit = Board.CriteriaData.CritType.Clear; break;
        }

        BoardCriteriaTypeImage.sprite = BoardCriteriaTypeSprites[((int)CurrentBoard.Board.Criteria.Crit)];
    }
    
    public void OpenDuplicateBoard() {
        BoardControlModal(false);
        ModalBackdrop.SetActive(true);
        ModalDuplicateName.SetActive(true);
    }
    public void DuplicateBoard() {
        BoardInfo bi = CurrentBoard.DeepCopy();
        bi.Name = DuplicateNameField.text;

        SystemFileIO.AddBoard(bi);
        UpdateBoardDropdown();

        CloseDuplicateBoard();
    }
    public void CloseDuplicateBoard() {
        ModalBackdrop.SetActive(false);
        ModalDuplicateName.SetActive(false);
    }

    public void OpenNewBoardModal() {
        BoardControlModal(false);
        ModalBackdrop.SetActive(true);
        ModalNewBoard.SetActive(true);
    }
    public void NewBoard() {
        Board b;
        if((Board.Ctgr)NewBoardCategoryTypeDropDown.value == Board.Ctgr.Level) {
            b = new Board() {
                Modifyable = true,
                ButtonType = (Board.Type)NewBoardButtonTypeDropDown.value,
                CategoryType = (Board.Ctgr)NewBoardCategoryTypeDropDown.value,
                Buttons = new Board.ButtonData[] { 
                    new Board.ButtonData() {
                        Lv = new List<Board.ButtonData.LvData>()
                    },
                    new Board.ButtonData() {
                        Lv = new List<Board.ButtonData.LvData>()
                    },
                    new Board.ButtonData() {
                        Lv = new List<Board.ButtonData.LvData>()
                    },
                    new Board.ButtonData() {
                        Lv = new List<Board.ButtonData.LvData>()
                    }
                }
            };
        }
        else {
            b = new Board() {
                Modifyable = true,
                ButtonType = (Board.Type)NewBoardButtonTypeDropDown.value,
                CategoryType = (Board.Ctgr)NewBoardCategoryTypeDropDown.value,
                Buttons = new Board.ButtonData[] { 
                    new Board.ButtonData() {
                        Lv = new List<Board.ButtonData.LvData>() { new Board.ButtonData.LvData() { Lv = 0 } }
                    },
                    new Board.ButtonData() {
                        Lv = new List<Board.ButtonData.LvData>() { new Board.ButtonData.LvData() { Lv = 0 } }
                    },
                    new Board.ButtonData() {
                        Lv = new List<Board.ButtonData.LvData>() { new Board.ButtonData.LvData() { Lv = 0 } }
                    },
                    new Board.ButtonData() {
                        Lv = new List<Board.ButtonData.LvData>() { new Board.ButtonData.LvData() { Lv = 0 } }
                    }
                }
            };
        }
        BoardInfo bi = new BoardInfo(NewBoardNameField.text, b);
        SystemFileIO.AddBoard(bi);
        UpdateBoardDropdown();

        CloseNewBoardModal();
    }
    public void CloseNewBoardModal() {
        ModalBackdrop.SetActive(false);
        ModalNewBoard.SetActive(false);
    }

    public void OpenDeleteBoardModal() {
        BoardControlModal(false);
        OpenDeletionAlert();
        DeletionConfirmButton.onClick.AddListener(() => DeleteBoard());
    }
    public void DeleteBoard() {
        SystemFileIO.RemoveBoard(CurrentBoard);
        SetInitBoard();
        UpdateBoardDropdown();

        CloseDeletionAlert();
    }

    public void OpenAddLevelModal() {
        ModalBackdrop.SetActive(true);
        ModalNewLevel.SetActive(true);
        NewLevelDropdown.ClearOptions();
        
        List<string> Options = new List<string>(15);
        for(byte i = 1; i <= 15; i++)
            if(CurrentBoard.Board.Buttons[(int)Manager.BoardButton].Lv.FindAll(l => l.Lv == i).Count <= 0)
                Options.Add(i.ToString());

        NewLevelDropdown.AddOptions(Options);
    }
    public void CloseAddLevelModal() {
        ModalBackdrop.SetActive(false);
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
                FL.SetEditMode(isEditing);
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)FL.transform);
                break;
            
            case Manager.ViewMode.Grid:
                A_Floor FG = CreateFloorGrid(fDat, parent, Lv, index);
                FG.transform.SetSiblingIndex(0);
                FG.SetEditMode(isEditing);
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
            if(CurrentBoard.Board.CategoryType == Board.Ctgr.Level && t.Value.Lv != AddTrackLv) return false;
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
                AchievementTracks.Add(CreateFloorListTrack(parent, Lv, index));
                break;
            
            case Manager.ViewMode.Grid:
                AchievementTracks.Add(CreateFloorGridTrack(parent, Lv, index));
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