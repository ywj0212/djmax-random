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
using AT;

[Serializable] public class AchievementData {
    public List<Achievement> achievements = new List<Achievement>();
}
[Serializable] public class Achievement {
    public ushort Index;
    public float Rate;
    public ushort Break;
    public enum State : sbyte { None = 0, Clear = 1, MaxCombo = 2, Perfect = 3 }
    public State Status;
}
[Serializable] public class BoardData {
    public List<BoardInfo> Boards = new List<BoardInfo>();
}

public class SystemFileIO : Singleton<SystemFileIO>
{
    [Header("References")]
    [SerializeField] private BoardManager BoardManager;

    [Header("Resources")]
    [SerializeField] private SerializableDictionary<string, Sprite> DifficultySprites;
    [SerializeField] private SerializableDictionary<string, Sprite> ButtonDifficultySprites;
    [SerializeField] private SerializableDictionary<string, Sprite> ButtonSprites;

    private static string BoardDataPath     => string.Format("{0}/{1}.bin", Application.persistentDataPath, "boards");
    private static string AchievementPath   => string.Format("{0}/{1}.json", Application.persistentDataPath, "achiv");
    
    public static MainData MainData = new MainData();

    public static AchievementData   AchievementData;
    public static List<BoardInfo>   Boards = new List<BoardInfo>();
    public static BoardData         BoardData = new BoardData();

    public static void LoadData() {
        Boards.AddRange(MainData.DefaultBoards);

        BinaryFormatter BF = new BinaryFormatter();
        FileInfo FI = new FileInfo(BoardDataPath);
        FileStream FS;
        if(!FI.Exists) {
            FS = File.Open(BoardDataPath, FileMode.Create);
            BF.Serialize(FS, new BoardData());
            FS.Close();
        }
        FS = File.Open(BoardDataPath, FileMode.Open);
        BoardData = (BoardData)BF.Deserialize(FS);
        FS.Close();
        Boards.AddRange(BoardData.Boards);

        AchievementData        = LoadJson<AchievementData>(AchievementPath) ?? new AchievementData();

        inst.BoardManager.SetInitBoard();
        inst.BoardManager.UpdateBoardDropdown();
    }
    public static void AddBoard(BoardInfo board) {
        Boards.Add(board);
        BoardData.Boards.Add(board);
    }

    private void SaveBoardData(BoardInfo boardinfo) {
        
    }
    private void CreateBoard(int index) {
        
    }
    private void DeleteBoard(int index) {
        
    }

    public static void SaveAchievementState(ushort index, Achievement.State state) {
        Achievement achievement = AchievementData.achievements.FirstOrDefault((x) => (x.Index == index));
        if(achievement == null) {
            achievement = new Achievement();
            AchievementData.achievements.Add(achievement);
            achievement.Index = index;
            achievement.Break = 0;
            achievement.Status = Achievement.State.None;
        }

        achievement.Status = state;

        SaveJson<AchievementData>(AchievementData, AchievementPath);
    }
    public static void SaveAchievementBreak(ushort index, ushort breaks) {
        Achievement achievement = AchievementData.achievements.FirstOrDefault((x) => (x.Index == index));
        if(achievement == null) {
            achievement = new Achievement();
            AchievementData.achievements.Add(achievement);
            achievement.Index = index;
            achievement.Status = Achievement.State.None;
        }
        achievement.Break = breaks;

        SaveJson<AchievementData>(AchievementData, AchievementPath);
    }
    public static void SaveAchievementRate(ushort index, float rate) {
        Achievement achievement = AchievementData.achievements.FirstOrDefault((x) => (x.Index == index));
        if(achievement == null) {
            achievement = new Achievement();
            AchievementData.achievements.Add(achievement);
            achievement.Index = index;
            achievement.Break = 0;
            achievement.Status = Achievement.State.None;
        }

        achievement.Rate = rate;

        SaveJson<AchievementData>(AchievementData, AchievementPath);
    }
    public static Achievement GetAchievementSave(ushort index) {
        Achievement achievement = AchievementData.achievements.FirstOrDefault((x) => (x.Index == index));
        if(achievement == null) {
            achievement = new Achievement();
            AchievementData.achievements.Add(achievement);
            achievement.Index = index;
            achievement.Break = 0;
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
        if(MainData.CategoryAbbr.ContainsKey(abbr))
            return MainData.CategoryAbbr[abbr];
        else
            return "COLLABORATION";
    }
    public static Sprite GetDifficultySprite(string button, string difficulty) {
        return inst.ButtonDifficultySprites[button+difficulty];
    }
    public static Sprite GetAchievementDifficultySprite(string difficulty) {
        return inst.DifficultySprites[difficulty];
    }
    public static Sprite GetAchievementButtonSprite(string button) {
        return inst.ButtonSprites[button];
    }

    public static ImageTweenDestroyer GetCategorySprite(Image target, string category) {
        if(!MainData.CategoryAbbr.ContainsKey(category))
            category = "COLLAB";
        
        return RequestSpriteFromWeb(target, $"category/{category}");
    }
    public static ImageTweenDestroyer GetThumbnailSprite(Image target, ushort index) {
        return RequestSpriteFromWeb(target, $"thumbnail/{index}");
    }
    public static ImageTweenDestroyer GetPreviewSprite(Image target, ushort index) {
        return RequestSpriteFromWeb(target, $"preview/{index}");
    }

    public static ImageTweenDestroyer GetLoadingSprite(Image target) {
        return RequestSpriteFromWeb(target, "BG");
    }
    public static ImageTweenDestroyer GetLoadingSprite(Image target, ushort index) {
        return RequestSpriteFromWeb(target, $"loading/{index}");
    }

    private static ImageTweenDestroyer RequestSpriteFromWeb(Image target, string path)  {
        ImageTweenDestroyer ITD = target.GetComponent<ImageTweenDestroyer>();
        target.fillAmount = 0;
        ITD.Image = target;
        ITD.Coroutine = inst.StartCoroutine(inst.ApplySpriteFromWeb(ITD, path));

        return ITD;
    }
    private IEnumerator ApplySpriteFromWeb(ImageTweenDestroyer target, string path)  {
        string webroot = "https://req.mirix.kr/dmrv-random/";
        string url = webroot + path + ".png";

        UnityWebRequest webReq = UnityWebRequestTexture.GetTexture(url);
        yield return webReq.SendWebRequest();

        if(webReq.result == UnityWebRequest.Result.ConnectionError || webReq.result == UnityWebRequest.Result.ProtocolError) {
            Debug.LogWarning($"{webReq.error} at {path}");
        }
        else {
            Texture2D texture = ((DownloadHandlerTexture)webReq.downloadHandler).texture;
            Rect rect = new Rect(0, 0, texture.width, texture.height);

            if(target == null || target.gameObject == null)
                yield break;

            target.Texture = texture;
            target.Sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));

            target.Image.sprite = target.Sprite;
            target.Tween = DOTween.To(() => target.Image.fillAmount, x => target.Image.fillAmount = x, 1f, 0.3f).SetDelay(0.2f);
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