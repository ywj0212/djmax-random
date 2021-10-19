using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragToToggle : MonoBehaviour
{
    private Toggle toggle;
    private void Start() {
        toggle = GetComponent<Toggle>();
    }

    public void Toggle() {
        if(Input.GetMouseButton(0)) {
            toggle.isOn ^= true;
        }
    }
}
