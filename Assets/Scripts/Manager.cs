using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using DG.Tweening;
using TMPro;
using Mirix.DMRV;

public class Manager : Singleton<Manager>
{
#region 인스펙터 입력 변수들
    [SerializeField] public float ToggleAllNoneDelay;

    [Header("Header")]
    [SerializeField] private TextMeshProUGUI TitleText;
    [SerializeField] private Animator TitleStarShine;
    [SerializeField] private TextMeshProUGUI VersionText;

    [Header("Modals")]
    [SerializeField] private GameObject ModalBackdrop;
    [SerializeField] private GameObject ModalInitFail;
    [SerializeField] private TextMeshProUGUI InitFailErrorCode;

    [Header("Body")]
    [SerializeField] private Image         _BG;
    [SerializeField] private Image         _BGPrev;
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
        public GameObject       NoneUIScreen;
        [Space]
        public GameObject       DLCSelectorPrefab;
        public Transform        DLCSelectorParent;
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
        public List<Toggle>     DifficultyParent;
        public LvToggleParent   LvParent;
        public List<Toggle>     DLCs;
        public TextMeshProUGUI  CountLabel;
    }
    [Serializable] public class AchievementUIElements {
        public Sprite[]          ButtonBackgroundSprites;
        public Sprite[]          ButtonTextSprites;
        [Space]
        public Image                        ButtonOptBG;
        public Image                        ButtonOptText;
        public AchievementGradientAnimation ButtonGradientAnimation;
        public Animator                     StarShineAnimator;
        [Space]
        public TextMeshProUGUI   Title;
        public TextMeshProUGUI   Composer;
        public Image             Category;
        public Image             Preview;
        public TMP_InputField    RateField;
        public TMP_InputField    BreakField;
        public Color[]           IndicatorColor;
        public Sprite[]          IndicatorSprite;
        public ButtonAnimation[] Difficulty;
        public LvToggleParent    LvToggleParent;
        public LvToggleParent    LvToggleSCParent;
        public ToggleGroup       StateToggleGroup;
        [Space]
        public TextMeshProUGUI   Averages;
        public TextMeshProUGUI   PerfectPlayRatio;
        public TextMeshProUGUI   MaxComboRatio;
        public TextMeshProUGUI   ClearRatio;
        public RectTransform     PerfectPlayBarChart;
        public RectTransform     MaxComboBarChart;
        public RectTransform     ClearBarChart;
        [Space]
        public Sprite            CreateSprite;
        public Sprite            DeleteSprite;
        [Space]
        public Sprite[]          SpeedOptionSprite;
        public Sprite[]          FeverOptionSprite;
        public Sprite[]          FaderOptionSprite;
        public Sprite[]          ChaosOptionSprite;
        [Space]
        public GameObject        GenerationScreen;
        public Transform         ScrollViewport;
        public Canvas            BoardCanvas;
        public GameObject        LevelDivPrefab;
        public GameObject        FloorListPrefab;
        public GameObject        FloorGridPrefab;
        public GameObject        ListPrefab;
        public GameObject        GridPrefab;
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

    public static readonly string[] ButtonString      = { "4B", "5B", "6B", "8B" };
    public static State AppState                = State.Achievements;
    public static ViewMode BoardViewMode        = ViewMode.List;
    public static Board.Button BoardButton      = Board.Button._4B;

    public static WaitForSeconds              ToggleDelay;
    public static Image                       BG                => inst._BG;
    public static Image                       BGPrev            => inst._BGPrev;
    public static RandomSelectorUIElements    RandomSelectorUI  => inst._RandomSelectorUI;
    public static AchievementUIElements       AchievementUI     => inst._AchievementUI;
    public static CustomLadderMatchUIElements CustomLadderUI    => inst._CustomLadderUI;

    private BoardManager BoardManager;
    private CustomLadderMatch CustomLadderMatch;
    private RandomSelector RandomSelector;
    private void Start() {
        DOTween.SetTweensCapacity(800, 100);
        ToggleDelay = new WaitForSeconds(ToggleAllNoneDelay);

        BoardManager = GetComponent<BoardManager>();
        CustomLadderMatch = GetComponent<CustomLadderMatch>();
        RandomSelector = GetComponent<RandomSelector>();

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;        
        StartCoroutine(Init());
    }

    public void InitFailAccept() {
        Application.Quit();
    }
    private IEnumerator Init() {
        ClearPanel();

        UnityWebRequest webReq = UnityWebRequest.Get("https://req.mirix.kr/dmrv-random/main.bin");
        yield return webReq.SendWebRequest();

        if(webReq.result == UnityWebRequest.Result.ConnectionError || webReq.result == UnityWebRequest.Result.ProtocolError) {
            InitFailErrorCode.text += webReq.error;
            ModalBackdrop.SetActive(true);
            ModalInitFail.SetActive(true);

            yield break;
        }

        MemoryStream ms = new MemoryStream(webReq.downloadHandler.data);
        BinaryFormatter bin = new BinaryFormatter();

        SystemFileIO.MainData = (MainData)bin.Deserialize(ms);
        SystemFileIO.GetLoadingSprite(BG);
        BGPrev.sprite = BG.sprite;

        RandomSelector.DLCs.AddRange(SystemFileIO.MainData.DLCList);
        BoardManager.DLCs.AddRange(SystemFileIO.MainData.DLCList);

        foreach (string dlc in SystemFileIO.MainData.DLCList)
        {
            GameObject selector = Instantiate(RandomSelectorUI.DLCSelectorPrefab, RandomSelectorUI.DLCSelectorParent);
            selector.transform.localScale = Vector3.one;

            GameObject board = Instantiate(RandomSelectorUI.DLCSelectorPrefab, BoardManager.DLCToggleParent);
            board.transform.localScale = Vector3.one;
            
            DLC_Selector sel = selector.GetComponent<DLC_Selector>();
            sel.Toggle.onValueChanged.AddListener((state) => RandomSelector.DLC(state, dlc));
            DLC_Selector bd = board.GetComponent<DLC_Selector>();
            bd.Toggle.onValueChanged.AddListener((state) => BoardManager.DLC(state, dlc));

            RandomSelectorUI.DLCs.Add(sel.Toggle);
            BoardManager.DLCToggles.Add(bd.Toggle);
            
            {
                webReq = UnityWebRequestTexture.GetTexture($"https://req.mirix.kr/dmrv-random/dlc/{dlc}_on.png");
                yield return webReq.SendWebRequest();

                if(webReq.result == UnityWebRequest.Result.ConnectionError || webReq.result == UnityWebRequest.Result.ProtocolError) {
                    print(webReq.error);
                }
                else {
                    Texture2D texture = ((DownloadHandlerTexture)webReq.downloadHandler).texture;

                    Rect rect = new Rect(0, 0, texture.width, texture.height);
                    sel.On.sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
                    bd.On.sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
                }
            }
            {
                webReq = UnityWebRequestTexture.GetTexture($"https://req.mirix.kr/dmrv-random/dlc/{dlc}_off.png");
                yield return webReq.SendWebRequest();

                if(webReq.result == UnityWebRequest.Result.ConnectionError || webReq.result == UnityWebRequest.Result.ProtocolError) {
                    print(webReq.error);
                }
                else {
                    Texture2D texture = ((DownloadHandlerTexture)webReq.downloadHandler).texture;

                    Rect rect = new Rect(0, 0, texture.width, texture.height);
                    sel.Off.sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
                    bd.Off.sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
                }
            }
        }

        SystemFileIO.LoadData();
        VersionText.text = SystemFileIO.MainData.Version;
        OpenAchievement(true);

    }

    public void ChangeApplicationMode() {
        switch(AppState) {
            case State.Achievements:
                AppState = State.Random;
                OpenRandomSelector(true);
                BoardManager.ClearBoard();
                break;
            
            case State.Random:
                AppState = State.Ladder;
                OpenCustomLadder(true);
                break;
            
            case State.Ladder:
                if(CustomLadderMatch.CustomLadderState != CustomLadderMatch.LadderState.Ready)
                    return;
                
                AppState = State.Achievements; OpenAchievement(true);
                break;
        }

        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        TitleText.text = StateTitleString[(int)AppState];
        TitleStarShine.SetTrigger("Shine");
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
        }
        
        BoardManager.OpenAchievement();
        AppState = State.Achievements;
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
            RandomSelector.OpenRandomSelector();
        }
        
        SystemFileIO.GetLoadingSprite(BG);
        BGPrev.sprite = BG.sprite;
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

        SystemFileIO.GetLoadingSprite(BG);
        BGPrev.sprite = BG.sprite;
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