using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class TrackData {
    public List<Track> tracks;
}
[System.Serializable]
public class Track {
    public string Ctgr;
    public string Name;
    public string Cmps;
    public string Bt;
    public string Diff;
    public int Lv;
}

public class JsonParser : MonoBehaviour
{
    [SerializeField] private TextAsset json;
    [Space]
    [SerializeField] private Transform musicListUIParent;
    [SerializeField] private GameObject musicListPrefab;
    [Space]
    [SerializeField] private Transform DiffParent;
    [SerializeField] private Transform LvParent;
    [SerializeField] private Transform DLCParent1;
    [SerializeField] private Transform DLCParent2;
    [SerializeField] private Text CountLabel;

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
    
    private int _Count = 8;

#region UI
    public void _StartBtn() { ShowTracks(); }

    public void _U_Count(Slider s) { _Count = (int)s.value; CountLabel.text = ((int)s.value).ToString(); }

    public void _DIFF_ALL() {
        foreach(Transform t in DiffParent) {
            Toggle tg = t.GetComponent<Toggle>();
            if(tg != null)
                tg.isOn = true;
        }
    }
    public void _DIFF_NONE() {
        foreach(Transform t in DiffParent) {
            Toggle tg = t.GetComponent<Toggle>();
            if(tg != null)
                tg.isOn = false;
        }
    }
    public void _LV_ALL() {
        foreach(Transform t in LvParent) {
            Toggle tg = t.GetComponent<Toggle>();
            if(tg != null)
                tg.isOn = true;
        }
    }
    public void _LV_NONE() {
        foreach(Transform t in LvParent) {
            Toggle tg = t.GetComponent<Toggle>();
            if(tg != null)
                tg.isOn = false;
        }
    }
    public void _DLC_ALL() {
        foreach(Transform t in DLCParent1) {
            Toggle tg = t.GetComponent<Toggle>();
            if(tg != null)
                tg.isOn = true;
        }
        foreach(Transform t in DLCParent2) {
            Toggle tg = t.GetComponent<Toggle>();
            if(tg != null)
                tg.isOn = true;
        }
    }
    public void _DLC_NONE() {
        foreach(Transform t in DLCParent1) {
            Toggle tg = t.GetComponent<Toggle>();
            if(tg != null)
                tg.isOn = false;
        }
        foreach(Transform t in DLCParent2) {
            Toggle tg = t.GetComponent<Toggle>();
            if(tg != null)
                tg.isOn = false;
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
    
    private TrackData trackData;
    private void Start() {
        trackData = JsonUtility.FromJson<TrackData>(json.text);
        print("Loaded TrackData: " + trackData.tracks.Count.ToString() + " Items");
    }

    public void ShowTracks() {
        List<Track> tracks = GetTracks(count:_Count);
        
        foreach(Transform t in musicListUIParent) {
            Destroy(t.gameObject);
        }

        foreach(Track t in tracks) {
            GameObject temp = Instantiate(musicListPrefab);
            temp.transform.SetParent(musicListUIParent);
            temp.transform.localScale = Vector3.one;
            
            if(t == null)
                continue;

            // ? Track > Album Jacket(Image)
            Sprite thumb = Resources.Load<Sprite>("Sprites/Album Jackets/" + t.Name);
            if(thumb != null)
                temp.transform.GetChild(0).GetComponent<Image>().sprite = thumb;
            else
                print(t.Name + " " + t.Bt + " " + t.Diff);

            // ? Track > Description > Difficulty(Image)
            Sprite diff = Resources.Load<Sprite>("Sprites/Diff/" + t.Bt+t.Diff);
            if(diff != null)
                temp.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = diff;
            else
                temp.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Diff/4BNM");
            
            // ? Track > Description > Category(Image)
            Sprite ctgr = Resources.Load<Sprite>("Sprites/Category/" + t.Ctgr);
            if(ctgr != null)
                temp.transform.GetChild(1).GetChild(1).GetComponent<Image>().sprite = ctgr;
            else
                temp.transform.GetChild(1).GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Category/COLLABOR");

            // ? Track > Description > Title(Text)
            temp.transform.GetChild(1).GetChild(2).GetComponent<Text>().text = t.Name;

            // ? Track > Description > Info(Text)
            temp.transform.GetChild(1).GetChild(3).GetComponent<Text>().text = t.Cmps;

            if(t.Diff == "SC") {
                // ? Track > Description > LevelToggle SC
                Transform levelToggleParent = temp.transform.GetChild(1).GetChild(4);
                Transform levelToggleParentSC = temp.transform.GetChild(1).GetChild(5);
                
                levelToggleParent.gameObject.SetActive(false);
                levelToggleParentSC.gameObject.SetActive(true);

                for(int i = 0; i < t.Lv; i++) {
                    levelToggleParentSC.GetChild(i).GetComponent<Toggle>().isOn = true;
                }
            }
            else {
                // ? Track > Description > LevelToggle
                Transform levelToggleParent = temp.transform.GetChild(1).GetChild(4);
                Transform levelToggleParentSC = temp.transform.GetChild(1).GetChild(5);

                levelToggleParent.gameObject.SetActive(true);
                levelToggleParentSC.gameObject.SetActive(false);

                for(int i = 0; i < t.Lv; i++) {
                    levelToggleParent.GetChild(i).GetComponent<Toggle>().isOn = true;
                }
            }         
        }
    }

    private List<Track> GetTracks(int count = 8) {

        List<Track> pass = trackData.tracks.FindAll( (t) =>  
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

        List<Track> tracks = new List<Track>();
        
        for(int i = 0; i < count; i++){
            if(i >= pass.Count)
                tracks.Add(null);
            else
                tracks.Add(pass[i]);
        }

        return tracks;
    }
}
