using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class BoardManager : MonoBehaviour
{
    [SerializeField] private Toggle[] DifficultyToggles;
    [SerializeField] private Toggle[] DLCToggles;

    public void SetFilter() {

    }
    public void Search(string query) {

    }

    private bool    _RE = true, _P1 = true, _P2 = true, _P3 = true,
                    _T1 = true, _T2 = true, _T3 = true,
                    _BS = true, _CE = true, _TR = true,
                    _VE = true, _ES = true, _CH = true, _GC = true,
                    _DM = true, _CY = true, _EM = true,
                    _GF = true, _GG = true, _NX = true,

                    _nm = true, _hd = true, _mx = true, _sc = true;
    
#region UI
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
    public void _D_NX(bool b) { _NX = b; }

    public void _NM(bool b) { _nm = b; }
    public void _HD(bool b) { _hd = b; }
    public void _MX(bool b) { _mx = b; }
    public void _SC(bool b) { _sc = b; }
#endregion

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
