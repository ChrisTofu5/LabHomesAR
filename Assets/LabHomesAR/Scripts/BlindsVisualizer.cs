using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GoogleARCore;
using GoogleARCoreInternal;
using UnityEngine;
using UnityEngine.UI;


public class BlindsVisualizer : MonoBehaviour
{
    public AugmentedImage Image;
    public GameObject blinds;

    public void Update()
    {
        if (Image == null || Image.TrackingState != TrackingState.Tracking)
        {
            blinds.SetActive(false);
            return;
        }
        blinds.SetActive(true);
    }
}