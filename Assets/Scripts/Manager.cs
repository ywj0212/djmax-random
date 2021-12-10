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

public class Manager : Singleton<Manager>
{
#region 인스펙터 입력 변수들
    [SerializeField] private float ToggleAllNoneDelay;

    [Header("Header")]
    [SerializeField] private TextMeshProUGUI TitleText;
    [SerializeField] private Animator TitleStarShine;

    [Header("Body")]
    // [SerializeField] private Canvas Canvas;
    [SerializeField] private RectTransform CanvasRect;
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
    [SerializeField] private RandomSelectorUIElements    _RandomSelectorUI;
    [SerializeField] private AchievementUIElements       _AchievementUI;
    [SerializeField] private CustomLadderMatchUIElements _CustomLadderUI;

    [Serializable] public class RandomSelectorUIElements {
        public GameObject       DefaultUIScreen;
        public GameObject       SingleUIScreen;
        [Space]
        public Image            SingleThumbnail;
        public Image            SingleCategory;
        public Image            SingleButtonAndDiff;
        public TextMeshProUGUI  SingleTitle;
        public TextMeshProUGUI  SingleComposer;
        public LvToggleParent   SingleLvParent;
        public LvToggleParent   SingleLvSCParent;
        [Space]
        public Transform        ResultListParent;
        public GameObject       ResultListPrefab;
        [Space]
        public Toggle[]         DifficultyParent;
        public LvToggleParent   LvParent;
        public Toggle[]         DLCs;
        public TextMeshProUGUI  CountLabel;
    }
    [Serializable] public class AchievementUIElements {
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
        public LvToggleParent   LvToggleParent;
        public LvToggleParent   LvToggleSCParent;
        public ToggleGroup      StateToggleGroup;
        [Space]
        public Transform        ScrollViewport;
        public GameObject       LevelDivPrefab;
        public GameObject       FloorListPrefab;
        public GameObject       FloorGridPrefab;
        public GameObject       ListPrefab;
        public GameObject       GridPrefab;
    }
    [Serializable] public class CustomLadderMatchUIElements {
        public Transform        BanPickScrollViewport;
        public Transform        RoundScrollViewport;
        [Space]
        public ScrollRect       BanPickScrollRect;
        public ScrollRect       RoundScrollRect;
        public Transform        BanPickGridParent;
        public Transform        RoundVerticalParent;
        [Space]
        public GameObject       BanPickPrefab;
        public GameObject       RoundPrefab;
        public GameObject       RoundTrackAllUsedPrefab;
    }
#endregion

    public enum State                           { Achievements, Random, Ladder }
    private readonly string[] StateTitleString  = { "Achievements", "Random Selector", "Custom Ladder" };
    public enum ViewMode                        { List, Grid }

    private readonly string[] ButtonString      = { "4B", "5B", "6B", "8B" };
    public static State AppState                = State.Achievements;
    public static ViewMode BoardViewMode        = ViewMode.List;
    public static Board.Button BoardButton      = Board.Button._4B;

    public static WaitForSeconds              ToggleDelay;
    public static RandomSelectorUIElements    RandomSelectorUI  => inst._RandomSelectorUI;
    public static AchievementUIElements       AchievementUI     => inst._AchievementUI;
    public static CustomLadderMatchUIElements CustomLadderUI    => inst._CustomLadderUI;

    private BoardManager BoardManager;
    private CustomLadderMatch CustomLadderMatch;
    private RandomSelector RandomSelector;
    private void Start() {
        ToggleDelay = new WaitForSeconds(ToggleAllNoneDelay);

        BoardManager = GetComponent<BoardManager>();
        CustomLadderMatch = GetComponent<CustomLadderMatch>();
        RandomSelector = GetComponent<RandomSelector>();

        ClearPanel();
        OpenAchievement(true);
    }

    // ! public void SetSiblingIndex(int index); 이용하여 자식 객체 순서 변경 가능
    public void ChangeApplicationMode() {
        switch(AppState) {
            case State.Achievements: AppState = State.Random;       OpenRandomSelector(true); break;
            case State.Random:       AppState = State.Ladder;       OpenCustomLadder(true);   break;
            case State.Ladder:       if(CustomLadderMatch.CustomLadderState != CustomLadderMatch.LadderState.Ready) return;
                                     AppState = State.Achievements; OpenAchievement(true);    break;
        }

        TitleText.text = StateTitleString[(int)AppState];
        TitleStarShine.SetTrigger("Shine");
    }

    public static string GetCategoryFullName(string abbr) {
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
    public static void ClearPanel() { inst.ClearPanelTween(); }
    private void ClearPanelTween() {
        SelectorOptionPanel.DOMoveX(-GetRectTransformWidth(SelectorOptionPanel), 0f).SetEase(Ease.InOutCirc);
        RandomResultPanel.DOMoveX(GetRectTransformWidth(BodyRect) + GetRectTransformWidth(RandomResultPanel), 0f).SetEase(Ease.InOutCirc);
        CustomLadderReadyPanel.DOMoveX(GetRectTransformWidth(BodyRect) + GetRectTransformWidth(CustomLadderReadyPanel), 0f).SetEase(Ease.InOutCirc);
        CustomLadderBanPickPanel.DOMoveY(-GetRectTransformHeight(CustomLadderBanPickPanel), 0f).SetEase(Ease.InOutCirc);
        CustomLadderMainPanel.DOMoveY(-GetRectTransformHeight(CustomLadderMainPanel), 0f).SetEase(Ease.InOutCirc);
        AchievementInfoPanel.DOMoveX(-GetRectTransformWidth(AchievementInfoPanel), 0f).SetEase(Ease.InOutCirc);
        AchievementResultPanel.DOMoveX(GetRectTransformWidth(BodyRect), 0f).SetEase(Ease.InOutCirc);
    }

    public static int? GetSelectedToggleIndex(ToggleGroup toggleGroup) {
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
    public static void ApplySelectionToToggleGroup(ToggleGroup toggleGroup, int index) {
        toggleGroup.SetAllTogglesOff();
        Toggle[] toggles = toggleGroup.GetComponentsInChildren<Toggle>();
        if(index < 0 || index >= toggles.Length)
            return;
        
        toggles[index].isOn = true;
    }
    public static float GetRectTransformWidth(RectTransform rect) {
        float wRatio = Screen.width  / inst.MainCanvasScaler.referenceResolution.x;
        float hRatio = Screen.height / inst.MainCanvasScaler.referenceResolution.y;
        float ratio = Mathf.Min(wRatio, hRatio);
        return rect.rect.width * ratio;
    }
    public static float GetRectTransformHeight(RectTransform rect) {
        float wRatio = Screen.width  / inst.MainCanvasScaler.referenceResolution.x;
        float hRatio = Screen.height / inst.MainCanvasScaler.referenceResolution.y;
        float ratio = Mathf.Min(wRatio, hRatio);
        return rect.rect.height * ratio;
    }

    Coroutine LoadingAchievement;
    public static void OpenAchievement(bool isAppStateChanged = false) { inst._OpenAchievement(isAppStateChanged); }
    public static void OpenRandomSelector(bool isAppStateChanged = false) { inst._OpenRandomSelector(isAppStateChanged); }
    public static void OpenCustomLadder(bool isAppStateChanged = false) { inst._OpenCustomLadder(isAppStateChanged); }
    public void _OpenAchievement(bool isAppStateChanged = false) {
        if(isAppStateChanged) {
            SelectorOptionPanel.DOMoveX(-GetRectTransformWidth(SelectorOptionPanel), 0.5f).SetEase(Ease.InOutCirc);
            RandomResultPanel.DOMoveX(GetRectTransformWidth(BodyRect) + GetRectTransformWidth(RandomResultPanel), 0.5f).SetEase(Ease.InOutCirc);
            CustomLadderReadyPanel.DOMoveX(GetRectTransformWidth(BodyRect) + GetRectTransformWidth(CustomLadderReadyPanel), 0.5f).SetEase(Ease.InOutCirc);
            CustomLadderBanPickPanel.DOMoveY(-GetRectTransformHeight(CustomLadderBanPickPanel), 0.5f).SetEase(Ease.InOutCirc);
            CustomLadderMainPanel.DOMoveY(-GetRectTransformHeight(CustomLadderMainPanel), 0.5f).SetEase(Ease.InOutCirc);
            AchievementInfoPanel.DOMoveX(0, 0.5f).SetEase(Ease.InOutCirc);
            AchievementResultPanel.DOMoveX(GetRectTransformWidth(BodyRect), 0.5f).SetEase(Ease.InOutCirc);
            // PanelUpdate();
        }
        
        if(LoadingAchievement != null)
            StopCoroutine(LoadingAchievement);
        
        AppState = State.Achievements;
        LoadingAchievement = StartCoroutine(BoardManager.OpenAchievementRoutine());
    }
    public void _OpenRandomSelector(bool isAppStateChanged = false) {
        if(isAppStateChanged) {
            SelectorOptionPanel.DOMoveX(0, 0.5f).SetEase(Ease.InOutCirc);
            RandomResultPanel.DOMoveX(GetRectTransformWidth(BodyRect), 0.5f).SetEase(Ease.InOutCirc);
            CustomLadderReadyPanel.DOMoveX(GetRectTransformWidth(BodyRect) + GetRectTransformWidth(CustomLadderReadyPanel), 0.5f).SetEase(Ease.InOutCirc);
            CustomLadderBanPickPanel.DOMoveY(-GetRectTransformHeight(CustomLadderBanPickPanel), 0.5f).SetEase(Ease.InOutCirc);
            CustomLadderMainPanel.DOMoveY(-GetRectTransformHeight(CustomLadderMainPanel), 0.5f).SetEase(Ease.InOutCirc);
            AchievementInfoPanel.DOMoveX(-GetRectTransformWidth(AchievementInfoPanel), 0.5f).SetEase(Ease.InOutCirc);
            AchievementResultPanel.DOMoveX(GetRectTransformWidth(BodyRect) + GetRectTransformWidth(AchievementResultPanel), 0.5f).SetEase(Ease.InOutCirc);

            RandomSelectorUI.DefaultUIScreen.SetActive(true);
            RandomSelectorUI.SingleUIScreen.SetActive(false);
            foreach(Transform t in RandomSelectorUI.ResultListParent) {
                Destroy(t.gameObject);
            }   
        }
    }
    public void _OpenCustomLadder(bool isAppStateChanged = false) {
        if(isAppStateChanged) {
            SelectorOptionPanel.DOMoveX(0, 0.5f).SetEase(Ease.InOutCirc);
            RandomResultPanel.DOMoveX(GetRectTransformWidth(BodyRect) + GetRectTransformWidth(RandomResultPanel), 0.5f).SetEase(Ease.InOutCirc);
            CustomLadderReadyPanel.DOMoveX(GetRectTransformWidth(BodyRect), 0.5f).SetEase(Ease.InOutCirc);
            CustomLadderBanPickPanel.DOMoveY(-GetRectTransformHeight(CustomLadderBanPickPanel), 0.5f).SetEase(Ease.InOutCirc);
            CustomLadderMainPanel.DOMoveY(-GetRectTransformHeight(CustomLadderMainPanel), 0.5f).SetEase(Ease.InOutCirc);
            AchievementInfoPanel.DOMoveX(-GetRectTransformWidth(AchievementInfoPanel), 0.5f).SetEase(Ease.InOutCirc);
            AchievementResultPanel.DOMoveX(GetRectTransformWidth(BodyRect) + GetRectTransformWidth(AchievementResultPanel), 0.5f).SetEase(Ease.InOutCirc);
        }

        CustomLadderMatch.CustomLadderState = CustomLadderMatch.LadderState.Ready;
    }
    
    public static void OpenCustomLadderBanPickPanel() { inst._OpenCustomLadderBanPickPanel(); }
    public void _OpenCustomLadderBanPickPanel() {
        SelectorOptionPanel.DOMoveX(-GetRectTransformWidth(SelectorOptionPanel), 0.5f).SetEase(Ease.InOutCirc);
        CustomLadderReadyPanel.DOMoveX(GetRectTransformWidth(BodyRect) + GetRectTransformWidth(CustomLadderReadyPanel), 0.5f).SetEase(Ease.InOutCirc);
        CustomLadderBanPickPanel.DOMoveY(0, 0.5f).SetEase(Ease.InOutCirc);
    }
    public static void CloseCustomLadderBanPickPanel() { inst._CloseCustomLadderBanPickPanel(); }
    public void _CloseCustomLadderBanPickPanel() {
        SelectorOptionPanel.DOMoveX(0, 0.5f).SetEase(Ease.InOutCirc);
        CustomLadderReadyPanel.DOMoveX(GetRectTransformWidth(BodyRect), 0.5f).SetEase(Ease.InOutCirc);

        CustomLadderBanPickPanel.DOMoveY(-GetRectTransformHeight(CustomLadderBanPickPanel), 0.5f).SetEase(Ease.InOutCirc);
        CustomLadderMainPanel.DOMoveY(-GetRectTransformHeight(CustomLadderMainPanel), 0.5f).SetEase(Ease.InOutCirc);
    }
    public static void OpenCustomLadderMatchPanel() { inst._OpenCustomLadderMatchPanel(); }
    public void _OpenCustomLadderMatchPanel() {
        CustomLadderBanPickPanel.DOMoveY(-Manager.GetRectTransformHeight(CustomLadderBanPickPanel), 0.5f).SetEase(Ease.InOutCirc);
        CustomLadderMainPanel.DOMoveY(0, 0.5f).SetEase(Ease.InOutCirc);
    }
    public static void CloseCustomLadderMatchPanel() { inst._CloseCustomLadderMatchPanel(); }
    public void _CloseCustomLadderMatchPanel() {
        SelectorOptionPanel.DOMoveX(0, 0.5f).SetEase(Ease.InOutCirc);
        CustomLadderReadyPanel.DOMoveX(Manager.GetRectTransformWidth(BodyRect), 0.5f).SetEase(Ease.InOutCirc);
        CustomLadderBanPickPanel.DOMoveY(-Manager.GetRectTransformHeight(CustomLadderBanPickPanel), 0.5f).SetEase(Ease.InOutCirc);
        CustomLadderMainPanel.DOMoveY(-Manager.GetRectTransformHeight(CustomLadderMainPanel), 0.5f).SetEase(Ease.InOutCirc);
    }

    public void _StartBtn() {
        switch(AppState) {
            case State.Random: RandomSelector.ShowRandomSelectorResult(); break;
            case State.Ladder: CustomLadderMatch.CustomLadderBanPick();   break;
        }
    }
}