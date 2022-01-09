using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using TMPro;
using DG.Tweening;
using Mirix.DMRV;

public class A_Floor : MonoBehaviour
{
    public TMP_InputField Title;
    public Transform TrackParent;
    public Transform FoldingParent;
    public A_ReorderableList ReorderableList;
    [Space]
    [SerializeField] private Transform FoldIndicator;
    public Button DeleteFloorButton;
    public Button NewTrackButton;
    [Space]
    [SerializeField] private GameObject QualificationField;
    public Button AddQualificationButton;
    public Button DeleteQualificationButton;
    [SerializeField] private GameObject PlayOptionField;
    [SerializeField] private Button[] PlayOptions;
    [SerializeField] private Toggle PlayOptionToggle;
    [SerializeField] private Image PlayOptionToggleImage;
    [SerializeField] private Image SpeedOption;
    [SerializeField] private Image FeverOption;
    [SerializeField] private Image FaderOption;
    [SerializeField] private Image ChaosOption;
    [Space]
    [SerializeField] private Button AddQualificationTierButton;
    [SerializeField] private Button[] TierDeleteButton;
    [Space]
    [SerializeField] private GameObject[] TierInfo;
    [SerializeField] private GameObject[] TierClear;
    [SerializeField] private GameObject[] Rate;
    [SerializeField] private GameObject[] Break;
    [SerializeField] private GameObject[] Additional;
    [SerializeField] private TextMeshProUGUI[] CurrentRate;
    [SerializeField] private TextMeshProUGUI[] CurrentBreak;
    [SerializeField] private TMP_InputField[] RateField;
    [SerializeField] private TMP_InputField[] BreakField;
    [SerializeField] private TMP_InputField[] AdditionalField;

    private bool isFolded = false;
    private bool isEditing = false;
    private bool isCategoryCustom;
    private int TierIndex = 0;
    Board.ButtonData.LvData.FloorData DataReference;
    public void Init(Board.ButtonData.LvData.FloorData reference, Board.Ctgr type) {
        DataReference = reference;
        isCategoryCustom = (type == Board.Ctgr.Custom);
        UpdateQualificationPanel();
    }
    public void OnAchievementUpdate(ushort index) {
        if(DataReference.Tracks.Contains(index)) UpdateTierCurrentValue();
    }
    private void UpdateQualificationPanel() {
        if(DataReference == null) return;
        if(DataReference.Qualification == null) {
            QualificationField.SetActive(false);
            return;
        }
        else QualificationField.SetActive(true);
        
        UpdatePlayOptionPanel();
        UpdateTierPanel();
    }
    private void UpdatePlayOptionPanel() {
        if(DataReference == null || DataReference.Qualification == null) return;

        if(isEditing) PlayOptionField.SetActive(true);
        else if(DataReference.Qualification.PlayOption != null) PlayOptionField.SetActive(true);
        else PlayOptionField.SetActive(false);

        if(DataReference.Qualification.PlayOption != null) {
            SpeedOption.sprite = Manager.AchievementUI.SpeedOptionSprite[DataReference.Qualification.PlayOption.Speed];
            FeverOption.sprite = Manager.AchievementUI.FeverOptionSprite[(int)DataReference.Qualification.PlayOption.Fever];
            FaderOption.sprite = Manager.AchievementUI.FaderOptionSprite[(int)DataReference.Qualification.PlayOption.Fader];
            ChaosOption.sprite = Manager.AchievementUI.ChaosOptionSprite[(int)DataReference.Qualification.PlayOption.Chaos];
        }
    }
    private void UpdateTierPanel() {
        foreach(GameObject o in TierInfo) o.SetActive(false);
        foreach(GameObject o in TierClear) o.SetActive(false);
        foreach(GameObject o in Rate) o.SetActive(false);
        foreach(GameObject o in Break) o.SetActive(false);
        foreach(GameObject o in Additional) o.SetActive(false);

        UpdateTierCurrentValue();
        if(DataReference == null || DataReference.Qualification == null) return;

        if(DataReference.Qualification.QualificationTier == null)
            DataReference.Qualification.QualificationTier = new List<Board.ButtonData.LvData.FloorData.QualificationTierData>();
        else
        for(int i = 0; i < DataReference.Qualification.QualificationTier.Count; i++) {
            var Q = DataReference.Qualification.QualificationTier[i];

            TierInfo[i].SetActive(true);
            if(Q.Rate.HasValue) {
                RateField[i].text = string.Format("{0:0.00}", Q.Rate.Value);
                Rate[i].SetActive(true);
            }
            else if(isEditing) {
                RateField[i].text = "";
                Rate[i].SetActive(true);
            }
            else Rate[i].SetActive(false);

            if(Q.Break.HasValue) {
                BreakField[i].text = Q.Break.Value.ToString();
                Break[i].SetActive(true);
            }
            else if(isEditing) {
                BreakField[i].text = "";
                Break[i].SetActive(true);
            }
            else Break[i].SetActive(false);
            
            if(isEditing) {
                AdditionalField[i].text = "";
                Additional[i].SetActive(true);
            }
            else if(string.IsNullOrWhiteSpace(Q.Additional)) Additional[i].SetActive(false);
            else {
                AdditionalField[i].text = Q.Additional;
                Additional[i].SetActive(true);
            }

            if(i < TierDeleteButton.Length)
                TierDeleteButton[i]?.gameObject.SetActive(isEditing);
        }

        if(gameObject.activeInHierarchy)
            StartCoroutine(RebuildLayout(true));
    }
    private void UpdateTierCurrentValue() {
        if(DataReference?.Qualification?.QualificationTier == null) return;

        for(int i = 0; i < DataReference.Qualification.QualificationTier.Count; i++) {
            var Q = DataReference.Qualification.QualificationTier[i];
            
            bool qualified = true;

            if(i < CurrentRate.Length && CurrentRate[i] != null) {
                var L = DataReference.Tracks.Where(t => SystemFileIO.GetAchievementSave(t).Status != Achievement.State.None);
                if(L.Count() > 0) {
                    float avgRate = L.Average(t => SystemFileIO.GetAchievementSave(t).Rate);
                    CurrentRate[i].text = string.Format("{0:0.00}%", avgRate);
                    qualified &= !Q.Rate.HasValue || (avgRate >= Q.Rate.Value);
                }
                else {
                    CurrentRate[i].text = "0.00%";
                    qualified = false;
                }
            } else qualified = false;
            if(i < CurrentBreak.Length && CurrentBreak[i] != null) {
                int sumBreak = DataReference.Tracks.Sum(t => SystemFileIO.GetAchievementSave(t).Break);
                CurrentBreak[i].text = sumBreak.ToString();

                qualified &= !Q.Break.HasValue || (sumBreak <= Q.Break.Value);
            } else qualified = false;

            TierClear[i].SetActive(!qualified);
        }
    }
    public void SetPlayOption(bool state) {
        if(state) {
            PlayOptionToggleImage.sprite = Manager.AchievementUI.DeleteSprite;
            foreach(Button o in PlayOptions) o.gameObject.SetActive(true);

            DataReference.Qualification.PlayOption = new Board.ButtonData.LvData.FloorData.PlayOptionData();
        }
        else {
            PlayOptionToggleImage.sprite = Manager.AchievementUI.CreateSprite;
            foreach(Button o in PlayOptions) o.gameObject.SetActive(false);

            DataReference.Qualification.PlayOption = null;
        }
        UpdatePlayOptionPanel();
    }
    public void AddQualificationTier() {
        DataReference.Qualification.QualificationTier.Add(new Board.ButtonData.LvData.FloorData.QualificationTierData());
        if(DataReference.Qualification.QualificationTier.Count >= 3) {
            AddQualificationTierButton.gameObject.SetActive(false);
        }
        UpdateTierPanel();
    }
    public void AddQuailfication() {
        DataReference.Qualification = new Board.ButtonData.LvData.FloorData.QualificationData();
        UpdateQualificationPanel();
        AddQualificationButton.gameObject.SetActive(false);
    }
    public void DeleteQualification() {
        DataReference.Qualification = null;
        UpdateQualificationPanel();
        AddQualificationButton.gameObject.SetActive(true);
    }
    public void SetTargetTierIndex(int index) {
        TierIndex = index;
    }
    public void DeleteQualificationTier() {
        DataReference.Qualification.QualificationTier.RemoveAt(TierIndex);
        AddQualificationTierButton.gameObject.SetActive(true);
        UpdateTierPanel();
    }
    public void SetQualificationTierRate(string input) {
        if(float.TryParse(input, out float r)) {
            RateField[TierIndex].text = string.Format("{0:0.00}", r);
            DataReference.Qualification.QualificationTier[TierIndex].Rate = r;
        }
        else {
            RateField[TierIndex].text = "";
            DataReference.Qualification.QualificationTier[TierIndex].Rate = null;
        }

        UpdateTierCurrentValue();
    }
    public void SetQualificationTierBreak(string input) {
        if(UInt16.TryParse(input, out ushort b)) {
            BreakField[TierIndex].text = b.ToString();
            DataReference.Qualification.QualificationTier[TierIndex].Break = b;
        }
        else {
            BreakField[TierIndex].text = "";
            DataReference.Qualification.QualificationTier[TierIndex].Break = null;
        }

        UpdateTierCurrentValue();
    }
    public void SetQualificationTierAdditional(string input) {
        DataReference.Qualification.QualificationTier[TierIndex].Additional = input;
    }
    public void SetFloorName(string input) {
        DataReference.Name = input;
    }

    public void SetEditMode(bool state) {
        isEditing = state;

        DeleteFloorButton?.gameObject.SetActive(state);
        NewTrackButton?.gameObject.SetActive(state);
        ReorderableList.SetState(state);

        PlayOptionToggle?.gameObject.SetActive(state);
        DeleteQualificationButton?.gameObject.SetActive(state);
        AddQualificationButton?.gameObject.SetActive(DataReference.Qualification == null && state);
        AddQualificationTierButton?.gameObject.SetActive((DataReference.Qualification?.QualificationTier == null || DataReference.Qualification.QualificationTier.Count < 3)
                                                      && state);
        Title.interactable = isCategoryCustom && state;

        foreach(TMP_InputField i in RateField) i.interactable = state;
        foreach(TMP_InputField i in BreakField) i.interactable = state;
        foreach(TMP_InputField i in AdditionalField) i.interactable = state;
        foreach(Button b in PlayOptions) b.interactable = state;
        foreach(Button b in TierDeleteButton) b.interactable = state;

        if(gameObject.activeInHierarchy)
            StartCoroutine(RebuildLayout(true));
        
        UpdateQualificationPanel();
    }
    public void FilterCheckEmpty(bool isFilter) {
        gameObject.SetActive(!isFilter || TrackParent.ChildCountActive() != 0);
    }
#if !UNITY_WEBGL
    private void OnDestroy() {
        BoardManager.FilterFloorEvent -= new BoardManager.FilterCallback(this.FilterCheckEmpty);
        BoardManager.BoardEditEvent -= new BoardManager.BoardEditCall(this.SetEditMode);
        BoardManager.AchievementUpdateEvent -= new BoardManager.AchievementUpdateCall(this.OnAchievementUpdate);
    }
#endif

    public void FoldToggle() {
        isFolded ^= true;
        FoldingParent.gameObject.SetActive(!isFolded);

        if(isFolded)
            FoldIndicator.DORotate(new Vector3(0, 0, 180), 0.3f);
        else
            FoldIndicator.DORotate(Vector3.zero, 0.3f);
        
        if(gameObject.activeInHierarchy)
            StartCoroutine(RebuildLayout(true));
    }
    public void OnReorder() {
        if(gameObject.activeInHierarchy)
            StartCoroutine(RebuildLayout());
    }
    private IEnumerator RebuildLayout(bool immediately = false) {
        foreach(RectTransform t in TrackParent) LayoutRebuilder.ForceRebuildLayoutImmediate(t);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)TrackParent);
        if(!immediately)
            yield return null;
        foreach(RectTransform t in FoldingParent) LayoutRebuilder.ForceRebuildLayoutImmediate(t);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)FoldingParent);
        if(!immediately)
            yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        if(transform.parent != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent);
        if(transform.parent?.parent != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent.parent);
        yield break;
    }

    public void SpeedUp() {
        if(!isEditing) return;

        DataReference.Qualification.PlayOption.Speed++;
        if(DataReference.Qualification.PlayOption.Speed >= Manager.AchievementUI.SpeedOptionSprite.Length) DataReference.Qualification.PlayOption.Speed = 0;

        UpdatePlayOptionPanel();
    }
    public void SpeedDown() {
        if(!isEditing) return;

        DataReference.Qualification.PlayOption.Speed--;
        if(DataReference.Qualification.PlayOption.Speed < 0) DataReference.Qualification.PlayOption.Speed = Manager.AchievementUI.SpeedOptionSprite.Length -1;

        UpdatePlayOptionPanel();
    }
    public void FeverUp() {
        if(!isEditing) return;

        int x = ((int)DataReference.Qualification.PlayOption.Fever) + 1;
        if(x >= Manager.AchievementUI.FeverOptionSprite.Length) x = 0;
        DataReference.Qualification.PlayOption.Fever = (Board.ButtonData.LvData.FloorData.PlayOptionData.FeverOption)x;

        UpdatePlayOptionPanel();
    }
    public void FeverDown() {
        if(!isEditing) return;

        int x = ((int)DataReference.Qualification.PlayOption.Fever) - 1;
        if(x < 0) x = Manager.AchievementUI.FeverOptionSprite.Length - 1;
        DataReference.Qualification.PlayOption.Fever = (Board.ButtonData.LvData.FloorData.PlayOptionData.FeverOption)x;

        UpdatePlayOptionPanel();
    }
    public void FaderUp() {
        if(!isEditing) return;

        int x = ((int)DataReference.Qualification.PlayOption.Fader) + 1;
        if(x >= Manager.AchievementUI.FaderOptionSprite.Length) x = 0;
        DataReference.Qualification.PlayOption.Fader = (Board.ButtonData.LvData.FloorData.PlayOptionData.FaderOption)x;

        UpdatePlayOptionPanel();
    }
    public void FaderDown() {
        if(!isEditing) return;

        int x = ((int)DataReference.Qualification.PlayOption.Fader) - 1;
        if(x < 0) x = Manager.AchievementUI.FaderOptionSprite.Length - 1;
        DataReference.Qualification.PlayOption.Fader = (Board.ButtonData.LvData.FloorData.PlayOptionData.FaderOption)x;

        UpdatePlayOptionPanel();
    }
    public void ChaosUp() {
        if(!isEditing) return;

        int x = ((int)DataReference.Qualification.PlayOption.Chaos) + 1;
        if(x >= Manager.AchievementUI.ChaosOptionSprite.Length) x = 0;
        DataReference.Qualification.PlayOption.Chaos = (Board.ButtonData.LvData.FloorData.PlayOptionData.ChaosOption)x;

        UpdatePlayOptionPanel();
    }
    public void ChaosDown() {
        if(!isEditing) return;
        
        int x = ((int)DataReference.Qualification.PlayOption.Chaos) - 1;
        if(x < 0) x = Manager.AchievementUI.ChaosOptionSprite.Length - 1;
        DataReference.Qualification.PlayOption.Chaos = (Board.ButtonData.LvData.FloorData.PlayOptionData.ChaosOption)x;

        UpdatePlayOptionPanel();
    }
}
