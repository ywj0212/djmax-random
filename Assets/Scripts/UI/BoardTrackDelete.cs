using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardTrackDelete : MonoBehaviour
{
    public void Delete() {
        foreach(Transform t in transform) Destroy(t.gameObject);
        Canvas.ForceUpdateCanvases();
    }
}
