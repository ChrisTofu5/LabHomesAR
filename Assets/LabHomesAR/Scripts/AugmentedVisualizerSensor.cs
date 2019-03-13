using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GoogleARCore;
using GoogleARCoreInternal;
using UnityEngine;

public class AugmentedVisualizerSensor : MonoBehaviour
{
	public AugmentedImage Image;
	public GameObject indoorTempData;
    public GameObject windowTempData;

    public void Update()
	{
		if (Image == null || Image.TrackingState != TrackingState.Tracking)
		{
            indoorTempData.SetActive(false);
            windowTempData.SetActive(false);
            return;
		}

        indoorTempData.SetActive(true);
        windowTempData.SetActive(true);

    }
}
