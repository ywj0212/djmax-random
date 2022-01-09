using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using TMPro;
using Mirix.DMRV;

public class RandomSelector : MonoBehaviour
{
    private static bool    _4b = true, _5b = true, _6b = true, _8b = true,
                           _nm = true, _hd = true, _mx = true, _sc = true,
                           _1 = true, _2 = true, _3 = true, _4 = true, _5 = true,
                           _6 = true, _7 = true, _8 = true, _9 = true, _10 = true,
                           _11 = true, _12 = true, _13 = true, _14 = true, _15 = true;
    
    public static List<string> DLCs = new List<string>();
    private static int _Count = 1;

    public void OpenRandomSelector() {
        foreach(Transform t in Manager.RandomSelectorUI.ResultListParent)
            Destroy(t.gameObject);
    }
    public void ShowRandomSelectorResult() {
        Manager.RandomSelectorUI.DefaultUIScreen.SetActive(false);
        foreach(Transform t in Manager.RandomSelectorUI.ResultListParent)
            Destroy(t.gameObject);

        List<MainData.TrackInfo> Tracks = GetTracks(count:_Count);
        if(Tracks.Count > 1) {
            Manager.RandomSelectorUI.SingleUIScreen.SetActive(false);
            Manager.RandomSelectorUI.NoneUIScreen.SetActive(false);

            foreach(MainData.TrackInfo TrackData in Tracks) {
                GameObject ResultList = Instantiate(Manager.RandomSelectorUI.ResultListPrefab, Manager.RandomSelectorUI.ResultListParent);
                ResultList.transform.localScale = Vector3.one;

                R_SelectedTrack RST = ResultList.GetComponent<R_SelectedTrack>();
                MainData.SongInfo SongData = SystemFileIO.GetSongData(TrackData.SongIndex);

                SystemFileIO.GetThumbnailSprite(RST.Thumbnail, TrackData.SongIndex);
                RST.Difficulty.sprite = SystemFileIO.GetDifficultySprite(TrackData.Bt, TrackData.Diff);
                SystemFileIO.GetCategorySprite(RST.Category, SongData.Ctgr);
                RST.Title.text        = SongData.Name;
                RST.Composer.text     = SongData.Cmps;

                if(TrackData.Diff == "SC") {
                    RST.LvToggleParent.gameObject.SetActive(false);
                    RST.LvToggleSCParent.gameObject.SetActive(true);

                    for(int i = 0; i < TrackData.Lv; i++) RST.LvToggleSCParent.Toggles[i].isOn = true;
                }
                else {
                    RST.LvToggleParent.gameObject.SetActive(true);
                    RST.LvToggleSCParent.gameObject.SetActive(false);

                    for(int i = 0; i < TrackData.Lv; i++) RST.LvToggleParent.Toggles[i].isOn = true;
                }
            }
        }
        else if(Tracks.Count == 1) {
            Manager.RandomSelectorUI.SingleUIScreen.SetActive(true);
            Manager.RandomSelectorUI.NoneUIScreen.SetActive(false);

            MainData.TrackInfo TrackData = Tracks[0];
            MainData.SongInfo SongData = SystemFileIO.GetSongData(TrackData.SongIndex);

            SystemFileIO.GetCategorySprite(Manager.RandomSelectorUI.SingleCategory, SongData.Ctgr);
            SystemFileIO.GetThumbnailSprite(Manager.RandomSelectorUI.SingleThumbnail, TrackData.SongIndex);
            Manager.RandomSelectorUI.SingleButtonAndDiff.sprite = SystemFileIO.GetDifficultySprite(TrackData.Bt, TrackData.Diff);
            Manager.RandomSelectorUI.SingleTitle.text = SongData.Name;
            Manager.RandomSelectorUI.SingleComposer.text = SongData.Cmps;

            if(TrackData.Diff == "SC") {
                Manager.RandomSelectorUI.SingleLvParent.gameObject.SetActive(false);
                Manager.RandomSelectorUI.SingleLvSCParent.gameObject.SetActive(true);

                for(int i = 0; i < 15; i++)           Manager.RandomSelectorUI.SingleLvSCParent.Toggles[i].isOn = false;
                for(int i = 0; i < TrackData.Lv; i++) Manager.RandomSelectorUI.SingleLvSCParent.Toggles[i].isOn = true;
            }
            else {
                Manager.RandomSelectorUI.SingleLvParent.gameObject.SetActive(true);
                Manager.RandomSelectorUI.SingleLvSCParent.gameObject.SetActive(false);

                for(int i = 0; i < 15; i++)           Manager.RandomSelectorUI.SingleLvParent.Toggles[i].isOn = false;
                for(int i = 0; i < TrackData.Lv; i++) Manager.RandomSelectorUI.SingleLvParent.Toggles[i].isOn = true;
            }
        }
        else {
            Manager.RandomSelectorUI.SingleUIScreen.SetActive(false);
            Manager.RandomSelectorUI.NoneUIScreen.SetActive(true);
        }
    }
    public static List<MainData.TrackInfo> GetTracks() { return GetTracks(_Count); }
    public static List<MainData.TrackInfo> GetTracks(int count = 1) {

        IEnumerable<KeyValuePair<ushort, MainData.TrackInfo>> pass = SystemFileIO.MainData.TrackTable.Where((t) => {
            MainData.TrackInfo TrackData = t.Value;

            if( !( (TrackData.Bt.Equals("4B") && _4b)  || (TrackData.Bt.Equals("5B") && _5b)  || (TrackData.Bt.Equals("6B") && _6b)  || (TrackData.Bt.Equals("8B") && _8b) ) ) return false;
            if( !( (TrackData.Diff.Equals("NM") && _nm) || (TrackData.Diff.Equals("HD") && _hd) || (TrackData.Diff.Equals("MX") && _mx) || (TrackData.Diff.Equals("SC") && _sc) ) ) return false;
            if( !( (t.Value.Lv == 1 && _1) || (t.Value.Lv == 2 && _2) || (t.Value.Lv == 3 && _3) || (t.Value.Lv == 4 && _4) || (t.Value.Lv == 5 && _5)  || (t.Value.Lv == 6 && _6) || (t.Value.Lv == 7 && _7) || (t.Value.Lv == 8 && _8) || (t.Value.Lv == 9 && _9) || (t.Value.Lv == 10 && _10) || (t.Value.Lv == 11 && _11) || (t.Value.Lv == 12 && _12) || (t.Value.Lv == 13 && _13) || (t.Value.Lv == 14 && _14) || (t.Value.Lv == 15 && _15) ) ) return false;
            
            MainData.SongInfo SongData = SystemFileIO.GetSongData(t.Value.SongIndex);
            if( !DLCs.Contains(SongData.Ctgr) ) return false;

            return true;
        });

        pass = pass.OrderBy(a => Guid.NewGuid());
        pass = pass.Take(Mathf.Min(pass.Count(), count));

        List<MainData.TrackInfo> tracks = new List<MainData.TrackInfo>(from x in pass select x.Value);

        return tracks;
    }
    
#region UI 이벤트용 함수
    public void _U_Count(Slider s) { _Count = (int)s.value; Manager.RandomSelectorUI.CountLabel.text = ((int)s.value).ToString(); }

    public void _DIFF_ALL() { StartCoroutine(_DIFF_ALL_ROUTINE()); }
    private IEnumerator _DIFF_ALL_ROUTINE() {
        foreach(Toggle t in Manager.RandomSelectorUI.DifficultyParent) {
            if(t != null)
                t.isOn = true;
            
            yield return Manager.ToggleDelay;
        }
    }
    public void _DIFF_NONE() { StartCoroutine(_DIFF_NONE_ROUTINE()); }
    private IEnumerator _DIFF_NONE_ROUTINE() {
        foreach(Toggle t in Manager.RandomSelectorUI.DifficultyParent) {
            if(t != null)
                t.isOn = false;
            
            yield return Manager.ToggleDelay;
        }
    }
    public void _LV_ALL() { StartCoroutine(_LV_ALL_ROUTINE()); }
    private IEnumerator _LV_ALL_ROUTINE() {
        foreach(Toggle t in Manager.RandomSelectorUI.LvParent.Toggles) {
            if(t != null)
                t.isOn = true;
            
            yield return Manager.ToggleDelay;
        }
    }
    public void _LV_NONE() { StartCoroutine(_LV_NONE_ROUTINE()); }
    private IEnumerator _LV_NONE_ROUTINE() {
        foreach(Toggle t in Manager.RandomSelectorUI.LvParent.Toggles) {
            if(t != null)
                t.isOn = false;
            
            yield return Manager.ToggleDelay;
        }
    }
    public void _DLC_ALL() { StartCoroutine(_DLC_ALL_ROUTINE());}
    private IEnumerator _DLC_ALL_ROUTINE() {
        foreach(Toggle t in Manager.RandomSelectorUI.DLCs) {
            if(t != null)
                t.isOn = true;
            
            yield return Manager.ToggleDelay;
        }
    }
    public void _DLC_NONE() { StartCoroutine(_DLC_NONE_ROUTINE());}
    private IEnumerator _DLC_NONE_ROUTINE() {
        foreach(Toggle t in Manager.RandomSelectorUI.DLCs) {
            if(t != null)
                t.isOn = false;
            
            yield return Manager.ToggleDelay;
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

    public static void DLC(bool state, string abbr) {
        if(state)
            DLCs.Add(abbr);
        else
            DLCs.Remove(abbr);
    }
#endregion
}
