using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using GoogleARCore;
using GoogleARCore.HelloAR;

public class ARGameController : MonoBehaviour
{
	/// <summary>
	/// The first-person camera being used to render the passthrough camera image (i.e. AR background).
	/// </summary>
	public Camera FirstPersonCamera;

	/// <summary>
	/// A prefab for tracking and visualizing detected planes.
	/// </summary>
	public GameObject TrackedPlanePrefab;

	/// <summary>
	/// A model to place when a raycast from a user touch hits a plane.
	/// </summary>
	public GameObject ObjectToClonePrefab;

	/// <summary>
	/// A gameobject parenting UI for displaying the "searching for planes" snackbar.
	/// </summary>
	public GameObject SearchingForPlaneUI;

	/// <summary>
	/// A list to hold new planes ARCore began tracking in the current frame. This object is used across
	/// the application to avoid per-frame allocations.
	/// </summary>
	private List<TrackedPlane> m_NewPlanes = new List<TrackedPlane>();

	/// <summary>
	/// A list to hold all planes ARCore is tracking in the current frame. This object is used across
	/// the application to avoid per-frame allocations.
	/// </summary>
	private List<TrackedPlane> m_AllPlanes = new List<TrackedPlane>();

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

	// Indicates the object was presented in the virtual space.
	private bool m_isObjectDrawn = false;

	public void Start()
	{
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

		if (m_leftButtonPrefab != null)
		{
			m_leftButtonPrefab.onClick.AddListener(delegate {
				OnLeftClicked();
			});
		}

		if (m_rightButtonPrefab != null)
		{
			m_rightButtonPrefab.onClick.AddListener(delegate {
				OnRightClicked();
			});
		}
	}

	public void DrawObject()
	{
		// Check that motion tracking is tracking.
		if (Frame.TrackingState != TrackingState.Tracking)
		{
			const int LOST_TRACKING_SLEEP_TIMEOUT = 15;
			Screen.sleepTimeout = LOST_TRACKING_SLEEP_TIMEOUT;
			return;
		}

		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		// Iterate over planes found in this frame and instantiate corresponding GameObjects to visualize them.
		Frame.GetPlanes(m_NewPlanes, TrackableQueryFilter.New);
		for (int i = 0; i < m_NewPlanes.Count; i++)
		{
			// Instantiate a plane visualization prefab and set it to track the new plane. The transform is set to
			// the origin with an identity rotation since the mesh for our prefab is updated in Unity World
			// coordinates.
			GameObject planeObject = Instantiate(TrackedPlanePrefab, Vector3.zero, Quaternion.identity,
				transform);
			planeObject.GetComponent<TrackedPlaneVisualizer>().Initialize(m_NewPlanes[i]);
		}

		// Disable the snackbar UI when no planes are valid.
		Frame.GetPlanes(m_AllPlanes);
		bool showSearchingUI = true;
		for (int i = 0; i < m_AllPlanes.Count; i++)
		{
			if (m_AllPlanes[i].TrackingState == TrackingState.Tracking)
			{
				showSearchingUI = false;
				break;
			}
		}

		SearchingForPlaneUI.SetActive(showSearchingUI);
		PresentControls (!showSearchingUI);

		// If the player has not touched the screen, we are done with this update.
		Touch touch;
		if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
		{
			return;
		}

		// Raycast against the location the player touched to search for planes.
		TrackableHit hit;
		TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinBounds | TrackableHitFlags.PlaneWithinPolygon;

		if (Session.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
		{
			m_canDrawObject = false;

			// Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
			// world evolves.
			var anchor = hit.Trackable.CreateAnchor(hit.Pose);
			PresentObject (hit, anchor);
		}
	}


	public void Update()
	{
		if (m_canDrawObject)
		{
			DrawObject();
		}
	}

	private void PresentObject(TrackableHit hit, Anchor anchor) 
	{
		Debug.Log ("ARGameController: m_mainObjectPrefab=" + m_mainObjectPrefab);

		if (m_mainObjectPrefab == null) {			

			var objectToClone = Instantiate(ObjectToClonePrefab, hit.Pose.position, hit.Pose.rotation);

			// Andy should look at the camera but still be flush with the plane.
			objectToClone.transform.LookAt(FirstPersonCamera.transform);
			objectToClone.transform.rotation = Quaternion.Euler(0.0f,
				objectToClone.transform.rotation.eulerAngles.y, objectToClone.transform.rotation.z);
			
			objectToClone.transform.localScale = new Vector3 (
						m_mainObjectSizeSlider.value, 
						m_mainObjectSizeSlider.value, 
						m_mainObjectSizeSlider.value);

			// Make Andy model a child of the anchor.
			objectToClone.transform.parent = anchor.transform;

			Debug.Log ("Object Attached!");

			m_mainObjectPrefab = objectToClone;

			m_isObjectDrawn = true;

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
		GameLogic.Instance.OnLeftClick ();
	}

	private void OnRightClicked()
	{
		Debug.Log ("ARGameController: On Right");
		GameLogic.Instance.OnRightClick ();
	}
}