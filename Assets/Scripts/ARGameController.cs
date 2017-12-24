using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using GoogleAR;
using GoogleARCore;
using GoogleARCore.HelloAR;

public class ARGameController : MonoBehaviour
{
	
	/// <summary>
	/// The first-person camera being used to render the passthrough camera.
	/// </summary>
	public Camera m_firstPersonCamera;

	/// <summary>
	/// A prefab for tracking and visualizing detected planes.
	/// </summary>
	public GameObject m_trackedPlanePrefab;

	/// <summary>
	/// A model to place when a raycast from a user touch hits a plane.
	/// </summary>
	public GameObject m_objectToClonePrefab;

	/// <summary>
	/// A gameobject parenting UI for displaying the "searching for planes" snackbar.
	/// </summary>
	public GameObject m_searchingForPlaneUI;

	/// <summary>
	/// UI elements displayed in the main canvas.
	/// </summary>
	//public GameObject m_scorePrefab;

	/// <summary>
	//Buttons
	/// </summary>
	public Button m_leftButtonPrefab;
	public Button m_rightButtonPrefab;
	public Button m_closeButtonPrefab;

	public GameObject m_sizeSliderContainer;
	public Slider m_mainObjectSizeSlider;

	public GameObject m_uiContainer;

	/// <summary>
	///  The object that will be drawn on the point clicked by the user. 
	/// </summary>
	private GameObject m_mainObjectPrefab;

	// Determine whether we can draw the main scene into the available plane or not. 
	private bool m_canDrawObject = true;

	private List<TrackedPlane> m_newPlanes = new List<TrackedPlane>();

	private List<TrackedPlane> m_allPlanes = new List<TrackedPlane>();

	private Color[] m_planeColors = new Color[] {
		new Color(1.0f, 1.0f, 1.0f),
		new Color(0.956f, 0.262f, 0.211f),
		new Color(0.913f, 0.117f, 0.388f)
	};

	public void Start() {
		
		Debug.Log ("AGC.Start");

		PresentControls (false);

		//Create listener for the slider
		if (m_mainObjectSizeSlider != null) 
		{			
			m_mainObjectSizeSlider.onValueChanged.AddListener(delegate {UpdateObjectScale(); });
		}
			
		if (m_closeButtonPrefab != null) 
		{
			m_closeButtonPrefab.onClick.AddListener(delegate {
				OnCloseClicked();
			});
		}	
	}

	public void DrawObjectToClone()
	{
		Utilities.QuitOnConnectionErrors();

		// The tracking state must be FrameTrackingState.Tracking in order to access the Frame.
		if (Frame.TrackingState != FrameTrackingState.Tracking)
		{
			const int LOST_TRACKING_SLEEP_TIMEOUT = 15;
			Screen.sleepTimeout = LOST_TRACKING_SLEEP_TIMEOUT;
			return;
		}

		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		Frame.GetNewPlanes(ref m_newPlanes);

		// Iterate over planes found in this frame and instantiate corresponding GameObjects to visualize them.
		for (int i = 0; i < m_newPlanes.Count; i++)
		{
			// Instantiate a plane visualization prefab and set it to track the new plane. The transform is set to
			// the origin with an identity rotation since the mesh for our prefab is updated in Unity World
			// coordinates.
			GameObject planeObject = Instantiate(m_trackedPlanePrefab, Vector3.zero, Quaternion.identity,
				transform);
			planeObject.GetComponent<TrackedPlaneVisualizer>().SetTrackedPlane(m_newPlanes[i]);

			// Apply a random color and grid rotation.
			planeObject.GetComponent<Renderer>().material.SetColor("_GridColor", m_planeColors[Random.Range(0,
				m_planeColors.Length - 1)]);
			planeObject.GetComponent<Renderer>().material.SetFloat("_UvRotation", Random.Range(0.0f, 360.0f));
		}

		// Disable the snackbar UI when no planes are valid.
		bool showSearchingUI = true;
		Frame.GetAllPlanes(ref m_allPlanes);
		for (int i = 0; i < m_allPlanes.Count; i++)
		{
			if (m_allPlanes[i].IsValid)
			{
				showSearchingUI = false;
				break;
			}
		}

		m_searchingForPlaneUI.SetActive(showSearchingUI);
		PresentControls (!showSearchingUI);

		Touch touch;
		if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
		{
			return;
		}

		TrackableHit hit;
		TrackableHitFlag raycastFilter = TrackableHitFlag.PlaneWithinBounds | TrackableHitFlag.PlaneWithinPolygon;

		if (Session.Raycast(m_firstPersonCamera.ScreenPointToRay(touch.position), raycastFilter, out hit))
		{
			m_canDrawObject = false;

			// Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
			// world evolves.
			var anchor = Session.CreateAnchor(hit.Point, Quaternion.identity);
			PresentObject (hit, anchor);
		}
	}

	public void Update()
	{
		if (m_canDrawObject)
		{
			DrawObjectToClone ();
		}
	}

	private void PresentObject(TrackableHit hit, Anchor anchor) 
	{
		Debug.Log ("ARGameController: m_mainObjectPrefab=" + m_mainObjectPrefab);

		if (m_mainObjectPrefab == null) {			

			// Intanstiate an Andy Android object as a child of the anchor; it's transform will now benefit
			// from the anchor's tracking.
			var objectToPresent = Instantiate(m_objectToClonePrefab, hit.Point, Quaternion.identity,
				anchor.transform);

			// Andy should look at the camera but still be flush with the plane.
			objectToPresent.transform.LookAt(m_firstPersonCamera.transform);
			objectToPresent.transform.rotation = Quaternion.Euler(0.0f,
				objectToPresent.transform.rotation.eulerAngles.y, objectToPresent.transform.rotation.z);
			objectToPresent.transform.localScale = 
					new Vector3 (
						m_mainObjectSizeSlider.value, 
						m_mainObjectSizeSlider.value, 
						m_mainObjectSizeSlider.value);

			// Use a plane attachment component to maintain Andy's y-offset from the plane
			// (occurs after anchor updates).
			objectToPresent.GetComponent<PlaneAttachment>().Attach(hit.Plane);

			Debug.Log ("Object Attached!");

			m_mainObjectPrefab = objectToPresent;

			Debug.Log ("ARGameController: PresentObject (m_mainObjectPrefab != null)=" + (m_mainObjectPrefab != null));
		} else {
			Debug.Log ("ARGameController: m_mainObjectPrefab!=null. No new instance will be created!");
		}

	}

	// this may be moved to a different prefab/script. A differenet piece of code which 
	// responsibility is only coordinating the visibility of UI elements plus synching with 
	// the game logic
	private void PresentControls(bool showComponents) 
	{
		m_uiContainer.SetActive (showComponents);
	}

	public void UpdateObjectScale() 
	{
		Debug.Log ("ARGameController: Update w/slider (m_mainObjectPrefab != null)=" + (m_mainObjectPrefab != null));
		if (m_mainObjectPrefab != null) {
			Debug.Log ("ARGameController: Update lastTable.gameObject.transform.localScale -> " + m_mainObjectSizeSlider.value);
			m_mainObjectPrefab.gameObject.transform.localScale = 
				new Vector3 (
					m_mainObjectSizeSlider.value, 
					m_mainObjectSizeSlider.value, 
					m_mainObjectSizeSlider.value);
		}
	}

	private void OnCloseClicked() 
	{
		Debug.Log ("ARGameController: Closing app");
		Application.Quit ();
	}

	private void OnLeftClicked() 
	{
		Debug.Log ("ARGameController: On Left");
	}

	private void OnRightClicked()
	{
		Debug.Log ("ARGameController: On Right");
	}
}