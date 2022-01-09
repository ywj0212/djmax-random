using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class A_ReorderableList : MonoBehaviour
{
    [HideInInspector] public byte Lv;
    [HideInInspector] public byte Floor;
    public ReorderableList List;

    public void SetState(bool state) {
        List.enabled = state;
        List.IsDraggable = state;
        List.IsDropable = state;
    }
    public void DropableCheck(byte? Lv) {
        if(!Lv.HasValue) List.IsDropable = true;
        else if(Lv.Value == this.Lv) List.IsDropable = true;
        else List.IsDropable = false;
    }
    
#if !UNITY_WEBGL
    private void OnDestroy() {
        BoardManager.BoardReorderEvent -= new BoardManager.BoardReorderCall(this.DropableCheck);
    }
#endif
}
