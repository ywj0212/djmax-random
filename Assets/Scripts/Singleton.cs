using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T inst = null;
    private void Awake() {
        if(inst is null) {
            inst = GetComponent<T>();
        }
        else {
            if(!inst.Equals(this)) {
                Destroy(inst);
            }
        }
    }
    private void OnDestroy() {
        inst = null;
    }
}
