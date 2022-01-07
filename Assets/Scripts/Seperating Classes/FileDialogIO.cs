using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using Ookii.Dialogs;
using Mirix.DMRV;

public class FileDialogIO : MonoBehaviour
{
    private static VistaOpenFileDialog OpenFileDialog = new VistaOpenFileDialog();
    private static VistaSaveFileDialog SaveFileDialog = new VistaSaveFileDialog();

    private static string GetBoardOpenFilePath() {
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
    private static string GetBoardSaveFilePath() {
        SaveFileDialog.Title = "Export Board File";
        SaveFileDialog.Filter = "DJMAX Data File |*.dmrvbd";
        SaveFileDialog.DefaultExt = "dmrvbd";
        SaveFileDialog.AddExtension = true;
        
        var Result = SaveFileDialog.ShowDialog();
        if(Result == System.Windows.Forms.DialogResult.OK) {
            return SaveFileDialog.FileName;
        }
        else
            return null;
    }
    private static string GetImageSaveFilePath() {
        SaveFileDialog.Title = "Export Board Image File";
        SaveFileDialog.DefaultExt = "png";
        SaveFileDialog.Filter = "PNG Image |*.png";
        SaveFileDialog.AddExtension = true;
        
        var Result = SaveFileDialog.ShowDialog();
        if(Result == System.Windows.Forms.DialogResult.OK) {
            return SaveFileDialog.FileName;
        }
        else
            return null;
    }

    public static void ExportBoardData(BoardInfo board) {
        string path = GetBoardSaveFilePath();

        if(path == null) {
            throw new System.NullReferenceException();
        }

        BinaryFormatter BF = new BinaryFormatter();
        BF.Serialize(File.Create(path), board);
    }
    public static BoardInfo ImportBoardData() {
        string path = GetBoardOpenFilePath();

        if(path == null) {
            throw new System.NullReferenceException();
        }

        BinaryFormatter BF = new BinaryFormatter();
        return (BoardInfo)BF.Deserialize(new FileStream(path, FileMode.Open));
    }
    public static void SavePNG(Texture2D texture2D) {

    }
}
