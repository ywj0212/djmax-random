using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoardTrack : MonoBehaviour
{
    public Image Indicator;
    public Image IndicatorIcon;
    public TextMeshProUGUI Rate;

    public Button OpenInfo;
    public GameObject LevelMismatchAlert;

    [HideInInspector] public ushort Index;
    [HideInInspector] public ushort SongIndex;
}
