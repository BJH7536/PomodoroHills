using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameSetter : MonoBehaviour
{
    [SerializeField] private int targetFrame;
    
    private void Awake()
    {
        Application.targetFrameRate = targetFrame;
    }
}
