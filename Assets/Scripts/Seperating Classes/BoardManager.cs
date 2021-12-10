using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public partial class BoardManager : MonoBehaviour
{
    [SerializeField] private GameObject DuplicateNameModal;

    public void Start() {
        CurrentBoard = SystemFileIO.Boards[0].Board;
    }
    public void ShowBoard(int index) {

    }
    public void DuplicateBoard(int index) {

    }
    public void DeleteBoard(int index) {

    }

    private Board CurrentBoard;
    public void ChangeBoard(int i) {
        
    }
    public void ExportBoard() {

    }
    public void ImportBoard() {

    }

    public IEnumerator OpenAchievementRoutine() {
        int Count = 0;

        Board TargetBoardData = SystemFileIO.Boards[0].Board;
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

                    for(int j = 0; j < Lv; j++)
                        LDUI.LvToggleParent.Toggles[j].isOn = true;

                    for(int j = LevelList[i].Floor.Count-1; j >= 0; j--) {
                        GameObject Floor = Instantiate(Manager.AchievementUI.FloorListPrefab, Vector3.zero, Quaternion.identity, LDUI.FloorParent);
                        Floor.transform.localScale = Vector3.one;

                        A_FloorList FL = Floor.GetComponent<A_FloorList>();
                        FL.Title.text = $"Floor {j+1}";
                        
                        foreach(ushort index in LevelList[i].Floor[j].Tracks) {
                            TrackAdvanced TrackData = SystemFileIO.GetTrackAdvancedData(index);
                            Achievement AchievementData = SystemFileIO.GetAchievementSave(index);

                            GameObject Track = Instantiate(Manager.AchievementUI.ListPrefab, Vector3.zero, Quaternion.identity, FL.ListParent);
                            Track.transform.localScale = Vector3.one;
                            A_FloorListTrack FLT = Track.GetComponent<A_FloorListTrack>();

                            FLT.Thumbnail.sprite = SystemFileIO.GetThumbnailSprite(TrackData.Name);
                            FLT.Difficulty.sprite = SystemFileIO.GetAchievementDifficultySprite(TrackData.Diff);
                            FLT.Indicator.color  = Manager.AchievementUI.IndicatorColor[(int)AchievementData.Status +1];
                            FLT.Category.sprite = SystemFileIO.GetCategorySprite(TrackData.Ctgr);
                            FLT.Title.text = TrackData.Name;
                            FLT.Composer.text = TrackData.Cmps;

                            Track.GetComponent<Button>().onClick.AddListener(() => {OpenAchievementInfo(index, FLT.Indicator);});

                            if(Count == 0)
                                OpenAchievementInfo(index, FLT.Indicator);
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

                    for(int j = 0; j < Lv; j++)
                        LDUI.LvToggleParent.Toggles[j].isOn = true;

                    for(int j = LevelList[i].Floor.Count-1; j >= 0; j--) {
                        if(LevelList[i].Floor[j].Tracks.Count == 0)
                            continue;
                        GameObject Floor = Instantiate(Manager.AchievementUI.FloorGridPrefab, Vector3.zero, Quaternion.identity, LDUI.FloorParent);
                        Floor.transform.localScale = Vector3.one;

                        A_FloorGrid FG = Floor.GetComponent<A_FloorGrid>();
                        FG.Title.text = $"Floor {j+1}";

                        foreach(ushort index in LevelList[i].Floor[j].Tracks) {
                            TrackAdvanced TrackData = SystemFileIO.GetTrackAdvancedData(index);
                            Achievement AchievementData = SystemFileIO.GetAchievementSave(index);

                            GameObject Track = Instantiate(Manager.AchievementUI.GridPrefab, Vector3.zero, Quaternion.identity, FG.GridParent);
                            Track.transform.localScale = Vector3.one;

                            A_FloorGridTrack FGT = Track.GetComponent<A_FloorGridTrack>();

                            FGT.Thumbnail.sprite = SystemFileIO.GetThumbnailSprite(TrackData.Name);
                            FGT.Difficulty.sprite = SystemFileIO.GetAchievementDifficultySprite(TrackData.Diff);
                            FGT.Indicator.color  = Manager.AchievementUI.IndicatorColor[(int)AchievementData.Status +1];

                            Track.GetComponent<Button>().onClick.AddListener(() => {OpenAchievementInfo(index, FGT.Indicator);});

                            if(Count == 0)
                                OpenAchievementInfo(index, FGT.Indicator);
                            Count++;
                            
                        }
                        yield return null;
                    }
                }
                break;
        }

        // LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Manager.AchievementUI.ScrollViewport);
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
    public void OpenAchievementInfo(ushort index, Image indicator) {
        TrackAdvanced TrackData = SystemFileIO.GetTrackAdvancedData(index);
        
        AchievementIndex = index;
        AchievementIndicator = indicator;

        Manager.AchievementUI.Title.text = TrackData.Name;
        Manager.AchievementUI.Composer.text = TrackData.Cmps;
        Manager.AchievementUI.Category.sprite = SystemFileIO.GetCategorySprite(TrackData.Ctgr);
        Manager.AchievementUI.Preview.sprite = SystemFileIO.GetPreviewSprite(TrackData.Name);

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

        int state = (int?)SystemFileIO.AchievementData.achievements.FirstOrDefault((x) => (x.Index == index))?.Status ?? -1;
        Manager.ApplySelectionToToggleGroup(Manager.AchievementUI.StateToggleGroup, state);
    }
    public void AchievementStateToggleUpdate() {
        int index = Manager.GetSelectedToggleIndex(Manager.AchievementUI.StateToggleGroup) ?? -1;

        SystemFileIO.SaveAchievement(AchievementIndex, (Achievement.State)index);
        AchievementIndicator.color = Manager.AchievementUI.IndicatorColor[index + 1];
    }

}
