using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using DG.Tweening;
using Mirix.DMRV;

public class CustomLadderMatch : MonoBehaviour
{
    [SerializeField] private Button MatchStartButton;

    public enum LadderState { Ready, BanPick, Main }
    [HideInInspector] public LadderState CustomLadderState = LadderState.Ready;

    private int CustomLadderRound = 1;
    private MainData.TrackInfo? PreviousRoundTrackInfo;
    private List<MainData.TrackInfo> CustomLadderAllTracks = new List<MainData.TrackInfo>();
    private List<MainData.TrackInfo> CustomLadderPickedTracksPool = new List<MainData.TrackInfo>();
    private List<MainData.TrackInfo> CustomLadderPickedUsedTracks = new List<MainData.TrackInfo>();
    private List<bool> CustomLadderBanResult = new List<bool>();

    public void CustomLadderBanPick() {
        CustomLadderState = LadderState.BanPick;
        foreach(Transform t in Manager.CustomLadderUI.BanPickGridParent)
            Destroy(t.gameObject);

        CustomLadderPickedTracksPool.Clear();
        CustomLadderPickedUsedTracks.Clear();
        CustomLadderBanResult.Clear();
        CustomLadderAllTracks = RandomSelector.GetTracks();

        int index = 0;
        foreach(MainData.TrackInfo TrackData in CustomLadderAllTracks) {

            GameObject BanPick = Instantiate(Manager.CustomLadderUI.BanPickPrefab, Vector3.zero, Quaternion.identity, Manager.CustomLadderUI.BanPickGridParent);
            BanPick.transform.localScale = Vector3.one;

            L_BanPickTrack LBPT = BanPick.GetComponent<L_BanPickTrack>();
            MainData.SongInfo SongData = SystemFileIO.GetSongData(TrackData.SongIndex);

            LBPT.Title.text = SongData.Name;
            LBPT.Category.text = SystemFileIO.GetCategoryFullName(SongData.Ctgr);
            SystemFileIO.GetThumbnailSprite(LBPT.Thumbnail, TrackData.SongIndex);
            LBPT.Difficulty.sprite = SystemFileIO.GetDifficultySprite(TrackData.Bt, TrackData.Diff);
            
            if(TrackData.Diff == "SC") {
                LBPT.LvToggleParent.gameObject.SetActive(false);
                LBPT.LvToggleSCParent.gameObject.SetActive(true);

                for(int i = 0; i < TrackData.Lv; i++)
                    LBPT.LvToggleSCParent.Toggles[i].isOn = true;
            }
            else {
                LBPT.LvToggleParent.gameObject.SetActive(true);
                LBPT.LvToggleSCParent.gameObject.SetActive(false);

                for(int i = 0; i < TrackData.Lv; i++)
                    LBPT.LvToggleParent.Toggles[i].isOn = true;
            }

            int _index = index;
            LBPT.Toggle.onValueChanged.AddListener((x) => {CustomLadderBanPickCallback(_index, x);});
            CustomLadderBanResult.Add(true);
            index++;
        }

        Manager.OpenCustomLadderBanPickPanel();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Manager.CustomLadderUI.BanPickScrollRect.transform);
    }
    public void CustomLadderBanPickCallback(int index, bool enable) {
        CustomLadderBanResult[index] = enable;

        if(CustomLadderBanResult.TrueForAll(b => !b))
            MatchStartButton.interactable = false;
        else
            MatchStartButton.interactable = true;
    }
    public void CustomLadderBanPickCancel() {
        CustomLadderState = LadderState.Ready;
        Manager.CloseCustomLadderBanPickPanel();
    }
    public void CustomLadderMatchStart() {
        CustomLadderState = LadderState.Main;
        CustomLadderRound = 1;
        foreach(Transform t in Manager.CustomLadderUI.RoundVerticalParent)
            Destroy(t.gameObject);

        for(int i = 0; i < CustomLadderBanResult.Count; i++)
            if(CustomLadderBanResult[i]) CustomLadderPickedTracksPool.Add(CustomLadderAllTracks[i]);
        
        Manager.OpenCustomLadderMatchPanel();
        CustomLadderNextRound();
    }
    public void CustomLadderNextRound() {
        if(CustomLadderPickedTracksPool.Count == 0) {
            GameObject Alert = Instantiate(Manager.CustomLadderUI.RoundTrackAllUsedPrefab, Vector3.zero, Quaternion.identity, Manager.CustomLadderUI.RoundVerticalParent);
            Alert.transform.localScale = Vector3.one;
            CustomLadderPickedTracksPool.AddRange(CustomLadderPickedUsedTracks);
            CustomLadderPickedUsedTracks.Clear();
        }

        int SelectedIndex;
        do 
            SelectedIndex = UnityEngine.Random.Range(0, CustomLadderPickedTracksPool.Count);
        while(PreviousRoundTrackInfo.HasValue && CustomLadderPickedTracksPool.Count > 1 && PreviousRoundTrackInfo.Value.Equals(CustomLadderPickedTracksPool[SelectedIndex]));
        MainData.TrackInfo TrackData = CustomLadderPickedTracksPool[SelectedIndex];
        PreviousRoundTrackInfo = TrackData;
        
        CustomLadderPickedTracksPool.RemoveAt(SelectedIndex);
        CustomLadderPickedUsedTracks.Add(TrackData);

        GameObject Round = Instantiate(Manager.CustomLadderUI.RoundPrefab, Vector3.zero, Quaternion.identity, Manager.CustomLadderUI.RoundVerticalParent);
        Round.transform.localScale = Vector3.one;

        L_Round LR = Round.GetComponent<L_Round>();
        MainData.SongInfo SongData = SystemFileIO.GetSongData(TrackData.SongIndex);

        LR.RoundTitle.text = $"ROUND {CustomLadderRound}";
        LR.Title.text = SongData.Name;
        SystemFileIO.GetThumbnailSprite(LR.Thumbnail, TrackData.SongIndex);
        LR.Difficulty.sprite = SystemFileIO.GetDifficultySprite(TrackData.Bt, TrackData.Diff);

        if(TrackData.Diff == "SC") {
            LR.LvToggleParent.gameObject.SetActive(false);
            LR.LvToggleSCParent.gameObject.SetActive(true);

            for(int i = 0; i < TrackData.Lv; i++)
                LR.LvToggleSCParent.Toggles[i].isOn = true;
        }
        else {
            LR.LvToggleParent.gameObject.SetActive(true);
            LR.LvToggleSCParent.gameObject.SetActive(false);

            for(int i = 0; i < TrackData.Lv; i++)
                LR.LvToggleParent.Toggles[i].isOn = true;
        }

        DOTween.To(() => Manager.CustomLadderUI.RoundScrollRect.verticalNormalizedPosition, x => Manager.CustomLadderUI.RoundScrollRect.verticalNormalizedPosition = x, 0f, 0.3f).SetDelay(0.3f);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Round.transform);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Manager.CustomLadderUI.RoundVerticalParent);
        CustomLadderRound++;
    }
    public void CustomLadderMatchEnd() {
        CustomLadderState = LadderState.Ready;
        Manager.CloseCustomLadderMatchPanel();
    }
}
