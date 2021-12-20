using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Mirix.DMRV;
using DG.Tweening;

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

    [Header("References")]
    [SerializeField] private BoardManager BoardManager;

    [Header("Resources Path")]
    [SerializeField] private string ThumbnailResourcePath;
    [SerializeField] private string PreviewResourcePath;
    [SerializeField] private string DifficultyResourcePath;
    [SerializeField] private string CategoryResourcePath;
    [SerializeField] private string AchievementTagResourcePath;

    private string BoardRootPath             => string.Format("{0}/{1}", Application.persistentDataPath, BoardDataRoot);
    private string AchievementPath           => string.Format("{0}/{1}.json", Application.persistentDataPath, "achievements");
    private string SettingPath               => string.Format("{0}/{1}.json", Application.persistentDataPath, "setting");
    private string CustomAchievementListPath => string.Format("{0}/{1}.json", Application.persistentDataPath, "customlist");
    
    public static MainData MainData = new MainData();

    public static AchievementData   AchievementData;
    public static List<BoardInfo>   Boards = new List<BoardInfo>();
    public static SettingData       SettingData;

    public static void LoadData() {
        Boards.AddRange(MainData.DefaultBoards);

        BinaryFormatter BF = new BinaryFormatter();
        DirectoryInfo DI = new DirectoryInfo(inst.BoardRootPath);
        if(!DI.Exists)
            Directory.CreateDirectory(inst.BoardRootPath);
        foreach(FileInfo f in DI.GetFiles()) {
            BoardInfo BI = new BoardInfo(f.Name, (Board)BF.Deserialize(new FileStream(f.FullName, FileMode.Open)));
            Boards.Add(BI);
        }

        AchievementData        = LoadJson<AchievementData>(inst.AchievementPath) ?? new AchievementData();
        SettingData            = LoadJson<SettingData>    (inst.SettingPath) ?? new SettingData();

        inst.BoardManager.ChangeBoard(0);
        inst.BoardManager.UpdateBoardDropdown();
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

    public static void SaveAchievementState(ushort index, Achievement.State state) {
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
    public static void SaveAchievementRate(ushort index, float rate) {
        Achievement achievement = AchievementData.achievements.FirstOrDefault((x) => (x.Index == index));
        if(achievement == null) {
            achievement = new Achievement();
            AchievementData.achievements.Add(achievement);
            achievement.Index = index;
            achievement.Status = Achievement.State.None;
        }

        achievement.Rate = rate;

        SaveJson<AchievementData>(AchievementData, inst.AchievementPath);
    }
    public static Achievement GetAchievementSave(ushort index) {
        Achievement achievement = AchievementData.achievements.FirstOrDefault((x) => (x.Index == index));
        if(achievement == null) {
            achievement = new Achievement();
            AchievementData.achievements.Add(achievement);
            achievement.Index = index;
            achievement.Status = Achievement.State.None;
            achievement.Rate = 0f;
        }

        return achievement;
    }
    
    public static MainData.TrackInfo GetTrackData(ushort index) {
        return MainData.TrackTable[index];
    }
    public static MainData.SongInfo GetSongData(ushort index) {
        return MainData.SongTable[index];
    }
    public static string GetCategoryFullName(string abbr) {
        try {
            return MainData.CategoryAbbr[abbr];
        }
        catch {
            return "COLLABORATION";
        }
    }
    public static Sprite GetDifficultySprite(string button, string difficulty) {
        return Resources.Load<Sprite>(inst.DifficultyResourcePath + button + difficulty) ?? Resources.Load<Sprite>(inst.DifficultyResourcePath + "4BNM");
    }
    public static Sprite GetAchievementDifficultySprite(string difficulty) {
        return Resources.Load<Sprite>(inst.AchievementTagResourcePath + difficulty);
    }
    public static Sprite GetAchievementButtonSprite(string button) {
        return Resources.Load<Sprite>(inst.AchievementTagResourcePath + button);
    }
    
    public static void GetCategorySprite(Image target, string category) {
        if(!MainData.CategoryAbbr.ContainsKey(category))
            category = "COLLAB";
        
        inst.StartCoroutine(inst.ApplySpriteFromWeb(target, $"category/{category}"));
    }
    public static void GetThumbnailSprite(Image target, ushort index) {
        inst.StartCoroutine(inst.ApplySpriteFromWeb(target, $"thumbnail/{index}"));
    }
    public static void GetPreviewSprite(Image target, ushort index) {
        inst.StartCoroutine(inst.ApplySpriteFromWeb(target, $"preview/{index}"));
    }

    public static void GetLoadingSprite(Image target) {
        inst.StartCoroutine(inst.ApplySpriteFromWeb(target, $"BG"));
    }
    public static void GetLoadingSprite(Image target, ushort index) {
        inst.StartCoroutine(inst.ApplySpriteFromWeb(target, $"loading/{index}"));
    }

    private IEnumerator ApplySpriteFromWeb(Image target, string path)  {
        target.fillAmount = 0;
        string webroot = "https://req.mirix.kr/dmrv-random/";
        string url = webroot + path + ".png";
        UnityWebRequest webReq = UnityWebRequestTexture.GetTexture(url);
        yield return webReq.SendWebRequest();

        if(webReq.result == UnityWebRequest.Result.ConnectionError || webReq.result == UnityWebRequest.Result.ProtocolError) {
            print($"{webReq.error} at {path}");
        }
        else {
            Texture2D texture = ((DownloadHandlerTexture)webReq.downloadHandler).texture;
            Rect rect = new Rect(0, 0, texture.width, texture.height);

            if(target == null || target.gameObject == null)
                yield break;

            target.sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
            Tween tw = DOTween.To(() => target.fillAmount, x => target.fillAmount = x, 1f, 0.3f).SetEase(Ease.InOutCirc);
            target.GetComponent<ImageTweenDestroyer>().Tween = tw;
        }
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