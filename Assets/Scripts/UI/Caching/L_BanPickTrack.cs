using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class L_BanPickTrack : MonoBehaviour
{
    public Toggle Toggle;
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Category;
    public Image Thumbnail;
    public Image Difficulty;
    public Animator CrossAnimator;
    public LvToggleParent LvToggleParent;
    public LvToggleParent LvToggleSCParent;

    public void SetState(bool b) {
        if(b) CrossAnimator.SetTrigger("Out");
        else  CrossAnimator.SetTrigger("In");
    }
}