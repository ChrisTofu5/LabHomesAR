//-----------------------------------------------------------------------
// File: LabHomesARController.cs
// Author: Christopher McCall
// Application: Lab Homes AR
// Programming Language: C#
// Course: Computer Science 423
// Semester: Spring 2019
// Team: Night Owls
//-----------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;
using UnityEngine.UI;

public class LabHomesARController : MonoBehaviour
{
    // Prefabs for visualizing an AugmentedImage.
    public AugmentedVisualizerSensor AugmentedVisualizerSensorPrefab;
    public BlindsVisualizer blinds;

    // The overlay containing the fit to scan user guide.
    public GameObject FitToScanOverlay;

    // Objects that are connected to the script through the scene
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
    public Slider slider;
    public Text monthIndicator;

    // Objects and variables that are defined in the script
    private GameObject ARCoreDevice;
    private List<AugmentedImage> m_TempAugmentedImages = new List<AugmentedImage>();
    private List<DetectedPlane> m_AllPlanes = new List<DetectedPlane>();
    private GameObject humanObject;
    private GameObject lampObject;
    private AugmentedVisualizerSensor sensorsVisualizer = null;
    private BlindsVisualizer blindsVisualizer = null;
    private GameObject doubleWindow = null;
    private GameObject tripleWindow = null;
    private bool planeFound = false;
    private bool lampOn = false;
    private bool runHumanScene = false;
    private bool movingWindow = false;
    private bool doubleSelected = false;
    private bool tripleSelected = false;
    private bool lampOnAudioPlayed = false;
    private bool doubleAudioPlayed = false;
    private bool tripleAudioPlayed = false;

    // The audio clips
    public AudioClip Sensors;
    public AudioClip AutoBlindsUp;
    public AudioClip AutoBlindsDown;
    public AudioClip LampOff;
    public AudioClip LampOn;
    public AudioClip DoubleSelected;
    public AudioClip TripleSelected;
    public AudioClip BothSelected;
    private AudioSource audioSource;

    // Start is called on the first frame. It is used here to create an AudioSource to play the audio clips.
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        ARCoreDevice = GameObject.Find("ARCore Device");
    }

    // This function is called every frame and controls the application. It runs the scenes and displays the buttons and 3D objects after the target images are detected.
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

        // Create visualizers and anchors for updated augmented images that are tracking and do not previously have a visualizer.
        foreach (var image in m_TempAugmentedImages)
        {
            // Upon detection of the "Sensors" image run the Sensors scene
            if (image.TrackingState == TrackingState.Tracking && sensorsVisualizer == null && blindsVisualizer == null && doubleWindow == null && runHumanScene == false && image.Name == "Sensors")
            {
                // Create an anchor to ensure that ARCore keeps tracking this augmented image.
                Anchor anchor = image.CreateAnchor(image.CenterPose);
                sensorsVisualizer = (AugmentedVisualizerSensor)Instantiate(AugmentedVisualizerSensorPrefab, anchor.transform);
                sensorsVisualizer.Image = image;

                ExitButton.gameObject.SetActive(true);
                ExitButton.onClick.AddListener(ExitSensorsScene);

                audioSource.Stop();
                audioSource.clip = Sensors;
                audioSource.Play();
            }

            // Upon detection of the "LightBulb" image run the Light Bulb/Human scene
            if (image.TrackingState == TrackingState.Tracking && sensorsVisualizer == null && blindsVisualizer == null && doubleWindow == null && runHumanScene == false && image.Name == "LightBulb")
            {
                runHumanScene = true;
                SearchingForPlaneUI.SetActive(true);
                ExitButton.gameObject.SetActive(true);
                ExitButton.onClick.AddListener(ExitHumanScene);
                FindObjectOfType<ARCoreSession>().SessionConfig.PlaneFindingMode = DetectedPlaneFindingMode.Horizontal;
                audioSource.Stop();
                audioSource.clip = LampOff;
                audioSource.Play();
            }

            // Upon detection of the "Blinds" image run the Automated Blinds scene
            if (image.TrackingState == TrackingState.Tracking && sensorsVisualizer == null && blindsVisualizer == null && doubleWindow == null && runHumanScene == false && image.Name == "Blinds")
            {
                slider.gameObject.SetActive(true);

                Anchor anchor = image.CreateAnchor(image.CenterPose);
                blindsVisualizer = (BlindsVisualizer)Instantiate(blinds, anchor.transform);
                blindsVisualizer.Image = image;

                audioSource.Stop();
                audioSource.clip = AutoBlindsUp;
                audioSource.Play();
                BlindsScene();
            }

            // Upon detection of the "Window" image run the Triple Pane Windows scene
            if (image.TrackingState == TrackingState.Tracking && sensorsVisualizer == null && blindsVisualizer == null && doubleWindow == null && runHumanScene == false && image.Name == "Window")
            {
                Anchor anchor = image.CreateAnchor(image.CenterPose);
                doubleWindow = Instantiate(DoublePaneWindow, anchor.transform);
                tripleWindow = Instantiate(TriplePaneWindow, anchor.transform);
                doubleWindow.SetActive(false);
                tripleWindow.SetActive(false);

                audioSource.Stop();
                WindowScene();
            }
        }

        // Do not show the fit-to-scan overlay if a scene is running
        if (sensorsVisualizer != null || blindsVisualizer != null || doubleWindow != null || runHumanScene == true)
        {
            FitToScanOverlay.SetActive(false);
        }
        else
        {
            FitToScanOverlay.SetActive(true);
        }
    }

    // This function destroys the current ARCore session and creates a new ARCore session
    IEnumerator CreateNewSession()
    {
        // Destroy the current session
        ARCoreSession session = ARCoreDevice.GetComponent<ARCoreSession>();
        ARCoreSessionConfig config = session.SessionConfig;
        DestroyImmediate(session);

        yield return null;

        // Create a new session
        session = ARCoreDevice.AddComponent<ARCoreSession>();
        session.SessionConfig = config;
        session.enabled = true;
    }

    // This function exits the Sensors scene
    void ExitSensorsScene()
    {
        audioSource.Stop();

        GameObject.Destroy(sensorsVisualizer.gameObject);

        // turn off UI elements
        ExitButton.gameObject.SetActive(false);
        ExitButton.onClick.RemoveListener(ExitSensorsScene);

        StartCoroutine(CreateNewSession());
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
            if (lampOnAudioPlayed == false)
            {
                StartCoroutine("LampOnAudio");
                lampOnAudioPlayed = true;
            }
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
        audioSource.Stop();
        StopCoroutine("LampOnAudio");
        lampOnAudioPlayed = false;
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

        StartCoroutine(CreateNewSession());
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

    // This function exits the Automated Blinds scene
    void ExitBlindsScene()
    {
        audioSource.Stop();
        StopCoroutine("CheckBlindsDown");
        StopCoroutine("BlindsDownAudio");

        GameObject.Destroy(blindsVisualizer.gameObject);

        // turn off UI elements
        ExitButton.gameObject.SetActive(false);
        ExitButton.onClick.RemoveListener(ExitBlindsScene);
        slider.gameObject.SetActive(false);
        monthIndicator.gameObject.SetActive(false);

        StartCoroutine(CreateNewSession());
    }

    // This function runs the Automated Blinds scene
    void BlindsScene()
    {
        StartCoroutine("CheckBlindsDown");

        // turn on UI elements
        ExitButton.gameObject.SetActive(true);
        ExitButton.onClick.AddListener(ExitBlindsScene);
        monthIndicator.gameObject.SetActive(true);
    }

    // This function adds the double pane window to the scene after the double pane window button is pressed
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

            if (doubleAudioPlayed == false)
            {
                StartCoroutine("DoubleAudio");
            }
        }
    }

    // This function adds the triple pane window to the scene after the triple pane window button is pressed
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

            if (tripleAudioPlayed == false)
            {
                StartCoroutine("TripleAudio");
            }
        }
    }

    // This function exits the Triple Pane Windows scene and resets several variables
    void ExitWindowScene()
    {
        audioSource.Stop();
        StopCoroutine("DoubleAudio");
        StopCoroutine("TripleAudio");
        StopCoroutine("BothAudio");
        doubleAudioPlayed = false;
        tripleAudioPlayed = false;
        movingWindow = false;
        doubleSelected = false;
        tripleSelected = false;
        GameObject.Destroy(doubleWindow);
        GameObject.Destroy(tripleWindow);

        // turn off UI elements
        ExitButton.gameObject.SetActive(false);
        ExitButton.onClick.RemoveListener(ExitWindowScene);
        DoublePaneButton.gameObject.SetActive(false);
        DoublePaneButton.onClick.RemoveListener(DoublePane);
        TriplePaneButton.gameObject.SetActive(false);
        DoublePaneButton.onClick.RemoveListener(TriplePane);

        StartCoroutine(CreateNewSession());
    }

    // This function runs the Triple Pane Windows scene
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

    // This function slides the double pane window over the real window
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

    // This function slides the double pane window off from the real window
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

    // This function slides the triple pane window over the real window
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

    // This function slides the triple pane window off from the real window
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

    // This function plays audio after the lamp is turned on in the Light Bulb/Human scene
    IEnumerator LampOnAudio()
    {
        while (audioSource.isPlaying)
        {
            yield return null;
        }
        audioSource.clip = LampOn;
        audioSource.Play();
    }

    // This function plays audio after the double pane window is selected in the Triple Pane Windows scene
    IEnumerator DoubleAudio()
    {
        while (audioSource.isPlaying)
        {
            yield return null;
        }
        audioSource.clip = DoubleSelected;
        audioSource.Play();
        doubleAudioPlayed = true;
        if (doubleAudioPlayed && tripleAudioPlayed)
        {
            StartCoroutine("BothAudio");
        }
    }

    // This function plays audio after the triple pane window is selected in the Triple Pane Windows scene
    IEnumerator TripleAudio()
    {
        while (audioSource.isPlaying)
        {
            yield return null;
        }
        audioSource.clip = TripleSelected;
        audioSource.Play();
        tripleAudioPlayed = true;
        if (doubleAudioPlayed && tripleAudioPlayed)
        {
            StartCoroutine("BothAudio");
        }
    }

    // This function plays audio after both windows are selected in the Triple Pane Windows scene
    IEnumerator BothAudio()
    {
        while (audioSource.isPlaying)
        {
            yield return null;
        }
        audioSource.clip = BothSelected;
        audioSource.Play();
    }

    // This function checks if the blinds are on a month where they go down in the Automated Blinds scene
    IEnumerator CheckBlindsDown()
    {
        while (slider.value != 1 && slider.value != 2 && slider.value != 3 && slider.value != 4 && slider.value != 11 && slider.value != 12)
        {
            yield return null;
        }
        StartCoroutine("BlindsDownAudio");
    }

    // This function plays audio after the blinds go down in the Automated Blinds scene
    IEnumerator BlindsDownAudio()
    {
        while (audioSource.isPlaying)
        {
            yield return null;
        }
        audioSource.clip = AutoBlindsDown;
        audioSource.Play();
    }

}
