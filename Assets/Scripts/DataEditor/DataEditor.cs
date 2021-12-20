using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using Ookii.Dialogs;
using TMPro;
using Mirix.DMRV;

public class DataEditor : MonoBehaviour
{
    [SerializeField] private TMP_InputField VersionField;
    [SerializeField] private TMP_InputField BoardNameField;
    [SerializeField] private TextMeshProUGUI ImportedBoardInfo;
    [SerializeField] private Transform NotificationList;
    [SerializeField] private GameObject NotificationPrefab;
    [SerializeField] private Transform CategoryList;
    [SerializeField] private GameObject CategoryPrefab;
    [SerializeField] private Transform SongTableList;
    [SerializeField] private GameObject SongTablePrefab;
    [SerializeField] private Transform TrackTableList;
    [SerializeField] private GameObject TrackTablePrefab;

    private VistaSaveFileDialog SaveMainDialog = new VistaSaveFileDialog {
        Title = "Export Main Data",
        Filter = "Binary File|*.bin",
        FilterIndex = 1,
        DefaultExt = ".bin",
        AddExtension = true
    };
    private VistaOpenFileDialog OpenMainDialog = new VistaOpenFileDialog {
        Title = "Import Main Data",
        Filter = "Binary File|*.bin",
        FilterIndex = 1,
        Multiselect = false,
        DefaultExt = ".bin"
    };
    private VistaOpenFileDialog OpenBoardDialog = new VistaOpenFileDialog {
        Title = "Import Board Data",
        Filter = "DJMAX Data File|*.dmrvbd",
        FilterIndex = 1,
        Multiselect = false,
        DefaultExt = ".dmrvbd"
    };

    private MainData main = new MainData();

    // private Mirix.DMRV.MainData NAmain = new Mirix.DMRV.MainData();

    private Board board;
    private string boardName;
    
    public void ImportMainData() {
        var Result = OpenMainDialog.ShowDialog();
        if(Result != DialogResult.OK)
            return;
        
        BinaryFormatter BinF = new BinaryFormatter();
        main = (MainData)BinF.Deserialize((FileStream)OpenMainDialog.OpenFile());

        //? 하단은 어셈블리 이전을 위한 코드
        // NAmain.Version = main.Version;
        // NAmain.CategoryAbbr = main.CategoryAbbr;
        // foreach(var n in main.Notifications) {
        //     NAmain.Notifications.Add(new Mirix.DMRV.MainData.Notification() { Header = n.Header, Body = n.Body });
        // }
        // foreach(var s in main.SongTable) {
        //     NAmain.SongTable[s.Key] = new Mirix.DMRV.MainData.SongInfo() { Ctgr = s.Value.Ctgr, Name = s.Value.Name, Cmps = s.Value.Cmps };
        // }
        // foreach(var t in main.TrackTable) {
        //     NAmain.TrackTable[t.Key] = new Mirix.DMRV.MainData.TrackInfo() { SongIndex = t.Value.SongIndex, Bt = t.Value.Bt, Diff = t.Value.Diff, Lv = t.Value.Lv };
        // }
        // ! 하단 코드는 삭제 하지 말고 기존 Board, BoardInfo 데이터들을 이전할 때 사용할 것!
        // NAmain.DefaultBoard.Name = main.DefaultBoard.Name;
        // NAmain.DefaultBoard.Board.Modifyable = main.DefaultBoard.Board.Modifyable;
        // NAmain.DefaultBoard.Board.ButtonType = (Mirix.DMRV.Board.Type)((byte)main.DefaultBoard.Board.ButtonType);
        // NAmain.DefaultBoard.Board.CategoryType = (Mirix.DMRV.Board.Ctgr)((byte)main.DefaultBoard.Board.CategoryType);

        // int btind = 0;
        // foreach(var bd in main.DefaultBoard.Board.Buttons) {
        //     foreach(var lv in bd.Lv) {
        //         Mirix.DMRV.Board.ButtonData.LvData level = new Mirix.DMRV.Board.ButtonData.LvData();
                
        //         foreach(var f in lv.Floor) {
        //             Mirix.DMRV.Board.ButtonData.LvData.FloorData floor = new Mirix.DMRV.Board.ButtonData.LvData.FloorData();
                    
        //             floor.Tracks = f.Tracks;
        //             floor.Name = f.Name;

        //             level.Floor.Add(floor);
        //         }

        //         NAmain.DefaultBoard.Board.Buttons[btind].Lv.Add(level);
        //     }
        //     btind++;
        // }
        // !##################################################################################

        // var Result2 = SaveMainDialog.ShowDialog();
        // if(Result2 != DialogResult.OK)
        //     return;
        
        // BinaryFormatter BinF2 = new BinaryFormatter();
        // BinF2.Serialize((FileStream)SaveMainDialog.OpenFile(), NAmain);
        // ?##################################################################################

        UpdatePanel();
    }
    public void GenerateMainData() {
        var Result = SaveMainDialog.ShowDialog();
        if(Result != DialogResult.OK)
            return;
        
        BinaryFormatter BinF = new BinaryFormatter();
        BinF.Serialize((FileStream)SaveMainDialog.OpenFile(), main);
    }
    public void ImportBoardData() {
        var Result = OpenBoardDialog.ShowDialog();
        if(Result != DialogResult.OK)
            return;
        
        BinaryFormatter BinF = new BinaryFormatter();
        board = (Board)BinF.Deserialize((FileStream)OpenBoardDialog.OpenFile());

        UpdatePanel();
    }

    public void NewNotification() {}
    public void NewCategoryAbbr() {}
    public void NewSongData() {}
    public void NewTrackData() {}

    public void DeleteNotification(int index) {}
    public void DeleteCategoryAbbr(int index) {}
    public void DeleteSongData(int index) {}
    public void DeleteTrackDate(int index) {}

    public void UpdatePanel() {
        foreach(Transform t in NotificationList)
            Destroy(t.gameObject);
        foreach(Transform t in CategoryList)
            Destroy(t.gameObject);
        foreach(Transform t in SongTableList)
            Destroy(t.gameObject);
        foreach(Transform t in TrackTableList)
            Destroy(t.gameObject);
        
        foreach(var n in main.Notifications) {
            GameObject NLO = Instantiate(NotificationPrefab, NotificationList);
            NLO.transform.localScale = Vector3.one;
            
            NL NL = NLO.GetComponent<NL>();
            NL.Title.text = n.Header;
            NL.Body.text = n.Body;
        }
        foreach(var c in main.CategoryAbbr) {
            GameObject CLO = Instantiate(CategoryPrefab, CategoryList);
            CLO.transform.localScale = Vector3.one;
            
            CL CL = CLO.GetComponent<CL>();
            CL.Key.text = c.Key;
            CL.Value.text = c.Value;
        }
        foreach(var s in main.SongTable.Reverse()) {
            GameObject STLO = Instantiate(SongTablePrefab, SongTableList);
            STLO.transform.localScale = Vector3.one;
            
            STL STL = STLO.GetComponent<STL>();
            STL.Index.text = s.Key.ToString() + " |";
            STL.Ctgr.text = s.Value.Ctgr + " |";
            STL.Name.text = s.Value.Name;
            STL.Cmps.text = s.Value.Cmps;
        }
        foreach(var t in main.TrackTable.Reverse().Take(100)) {
            GameObject TTLO = Instantiate(TrackTablePrefab, TrackTableList);
            TTLO.transform.localScale = Vector3.one;
            
            TTL TTL = TTLO.GetComponent<TTL>();
            TTL.Index.text = t.Key.ToString() + " |";
            TTL.SongIndex.text = t.Value.SongIndex.ToString() + " |";
            TTL.Bt.text = t.Value.Bt;
            TTL.Diff.text = t.Value.Diff;
            TTL.Lv.text = t.Value.Lv.ToString();
        }

        ImportedBoardInfo.text = (board == null) ? "None" : board.ToString();
    }
}
