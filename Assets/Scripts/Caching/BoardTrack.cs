using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardTrack : MonoBehaviour
{
    public Button OpenInfo;
    public GameObject LevelMismatchAlert;

    [HideInInspector] public ushort SongIndex;
    [HideInInspector] public string Ctgr;
    [HideInInspector] public string Diff;
}
