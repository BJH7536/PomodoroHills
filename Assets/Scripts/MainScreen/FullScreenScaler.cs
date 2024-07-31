using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullScreenScaler : MonoBehaviour
{
    private void Awake()
    {
        Screen.fullScreen = false;
    }
}
