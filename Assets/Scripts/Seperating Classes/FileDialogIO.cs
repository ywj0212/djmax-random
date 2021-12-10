using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using Ookii.Dialogs;

public class FileDialogIO : MonoBehaviour
{
    private VistaOpenFileDialog OpenFileDialog = new VistaOpenFileDialog();
    private VistaSaveFileDialog SaveFileDialog = new VistaSaveFileDialog();

    private string GetOpenFilePath() {
        OpenFileDialog.Title = "Import Board File";
        OpenFileDialog.Filter = "DJMAX Data File |*.dmrvbd";
        OpenFileDialog.Multiselect = false;
        
        var Result = OpenFileDialog.ShowDialog();
        if(Result == System.Windows.Forms.DialogResult.OK) {
            return OpenFileDialog.FileName;
        }
        else
            return null;
    }
    private string GetSaveFilePath() {
        SaveFileDialog.Title = "Import Board File";
        SaveFileDialog.Filter = "DJMAX Data File |*.dmrvbd";
        
        var Result = SaveFileDialog.ShowDialog();
        if(Result == System.Windows.Forms.DialogResult.OK) {
            return SaveFileDialog.FileName;
        }
        else
            return null;
    }

    public void ExportBoardData(Board board) {
        string path = GetSaveFilePath();

        if(path == null) {
            throw new System.NullReferenceException();
            return;
        }

        BinaryFormatter BF = new BinaryFormatter();
        BF.Serialize(File.Create(path), board);
    }
    public BoardInfo ImportBoardData() {
        string path = GetOpenFilePath();

        if(path == null) {
            throw new System.NullReferenceException();
            return null;
        }

        // TODO: 보드 명 입력 루틴 추가 필요

        BinaryFormatter BF = new BinaryFormatter();
        return new BoardInfo("", (Board)BF.Deserialize(new FileStream(path, FileMode.Open)));
    }
    public void SavePng(string path, Texture2D texture2D) {

    }
}
