using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

// TODO 디버그 용 임포트
using System.Diagnostics;

public class ImageDownloadTest : MonoBehaviour
{
    private void Start() {
        print("Loading...");
        StartCoroutine(GetTexture());
    }
 
    private IEnumerator GetTexture() {
        Stopwatch stopwatch = new Stopwatch(); //객체 선언
        stopwatch.Start(); // 시간측정 시작

        UnityWebRequest webReq = UnityWebRequestTexture.GetTexture("https://req.mirix.kr/dmrv-random/BG.png");
        yield return webReq.SendWebRequest();

        if(webReq.result == UnityWebRequest.Result.ConnectionError || webReq.result == UnityWebRequest.Result.ProtocolError) {
            print(webReq.error);
        }
        else {
            Texture2D texture = ((DownloadHandlerTexture)webReq.downloadHandler).texture;

            Rect rect = new Rect(0, 0, texture.width, texture.height);
            GetComponent<Image>().sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
        }

        stopwatch.Stop();
        print($"Done! (elapsed time: {stopwatch.ElapsedMilliseconds}ms)");
    }
}
