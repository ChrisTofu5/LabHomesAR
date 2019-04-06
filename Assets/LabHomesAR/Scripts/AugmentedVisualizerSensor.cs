using GoogleARCore;
using UnityEngine;

public class AugmentedVisualizerSensor : MonoBehaviour
{
    public AugmentedImage Image;
    public GameObject indoorTempData;
    public GameObject windowTempData;

    // Update is called once per frame. It is used here to show the sensors graphs if the target image is detected.
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
