using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickRecordButton : MonoBehaviour
{
    private Animator animator;
    private void Start() {
        animator = GetComponent<Animator>();
    }

    public void HoverStart() {
        animator.Play("On");
    }
    public void HoverEnd() {
        animator.Play("Off");
    }
}
