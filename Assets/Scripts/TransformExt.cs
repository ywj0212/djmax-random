using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExt
{
    public static int ChildCountActive( this Transform t )
    {
        int k = 0;
        foreach(Transform c in t)
        {
            if(c.gameObject.activeSelf)
            k++;
        }
        return k;
    }
}
