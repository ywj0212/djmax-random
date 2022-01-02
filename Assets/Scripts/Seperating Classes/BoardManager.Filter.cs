using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public partial class BoardManager : MonoBehaviour
{
    [Header("Filter")]
    public Transform DLCToggleParent;
    public List<Toggle> DLCToggles = new List<Toggle>();
    public List<Toggle> DifficultyToggles = new List<Toggle>();
    [SerializeField] private GameObject ModalFilter;

    private bool _nm = true, _hd = true, _mx = true, _sc = true;
    private string search = "";
    private bool isFilter = false;

    public static List<string> DLCs = new List<string>();
    public static void DLC(bool state, string abbr) {
        if(state)
            DLCs.Add(abbr);
        else
            DLCs.Remove(abbr);
    }
    
    private bool CheckFilter(BoardTrack boardTrack) {
        if(!string.IsNullOrWhiteSpace(search)) {
            return SystemFileIO.GetSongData(boardTrack.SongIndex).Name.Contains(search, System.StringComparison.OrdinalIgnoreCase) && DLCs.Contains(SystemFileIO.GetSongData(boardTrack.SongIndex).Ctgr) && ((SystemFileIO.GetTrackData(boardTrack.Index).Diff.Equals("NM") && _nm) || (SystemFileIO.GetTrackData(boardTrack.Index).Diff.Equals("HD") && _hd) || (SystemFileIO.GetTrackData(boardTrack.Index).Diff.Equals("MX") && _mx) || (SystemFileIO.GetTrackData(boardTrack.Index).Diff.Equals("SC") && _sc));
        }
        else {
            return DLCs.Contains(SystemFileIO.GetSongData(boardTrack.SongIndex).Ctgr) && ((SystemFileIO.GetTrackData(boardTrack.Index).Diff.Equals("NM") && _nm) || (SystemFileIO.GetTrackData(boardTrack.Index).Diff.Equals("HD") && _hd) || (SystemFileIO.GetTrackData(boardTrack.Index).Diff.Equals("MX") && _mx) || (SystemFileIO.GetTrackData(boardTrack.Index).Diff.Equals("SC") && _sc));
        }
    }
    private void ApplyFilter() {
        isFilter = true;
        if(!string.IsNullOrWhiteSpace(search)) {
            var songs = 
                (from s in SystemFileIO.MainData.SongTable
                 where s.Value.Name.Contains(search, System.StringComparison.OrdinalIgnoreCase)
                 select s.Key);

            foreach (var Track in AchievementTracks) {
                Track.gameObject.SetActive(songs.Contains(Track.SongIndex) && DLCs.Contains(SystemFileIO.GetSongData(Track.SongIndex).Ctgr) && ((SystemFileIO.GetTrackData(Track.Index).Diff.Equals("NM") && _nm) || (SystemFileIO.GetTrackData(Track.Index).Diff.Equals("HD") && _hd) || (SystemFileIO.GetTrackData(Track.Index).Diff.Equals("MX") && _mx) || (SystemFileIO.GetTrackData(Track.Index).Diff.Equals("SC") && _sc)));
            }

            FilterFloorEvent?.Invoke(isFilter);
            FilterLevelEvent?.Invoke(isFilter);

            foreach(Transform t in Manager.AchievementUI.ScrollViewport) {
                if(t.gameObject.activeInHierarchy) {
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)t);

                    foreach(Transform tt in t) {
                        if(t.gameObject.activeInHierarchy)
                            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)t);
                    }
                }
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Manager.AchievementUI.ScrollViewport);
        }
        else
        {
            if(DLCs.Count == SystemFileIO.MainData.DLCList.Count) isFilter = false;

            foreach(var Track in AchievementTracks) {
                Track.gameObject.SetActive( DLCs.Contains(SystemFileIO.GetSongData(Track.SongIndex).Ctgr) && ((SystemFileIO.GetTrackData(Track.Index).Diff.Equals("NM") && _nm) || (SystemFileIO.GetTrackData(Track.Index).Diff.Equals("HD") && _hd) || (SystemFileIO.GetTrackData(Track.Index).Diff.Equals("MX") && _mx) || (SystemFileIO.GetTrackData(Track.Index).Diff.Equals("SC") && _sc)) );
            }

            FilterFloorEvent?.Invoke(isFilter);
            FilterLevelEvent?.Invoke(isFilter);

            foreach(Transform t in Manager.AchievementUI.ScrollViewport) {
                if(t.gameObject.activeInHierarchy) {
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)t);

                    foreach(Transform tt in t) {
                        if(t.gameObject.activeInHierarchy)
                            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)t);
                    }
                }
            }
        }
    }

    public void FilterModal(bool state) {
        ModalFilter.SetActive(state);

        if(!state) ApplyFilter();
    }
    public void Search(string query) {
        search = query;
        ApplyFilter();
    }

    public void _NM(bool b) { _nm = b; }
    public void _HD(bool b) { _hd = b; }
    public void _MX(bool b) { _mx = b; }
    public void _SC(bool b) { _sc = b; }
    public void _DIFF_ALL() { StartCoroutine(_DIFF_ALL_ROUTINE()); }
    private IEnumerator _DIFF_ALL_ROUTINE() {
        foreach(Toggle t in DifficultyToggles) {
            if(t != null)
                t.isOn = true;
            
            yield return Manager.ToggleDelay;
        }
    }
    public void _DIFF_NONE() { StartCoroutine(_DIFF_NONE_ROUTINE()); }
    private IEnumerator _DIFF_NONE_ROUTINE() {
        foreach(Toggle t in DifficultyToggles) {
            if(t != null)
                t.isOn = false;
            
            yield return Manager.ToggleDelay;
        }
    }
    public void _DLC_ALL() { StartCoroutine(_DLC_ALL_ROUTINE());}
    private IEnumerator _DLC_ALL_ROUTINE() {
        foreach(Toggle t in DLCToggles) {
            if(t != null)
                t.isOn = true;
            
            yield return Manager.ToggleDelay;
        }
    }
    public void _DLC_NONE() { StartCoroutine(_DLC_NONE_ROUTINE());}
    private IEnumerator _DLC_NONE_ROUTINE() {
        foreach(Toggle t in DLCToggles) {
            if(t != null)
                t.isOn = false;
            
            yield return Manager.ToggleDelay;
        }
    }
}
