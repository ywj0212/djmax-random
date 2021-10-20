using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

[System.Serializable] public class TrackAdvancedData {
    public List<TrackAdvanced> tracks;
}
[System.Serializable] public class TrackAdvanced {
    public int Index;
    public string Ctgr;
    public string Name;
    public string Cmps;
    public string Bt;
    public string Diff;
    public int Lv;
}
[System.Serializable] public class TrackSimpleData {
    public List<TrackSimple> tracks;
}
[System.Serializable] public class TrackSimple {
    public string Ctgr;
    public string Name;
    public string Cmps;
}
[System.Serializable] public class AchievementList {
    public AchievementButton _4B;
    public AchievementButton _5B;
    public AchievementButton _6B;
    public AchievementButton _8B;
}
[System.Serializable] public class AchievementButton {
    public AchievementLv[] Lv;
}
[System.Serializable] public class AchievementLv {
    public AchievementFineLv[] FineLv;
}
[System.Serializable] public class AchievementFineLv {
    public List<int> Index;
}
[System.Serializable] public class AchievementData {
    public List<Achievement> achievements = new List<Achievement>();
}
[System.Serializable] public class Achievement {
    public int Index;
    public enum State { None = -1, Fail = 0, Clear = 1, MaxCombo = 2, Perfect = 3 }
    public State Status;
}
[System.Serializable] public class SettingData {
    
}

public class Manager : MonoBehaviour
{
    [Header("JSON Data")]
    [SerializeField] private TextAsset advancedJson;
    [SerializeField] private TextAsset simpleJson;
    [SerializeField] private TextAsset defaultAchievementJson;
    [SerializeField] private float     ToggleAllNoneDelay;

    [Header("Resources Path")]
    [SerializeField] private string ThumbnailResourcePath;
    [SerializeField] private string PreviewResourcePath;
    [SerializeField] private string DifficultyResourcePath;
    [SerializeField] private string CategoryResourcePath;
    [SerializeField] private string AchievementDifficultyResourcePath;

    [Header("Header")]
    [SerializeField] private TextMeshProUGUI TitleText;
    [SerializeField] private Animator TitleStarShine;

    [Header("Body")]
    [SerializeField] private CanvasScaler  MainCanvasScaler;
    [SerializeField] private RectTransform BodyRect;
    [Space]
    [SerializeField] private RectTransform SelectorOptionPanel;
    [SerializeField] private RectTransform RandomResultPanel;
    [SerializeField] private RectTransform CustomLadderReadyPanel;
    [SerializeField] private RectTransform CustomLadderBanPickPanel;
    [SerializeField] private RectTransform CustomLadderMainPanel;
    [SerializeField] private RectTransform AchievementInfoPanel;
    [SerializeField] private RectTransform AchievementResultPanel;
    [Space]
    [SerializeField] private RandomSelectorUIElements    RandomSelectorUI;
    [SerializeField] private AchievementUIElements       AchievementUI;
    [SerializeField] private CustomLadderMatchUIElements CustomLadderUI;

    [System.Serializable] public class RandomSelectorUIElements {
        public GameObject       DefaultUIScreen;
        public GameObject       SingleUIScreen;
        [Space]
        public Image            SingleThumbnail;
        public Image            SingleCategory;
        public Image            SingleButtonAndDiff;
        public TextMeshProUGUI  SingleTitle;
        public TextMeshProUGUI  SingleComposer;
        public Transform        SingleLvParent;
        public Transform        SingleLvSCParent;
        [Space]
        public Transform        ResultListParent;
        public GameObject       ResultListPrefab;
        [Space]
        public Transform        DifficultyParent;
        public Transform        LvParent;
        public Transform[]      DLCParents;
        public TextMeshProUGUI  CountLabel;
    }
    [System.Serializable] public class AchievementUIElements {
        public Sprite[]         ButtonBackgroundSprites;
        public Sprite[]         ButtonTextSprites;
        [Space]
        public Image            ButtonOptBG;
        public Image            ButtonOptText;
        public Animator         StarShineAnimator;
        [Space]
        public TextMeshProUGUI  Title;
        public TextMeshProUGUI  Composer;
        public Image            Category;
        public Image            Preview;
        public Color[]          IndicatorColor;
        public Transform        LvToggleParent;
        public Transform        LvToggleSCParent;
        public ToggleGroup      StateToggleGroup;
        [Space]
        public Transform        ScrollViewport;
        public GameObject       LevelDivPrefab;
        public GameObject       FloorListPrefab;
        public GameObject       FloorGridPrefab;
        public GameObject       ListPrefab;
        public GameObject       GridPrefab;
    }
    [System.Serializable] public class CustomLadderMatchUIElements {
        public GameObject       DefaultUIScreen;
        public Transform        ScrollViewport;
        [Space]
        public GameObject       RoundPrefab;
    }

    private string AchievementPath           => string.Format("{0}/{1}.json", Application.persistentDataPath, "achievements");
    private string SettingPath               => string.Format("{0}/{1}.json", Application.persistentDataPath, "setting");
    private string CustomAchievementListPath => string.Format("{0}/{1}.json", Application.persistentDataPath, "customlist");
    private AchievementData achievementData;
    private SettingData settingData;

    private enum State                          { Achievements, Random, Ladder }
    private readonly string[] StateTitleString  = { "Achievements", "Random Selector", "Custom Ladder" };
    // private enum SelectionMode                  { Simple, Advanced }
    private enum ViewMode                       { List, Grid }
    private enum AchievementDataType            { Default, Custom }
    private enum Buttons                        { Four, Five, Six, Eight }
    private readonly string[] ButtonString      = { "4B", "5B", "6B", "8B" };
    private State AppState                      = State.Achievements;
    // private SelectionMode RandomSelectionMode   = SelectionMode.Advanced;
    private ViewMode AchievementViewMode        = ViewMode.List;
    private AchievementDataType AchievementType = AchievementDataType.Default;
    private Buttons AchievementButton           = Buttons.Four;

    private TrackAdvancedData trackAdvancedData;
    private TrackSimpleData   trackSimpleData;
    private AchievementList   defaultAchievementList;
    private AchievementList   customAchievementList;

    private WaitForSeconds toggleDelay;
    private void Start() { // Parse JSON Data
        toggleDelay = new WaitForSeconds(ToggleAllNoneDelay);

        trackAdvancedData      = JsonUtility.FromJson<TrackAdvancedData>(advancedJson.text);
        trackSimpleData        = JsonUtility.FromJson<TrackSimpleData>  (simpleJson.text);
        defaultAchievementList = JsonUtility.FromJson<AchievementList>  (defaultAchievementJson.text);

        achievementData        = LoadJson<AchievementData>(AchievementPath) ?? new AchievementData();
        settingData            = LoadJson<SettingData>    (SettingPath) ?? new SettingData();
        customAchievementList  = LoadJson<AchievementList>(CustomAchievementListPath) ?? JsonUtility.FromJson<AchievementList>(defaultAchievementJson.text);

        // * TEST CODES
        ClearPanel();
        OpenAchievement(true);
    }


    // TODO : 키보드 입력 추가
    // ! 반응형 해상도 맞춰서 슬라이딩 모션 넣는거 문제 발생.... 해결하기 매우 어려울 것으로 예상 됨...
    // private void Update() {}

    private void ClearPanel() {
        SelectorOptionPanel.DOMoveX(-GetRectTransformWidth(SelectorOptionPanel), 0f).SetEase(Ease.InOutCirc);
        RandomResultPanel.DOMoveX(GetRectTransformWidth(BodyRect) + GetRectTransformWidth(RandomResultPanel), 0f).SetEase(Ease.InOutCirc);
        CustomLadderReadyPanel.DOMoveX(GetRectTransformWidth(BodyRect) + GetRectTransformWidth(CustomLadderReadyPanel), 0f).SetEase(Ease.InOutCirc);
        // CustomLadderBanPickPanel.DOMoveX(-CustomLadderBanPickPanel, 0.5f).SetEase(Ease.InOutCirc);
        // CustomLadderMainPanel.DOMoveX(-CustomLadderMainPanel, 0.5f).SetEase(Ease.InOutCirc);
        AchievementInfoPanel.DOMoveX(-GetRectTransformWidth(AchievementInfoPanel), 0f).SetEase(Ease.InOutCirc);
        AchievementResultPanel.DOMoveX(GetRectTransformWidth(BodyRect) + GetRectTransformWidth(AchievementResultPanel), 0f).SetEase(Ease.InOutCirc);
    }
    Coroutine LoadingAchievement;
    public void OpenAchievement(bool isAppStateChanged = false) {
        if(isAppStateChanged) {
            SelectorOptionPanel.DOMoveX(-GetRectTransformWidth(SelectorOptionPanel), 0.5f).SetEase(Ease.InOutCirc);
            RandomResultPanel.DOMoveX(GetRectTransformWidth(BodyRect) + GetRectTransformWidth(RandomResultPanel), 0.5f).SetEase(Ease.InOutCirc);
            CustomLadderReadyPanel.DOMoveX(GetRectTransformWidth(BodyRect) + GetRectTransformWidth(CustomLadderReadyPanel), 0.5f).SetEase(Ease.InOutCirc);
            // CustomLadderBanPickPanel.DOMoveX(-CustomLadderBanPickPanel, 0.5f).SetEase(Ease.InOutCirc);
            // CustomLadderMainPanel.DOMoveX(-CustomLadderMainPanel, 0.5f).SetEase(Ease.InOutCirc);
            AchievementInfoPanel.DOMoveX(0, 0.5f).SetEase(Ease.InOutCirc);
            AchievementResultPanel.DOMoveX(GetRectTransformWidth(BodyRect), 0.5f).SetEase(Ease.InOutCirc);
        }
        
        if(LoadingAchievement != null)
            StopCoroutine(LoadingAchievement);
        LoadingAchievement = StartCoroutine(OpenAchievementRoutine());
    }
    private IEnumerator OpenAchievementRoutine() {
        AppState = State.Achievements;
        int Count = 0;

        AchievementList TargetAchievementData;
        switch(AchievementType) {
            case AchievementDataType.Default: TargetAchievementData = defaultAchievementList; break;
            default:                          TargetAchievementData = customAchievementList;  break;
        }
        AchievementLv[] LevelArray;
        switch(AchievementButton) {
            case Buttons.Five:  LevelArray = TargetAchievementData._5B.Lv; break;
            case Buttons.Six:   LevelArray = TargetAchievementData._6B.Lv; break;
            case Buttons.Eight: LevelArray = TargetAchievementData._8B.Lv; break;
            default:            LevelArray = TargetAchievementData._4B.Lv; break;
        }

        foreach(Transform t in AchievementUI.ScrollViewport) Destroy(t.gameObject);
        switch(AchievementViewMode) {
            case ViewMode.List:
                for(int i = LevelArray.Length-1; i >=0 ; i--) {
                    if(LevelArray[i].FineLv.Sum((x) => (x.Index.Count)) == 0)
                        continue;
                    
                    GameObject LevelDiv = Instantiate(AchievementUI.LevelDivPrefab, Vector3.zero, Quaternion.identity, AchievementUI.ScrollViewport);
                    LevelDiv.transform.localScale = Vector3.one;

                    LevelDiv.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"Lv {i+1}";
                    Transform LevelStarToggleParent = LevelDiv.transform.GetChild(2);
                    for(int j = 0; j < i+1; j++) {
                        LevelStarToggleParent.GetChild(j).GetComponent<Toggle>().isOn = true;
                    }

                    for(int j = 10-1; j >= 0; j--) {
                        if(LevelArray[i].FineLv[j].Index.Count == 0)
                            continue;
                        GameObject Floor = Instantiate(AchievementUI.FloorListPrefab, Vector3.zero, Quaternion.identity, AchievementUI.ScrollViewport);
                        Floor.transform.localScale = Vector3.one;

                        TextMeshProUGUI FloorHeader = Floor.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
                        FloorHeader.text = $"Floor {j+1}";

                        Transform TrackList = Floor.transform.GetChild(1);
                        foreach(int index in LevelArray[i].FineLv[j].Index) {
                            TrackAdvanced TrackData = GetTrackAdvancedData(index);
                            Achievement AchievementData = GetAchievementSave(index);

                            GameObject Track = Instantiate(AchievementUI.ListPrefab, Vector3.zero, Quaternion.identity, TrackList);
                            Track.transform.localScale = Vector3.one;

                            Image Thumbnail = Track.transform.GetChild(0).GetComponent<Image>();
                            Image Difficulty = Track.transform.GetChild(0).GetChild(0).GetComponent<Image>();
                            Image Indicator = Track.transform.GetChild(1).GetComponent<Image>();
                            Image Category = Track.transform.GetChild(2).GetChild(0).GetComponent<Image>();
                            TextMeshProUGUI Title = Track.transform.GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>();
                            TextMeshProUGUI Composer = Track.transform.GetChild(2).GetChild(2).GetComponent<TextMeshProUGUI>();

                            Thumbnail.sprite = GetThumbnailSprite(TrackData.Name);
                            Difficulty.sprite = GetAchievementDifficultySprite(TrackData.Diff);
                            Indicator.color  = AchievementUI.IndicatorColor[(int)AchievementData.Status +1];
                            Category.sprite = GetCategorySprite(TrackData.Ctgr);
                            Title.text = TrackData.Name;
                            Composer.text = TrackData.Cmps;

                            Track.GetComponent<Button>().onClick.AddListener(() => {OpenAchievementInfo(index, Indicator);});

                            if(Count == 0)
                                OpenAchievementInfo(index, Indicator);
                            Count++;
                        }

                        yield return null;
                    }
                }
                break;

            case ViewMode.Grid:
                for(int i = LevelArray.Length-1; i >=0 ; i--) {
                    if(LevelArray[i].FineLv.Sum((x) => (x.Index.Count)) == 0)
                        continue;

                    GameObject LevelDiv = Instantiate(AchievementUI.LevelDivPrefab, Vector3.zero, Quaternion.identity, AchievementUI.ScrollViewport);
                    LevelDiv.transform.localScale = Vector3.one;

                    LevelDiv.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"Lv {i+1}";
                    Transform LevelStarToggleParent = LevelDiv.transform.GetChild(2);
                    for(int j = 0; j < i+1; j++) {
                        LevelStarToggleParent.GetChild(j).GetComponent<Toggle>().isOn = true;
                    }

                    for(int j = 10-1; j >= 0; j--) {
                        if(LevelArray[i].FineLv[j].Index.Count == 0)
                            continue;
                        GameObject Floor = Instantiate(AchievementUI.FloorGridPrefab, Vector3.zero, Quaternion.identity, AchievementUI.ScrollViewport);
                        Floor.transform.localScale = Vector3.one;

                        TextMeshProUGUI FloorHeader = Floor.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
                        FloorHeader.text = $"Floor {j+1}";

                        Transform TrackGrid = Floor.transform.GetChild(1);
                        foreach(int index in LevelArray[i].FineLv[j].Index) {
                            TrackAdvanced TrackData = GetTrackAdvancedData(index);
                            Achievement AchievementData = GetAchievementSave(index);

                            GameObject Track = Instantiate(AchievementUI.GridPrefab, Vector3.zero, Quaternion.identity, TrackGrid);
                            Track.transform.localScale = Vector3.one;

                            Image Thumbnail = Track.transform.GetChild(0).GetComponent<Image>();
                            Image Difficulty = Track.transform.GetChild(0).GetChild(0).GetComponent<Image>();
                            Image Indicator = Track.transform.GetComponent<Image>();

                            Thumbnail.sprite = GetThumbnailSprite(TrackData.Name);
                            Difficulty.sprite = GetAchievementDifficultySprite(TrackData.Diff);
                            Indicator.color  = AchievementUI.IndicatorColor[(int)AchievementData.Status +1];

                            Track.GetComponent<Button>().onClick.AddListener(() => {OpenAchievementInfo(index, Indicator);});

                            if(Count == 0)
                                OpenAchievementInfo(index, Indicator);
                            Count++;
                        }

                        yield return null;
                    }
                }
                break;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)AchievementUI.ScrollViewport);
    }
    public void OpenRandomSelector(bool isAppStateChanged = false) {
        if(isAppStateChanged) {
            SelectorOptionPanel.DOMoveX(0, 0.5f).SetEase(Ease.InOutCirc);
            RandomResultPanel.DOMoveX(GetRectTransformWidth(BodyRect), 0.5f).SetEase(Ease.InOutCirc);
            CustomLadderReadyPanel.DOMoveX(GetRectTransformWidth(BodyRect) + GetRectTransformWidth(CustomLadderReadyPanel), 0.5f).SetEase(Ease.InOutCirc);
            // CustomLadderBanPickPanel.DOMoveX(-CustomLadderBanPickPanel, 0.5f).SetEase(Ease.InOutCirc);
            // CustomLadderMainPanel.DOMoveX(-CustomLadderMainPanel, 0.5f).SetEase(Ease.InOutCirc);
            AchievementInfoPanel.DOMoveX(-GetRectTransformWidth(AchievementInfoPanel), 0.5f).SetEase(Ease.InOutCirc);
            AchievementResultPanel.DOMoveX(GetRectTransformWidth(BodyRect) + GetRectTransformWidth(AchievementResultPanel), 0.5f).SetEase(Ease.InOutCirc);

            RandomSelectorUI.DefaultUIScreen.SetActive(true);
            RandomSelectorUI.SingleUIScreen.SetActive(false);
            foreach(Transform t in RandomSelectorUI.ResultListParent) {
                Destroy(t.gameObject);
            }   
        }
    }
    public void OpenCustomLadder(bool isAppStateChanged = false) {
        if(isAppStateChanged) {
            SelectorOptionPanel.DOMoveX(0, 0.5f).SetEase(Ease.InOutCirc);
            RandomResultPanel.DOMoveX(GetRectTransformWidth(BodyRect) + GetRectTransformWidth(RandomResultPanel), 0.5f).SetEase(Ease.InOutCirc);
            CustomLadderReadyPanel.DOMoveX(GetRectTransformWidth(BodyRect), 0.5f).SetEase(Ease.InOutCirc);
            // CustomLadderBanPickPanel.DOMoveX(-CustomLadderBanPickPanel, 0.5f).SetEase(Ease.InOutCirc);
            // CustomLadderMainPanel.DOMoveX(-CustomLadderMainPanel, 0.5f).SetEase(Ease.InOutCirc);
            AchievementInfoPanel.DOMoveX(-GetRectTransformWidth(AchievementInfoPanel), 0.5f).SetEase(Ease.InOutCirc);
            AchievementResultPanel.DOMoveX(GetRectTransformWidth(BodyRect) + GetRectTransformWidth(AchievementResultPanel), 0.5f).SetEase(Ease.InOutCirc);
        }

        CustomLadderState = LadderState.Ready;
    }
    private enum LadderState { Ready, BanPick, Main }
    private LadderState CustomLadderState = LadderState.Ready;
    private List<TrackAdvanced> CustomLadderPickedTracks = new List<TrackAdvanced>();
    private void CustomLadderBanPick() {
        CustomLadderState = LadderState.BanPick;
    }
    // TODO : 여기가 커스텀 래더 매치 코드 개발 이어서 해야하는 부분!
    public void CustomLadderBanPickCancel() {}
    private void StartCustomLadderMatch() {
        CustomLadderState = LadderState.Main;
    }
    public void CustomLadderNextRound() {}
    public void CustomLadderMatchEnd() {}
    
    private string GetCategoryFullName(string abbr) {
        switch(abbr) {
            case "P1": return "PORTABLE 1";
            case "P2": return "PORTABLE 2";
            case "P3": return "PORTABLE 3";
            case "RE": return "RESPECT";
            case "VE": return "V EXTENSION";
            case "ES": return "EMOTIONAL S.";
            case "CE": return "CLAZZIQUAI EDITION";
            case "BS": return "BLACK SQUARE";
            case "TR": return "TRILOGY";
            case "T1": return "TECHNIKA";
            case "T2": return "TECHNIKA 2";
            case "T3": return "TECHNIKA 3";
            default:   return "COLLABORATION";
        }
    }


    private Tween AchievementButtonSpriteSwap;
    public void AchievementButtonToggle() {
        AchievementButtonSpriteSwap?.Kill();
        AchievementButtonSpriteSwap = DOVirtual.DelayedCall(0.5f, AchievementButtonSpriteSwapDelay);
        AchievementUI.StarShineAnimator.SetTrigger("Shine");
    }
    private void AchievementButtonSpriteSwapDelay() {
        switch(AchievementButton) {
            case Buttons.Four:   AchievementButton = Buttons.Five;   break;
            case Buttons.Five:   AchievementButton = Buttons.Six;    break;
            case Buttons.Six:    AchievementButton = Buttons.Eight;  break;
            case Buttons.Eight:  AchievementButton = Buttons.Four;   break;
        }
        AchievementUI.ButtonOptText.sprite = AchievementUI.ButtonTextSprites[(int)AchievementButton];
        AchievementUI.ButtonOptBG.sprite   = AchievementUI.ButtonBackgroundSprites[(int)AchievementButton];

        OpenAchievement();
    }
    public void SetAchievementViewTypeList() {
        if(AchievementViewMode != ViewMode.List) {
            AchievementViewMode = ViewMode.List;
            OpenAchievement();
        }
    }
    public void SetAchievementViewTypeGrid() {
        if(AchievementViewMode != ViewMode.Grid) {
            AchievementViewMode = ViewMode.Grid;
            OpenAchievement();
        }
    }

    public void OpenAchievementInfo(int index, Image indicator) {
        TrackAdvanced TrackData = GetTrackAdvancedData(index);
        
        AchievementIndex = index;
        AchievementIndicator = indicator;

        AchievementUI.Title.text = TrackData.Name;
        AchievementUI.Composer.text = TrackData.Cmps;
        AchievementUI.Category.sprite = GetCategorySprite(TrackData.Ctgr);
        AchievementUI.Preview.sprite = GetPreviewSprite(TrackData.Name);

        if(TrackData.Diff == "SC") {
            AchievementUI.LvToggleParent.gameObject.SetActive(false);
            AchievementUI.LvToggleSCParent.gameObject.SetActive(true);

            for(int i = 0; i < 15; i++) AchievementUI.LvToggleSCParent.GetChild(i).GetComponent<Toggle>().isOn = false;
            for(int i = 0; i < TrackData.Lv; i++) AchievementUI.LvToggleSCParent.GetChild(i).GetComponent<Toggle>().isOn = true;
        }
        else {
            AchievementUI.LvToggleParent.gameObject.SetActive(true);
            AchievementUI.LvToggleSCParent.gameObject.SetActive(false);
            
            for(int i = 0; i < 15; i++) AchievementUI.LvToggleParent.GetChild(i).GetComponent<Toggle>().isOn = false;
            for(int i = 0; i < TrackData.Lv; i++) AchievementUI.LvToggleParent.GetChild(i).GetComponent<Toggle>().isOn = true;
        }

        int state = (int?)achievementData.achievements.FirstOrDefault((x) => (x.Index == index))?.Status ?? -1;
        ApplySelectionToToggleGroup(AchievementUI.StateToggleGroup, state);
    }
    private TrackAdvanced GetTrackAdvancedData(int index) {
        TrackAdvanced trackData = trackAdvancedData.tracks.FirstOrDefault((x) => (x.Index == index));
        if(trackData == null)
            throw new InvalidDataException();
        
        return trackData;
    }
    private Sprite GetThumbnailSprite(string name) {
        return Resources.Load<Sprite>(ThumbnailResourcePath + name);
    }
    private Sprite GetPreviewSprite(string name) {
        return Resources.Load<Sprite>(PreviewResourcePath + name);
    }
    private Sprite GetDifficultySprite(string button, string difficulty) {
        return Resources.Load<Sprite>(DifficultyResourcePath + button + difficulty) ?? Resources.Load<Sprite>(DifficultyResourcePath + "4BNM");
    }
    private Sprite GetCategorySprite(string category) {
        return Resources.Load<Sprite>(CategoryResourcePath + category) ?? Resources.Load<Sprite>(CategoryResourcePath + "COLLABOR");
    }
    private Sprite GetAchievementDifficultySprite(string difficulty) {
        return Resources.Load<Sprite>(AchievementDifficultyResourcePath + difficulty);
    }

    private int AchievementIndex;
    private Image AchievementIndicator;
    public void AchievementStateToggleUpdate() {
        int index = GetSelectedToggleIndex(AchievementUI.StateToggleGroup) ?? -1;

        UpdateAchievementSave(AchievementIndex, (Achievement.State)index);
        AchievementIndicator.color = AchievementUI.IndicatorColor[index + 1];
    }
    
    /// <summary>인덱스 값을 입력하면 해당 성취가 세이브에 있으면 업데이트, 없으면 추가합니다.</summary>
    private void UpdateAchievementSave(int index, Achievement.State state) {
        Achievement achievement = achievementData.achievements.FirstOrDefault((x) => (x.Index == index));
        if(achievement == null) {
            achievement = new Achievement();
            achievementData.achievements.Add(achievement);
            achievement.Index = index;
            achievement.Status = Achievement.State.None;
        }

        achievement.Status = state;

        SaveJson<AchievementData>(achievementData, AchievementPath);
    }
    private int? GetSelectedToggleIndex(ToggleGroup toggleGroup) {
        toggleGroup.EnsureValidState();
        Toggle[] toggles = toggleGroup.GetComponentsInChildren<Toggle>();
        Toggle target = toggleGroup.ActiveToggles().FirstOrDefault();

        if(target == null)
            return null;
        else
        for(int i = 0; i < toggles.Length; i++) {
            if(target == toggles[i])
                return i;
        }

        return null;
    }
    private void ApplySelectionToToggleGroup(ToggleGroup toggleGroup, int index) {
        toggleGroup.SetAllTogglesOff();
        Toggle[] toggles = toggleGroup.GetComponentsInChildren<Toggle>();
        if(index < 0 || index >= toggles.Length)
            return;
        
        toggles[index].isOn = true;
    }
    private Achievement GetAchievementSave(int index) {
        Achievement achievement = achievementData.achievements.FirstOrDefault((x) => (x.Index == index));
        if(achievement == null) {
            achievement = new Achievement();
            achievementData.achievements.Add(achievement);
            achievement.Index = index;
            achievement.Status = Achievement.State.None;
        }

        return achievement;
    }


    private bool    _4b = true, _5b = true, _6b = true, _8b = true,
                    _nm = true, _hd = true, _mx = true, _sc = true,
                    _1 = true, _2 = true, _3 = true, _4 = true, _5 = true, 
                    _6 = true, _7 = true, _8 = true, _9 = true, _10 = true, 
                    _11 = true, _12 = true, _13 = true, _14 = true, _15 = true;
    
    private bool    _RE = true, _P1 = true, _P2 = true, _P3 = true,
                    _T1 = true, _T2 = true, _T3 = true,
                    _BS = true, _CE = true, _TR = true,
                    _VE = true, _ES = true, _CH = true, _GC = true,
                    _DM = true, _CY = true, _EM = true,
                    _GF = true, _GG = true;
    
    private int _Count = 1;

    public void ChangeApplicationMode() {
        switch(AppState) {
            case State.Achievements: AppState = State.Random;       OpenRandomSelector(true); break;
            case State.Random:       AppState = State.Ladder;       OpenCustomLadder(true);   break;
            case State.Ladder:       if(CustomLadderState != LadderState.Ready) return;
                                     AppState = State.Achievements; OpenAchievement(true);    break;
        }

        TitleText.text = StateTitleString[(int)AppState];
        TitleStarShine.SetTrigger("Shine");
    }

    private float GetRectTransformWidth(RectTransform rect) {
        float wRatio = Screen.width  / MainCanvasScaler.referenceResolution.x;
        float hRatio = Screen.height / MainCanvasScaler.referenceResolution.y;

        float ratio =
            wRatio * (1f - MainCanvasScaler.matchWidthOrHeight) +
            hRatio * (MainCanvasScaler.matchWidthOrHeight);

        return rect.rect.width * ratio;
    }
    private float GetRectTransformHeight(RectTransform rect) {
        float wRatio = Screen.width  / MainCanvasScaler.referenceResolution.x;
        float hRatio = Screen.height / MainCanvasScaler.referenceResolution.y;

        float ratio =
            wRatio * (1f - MainCanvasScaler.matchWidthOrHeight) +
            hRatio * (MainCanvasScaler.matchWidthOrHeight);

        return rect.rect.height * ratio;
    }

#region JSON Save & Load
    private bool FileExists(string path) {
        FileInfo fileInfo = new FileInfo(path);
        return fileInfo.Exists;
    }
    private string ReadFile(string path) {
        FileStream fileStream = new FileStream(path, FileMode.Open);
        byte[] data = new byte[fileStream.Length];
        fileStream.Read(data, 0, data.Length);
        fileStream.Close();
        return Encoding.UTF8.GetString(data);
    }
    private void SaveJson<T>(T rawData, string path) {
        string jsonData = JsonUtility.ToJson(rawData);
        FileStream fileStream = new FileStream(path, FileMode.Create);
        byte[] data = Encoding.UTF8.GetBytes(jsonData);
        fileStream.Write(data, 0, data.Length);
        fileStream.Close();
    }
    private T LoadJson<T>(string path) {
        if(!FileExists(path))
            return default(T);

        return JsonUtility.FromJson<T>(ReadFile(path));
    }
#endregion
#region UI
    public void _StartBtn() {
        switch(AppState) {
            case State.Random: ShowRandomSelectorResult(); break;
            case State.Ladder: CustomLadderBanPick();   break;
        }
    }

    public void _U_Count(Slider s) { _Count = (int)s.value; RandomSelectorUI.CountLabel.text = ((int)s.value).ToString(); }

    public void _DIFF_ALL() { StartCoroutine(_DIFF_ALL_ROUTINE()); }
    private IEnumerator _DIFF_ALL_ROUTINE() {
        foreach(Transform t in RandomSelectorUI.DifficultyParent) {
            Toggle tg = t.GetComponent<Toggle>();
            if(tg != null)
                tg.isOn = true;
            
            yield return toggleDelay;
        }
    }
    public void _DIFF_NONE() { StartCoroutine(_DIFF_NONE_ROUTINE()); }
    private IEnumerator _DIFF_NONE_ROUTINE() {
        foreach(Transform t in RandomSelectorUI.DifficultyParent) {
            Toggle tg = t.GetComponent<Toggle>();
            if(tg != null)
                tg.isOn = false;
            
            yield return toggleDelay;
        }
    }
    public void _LV_ALL() { StartCoroutine(_LV_ALL_ROUTINE()); }
    private IEnumerator _LV_ALL_ROUTINE() {
        foreach(Transform t in RandomSelectorUI.LvParent) {
            Toggle tg = t.GetComponent<Toggle>();
            if(tg != null)
                tg.isOn = true;
            
            yield return toggleDelay;
        }
    }
    public void _LV_NONE() { StartCoroutine(_LV_NONE_ROUTINE()); }
    private IEnumerator _LV_NONE_ROUTINE() {
        foreach(Transform t in RandomSelectorUI.LvParent) {
            Toggle tg = t.GetComponent<Toggle>();
            if(tg != null)
                tg.isOn = false;
            
            yield return toggleDelay;
        }
    }
    public void _DLC_ALL() { StartCoroutine(_DLC_ALL_ROUTINE());}
    private IEnumerator _DLC_ALL_ROUTINE() {
        foreach(Transform tp in RandomSelectorUI.DLCParents)
        foreach(Transform t in tp) {
            Toggle tg = t.GetComponent<Toggle>();
            if(tg != null)
                tg.isOn = true;
            
            yield return toggleDelay;
        }
    }
    public void _DLC_NONE() { StartCoroutine(_DLC_NONE_ROUTINE());}
    private IEnumerator _DLC_NONE_ROUTINE() {
        foreach(Transform tp in RandomSelectorUI.DLCParents)
        foreach(Transform t in tp) {
            Toggle tg = t.GetComponent<Toggle>();
            if(tg != null)
                tg.isOn = false;
            
            yield return toggleDelay;
        }
    }

    public void _4B(bool b) { _4b = b; }
    public void _5B(bool b) { _5b = b; }
    public void _6B(bool b) { _6b = b; }
    public void _8B(bool b) { _8b = b; }

    public void _NM(bool b) { _nm = b; }
    public void _HD(bool b) { _hd = b; }
    public void _MX(bool b) { _mx = b; }
    public void _SC(bool b) { _sc = b; }

    public void _Lv1(bool b) { _1 = b; }
    public void _Lv2(bool b) { _2 = b; }
    public void _Lv3(bool b) { _3 = b; }
    public void _Lv4(bool b) { _4 = b; }
    public void _Lv5(bool b) { _5 = b; }
    public void _Lv6(bool b) { _6 = b; }
    public void _Lv7(bool b) { _7 = b; }
    public void _Lv8(bool b) { _8 = b; }
    public void _Lv9(bool b) { _9 = b; }
    public void _Lv10(bool b) { _10 = b; }
    public void _Lv11(bool b) { _11 = b; }
    public void _Lv12(bool b) { _12 = b; }
    public void _Lv13(bool b) { _13 = b; }
    public void _Lv14(bool b) { _14 = b; }
    public void _Lv15(bool b) { _15 = b; }

    public void _D_RE(bool b) { _RE = b; }
    public void _D_P1(bool b) { _P1 = b; }
    public void _D_P2(bool b) { _P2 = b; }
    public void _D_P3(bool b) { _P3 = b; }
    public void _D_T1(bool b) { _T1 = b; }
    public void _D_T2(bool b) { _T2 = b; }
    public void _D_T3(bool b) { _T3 = b; }
    public void _D_BS(bool b) { _BS = b; }
    public void _D_CE(bool b) { _CE = b; }
    public void _D_TR(bool b) { _TR = b; }
    public void _D_VE(bool b) { _VE = b; }
    public void _D_ES(bool b) { _ES = b; }
    public void _D_CH(bool b) { _CH = b; }
    public void _D_GC(bool b) { _GC = b; }
    public void _D_DM(bool b) { _DM = b; }
    public void _D_CY(bool b) { _CY = b; }
    public void _D_EM(bool b) { _EM = b; }
    public void _D_GF(bool b) { _GF = b; }
    public void _D_GG(bool b) { _GG = b; }
#endregion

    public void ShowRandomSelectorResult() {
        RandomSelectorUI.DefaultUIScreen.SetActive(false);
        foreach(Transform t in RandomSelectorUI.ResultListParent) {
            Destroy(t.gameObject);
        }

        List<TrackAdvanced> Tracks = GetTracks(count:_Count);
        if(_Count > 1) {
            RandomSelectorUI.SingleUIScreen.SetActive(false);
            foreach(TrackAdvanced TrackData in Tracks) {
                GameObject ResultList = Instantiate(RandomSelectorUI.ResultListPrefab);
                ResultList.transform.SetParent(RandomSelectorUI.ResultListParent);
                ResultList.transform.localScale = Vector3.one;
                
                if(TrackData == null)
                    continue;

                // ? Track > Album Jacket(Image)
                Sprite thumb = GetThumbnailSprite(TrackData.Name);
                if(thumb != null)
                    ResultList.transform.GetChild(0).GetComponent<Image>().sprite = thumb;
                else
                    print(TrackData.Name + " " + TrackData.Bt + " " + TrackData.Diff);

                // ? Track > Description > Difficulty(Image)
                ResultList.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = GetDifficultySprite(TrackData.Bt, TrackData.Diff);
                
                // ? Track > Description > Category(Image)
                ResultList.transform.GetChild(1).GetChild(1).GetComponent<Image>().sprite = GetCategorySprite(TrackData.Ctgr);

                // ? Track > Description > Title(Text)
                ResultList.transform.GetChild(1).GetChild(2).GetComponent<TextMeshProUGUI>().text = TrackData.Name;

                // ? Track > Description > Info(Text)
                ResultList.transform.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text = TrackData.Cmps;

                if(TrackData.Diff == "SC") {
                    // ? Track > Description > LevelToggle SC
                    Transform levelToggleParent = ResultList.transform.GetChild(1).GetChild(4);
                    Transform levelToggleParentSC = ResultList.transform.GetChild(1).GetChild(5);
                    
                    levelToggleParent.gameObject.SetActive(false);
                    levelToggleParentSC.gameObject.SetActive(true);

                    for(int i = 0; i < TrackData.Lv; i++) {
                        levelToggleParentSC.GetChild(i).GetComponent<Toggle>().isOn = true;
                    }
                }
                else {
                    // ? Track > Description > LevelToggle
                    Transform levelToggleParent = ResultList.transform.GetChild(1).GetChild(4);
                    Transform levelToggleParentSC = ResultList.transform.GetChild(1).GetChild(5);

                    levelToggleParent.gameObject.SetActive(true);
                    levelToggleParentSC.gameObject.SetActive(false);

                    for(int i = 0; i < TrackData.Lv; i++) {
                        levelToggleParent.GetChild(i).GetComponent<Toggle>().isOn = true;
                    }
                }         
            }
        }
        else {
            RandomSelectorUI.SingleUIScreen.SetActive(true);
            TrackAdvanced TrackData = Tracks[0];

            RandomSelectorUI.SingleCategory.sprite = GetCategorySprite(TrackData.Ctgr);
            RandomSelectorUI.SingleThumbnail.sprite = GetThumbnailSprite(TrackData.Name);
            RandomSelectorUI.SingleButtonAndDiff.sprite = GetDifficultySprite(TrackData.Bt, TrackData.Diff);
            RandomSelectorUI.SingleTitle.text = TrackData.Name;
            RandomSelectorUI.SingleComposer.text = TrackData.Cmps;

            if(TrackData.Diff == "SC") {
                RandomSelectorUI.SingleLvParent.gameObject.SetActive(false);
                RandomSelectorUI.SingleLvSCParent.gameObject.SetActive(true);

                for(int i = 0; i < 15; i++)           RandomSelectorUI.SingleLvSCParent.GetChild(i).GetComponent<Toggle>().isOn = false;
                for(int i = 0; i < TrackData.Lv; i++) RandomSelectorUI.SingleLvSCParent.GetChild(i).GetComponent<Toggle>().isOn = true;
            }
            else {
                RandomSelectorUI.SingleLvParent.gameObject.SetActive(true);
                RandomSelectorUI.SingleLvSCParent.gameObject.SetActive(false);

                for(int i = 0; i < 15; i++)           RandomSelectorUI.SingleLvParent.GetChild(i).GetComponent<Toggle>().isOn = false;
                for(int i = 0; i < TrackData.Lv; i++) RandomSelectorUI.SingleLvParent.GetChild(i).GetComponent<Toggle>().isOn = true;
            }
        }
    }
    private List<TrackAdvanced> GetTracks(int count = 1) {

        List<TrackAdvanced> pass = trackAdvancedData.tracks.FindAll( (t) =>  
        (
               ( (t.Bt.Equals("4B") && _4b)   || (t.Bt.Equals("5B") && _5b)   || (t.Bt.Equals("6B") && _6b)   || (t.Bt.Equals("8B") && _8b) )
            && ( (t.Diff.Equals("NM") && _nm) || (t.Diff.Equals("HD") && _hd) || (t.Diff.Equals("MX") && _mx) || (t.Diff.Equals("SC") && _sc) )
            && (
                 (t.Lv == 1 && _1)   || (t.Lv == 2 && _2)   || (t.Lv == 3 && _3)   || (t.Lv == 4 && _4)   || (t.Lv == 5 && _5)   ||
                 (t.Lv == 6 && _6)   || (t.Lv == 7 && _7)   || (t.Lv == 8 && _8)   || (t.Lv == 9 && _9)   || (t.Lv == 10 && _10) ||
                 (t.Lv == 11 && _11) || (t.Lv == 12 && _12) || (t.Lv == 13 && _13) || (t.Lv == 14 && _14) || (t.Lv == 15 && _15)
               )
            && (
                 (t.Ctgr == "RE" && _RE) || (t.Ctgr == "P1" && _P1) || (t.Ctgr == "P2" && _P2) || (t.Ctgr == "P3" && _P3) || (t.Ctgr == "T1" && _T1) || (t.Ctgr == "T2" && _T2) || (t.Ctgr == "T3" && _T3) || 
                 (t.Ctgr == "BS" && _BS) || (t.Ctgr == "CE" && _CE) || (t.Ctgr == "TR" && _TR) || 
                 (t.Ctgr == "VE" && _VE) || (t.Ctgr == "ES" && _ES) || (t.Ctgr == "CH" && _CH) || (t.Ctgr == "GC" && _GC) || 
                 (t.Ctgr == "DM" && _DM) || (t.Ctgr == "CY" && _CY) || (t.Ctgr == "EM" && _EM) || 
                 (t.Ctgr == "GF" && _GF) || (t.Ctgr == "GG" && _GG)
               )
        ) );

        pass = pass.OrderBy(a => Guid.NewGuid()).ToList();

        List<TrackAdvanced> tracks = new List<TrackAdvanced>();
        
        for(int i = 0; i < count; i++){
            if(i >= pass.Count)
                tracks.Add(null);
            else
                tracks.Add(pass[i]);
        }

        return tracks;
    }
}