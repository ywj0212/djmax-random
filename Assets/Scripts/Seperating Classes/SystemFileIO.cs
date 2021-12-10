using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

// TODO 개발자 도구부터 제작하여 초기화 구문 삭제, %appdata% 에 있는 시리얼라이즈드 된 결과 사용
[Serializable] public class MainData {
    public string Version = "211225 v6 Stable";
    public Dictionary<string, string> CategoryAbbr = new Dictionary<string, string>() { // TODO 데이터 수정할 수 있는 개발자 도구 생기면 초기화 구문 삭제!!!
        { "P1", "PORTABLE 1" },
        { "P2", "PORTABLE 2" },
        { "P3", "PORTABLE 3" },
        { "RE", "RESPECT" },
        { "VE", "V EXTENSION" },
        { "ES", "EMOTIONAL S." },
        { "CE", "CLAZZIQUAI EDITION" },
        { "BS", "BLACK SQUARE" },
        { "TR", "TRILOGY" },
        { "T1", "TECHNIKA" },
        { "T2", "TECHNIKA 2" },
        { "T3", "TECHNIKA 3" }
    };
   
    public Notification[] Notifications;
    [Serializable] public struct Notification {
        public string Header;
        public string Body;
    }

    public Dictionary<ushort, SongInfo>  SongTable;
    public Dictionary<ushort, TrackInfo> TrackTable;
    [Serializable] public struct SongInfo {
        public string Ctgr;
        public string Name;
        public string Cmps;
    }
    [Serializable] public struct TrackInfo {
        public ushort SongIndex;
        public string Bt;
        public string Diff;
        public byte   Lv;
    }

    public BoardInfo DefaultBoard;
}


// ! 파일 형식 변화로 인해서 업데이트 되어야 함!
// ? 데이터 파일이 모두 웹서버로 이전됨에 따라 모든 데이터 셋이 MainData 클래스로 통합되어 삭제됨.
[Serializable] public class TrackAdvancedData {
    public List<TrackAdvanced> tracks;
}
[Serializable] public class TrackAdvanced {
    public ushort Index;
    public string Ctgr;
    public string Name;
    public string Cmps;
    public string Bt;
    public string Diff;
    public byte Lv;
}
[Serializable] public class TrackSimpleData {
    public List<TrackSimple> tracks;
}
[Serializable] public class TrackSimple {
    public string Ctgr;
    public string Name;
    public string Cmps;
}
// ! 여기까지


[Serializable] public class Board {
    public enum Type : byte { Seperated, Combined }
    public enum Ctgr : byte { Level, Custom }
    public enum Button : byte { _4B = 0, _5B = 1, _6B = 2, _8B = 3, All = 0 }
    
    public bool Modifyable;
    public Type ButtonType;
    public Ctgr CategoryType;
    public ButtonData[] Buttons = new ButtonData[4];

    [Serializable] public class ButtonData {
        public List<LvData> Lv;

        [Serializable] public class LvData {
            public byte Lv;
            public List<FloorData> Floor;

            [Serializable] public class FloorData {
                public string Name;
                public List<ushort> Tracks;
            }
        }
    }
}
[Serializable] public class BoardInfo {
    public string Name;
    public Board Board;

    public BoardInfo(string name, Board board) {
        Name = name;
        Board = board;
    }
}

[Serializable] public class AchievementData {
    public List<Achievement> achievements = new List<Achievement>();
}
[Serializable] public class Achievement {
    public ushort Index;
    public float Rate;
    public enum State : sbyte { None = 0, Clear = 1, MaxCombo = 2, Perfect = 3 }
    public State Status;
}
[Serializable] public class SettingData {
    public List<ushort> BannedSong;
}

public class SystemFileIO : Singleton<SystemFileIO>
{
    [Header("Save Path")]
    [SerializeField] private string BoardDataRoot;

    [Header("JSON Data")]
    [SerializeField] private TextAsset AdvancedJson;
    [SerializeField] private TextAsset SimpleJson;
    [SerializeField] private TextAsset DefaultBoardJson;

    [Header("Resources Path")]
    [SerializeField] private string ThumbnailResourcePath;
    [SerializeField] private string PreviewResourcePath;
    [SerializeField] private string DifficultyResourcePath;
    [SerializeField] private string CategoryResourcePath;
    [SerializeField] private string AchievementDifficultyResourcePath;

    private string BoardRootPath             => string.Format("{0}/{1}", Application.persistentDataPath, BoardDataRoot);
    private string AchievementPath           => string.Format("{0}/{1}.json", Application.persistentDataPath, "achievements");
    private string SettingPath               => string.Format("{0}/{1}.json", Application.persistentDataPath, "setting");
    private string CustomAchievementListPath => string.Format("{0}/{1}.json", Application.persistentDataPath, "customlist");
    
    public static TrackAdvancedData TrackAdvancedData;
    public static TrackSimpleData   TrackSimpleData;
    public static AchievementData   AchievementData;
    public static List<BoardInfo>   Boards = new List<BoardInfo>();
    public static SettingData       SettingData;

    private void Start() {
        LoadData();
    }
    private void LoadData() {
        TrackAdvancedData      = JsonUtility.FromJson<TrackAdvancedData>(AdvancedJson.text);
        TrackSimpleData        = JsonUtility.FromJson<TrackSimpleData>  (SimpleJson.text);

        BoardInfo DefaultBoardData = new BoardInfo("Unofficial Level Table", JsonUtility.FromJson<Board>(DefaultBoardJson.text));
        Boards.Add(DefaultBoardData);

        BinaryFormatter BF = new BinaryFormatter();
        DirectoryInfo DI = new DirectoryInfo(BoardRootPath);
        if(!DI.Exists)
            Directory.CreateDirectory(BoardRootPath);
        foreach(FileInfo f in DI.GetFiles()) {
            BoardInfo BI = new BoardInfo(f.Name, (Board)BF.Deserialize(new FileStream(f.FullName, FileMode.Open)));
            Boards.Add(BI);
        }

        AchievementData        = LoadJson<AchievementData>(AchievementPath) ?? new AchievementData();
        SettingData            = LoadJson<SettingData>    (SettingPath) ?? new SettingData();
    }

    private void SaveBoardData(BoardInfo boardinfo) {
        string path = $"{BoardRootPath}/{boardinfo.Name}.djmxdat";
        BinaryFormatter BF = new BinaryFormatter();
        BF.Serialize(File.Create(path), boardinfo.Board);
    }
    private void CreateBoard(string name) {
        File.Create($"{BoardRootPath}/{name}.djmxdat");
        new BoardInfo(name, new Board());
    }
    private void DeleteBoard(string name) {
        File.Delete($"{BoardRootPath}/{name}.djmxdat");
    }

    public static void SaveAchievement(ushort index, Achievement.State state) {
        Achievement achievement = AchievementData.achievements.FirstOrDefault((x) => (x.Index == index));
        if(achievement == null) {
            achievement = new Achievement();
            AchievementData.achievements.Add(achievement);
            achievement.Index = index;
            achievement.Status = Achievement.State.None;
        }

        achievement.Status = state;

        SaveJson<AchievementData>(AchievementData, inst.AchievementPath);
    }
    public static Achievement GetAchievementSave(ushort index) {
        Achievement achievement = AchievementData.achievements.FirstOrDefault((x) => (x.Index == index));
        if(achievement == null) {
            achievement = new Achievement();
            AchievementData.achievements.Add(achievement);
            achievement.Index = index;
            achievement.Status = Achievement.State.None;
        }

        return achievement;
    }
    public static TrackAdvanced GetTrackAdvancedData(int index) {
        TrackAdvanced trackData = TrackAdvancedData.tracks.FirstOrDefault((x) => (x.Index == index));
        if(trackData == null)
            throw new InvalidDataException();
        
        return trackData;
    }
    public static Sprite GetThumbnailSprite(string name) {
        return Resources.Load<Sprite>(inst.ThumbnailResourcePath + name);
    }
    public static Sprite GetPreviewSprite(string name) {
        return Resources.Load<Sprite>(inst.PreviewResourcePath + name);
    }
    public static Sprite GetDifficultySprite(string button, string difficulty) {
        return Resources.Load<Sprite>(inst.DifficultyResourcePath + button + difficulty) ?? Resources.Load<Sprite>(inst.DifficultyResourcePath + "4BNM");
    }
    public static Sprite GetCategorySprite(string category) {
        return Resources.Load<Sprite>(inst.CategoryResourcePath + category) ?? Resources.Load<Sprite>(inst.CategoryResourcePath + "COLLABOR");
    }
    public static Sprite GetAchievementDifficultySprite(string difficulty) {
        return Resources.Load<Sprite>(inst.AchievementDifficultyResourcePath + difficulty);
    }

    public static bool FileExists(string path) {
        FileInfo fileInfo = new FileInfo(path);
        return fileInfo.Exists;
    }
    public static string ReadFile(string path) {
        FileStream fileStream = new FileStream(path, FileMode.Open);
        byte[] data = new byte[fileStream.Length];
        fileStream.Read(data, 0, data.Length);
        fileStream.Close();
        return Encoding.UTF8.GetString(data);
    }
    public static void SaveJson<T>(T rawData, string path) where T : class {
        string jsonData = JsonUtility.ToJson(rawData);
        FileStream fileStream = new FileStream(path, FileMode.Create);
        byte[] data = Encoding.UTF8.GetBytes(jsonData);
        fileStream.Write(data, 0, data.Length);
        fileStream.Flush();
        fileStream.Close();
    }
    public static T LoadJson<T>(string path) where T : class {
        if(!FileExists(path))
            return default(T);

        return JsonUtility.FromJson<T>(ReadFile(path));
    }
}