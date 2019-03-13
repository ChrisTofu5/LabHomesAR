﻿using System.Collections.Generic;
using System.Runtime.InteropServices;
using GoogleARCore;
using UnityEngine;
using UnityEngine.UI;

public class LabHomesARController : MonoBehaviour
{
	/// <summary>
	/// A prefab for visualizing an AugmentedImage.
	/// </summary>
	public AugmentedVisualizerSensor AugmentedVisualizerSensorPrefab;

	/// <summary>
	/// The overlay containing the fit to scan user guide.
	/// </summary>
	public GameObject FitToScanOverlay;

	private Dictionary<int, AugmentedVisualizerSensor> m_Visualizers
		= new Dictionary<int, AugmentedVisualizerSensor>();

	private List<AugmentedImage> m_TempAugmentedImages = new List<AugmentedImage>();

    public GameObject DetectedPlanePrefab;
    public GameObject Lamp;
    public GameObject Human;
    public Transform User;
    public Button LampButton;
    public Button ExitButton;
    public GameObject SearchingForPlaneUI;
    private List<DetectedPlane> m_AllPlanes = new List<DetectedPlane>();
    private GameObject humanObject;
    private GameObject lampObject;
    private bool planeFound = false;
    private bool lampOn = false;
    private bool runHumanScene = false;

    /// <summary>
    /// The Unity Update method.
    /// </summary>
    public void Update()
	{
        // Run the Light Bulb/Human scene if the target image was detected
        if (runHumanScene == true)
        {
            HumanScene();
        }

        // Exit the app when the 'back' button is pressed.
        if (Input.GetKey(KeyCode.Escape))
		{
			Application.Quit();
		}

		// Check that motion tracking is tracking.
		if (Session.Status != SessionStatus.Tracking)
		{
			return;
		}

		// Get updated augmented images for this frame.
		Session.GetTrackables<AugmentedImage>(m_TempAugmentedImages, TrackableQueryFilter.Updated);

		// Create visualizers and anchors for updated augmented images that are tracking and do not previously
		// have a visualizer. Remove visualizers for stopped images.
		foreach (var image in m_TempAugmentedImages)
		{
			AugmentedVisualizerSensor visualizer = null;
			m_Visualizers.TryGetValue(image.DatabaseIndex, out visualizer);
            // Upon detection of the "Dog" image run the sensors scene
			if (image.TrackingState == TrackingState.Tracking && visualizer == null && image.Name == "Dog")
			{
				// Create an anchor to ensure that ARCore keeps tracking this augmented image.
				Anchor anchor = image.CreateAnchor(image.CenterPose);
				visualizer = (AugmentedVisualizerSensor)Instantiate(AugmentedVisualizerSensorPrefab, anchor.transform);
				visualizer.Image = image;
				m_Visualizers.Add(image.DatabaseIndex, visualizer);
			}
            // Upon detection of the "Earth" image run the Light Bulb/Human scene
            if (image.TrackingState == TrackingState.Tracking && visualizer == null && image.Name == "Earth")
            {
                if (runHumanScene == false)
                {
                    SearchingForPlaneUI.SetActive(true);
                    ExitButton.gameObject.SetActive(true);
                    ExitButton.onClick.AddListener(ExitHumanScene);
                }
                runHumanScene = true;
                FindObjectOfType<ARCoreSession>().SessionConfig.PlaneFindingMode = DetectedPlaneFindingMode.Horizontal;
            }
            if (image.TrackingState == TrackingState.Stopped && visualizer != null)
			{
				m_Visualizers.Remove(image.DatabaseIndex);
				GameObject.Destroy(visualizer.gameObject);
			}
		}

        // Show the fit-to-scan overlay if there are no images that are Tracking.
        foreach (var visualizer in m_Visualizers.Values)
        {
            if (visualizer.Image.TrackingState == TrackingState.Tracking)
            {
                FitToScanOverlay.SetActive(false);
                return;
            }
        }
        // Do not show the fit-to-scan overlay if the Light Bulb/Human scene is running
        if (runHumanScene == true)
        {
            FitToScanOverlay.SetActive(false);
        }
        else
        {
            FitToScanOverlay.SetActive(true);
        }
    }

    // This function toggles the lamp for the Light Bulb/Human scene
    void ToggleLamp()
    {
        if (lampOn == false)
        {
            humanObject.SetActive(true);
            LampButton.GetComponentInChildren<Text>().text = "Turn Lamp Off";
            lampObject.GetComponentInChildren<Light>().enabled = true;
            lampOn = true;
        }
        else
        {
            humanObject.SetActive(false);
            LampButton.GetComponentInChildren<Text>().text = "Turn Lamp On";
            lampObject.GetComponentInChildren<Light>().enabled = false;
            lampOn = false;
        }
    }

    // This function exits the Light Bulb/Human scene and resets several variables
    void ExitHumanScene()
    {
        planeFound = false;
        lampOn = false;
        runHumanScene = false;
        Destroy(lampObject);
        Destroy(humanObject);
        LampButton.GetComponentInChildren<Text>().text = "Turn Lamp On";
        LampButton.gameObject.SetActive(false);
        LampButton.onClick.RemoveListener(ToggleLamp);
        ExitButton.gameObject.SetActive(false);
        ExitButton.onClick.RemoveListener(ExitHumanScene);
        SearchingForPlaneUI.SetActive(false);
        FindObjectOfType<ARCoreSession>().SessionConfig.PlaneFindingMode = DetectedPlaneFindingMode.Disabled;
    }

    // This function runs the Light Bulb/Human scene
    void HumanScene()
    {
        Session.GetTrackables<DetectedPlane>(m_AllPlanes);
        for (int i = 0; i < m_AllPlanes.Count; i++)
        {
            if (m_AllPlanes[i].TrackingState == TrackingState.Tracking && planeFound == false)
            {
                // Add the lamp to the scene
                lampObject = Instantiate(Lamp, m_AllPlanes[i].CenterPose.position, m_AllPlanes[i].CenterPose.rotation);

                // Add the human to the scene
                humanObject = Instantiate(Human, m_AllPlanes[i].CenterPose.position, m_AllPlanes[i].CenterPose.rotation);

                var anchor = m_AllPlanes[i].CreateAnchor(m_AllPlanes[i].CenterPose);
                // Make lamp model a child of the anchor.
                lampObject.transform.parent = anchor.transform;
                // Make human model a child of the anchor.
                humanObject.transform.parent = anchor.transform;

                // Have the human face the user
                Vector3 targetPostition = new Vector3(User.position.x,
                                        humanObject.transform.position.y,
                                        User.position.z);
                humanObject.transform.LookAt(targetPostition);

                // Spawn the lamp 0.45 meters away from the human
                lampObject.transform.position = new Vector3(lampObject.transform.position.x - 0.45f,
                                              lampObject.transform.position.y,
                                              lampObject.transform.position.z);

                humanObject.SetActive(false);

                LampButton.gameObject.SetActive(true);
                LampButton.onClick.AddListener(ToggleLamp);

                SearchingForPlaneUI.SetActive(false);

                planeFound = true;
                break;
            }
        }
    }

}