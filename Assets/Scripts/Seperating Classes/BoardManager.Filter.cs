using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public partial class BoardManager : MonoBehaviour
{
    public Transform DLCToggleParent;
    public List<Toggle> DLCToggles = new List<Toggle>();
    public List<Toggle> DifficultyToggles = new List<Toggle>();
    [SerializeField] private GameObject ModalFilter;

    private bool _nm = true, _hd = true, _mx = true, _sc = true;
    private string search = "";

    public static List<string> DLCs = new List<string>();
    public static void DLC(bool state, string abbr) {
        if(state)
            DLCs.Add(abbr);
        else
            DLCs.Remove(abbr);
    }
    
    private void ApplyFilter() {
        if(!string.IsNullOrEmpty(search)) {
            var songs = 
                (from s in SystemFileIO.MainData.SongTable
                 where s.Value.Name.Contains(search, System.StringComparison.OrdinalIgnoreCase)
                 select s.Key);

            foreach (var Track in AchievementTracks) {
                Track.gameObject.SetActive(songs.Contains(Track.SongIndex) && DLCs.Contains(Track.Ctgr) && ((Track.Diff.Equals("NM") && _nm) || (Track.Diff.Equals("HD") && _hd) || (Track.Diff.Equals("MX") && _mx) || (Track.Diff.Equals("SC") && _sc)));
            }

            FilterFloorEvent?.Invoke();
            FilterLevelEvent?.Invoke();

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
            foreach(var Track in AchievementTracks) {
                Track.gameObject.SetActive( DLCs.Contains(Track.Ctgr) && ((Track.Diff.Equals("NM") && _nm) || (Track.Diff.Equals("HD") && _hd) || (Track.Diff.Equals("MX") && _mx) || (Track.Diff.Equals("SC") && _sc)) );
            }

            FilterFloorEvent?.Invoke();
            FilterLevelEvent?.Invoke();

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
