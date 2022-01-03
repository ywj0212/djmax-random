using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Mirix.DMRV;

public partial class BoardManager : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private ushort             ProgramVersion;
    [SerializeField] private TMP_Dropdown       BoardDropdown;
    [SerializeField] private TextMeshProUGUI    DropdownLabel;
    [Space]
    [SerializeField] private GameObject         ModalBackdrop;
    [SerializeField] private GameObject         ModalQuickRecord;
    [SerializeField] private Transform          QuickRecordSearchListParent;
    [SerializeField] private TMP_InputField     QuickRecordSearchField;

    public void UpdateBoardDropdown() {
        BoardDropdown.options.Clear();
        DropdownLabel.text = CurrentBoard.Name;

        foreach(var b in SystemFileIO.Boards)
            BoardDropdown.options.Add(new TMP_Dropdown.OptionData() { text = b.Name } );
    }

    private BoardInfo CurrentBoard;
    public void SetInitBoard() {
        CurrentBoard = SystemFileIO.Boards[0];
    }
    public void ChangeBoard(int i) {
        CurrentBoard = SystemFileIO.Boards[i];
        OpenAchievement();
    }

    public void OpenQuickRecordModal() {
        foreach(Transform t in QuickRecordSearchListParent) Destroy(t.gameObject);
        QuickRecordSearchField.text = string.Empty;

        ModalBackdrop.SetActive(true);
        ModalQuickRecord.SetActive(true);
    }
    public void CloseQuickRecordModal() {
        foreach(Transform t in QuickRecordSearchListParent) Destroy(t.gameObject);

        ModalBackdrop.SetActive(false);
        ModalQuickRecord.SetActive(false);
    }
    public void QuickRecordSearch(string query) {
        foreach(Transform t in QuickRecordSearchListParent) Destroy(t.gameObject);
        if(string.IsNullOrWhiteSpace(query)) return;
        var tracks = SystemFileIO.MainData.TrackTable.Where(t => {
            MainData.SongInfo SongData = SystemFileIO.GetSongData(t.Value.SongIndex);
            string match = $"{SongData.Name} {t.Value.Bt} {t.Value.Diff}";
            return match.Contains(query, StringComparison.OrdinalIgnoreCase);
        }).Take(16);
        foreach(var ti in tracks) {
            ushort index = ti.Key;
            MainData.TrackInfo TrackData = SystemFileIO.GetTrackData(index);
            MainData.SongInfo SongData = SystemFileIO.GetSongData(TrackData.SongIndex);
            Achievement AchievementData = SystemFileIO.GetAchievementSave(index);

            GameObject Track = Instantiate(Manager.AchievementUI.ListPrefab, Vector3.zero, Quaternion.identity, QuickRecordSearchListParent);
            Track.transform.localScale = Vector3.one;
            
            A_FloorListTrack FLT = Track.GetComponent<A_FloorListTrack>();
            FLT.Index = ti.Key;
            FLT.SongIndex = TrackData.SongIndex;

            SystemFileIO.GetThumbnailSprite(FLT.Thumbnail, TrackData.SongIndex);
            FLT.Difficulty.sprite = SystemFileIO.GetAchievementDifficultySprite(TrackData.Diff);
            FLT.Button.sprite = SystemFileIO.GetAchievementButtonSprite(TrackData.Bt);
            
            FLT.Indicator.color  = Manager.AchievementUI.IndicatorColor[Mathf.Clamp((int)AchievementData.Status, (int)Achievement.State.None, (int)Achievement.State.Perfect)];
            FLT.IndicatorIcon.sprite = Manager.AchievementUI.IndicatorSprite[Mathf.Clamp((int)AchievementData.Status, (int)Achievement.State.None, (int)Achievement.State.Perfect)];
            FLT.Rate.text = AchievementData.Rate < 0.01 ? "-" : string.Format("{0:0.00}%", AchievementData.Rate);
            
            SystemFileIO.GetCategorySprite(FLT.Category, SongData.Ctgr);
            FLT.Title.text = SongData.Name;
            FLT.Composer.text = SongData.Cmps;
            
            BoardTrack BT = AchievementTracks.Where(t => t.Index == index).FirstOrDefault();
            FLT.OpenInfo.onClick.AddListener(() => {
                OpenAchievementInfo(index);
                CloseQuickRecordModal();
            });
        }
    }
    public void UpdateStatistics() {
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
        
        Manager.AchievementUI.Averages.text = ((ratecount != 0) ? string.Format("{0:0.00}%", average) : "0.00%");
        Manager.AchievementUI.PerfectPlayRatio.text = $"{perfect}/{count}";
        Manager.AchievementUI.MaxComboRatio.text = $"{maxcombo}/{count}";
        Manager.AchievementUI.ClearRatio.text = $"{clear}/{count}";
        Manager.AchievementUI.PerfectPlayBarChart.localScale = new Vector3((float)perfect/count, 1, 1);
        Manager.AchievementUI.MaxComboBarChart.localScale = new Vector3((float)maxcombo/count, 1, 1);
        Manager.AchievementUI.ClearBarChart.localScale = new Vector3((float)clear/count, 1, 1);
    }

    private List<BoardTrack> AchievementTracks = new List<BoardTrack>();
    private List<ushort> AchievementTrackIndexes = new List<ushort>();
    private delegate void FilterCallback(bool isFilter);
    private delegate void BoardEditCall(bool isEditing);
    private delegate void BoardReorderCall(byte? lv);
    private event FilterCallback FilterFloorEvent;
    private event FilterCallback FilterLevelEvent;
    private event BoardEditCall BoardEditEvent;
    private event BoardReorderCall BoardReorderEvent;
    private Coroutine LoadingAchievement;
    public void ClearBoard() {
        AchievementTracks.Clear();
        AchievementTrackIndexes.Clear();
        FilterLevelEvent = null;
        FilterFloorEvent = null;
        BoardEditEvent = null;
        BoardReorderEvent = null;
        foreach(Transform t in Manager.AchievementUI.ScrollViewport) Destroy(t.gameObject);
    }
    public void OpenAchievement() {
        if(LoadingAchievement != null)
            StopCoroutine(LoadingAchievement);
        LoadingAchievement = StartCoroutine(OpenAchievementRoutine());
    }
    public IEnumerator OpenAchievementRoutine() {
        int Count = 0;
        ushort InitInfoIndex = 0;
        ClearBoard();

        Resources.UnloadUnusedAssets();
        System.GC.Collect();

        Board TargetBoardData = CurrentBoard.Board;
        Board.ButtonData ButtonData = TargetBoardData.Buttons[(TargetBoardData.ButtonType == Board.Type.Seperated) ? ((int)Manager.BoardButton) : ((int)Board.Button.All)];
        foreach(var l in ButtonData.Lv)
            foreach(var f in l.Floor)
                AchievementTrackIndexes.AddRange(f.Tracks);
        UpdateStatistics();

        List<Board.ButtonData.LvData> LevelList = ButtonData.Lv;
        Manager.AchievementUI.ScrollViewport.gameObject.SetActive(false);
        Manager.AchievementUI.GenerationScreen.SetActive(true);
        Manager.AchievementUI.BoardCanvas.enabled = false;

        switch(Manager.BoardViewMode) {
            case Manager.ViewMode.List:
                for(int i = LevelList.Count-1; i >=0 ; i--) {

                    GameObject LevelDiv = Instantiate(Manager.AchievementUI.LevelDivPrefab, Vector3.zero, Quaternion.identity, Manager.AchievementUI.ScrollViewport);
                    LevelDiv.transform.localScale = Vector3.one;
                    A_LevelDivider LDUI = LevelDiv.GetComponent<A_LevelDivider>();

                    byte Lv = LevelList[i].Lv;
                    if(TargetBoardData.CategoryType == Board.Ctgr.Level) {
                        LDUI.Title.text = $"Lv {Lv}";

                        for(int j = 0; j < Lv; j++)
                            LDUI.LvToggleParent.Toggles[j].isOn = true;
                    }
                    else if(TargetBoardData.CategoryType == Board.Ctgr.Custom) {
                        LDUI.Title.gameObject.SetActive(false);
                        LDUI.LvToggleParent.gameObject.SetActive(false);
                    }
                    FilterLevelEvent += new FilterCallback(LDUI.FilterCheckEmpty);
                    BoardEditEvent += new BoardEditCall(LDUI.SetEditMode);
                    LDUI.DeleteLevelButton.onClick.AddListener(() => OpenDeleteLevelModal(Lv));
                    LDUI.NewFloorButton.onClick.AddListener(() => AddFloor(LDUI.FloorParent, Lv));

                    for(int j = (LevelList[i].Floor.Count - 1); j >= 0; j--) {
                        GameObject Floor = Instantiate(Manager.AchievementUI.FloorListPrefab, Vector3.zero, Quaternion.identity, LDUI.FloorParent);
                        Floor.transform.localScale = Vector3.one;

                        A_FloorList FL = Floor.GetComponent<A_FloorList>();
                        FL.Init(LevelList[i].Floor[j]);
                        FL.ReorderableList.Lv = Lv;
                        FL.ReorderableList.Floor = (byte)j;
                        FilterFloorEvent += new FilterCallback(FL.FilterCheckEmpty);
                        BoardEditEvent += new BoardEditCall(FL.SetEditMode);
                        BoardReorderEvent += new BoardReorderCall(FL.ReorderableList.DropableCheck);
                        FL.ReorderableList.List.OnElementGrabbed.AddListener(ReorderablePick);
                        FL.ReorderableList.List.OnElementAdded.AddListener(ReorderableDrop);

                        int floorIndex = j;
                        FL.NewTrackButton.onClick.AddListener(() => OpenAddTrackToFloorModal(FL.ListParent, Lv, floorIndex));
                        FL.DeleteFloorButton.onClick.AddListener(() => OpenDeleteFloorModal(LDUI.FloorParent, Lv, floorIndex));

                        FL.Title.text = (CurrentBoard.Board.CategoryType == Board.Ctgr.Level) ? $"Floor {j+1}" : LevelList[i].Floor[j].Name;
                        
                        foreach(ushort index in LevelList[i].Floor[j].Tracks) {
                            MainData.TrackInfo TrackData = SystemFileIO.GetTrackData(index);
                            MainData.SongInfo SongData = SystemFileIO.GetSongData(TrackData.SongIndex);
                            Achievement AchievementData = SystemFileIO.GetAchievementSave(index);

                            GameObject Track = Instantiate(Manager.AchievementUI.ListPrefab, Vector3.zero, Quaternion.identity, FL.ListParent);
                            Track.transform.localScale = Vector3.one;

                            A_FloorListTrack FLT = Track.GetComponent<A_FloorListTrack>();
                            FLT.Index = index;
                            FLT.SongIndex = TrackData.SongIndex;

                            SystemFileIO.GetThumbnailSprite(FLT.Thumbnail, TrackData.SongIndex);
                            FLT.Difficulty.sprite = SystemFileIO.GetAchievementDifficultySprite(TrackData.Diff);
                            FLT.Button.sprite = SystemFileIO.GetAchievementButtonSprite(TrackData.Bt);
                            
                            FLT.Indicator.color  = Manager.AchievementUI.IndicatorColor[Mathf.Clamp((int)AchievementData.Status, (int)Achievement.State.None, (int)Achievement.State.Perfect)];
                            FLT.IndicatorIcon.sprite = Manager.AchievementUI.IndicatorSprite[Mathf.Clamp((int)AchievementData.Status, (int)Achievement.State.None, (int)Achievement.State.Perfect)];
                            FLT.Rate.text = AchievementData.Rate < 0.01 ? "-" : string.Format("{0:0.00}%", AchievementData.Rate);
                            
                            SystemFileIO.GetCategorySprite(FLT.Category, SongData.Ctgr);
                            FLT.Title.text = SongData.Name;
                            FLT.Composer.text = SongData.Cmps;

                            AchievementTracks.Add(FLT);

                            FLT.OpenInfo.onClick.AddListener(() => {OpenAchievementInfo(index);});

                            Track.SetActive(CheckFilter(FLT));
                            if(TrackData.Lv != Lv && TargetBoardData.CategoryType == Board.Ctgr.Level)
                                FLT.LevelMismatchAlert.SetActive(true);
                            if(Count == 0)
                                InitInfoIndex = index;
                            Count++;
                            
                        }
                        
                        FL.FilterCheckEmpty(isFilter);
                        FL.SetEditMode(isEditing);
                        yield return null;
                    }

                    LDUI.FilterCheckEmpty(isFilter);
                    LDUI.SetEditMode(isEditing);
                }
                break;

            case Manager.ViewMode.Grid:
                for(int i = LevelList.Count-1; i >=0 ; i--) {
                    GameObject LevelDiv = Instantiate(Manager.AchievementUI.LevelDivPrefab, Vector3.zero, Quaternion.identity, Manager.AchievementUI.ScrollViewport);
                    LevelDiv.transform.localScale = Vector3.one;
                    A_LevelDivider LDUI = LevelDiv.GetComponent<A_LevelDivider>();
                    byte Lv = LevelList[i].Lv;
                    LDUI.Title.text = $"Lv {Lv}";
                    FilterLevelEvent += new FilterCallback(LDUI.FilterCheckEmpty);
                    BoardEditEvent += new BoardEditCall(LDUI.SetEditMode);
                    LDUI.DeleteLevelButton.onClick.AddListener(() => OpenDeleteLevelModal(Lv));
                    LDUI.NewFloorButton.onClick.AddListener(() => AddFloor(LDUI.FloorParent, Lv));

                    for(int j = 0; j < Lv; j++)
                        LDUI.LvToggleParent.Toggles[j].isOn = true;

                    for(int j = (LevelList[i].Floor.Count - 1); j >= 0; j--) {
                        GameObject Floor = Instantiate(Manager.AchievementUI.FloorGridPrefab, Vector3.zero, Quaternion.identity, LDUI.FloorParent);
                        Floor.transform.localScale = Vector3.one;
                        
                        A_FloorGrid FG = Floor.GetComponent<A_FloorGrid>();
                        FG.ReorderableList.Lv = Lv;
                        FG.ReorderableList.Floor = (byte)j;
                        FilterFloorEvent += new FilterCallback(FG.FilterCheckEmpty);
                        BoardEditEvent += new BoardEditCall(FG.SetEditMode);
                        BoardReorderEvent += new BoardReorderCall(FG.ReorderableList.DropableCheck);
                        FG.ReorderableList.List.OnElementGrabbed.AddListener(ReorderablePick);
                        FG.ReorderableList.List.OnElementAdded.AddListener(ReorderableDrop);

                        int floorIndex = j;
                        FG.NewTrackButton.onClick.AddListener(() => OpenAddTrackToFloorModal(FG.GridParent, Lv, floorIndex));
                        FG.DeleteFloorButton.onClick.AddListener(() => OpenDeleteFloorModal(LDUI.FloorParent, Lv, floorIndex));

                        FG.Title.text = (CurrentBoard.Board.CategoryType == Board.Ctgr.Level) ? $"Floor {j+1}" : LevelList[i].Floor[j].Name;

                        foreach(ushort index in LevelList[i].Floor[j].Tracks) {
                            MainData.TrackInfo TrackData = SystemFileIO.GetTrackData(index);
                            MainData.SongInfo SongData = SystemFileIO.GetSongData(TrackData.SongIndex);
                            Achievement AchievementData = SystemFileIO.GetAchievementSave(index);

                            GameObject Track = Instantiate(Manager.AchievementUI.GridPrefab, Vector3.zero, Quaternion.identity, FG.GridParent);
                            Track.transform.localScale = Vector3.one;

                            A_FloorGridTrack FGT = Track.GetComponent<A_FloorGridTrack>();
                            FGT.Index = index;
                            FGT.SongIndex = TrackData.SongIndex;

                            SystemFileIO.GetThumbnailSprite(FGT.Thumbnail, TrackData.SongIndex);
                            FGT.Difficulty.sprite = SystemFileIO.GetAchievementDifficultySprite(TrackData.Diff);
                            FGT.Button.sprite = SystemFileIO.GetAchievementButtonSprite(TrackData.Bt);

                            FGT.Indicator.color  = Manager.AchievementUI.IndicatorColor[Mathf.Clamp((int)AchievementData.Status, (int)Achievement.State.None, (int)Achievement.State.Perfect)];
                            FGT.IndicatorIcon.sprite = Manager.AchievementUI.IndicatorSprite[Mathf.Clamp((int)AchievementData.Status, (int)Achievement.State.None, (int)Achievement.State.Perfect)];
                            FGT.Rate.text = AchievementData.Rate < 0.01 ? "-" : string.Format("{0:0.00}%", AchievementData.Rate);

                            AchievementTracks.Add(FGT);
                            
                            FGT.OpenInfo.onClick.AddListener(() => {OpenAchievementInfo(index);});

                            Track.SetActive(CheckFilter(FGT));
                            if(TrackData.Lv != Lv && TargetBoardData.CategoryType == Board.Ctgr.Level)
                                FGT.LevelMismatchAlert.SetActive(true);
                            if(Count == 0)
                                InitInfoIndex = index;
                            Count++;
                            
                        }
                        
                        FG.FilterCheckEmpty(isFilter);
                        FG.SetEditMode(isEditing);
                        yield return null;
                    }

                    LDUI.FilterCheckEmpty(isFilter);
                    LDUI.SetEditMode(isEditing);
                }
                break;
        }

        Manager.AchievementUI.ScrollViewport.gameObject.SetActive(true);
        OpenAchievementInfo(InitInfoIndex);

        BoardCriteriaPanel.SetActive(TargetBoardData.Criteria != null);
        BoardCriteriaPP.gameObject.SetActive(TargetBoardData.Criteria != null && TargetBoardData.Criteria.Crit == Board.CriteriaData.CritType.Perfect);
        BoardCriteriaMC.gameObject.SetActive(TargetBoardData.Criteria != null && TargetBoardData.Criteria.Crit == Board.CriteriaData.CritType.MaxCombo);
        BoardCriteriaRate.gameObject.SetActive(TargetBoardData.Criteria?.Rate != null);

        if(TargetBoardData.Criteria != null) {
            BoardCriteriaPP.isOn = TargetBoardData.Criteria.Crit == Board.CriteriaData.CritType.Perfect;
            BoardCriteriaMC.isOn = TargetBoardData.Criteria.Crit == Board.CriteriaData.CritType.MaxCombo;

            if(TargetBoardData.Criteria.Rate.HasValue)
                BoardCriteriaRate.text = string.Format("{0:0.00}", TargetBoardData.Criteria.Rate.Value);
        }
        
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Manager.AchievementUI.ScrollViewport);
        foreach(RectTransform RT in (RectTransform)Manager.AchievementUI.ScrollViewport)
            LayoutRebuilder.ForceRebuildLayoutImmediate(RT);
        yield return null;
        yield return null;
        yield return null;
        Manager.AchievementUI.GenerationScreen.SetActive(false);
        Manager.AchievementUI.BoardCanvas.enabled = true;
        Canvas.ForceUpdateCanvases();
    }

    private Tween AchievementButtonSpriteSwap;
    public void AchievementButtonToggle() {
        if(CurrentBoard.Board.ButtonType == Board.Type.Combined) return;

        switch(Manager.BoardButton) {
            case Board.Button._4B:   Manager.BoardButton = Board.Button._5B;   break;
            case Board.Button._5B:   Manager.BoardButton = Board.Button._6B;    break;
            case Board.Button._6B:   Manager.BoardButton = Board.Button._8B;  break;
            case Board.Button._8B:   Manager.BoardButton = Board.Button._4B;   break;
        }
        Manager.OpenAchievement();
    }
    private void SetAchievementButton(Board.Button button) {
        AchievementButtonSpriteSwap?.Kill();
        Manager.BoardButton = button;
        Manager.AchievementUI.ButtonGradientAnimation.Animation();
        Manager.AchievementUI.StarShineAnimator.SetTrigger("Shine");
        AchievementButtonSpriteSwap = DOVirtual.DelayedCall(0.5f, SetAchievementButtonDelay);
    }
    private void SetAchievementButtonDelay() {
        Manager.AchievementUI.ButtonOptText.sprite = Manager.AchievementUI.ButtonTextSprites[(int)Manager.BoardButton];
        Manager.AchievementUI.ButtonOptBG.sprite   = Manager.AchievementUI.ButtonBackgroundSprites[(int)Manager.BoardButton];
    }
    
    public void SetViewTypeList() {
        if(Manager.BoardViewMode != Manager.ViewMode.List) {
            Manager.BoardViewMode = Manager.ViewMode.List;
            Manager.OpenAchievement();
        }
    }
    public void SetViewTypeGrid() {
        if(Manager.BoardViewMode != Manager.ViewMode.Grid) {
            Manager.BoardViewMode = Manager.ViewMode.Grid;
            Manager.OpenAchievement();
        }
    }
    
    private ushort AchievementIndex;
    private List<Image> AchievementIndicators = new List<Image>();
    private List<Image> AchievementIndicatorIcons = new List<Image>();
    private List<TextMeshProUGUI> AchievementRates = new List<TextMeshProUGUI>();
    public void OpenAchievementInfo(ushort index) {
        AchievementIndex = index;
        Achievement AchievementData = SystemFileIO.GetAchievementSave(index);
        MainData.TrackInfo TrackData = SystemFileIO.GetTrackData(index);
        MainData.SongInfo SongData = SystemFileIO.GetSongData(TrackData.SongIndex);
        
        AchievementIndicators.Clear();
        AchievementIndicatorIcons.Clear();
        AchievementRates.Clear();
        
        AchievementTracks.Where(t => t.Index == index).ToList().ForEach(t => {
            AchievementIndicators.Add(t.Indicator);
            AchievementIndicatorIcons.Add(t.IndicatorIcon);
            AchievementRates.Add(t.Rate);
        });

        switch(TrackData.Bt) {
            case "4B": SetAchievementButton(Board.Button._4B); break;
            case "5B": SetAchievementButton(Board.Button._5B); break;
            case "6B": SetAchievementButton(Board.Button._6B); break;
            case "8B": SetAchievementButton(Board.Button._8B); break;
        }

        Manager.AchievementUI.Title.text = SongData.Name;
        Manager.AchievementUI.Composer.text = SongData.Cmps;
        SystemFileIO.GetCategorySprite(Manager.AchievementUI.Category, SongData.Ctgr);
        SystemFileIO.GetPreviewSprite(Manager.AchievementUI.Preview, TrackData.SongIndex);
        Manager.BGPrev.sprite = Manager.BG.sprite;
        SystemFileIO.GetLoadingSprite(Manager.BG, TrackData.SongIndex);
        Manager.AchievementUI.RateField.text = string.Format("{0:0.00}", AchievementData.Rate);
        Manager.AchievementUI.BreakField.text = AchievementData.Break.ToString();

        PrevRate = AchievementData.Rate;
        PrevBreak = AchievementData.Break;
        
        switch(TrackData.Diff) {
            case "NM": 
                Manager.AchievementUI.Difficulty[0].Animate(true);
                Manager.AchievementUI.Difficulty[1].Animate(false);
                Manager.AchievementUI.Difficulty[2].Animate(false);
                Manager.AchievementUI.Difficulty[3].Animate(false);
                break;
            case "HD":
                Manager.AchievementUI.Difficulty[0].Animate(false);
                Manager.AchievementUI.Difficulty[1].Animate(true);
                Manager.AchievementUI.Difficulty[2].Animate(false);
                Manager.AchievementUI.Difficulty[3].Animate(false);
                break;
            case "MX":
                Manager.AchievementUI.Difficulty[0].Animate(false);
                Manager.AchievementUI.Difficulty[1].Animate(false);
                Manager.AchievementUI.Difficulty[2].Animate(true);
                Manager.AchievementUI.Difficulty[3].Animate(false);
                break;
            case "SC":
                Manager.AchievementUI.Difficulty[0].Animate(false);
                Manager.AchievementUI.Difficulty[1].Animate(false);
                Manager.AchievementUI.Difficulty[2].Animate(false);
                Manager.AchievementUI.Difficulty[3].Animate(true);
                break;
        }

        int state = (int)AchievementData.Status <= (int)Achievement.State.Clear ? -1 : (int)AchievementData.Status - (int)Achievement.State.MaxCombo;
        Manager.ApplySelectionToToggleGroup(Manager.AchievementUI.StateToggleGroup, state);

        if(TrackData.Diff == "SC") {
            Manager.AchievementUI.LvToggleParent.gameObject.SetActive(false);
            Manager.AchievementUI.LvToggleSCParent.gameObject.SetActive(true);

            for(int i = 0; i < 15; i++) Manager.AchievementUI.LvToggleSCParent.Toggles[i].isOn = false;
            for(int i = 0; i < TrackData.Lv; i++) Manager.AchievementUI.LvToggleSCParent.Toggles[i].isOn = true;
        }
        else {
            Manager.AchievementUI.LvToggleParent.gameObject.SetActive(true);
            Manager.AchievementUI.LvToggleSCParent.gameObject.SetActive(false);
            
            for(int i = 0; i < 15; i++) Manager.AchievementUI.LvToggleParent.Toggles[i].isOn = false;
            for(int i = 0; i < TrackData.Lv; i++) Manager.AchievementUI.LvToggleParent.Toggles[i].isOn = true;
        }
    }
    private float PrevRate;
    private ushort PrevBreak;
    public void AchievementRateUpdate(string input) {
        if(float.TryParse(input, out float rate)) {
            Achievement achievement = SystemFileIO.GetAchievementSave(AchievementIndex);
            Manager.AchievementUI.RateField.text = string.Format("{0:0.00}", rate);
            PrevRate = rate;

            SetAchievementListRate(rate);
            SystemFileIO.SaveAchievementRate(AchievementIndex, rate);

            if(rate <= 0.01f) {
                SystemFileIO.SaveAchievementState(AchievementIndex, Achievement.State.None);
                SystemFileIO.SaveAchievementBreak(AchievementIndex, 0);
                
                SetToggle(Achievement.State.None);
                SetIndicator(Achievement.State.None);
            }
            else if(rate >= 100f) {
                SystemFileIO.SaveAchievementState(AchievementIndex, Achievement.State.Perfect);
                Manager.ApplySelectionToToggleGroup(Manager.AchievementUI.StateToggleGroup, 1);

                SetToggle(Achievement.State.Perfect);
                SetIndicator(Achievement.State.Perfect);
            }
            else if(achievement.Status == Achievement.State.Perfect) {
                SystemFileIO.SaveAchievementState(AchievementIndex, Achievement.State.MaxCombo);
                Manager.ApplySelectionToToggleGroup(Manager.AchievementUI.StateToggleGroup, 0);

                SetToggle(Achievement.State.MaxCombo);
                SetIndicator(Achievement.State.MaxCombo);
            }
            else if(achievement.Status != Achievement.State.MaxCombo) {
                SystemFileIO.SaveAchievementState(AchievementIndex, Achievement.State.Clear);
                Manager.ApplySelectionToToggleGroup(Manager.AchievementUI.StateToggleGroup, -1);
                
                PrevBreak = (ushort)Mathf.Max(1, PrevBreak);
                SystemFileIO.SaveAchievementBreak(AchievementIndex, PrevBreak);

                Manager.AchievementUI.BreakField.text = PrevBreak.ToString();
                SetToggle(Achievement.State.Clear);
                SetIndicator(Achievement.State.Clear);
            }
        }
        else
            Manager.AchievementUI.RateField.text = string.Format("{0:0.00}", PrevRate); // Do not update
        
        UpdateStatistics();
    }
    public void AchievementBreakUpdate(string input) {
        if(ushort.TryParse(input, out ushort breaks)) {
            Achievement achievement = SystemFileIO.GetAchievementSave(AchievementIndex);
            Manager.AchievementUI.BreakField.text = breaks.ToString();
            PrevBreak = breaks;

            if(breaks == 0) {
                SystemFileIO.SaveAchievementState(AchievementIndex, Achievement.State.MaxCombo);
                
                SetToggle(Achievement.State.MaxCombo);
                SetIndicator(Achievement.State.MaxCombo);
            }
            else if(PrevRate >= 0.01f) {
                SystemFileIO.SaveAchievementState(AchievementIndex, Achievement.State.Clear);

                SetToggle(Achievement.State.Clear);
                SetIndicator(Achievement.State.Clear);
            }
            else {
                SystemFileIO.SaveAchievementState(AchievementIndex, Achievement.State.None);

                SetToggle(Achievement.State.None);
                SetIndicator(Achievement.State.None);
            }

            SystemFileIO.SaveAchievementBreak(AchievementIndex, breaks);
        }
        else
            Manager.AchievementUI.BreakField.text = PrevBreak.ToString();
        
        UpdateStatistics();
    }
    public void AchievementStateToggleUpdate() {
        Achievement.State state = (Achievement.State)(Manager.GetSelectedToggleIndex(Manager.AchievementUI.StateToggleGroup) + 2 ?? 0);

        if(state == Achievement.State.Perfect) {
            PrevRate = 100f;
            PrevBreak = 0;
            
            SetAchievementListRate(PrevRate);
        }
        else {
            PrevRate = Mathf.Clamp(PrevRate, 0f, 99.99f);
            if(state == Achievement.State.MaxCombo)
                PrevBreak = 0;
                        
            if(state == Achievement.State.None) {
                if(PrevRate <= 0.01f)
                    state = Achievement.State.None;
                else
                    state = Achievement.State.Clear;
            }
            
            SetAchievementListRate(PrevRate);
        }

        SystemFileIO.SaveAchievementRate(AchievementIndex, PrevRate);
        SystemFileIO.SaveAchievementBreak(AchievementIndex, PrevBreak);
        SystemFileIO.SaveAchievementState(AchievementIndex, state);
        Manager.AchievementUI.RateField.text = string.Format("{0:0.00}", PrevRate);
        Manager.AchievementUI.BreakField.text = PrevBreak.ToString();
        SetIndicator(state);
        
        UpdateStatistics();
    }
    private void SetToggle(Achievement.State state) {
        Manager.ApplySelectionToToggleGroup(Manager.AchievementUI.StateToggleGroup, (int)state - 2);
    }
    private void SetIndicator(Achievement.State state) {
        foreach(var t in AchievementIndicators)
            if(t != null) t.color = Manager.AchievementUI.IndicatorColor[(int)state];            

        foreach(var t in AchievementIndicatorIcons)
            if(t != null) t.sprite = Manager.AchievementUI.IndicatorSprite[(int)state];
    }
    private void SetAchievementListRate(float rate) {
        foreach(var t in AchievementRates)
            if(t != null)
                t.text = rate <= 0.01f ? "-" : string.Format("{0:0.00}%", rate);
    }
}
