using System.Collections;
using System.Collections.Generic;
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
    public BlindsVisualizer blinds;

    /// <summary>
    /// The overlay containing the fit to scan user guide.
    /// </summary>
    public GameObject FitToScanOverlay;

	private Dictionary<int, AugmentedVisualizerSensor> m_Visualizers
		= new Dictionary<int, AugmentedVisualizerSensor>();

    private List<AugmentedImage> m_TempAugmentedImages = new List<AugmentedImage>();

    public GameObject DoublePaneWindow;
    public GameObject TriplePaneWindow;
    public Button DoublePaneButton;
    public Button TriplePaneButton;
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
    private BlindsVisualizer visualizerBlinds = null;
    private GameObject doubleWindow = null;
    private GameObject tripleWindow = null;
    public Slider slider;
    public Text monthIndicator;
    private bool planeFound = false;
    private bool lampOn = false;
    private bool runHumanScene = false;
    private bool movingWindow = false;
    private bool doubleSelected = false;
    private bool tripleSelected = false;

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
            // Upon detection of the "Sensors" image run the Sensors scene
			if (image.TrackingState == TrackingState.Tracking && visualizer == null && image.Name == "Sensors")
			{
				// Create an anchor to ensure that ARCore keeps tracking this augmented image.
				Anchor anchor = image.CreateAnchor(image.CenterPose);
				visualizer = (AugmentedVisualizerSensor)Instantiate(AugmentedVisualizerSensorPrefab, anchor.transform);
				visualizer.Image = image;
				m_Visualizers.Add(image.DatabaseIndex, visualizer);
			}
            // Upon detection of the "LightBulb" image run the Light Bulb/Human scene
            if (image.TrackingState == TrackingState.Tracking && visualizerBlinds == null && doubleWindow == null && runHumanScene == false && image.Name == "LightBulb")
            {
                if (runHumanScene == false)
                {
                    SearchingForPlaneUI.SetActive(true);
                    ExitButton.gameObject.SetActive(true);
                    ExitButton.onClick.AddListener(ExitHumanScene);
                    runHumanScene = true;
                    FindObjectOfType<ARCoreSession>().SessionConfig.PlaneFindingMode = DetectedPlaneFindingMode.Horizontal;
                }
            }
            // Upon detection of the "Blinds" image run the Automated Blinds scene
            if (image.TrackingState == TrackingState.Tracking && visualizerBlinds == null && doubleWindow == null && runHumanScene == false && image.Name == "Blinds")
            {
                slider.gameObject.SetActive(true);

                Anchor anchor = image.CreateAnchor(image.CenterPose);
                visualizerBlinds = (BlindsVisualizer)Instantiate(blinds, anchor.transform);
                visualizerBlinds.Image = image;

                BlindsScene();
            }
            // Upon detection of the "Window" image run the Triple Pane Windows scene
            if (image.TrackingState == TrackingState.Tracking && visualizerBlinds == null && doubleWindow == null && runHumanScene == false && image.Name == "Window")
            {
                Anchor anchor = image.CreateAnchor(image.CenterPose);
                doubleWindow = Instantiate(DoublePaneWindow, anchor.transform);
                tripleWindow = Instantiate(TriplePaneWindow, anchor.transform);
                doubleWindow.SetActive(false);
                tripleWindow.SetActive(false);

                WindowScene();
            }
            if (image.TrackingState == TrackingState.Stopped && visualizer != null)
			{
				m_Visualizers.Remove(image.DatabaseIndex);
				GameObject.Destroy(visualizer.gameObject);
			}
            if (image.TrackingState == TrackingState.Stopped && visualizerBlinds != null)
            {
                GameObject.Destroy(visualizerBlinds.gameObject);
            }
            if (image.TrackingState == TrackingState.Stopped && doubleWindow != null)
            {
                GameObject.Destroy(doubleWindow);
                GameObject.Destroy(tripleWindow);
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
        // Do not show the fit-to-scan overlay if a scene is running
        if (runHumanScene == true || visualizerBlinds != null || doubleWindow != null)
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

    void ExitBlindsScene()
    {
        GameObject.Destroy(visualizerBlinds.gameObject);

        // turn off UI elements
        ExitButton.gameObject.SetActive(false);
        ExitButton.onClick.RemoveListener(ExitBlindsScene);
        slider.gameObject.SetActive(false);
        monthIndicator.gameObject.SetActive(false);
    }

    void BlindsScene()
    {
        // turn on UI elements
        ExitButton.gameObject.SetActive(true);
        ExitButton.onClick.AddListener(ExitBlindsScene);
        monthIndicator.gameObject.SetActive(true);
    }

    void DoublePane()
    {
        if (movingWindow == false)
        {
            doubleWindow.SetActive(true);

            ColorBlock doublePaneColors = DoublePaneButton.colors;
            doublePaneColors.highlightedColor = new Color(0.7f, 0.85f, 0.9f, 1f);
            DoublePaneButton.colors = doublePaneColors;

            if (doubleSelected == false)
            {
                doubleSelected = true;
                StartCoroutine(MoveDoubleIn());
            }

            if (tripleWindow.activeSelf == true)
            {
                StartCoroutine(MoveTripleOut());
            }

            tripleSelected = false;
        }
    }

    void TriplePane()
    {
        if (movingWindow == false)
        {
            tripleWindow.SetActive(true);

            ColorBlock triplePaneColors = TriplePaneButton.colors;
            triplePaneColors.highlightedColor = new Color(0.7f, 0.85f, 0.9f, 1f);
            TriplePaneButton.colors = triplePaneColors;

            if (tripleSelected == false)
            {
                tripleSelected = true;
                StartCoroutine(MoveTripleIn());
            }

            if (doubleWindow.activeSelf == true)
            {
                StartCoroutine(MoveDoubleOut());
            }

            doubleSelected = false;
        }
    }

    void ExitWindowScene()
    {
        movingWindow = false;
        doubleSelected = false;
        tripleSelected = false;
        GameObject.Destroy(doubleWindow);
        GameObject.Destroy(tripleWindow);

        // turn on UI elements
        ExitButton.gameObject.SetActive(false);
        ExitButton.onClick.RemoveListener(ExitWindowScene);
        DoublePaneButton.gameObject.SetActive(false);
        DoublePaneButton.onClick.RemoveListener(DoublePane);
        TriplePaneButton.gameObject.SetActive(false);
        DoublePaneButton.onClick.RemoveListener(TriplePane);
    }

    void WindowScene()
    {
        // turn on UI elements
        ExitButton.gameObject.SetActive(true);
        ExitButton.onClick.AddListener(ExitWindowScene);

        DoublePaneButton.gameObject.SetActive(true);
        DoublePaneButton.onClick.AddListener(DoublePane);

        TriplePaneButton.gameObject.SetActive(true);
        TriplePaneButton.onClick.AddListener(TriplePane);
    }

    IEnumerator MoveDoubleIn()
    {
        movingWindow = true;
        float t = 0.0f;
        Vector3 start = new Vector3(doubleWindow.transform.position.x - 5.0f,
                                    doubleWindow.transform.position.y,
                                    doubleWindow.transform.position.z - 0.5f);
        Vector3 middle = new Vector3(doubleWindow.transform.position.x,
                                     doubleWindow.transform.position.y,
                                     doubleWindow.transform.position.z - 0.5f);
        Vector3 end = doubleWindow.transform.position;
        while (t < 1.0f)
        {
            t += Time.deltaTime;
            doubleWindow.transform.position = Vector3.Lerp(start, middle, t / 1.0f);
            yield return null;
        }
        while (t < 1.5f)
        {
            t += Time.deltaTime;
            doubleWindow.transform.position = Vector3.Lerp(middle, end, (t-1.0f) / 0.5f);
            yield return null;
        }
        movingWindow = false;
    }

    IEnumerator MoveDoubleOut()
    {
        movingWindow = true;
        float t = 0.0f;
        Vector3 start = doubleWindow.transform.position;
        Vector3 middle = new Vector3(doubleWindow.transform.position.x,
                                     doubleWindow.transform.position.y,
                                     doubleWindow.transform.position.z - 0.5f);
        Vector3 end = new Vector3(doubleWindow.transform.position.x - 5.0f,
                                  doubleWindow.transform.position.y,
                                  doubleWindow.transform.position.z - 0.5f);
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            doubleWindow.transform.position = Vector3.Lerp(start, middle, t / 0.5f);
            yield return null;
        }
        while (t < 1.5f)
        {
            t += Time.deltaTime;
            doubleWindow.transform.position = Vector3.Lerp(middle, end, (t - 0.5f) / 1.0f);
            yield return null;
        }
        doubleWindow.transform.position = start;
        movingWindow = false;
        doubleWindow.SetActive(false);
    }

    IEnumerator MoveTripleIn()
    {
        movingWindow = true;
        float t = 0.0f;
        Vector3 start = new Vector3(tripleWindow.transform.position.x + 5.0f,
                                    tripleWindow.transform.position.y,
                                    tripleWindow.transform.position.z - 0.5f);
        Vector3 middle = new Vector3(tripleWindow.transform.position.x,
                                     tripleWindow.transform.position.y,
                                     tripleWindow.transform.position.z - 0.5f);
        Vector3 end = tripleWindow.transform.position;
        while (t < 1.0f)
        {
            t += Time.deltaTime;
            tripleWindow.transform.position = Vector3.Lerp(start, middle, t / 1.0f);
            yield return null;
        }
        while (t < 1.5f)
        {
            t += Time.deltaTime;
            tripleWindow.transform.position = Vector3.Lerp(middle, end, (t - 1.0f) / 0.5f);
            yield return null;
        }
        movingWindow = false;
    }

    IEnumerator MoveTripleOut()
    {
        movingWindow = true;
        float t = 0.0f;
        Vector3 start = tripleWindow.transform.position;
        Vector3 middle = new Vector3(tripleWindow.transform.position.x,
                                     tripleWindow.transform.position.y,
                                     tripleWindow.transform.position.z - 0.5f);
        Vector3 end = new Vector3(tripleWindow.transform.position.x + 5.0f,
                                  tripleWindow.transform.position.y,
                                  tripleWindow.transform.position.z - 0.5f);
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            tripleWindow.transform.position = Vector3.Lerp(start, middle, t / 0.5f);
            yield return null;
        }
        while (t < 1.5f)
        {
            t += Time.deltaTime;
            tripleWindow.transform.position = Vector3.Lerp(middle, end, (t - 0.5f) / 1.0f);
            yield return null;
        }
        tripleWindow.transform.position = start;
        movingWindow = false;
        tripleWindow.SetActive(false);
    }

}
