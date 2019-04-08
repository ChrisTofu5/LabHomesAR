//-----------------------------------------------------------------------
// File: BlindsVisualizer.cs
// Author: Moriyoshi Rempola
// Application: Lab Homes AR
// Programming Language: C#
// Course: Computer Science 423
// Semester: Spring 2019
// Team: Night Owls
//-----------------------------------------------------------------------

using GoogleARCore;
using UnityEngine;

public class BlindsVisualizer : MonoBehaviour
{
    public AugmentedImage Image;
    public GameObject blinds;

    // Update is called once per frame. It is used here to show the blinds if the target image is detected.
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
