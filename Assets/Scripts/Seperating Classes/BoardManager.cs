using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Mirix.DMRV;

public partial class BoardManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown BoardDropdown;
    [SerializeField] private TextMeshProUGUI DropdownLabel;
    [Space]
    [SerializeField] private GameObject ModalBackdrop;
    [SerializeField] private GameObject ModalDuplicateName;
    [SerializeField] private GameObject ModalNewBoard;
    [SerializeField] private GameObject ModalDeletionConfirm;

    public void UpdateBoardDropdown() {
        BoardDropdown.options.Clear();
        DropdownLabel.text = CurrentBoard.Name;

        foreach(var b in SystemFileIO.Boards)
            BoardDropdown.options.Add(new TMP_Dropdown.OptionData() { text = b.Name } );
    }

    public void ShowBoard(int index) {

    }
    public void DuplicateBoard(int index) {

    }
    public void DeleteBoard(int index) {

    }

    private BoardInfo CurrentBoard;
    public void ChangeBoard(int i) {
        CurrentBoard = SystemFileIO.Boards[i];
    }
    public void ExportBoard() {

    }
    public void ImportBoard() {

    }

    private List<BoardTrack> AchievementTracks = new List<BoardTrack>();
    private delegate void FilterCallback();
    private event FilterCallback FilterFloorEvent;
    private event FilterCallback FilterLevelEvent;
    public IEnumerator OpenAchievementRoutine() {
        int Count = 0;
        AchievementTracks.Clear();
        FilterLevelEvent = null;
        FilterFloorEvent = null;

        Board TargetBoardData = CurrentBoard.Board;
        Board.ButtonData ButtonData = TargetBoardData.Buttons[((int)Manager.BoardButton)];
        List<Board.ButtonData.LvData> LevelList = ButtonData.Lv;

        foreach(Transform t in Manager.AchievementUI.ScrollViewport) Destroy(t.gameObject);
        switch(Manager.BoardViewMode) {
            case Manager.ViewMode.List:
                for(int i = LevelList.Count-1; i >=0 ; i--) {

                    GameObject LevelDiv = Instantiate(Manager.AchievementUI.LevelDivPrefab, Vector3.zero, Quaternion.identity, Manager.AchievementUI.ScrollViewport);
                    LevelDiv.transform.localScale = Vector3.one;

                    A_LevelDivider LDUI = LevelDiv.GetComponent<A_LevelDivider>();
                    int Lv = LevelList[i].Lv;
                    LDUI.Title.text = $"Lv {Lv}";
                    FilterLevelEvent += new FilterCallback(LDUI.FilterCheckEmpty);

                    for(int j = 0; j < Lv; j++)
                        LDUI.LvToggleParent.Toggles[j].isOn = true;

                    for(int j = LevelList[i].Floor.Count-1; j >= 0; j--) {
                        GameObject Floor = Instantiate(Manager.AchievementUI.FloorListPrefab, Vector3.zero, Quaternion.identity, LDUI.FloorParent);
                        Floor.transform.localScale = Vector3.one;

                        A_FloorList FL = Floor.GetComponent<A_FloorList>();
                        FilterFloorEvent += new FilterCallback(FL.FilterCheckEmpty);
                        FL.Title.text = $"Floor {j+1}";
                        
                        foreach(ushort index in LevelList[i].Floor[j].Tracks) {
                            MainData.TrackInfo TrackData = SystemFileIO.GetTrackData(index);
                            MainData.SongInfo SongData = SystemFileIO.GetSongData(TrackData.SongIndex);
                            Achievement AchievementData = SystemFileIO.GetAchievementSave(index);

                            GameObject Track = Instantiate(Manager.AchievementUI.ListPrefab, Vector3.zero, Quaternion.identity, FL.ListParent);
                            Track.transform.localScale = Vector3.one;
                            
                            A_FloorListTrack FLT = Track.GetComponent<A_FloorListTrack>();
                            FLT.SongIndex = TrackData.SongIndex;
                            FLT.Ctgr = SongData.Ctgr;
                            FLT.Diff = TrackData.Diff;

                            SystemFileIO.GetThumbnailSprite(FLT.Thumbnail, TrackData.SongIndex);
                            FLT.Difficulty.sprite = SystemFileIO.GetAchievementDifficultySprite(TrackData.Diff);
                            FLT.Button.sprite = SystemFileIO.GetAchievementButtonSprite(TrackData.Bt);
                            
                            FLT.Indicator.color  = Manager.AchievementUI.IndicatorColor[Mathf.Clamp((int)AchievementData.Status, (int)Achievement.State.None, (int)Achievement.State.Perfect)];
                            FLT.Rate.text = AchievementData.Rate < 0.01 ? "-" : string.Format("{0:0.00}%", AchievementData.Rate);
                            
                            SystemFileIO.GetCategorySprite(FLT.Category, SongData.Ctgr);
                            FLT.Title.text = SongData.Name;
                            FLT.Composer.text = SongData.Cmps;

                            AchievementTracks.Add(FLT);

                            FLT.OpenInfo.onClick.AddListener(() => {OpenAchievementInfo(index, FLT.Indicator, FLT.Rate);});

                            if(TrackData.Lv != Lv)
                                FLT.LevelMismatchAlert.SetActive(true);

                            if(Count == 0)
                                OpenAchievementInfo(index, FLT.Indicator, FLT.Rate);
                            Count++;
                            
                        }
                        yield return null;
                    }
                }
                break;

            case Manager.ViewMode.Grid:
                for(int i = LevelList.Count-1; i >=0 ; i--) {
                    if(LevelList[i].Floor.Sum((x) => (x.Tracks.Count)) == 0)
                        continue;

                    GameObject LevelDiv = Instantiate(Manager.AchievementUI.LevelDivPrefab, Vector3.zero, Quaternion.identity, Manager.AchievementUI.ScrollViewport);
                    LevelDiv.transform.localScale = Vector3.one;

                    A_LevelDivider LDUI = LevelDiv.GetComponent<A_LevelDivider>();
                    int Lv = LevelList[i].Lv;
                    LDUI.Title.text = $"Lv {Lv}";
                    FilterLevelEvent += new FilterCallback(LDUI.FilterCheckEmpty);

                    for(int j = 0; j < Lv; j++)
                        LDUI.LvToggleParent.Toggles[j].isOn = true;

                    for(int j = LevelList[i].Floor.Count-1; j >= 0; j--) {
                        if(LevelList[i].Floor[j].Tracks.Count == 0)
                            continue;
                        GameObject Floor = Instantiate(Manager.AchievementUI.FloorGridPrefab, Vector3.zero, Quaternion.identity, LDUI.FloorParent);
                        Floor.transform.localScale = Vector3.one;

                        A_FloorGrid FG = Floor.GetComponent<A_FloorGrid>();
                        FilterFloorEvent += new FilterCallback(FG.FilterCheckEmpty);
                        FG.Title.text = $"Floor {j+1}";

                        foreach(ushort index in LevelList[i].Floor[j].Tracks) {
                            MainData.TrackInfo TrackData = SystemFileIO.GetTrackData(index);
                            MainData.SongInfo SongData = SystemFileIO.GetSongData(TrackData.SongIndex);
                            Achievement AchievementData = SystemFileIO.GetAchievementSave(index);

                            GameObject Track = Instantiate(Manager.AchievementUI.GridPrefab, Vector3.zero, Quaternion.identity, FG.GridParent);
                            Track.transform.localScale = Vector3.one;

                            A_FloorGridTrack FGT = Track.GetComponent<A_FloorGridTrack>();
                            FGT.SongIndex = TrackData.SongIndex;
                            FGT.Ctgr = SongData.Ctgr;
                            FGT.Diff = TrackData.Diff;

                            SystemFileIO.GetThumbnailSprite(FGT.Thumbnail, TrackData.SongIndex);
                            FGT.Difficulty.sprite = SystemFileIO.GetAchievementDifficultySprite(TrackData.Diff);
                            FGT.Button.sprite = SystemFileIO.GetAchievementButtonSprite(TrackData.Bt);

                            FGT.Indicator.color  = Manager.AchievementUI.IndicatorColor[Mathf.Clamp((int)AchievementData.Status, (int)Achievement.State.None, (int)Achievement.State.Perfect)];
                            FGT.Rate.text = AchievementData.Rate < 0.01 ? "-" : string.Format("{0:0.00}%", AchievementData.Rate);

                            AchievementTracks.Add(FGT);

                            FGT.OpenInfo.onClick.AddListener(() => {OpenAchievementInfo(index, FGT.Indicator, FGT.Rate);});

                            if(TrackData.Lv != Lv)
                                FGT.LevelMismatchAlert.SetActive(true);

                            if(Count == 0)
                                OpenAchievementInfo(index, FGT.Indicator, FGT.Rate);
                            Count++;
                            
                        }
                        yield return null;
                    }
                }
                break;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Manager.AchievementUI.ScrollViewport);
    }

    private Tween AchievementButtonSpriteSwap;
    public void AchievementButtonToggle() {
        AchievementButtonSpriteSwap?.Kill();
        AchievementButtonSpriteSwap = DOVirtual.DelayedCall(0.5f, AchievementButtonSpriteSwapDelay);
        Manager.AchievementUI.StarShineAnimator.SetTrigger("Shine");
    }
    private void AchievementButtonSpriteSwapDelay() {
        switch(Manager.BoardButton) {
            case Board.Button._4B:   Manager.BoardButton = Board.Button._5B;   break;
            case Board.Button._5B:   Manager.BoardButton = Board.Button._6B;    break;
            case Board.Button._6B:   Manager.BoardButton = Board.Button._8B;  break;
            case Board.Button._8B:   Manager.BoardButton = Board.Button._4B;   break;
        }
        Manager.AchievementUI.ButtonOptText.sprite = Manager.AchievementUI.ButtonTextSprites[(int)Manager.BoardButton];
        Manager.AchievementUI.ButtonOptBG.sprite   = Manager.AchievementUI.ButtonBackgroundSprites[(int)Manager.BoardButton];

        Manager.OpenAchievement();
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
    private Image AchievementIndicator;
    private TextMeshProUGUI AchievementRate;
    public void OpenAchievementInfo(ushort index, Image indicator, TextMeshProUGUI rate) {
        Achievement AchievementData = SystemFileIO.GetAchievementSave(index);
        MainData.TrackInfo TrackData = SystemFileIO.GetTrackData(index);
        MainData.SongInfo SongData = SystemFileIO.GetSongData(TrackData.SongIndex);
        
        AchievementIndex = index;
        AchievementIndicator = indicator;
        AchievementRate = rate;

        Manager.AchievementUI.Title.text = SongData.Name;
        Manager.AchievementUI.Composer.text = SongData.Cmps;
        SystemFileIO.GetCategorySprite(Manager.AchievementUI.Category, SongData.Ctgr);
        SystemFileIO.GetPreviewSprite(Manager.AchievementUI.Preview, TrackData.SongIndex);
        SystemFileIO.GetLoadingSprite(Manager.BG, TrackData.SongIndex);
        Manager.BGPrev.sprite = Manager.BG.sprite;
        Manager.AchievementUI.RateField.text = string.Format("{0:0.00}", AchievementData.Rate);
        PrevRate = AchievementData.Rate;
        
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
    public void AchievementRateUpdate(string input) {
        if(float.TryParse(input, out float rate)) {
            Achievement achievement = SystemFileIO.GetAchievementSave(AchievementIndex);
            Manager.AchievementUI.RateField.text = string.Format("{0:0.00}", rate);
            PrevRate = rate;

            SetAchievementListRate(rate);
            SystemFileIO.SaveAchievementRate(AchievementIndex, rate);

            if(rate <= 0.01f) {
                SystemFileIO.SaveAchievementState(AchievementIndex, Achievement.State.None);
                Manager.ApplySelectionToToggleGroup(Manager.AchievementUI.StateToggleGroup, -1);
                AchievementIndicator.color = Manager.AchievementUI.IndicatorColor[(int)Achievement.State.None];
            }
            else if(rate >= 100f) {
                SystemFileIO.SaveAchievementState(AchievementIndex, Achievement.State.Perfect);
                Manager.ApplySelectionToToggleGroup(Manager.AchievementUI.StateToggleGroup, (int)Achievement.State.Perfect - (int)Achievement.State.MaxCombo);
                AchievementIndicator.color = Manager.AchievementUI.IndicatorColor[(int)Achievement.State.Perfect];
            }
            else if(achievement.Status == Achievement.State.Perfect) {
                SystemFileIO.SaveAchievementState(AchievementIndex, Achievement.State.MaxCombo);
                Manager.ApplySelectionToToggleGroup(Manager.AchievementUI.StateToggleGroup, (int)Achievement.State.MaxCombo - (int)Achievement.State.MaxCombo);
                AchievementIndicator.color = Manager.AchievementUI.IndicatorColor[(int)Achievement.State.MaxCombo];
            }
            else {
                SystemFileIO.SaveAchievementState(AchievementIndex, Achievement.State.Clear);
                Manager.ApplySelectionToToggleGroup(Manager.AchievementUI.StateToggleGroup, -1);
                AchievementIndicator.color = Manager.AchievementUI.IndicatorColor[(int)Achievement.State.Clear];
            }
        }
        else
            Manager.AchievementUI.RateField.text = string.Format("{0:0.00}", PrevRate); // Do not update
    }
    public void AchievementStateToggleUpdate() {
        int index = Manager.GetSelectedToggleIndex(Manager.AchievementUI.StateToggleGroup) + 2 ?? 0;

        if(index == (int)Achievement.State.Perfect) {
            PrevRate = 100f;
            Manager.AchievementUI.RateField.text = string.Format("{0:0.00}", PrevRate);
            SetAchievementListRate(PrevRate);
            SystemFileIO.SaveAchievementRate(AchievementIndex, PrevRate);
        }
        else if(index == 0) {
            if(PrevRate <= 0.01f)
                index = (int)Achievement.State.None;
            else
                index = (int)Achievement.State.Clear;
        }
        else {
            PrevRate = Mathf.Clamp(PrevRate, 0f, 99.99f);
            Manager.AchievementUI.RateField.text = string.Format("{0:0.00}", PrevRate);
            SetAchievementListRate(PrevRate);
            SystemFileIO.SaveAchievementRate(AchievementIndex, PrevRate);
        }

        SystemFileIO.SaveAchievementState(AchievementIndex, (Achievement.State)index);
        AchievementIndicator.color = Manager.AchievementUI.IndicatorColor[index];
    }
    private void SetAchievementListRate(float rate) {
        AchievementRate.text = rate <= 0.01f ? "-" : string.Format("{0:0.00}%", rate);
    }
}
