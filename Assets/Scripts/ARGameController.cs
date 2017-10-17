using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using GoogleAR;
using GoogleARCore;
using GoogleARCore.HelloAR;
//using UnityEngine.Experimental.UIElements;

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
	public GameObject m_objectPrefab;

	/// <summary>
	/// A gameobject parenting UI for displaying the "searching for planes" snackbar.
	/// </summary>
	public GameObject m_searchingForPlaneUI;

	/// <summary>
	/// UI elements displayed in the main canvas.
	/// </summary>
	public GameObject m_scorePrefab;

	//public GameObject m_optionsPrefab;
	public Button m_leftButtonPrefab;
	public Button m_rightButtonPrefab;
	public Button m_closeButtonPrefab;
	public Button m_connectButtonPrefab;
	public Button m_hostButtonPrefab;
	public GameObject m_ipAddressPrefab;

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
		new Color(0.913f, 0.117f, 0.388f),
		new Color(0.611f, 0.152f, 0.654f),
		new Color(0.403f, 0.227f, 0.717f),
		new Color(0.247f, 0.317f, 0.709f),
		new Color(0.129f, 0.588f, 0.952f),
		new Color(0.011f, 0.662f, 0.956f),
		new Color(0f, 0.737f, 0.831f),
		new Color(0f, 0.588f, 0.533f),
		new Color(0.298f, 0.686f, 0.313f),
		new Color(0.545f, 0.764f, 0.290f),
		new Color(0.803f, 0.862f, 0.223f),
		new Color(1.0f, 0.921f, 0.231f),
		new Color(1.0f, 0.756f, 0.027f)
	};

	public void Start() {
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
		
		if (m_connectButtonPrefab != null) 
		{
			m_connectButtonPrefab.onClick.AddListener(delegate {
				OnConnectClicked();
			});
		}

		if (m_hostButtonPrefab != null) 
		{
			m_hostButtonPrefab.onClick.AddListener(delegate {
				OnHostClicked();
			});
		}
			
	}

	public void Update() {
		//if (m_canDrawObject) 
		{
			DrawObject ();
		}

	}

	/// <summary>
	/// The Unity Update() method.
	/// </summary>
	public void DrawObject ()
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

	private void PresentObject(TrackableHit hit, Anchor anchor) 
	{
		/*
		Debug.Log ("ARGameController: PresentObject (m_objectPrefab != null)=" + (m_objectPrefab != null));
		Debug.Log ("ARGameController: hit=" + hit);
		Debug.Log ("ARGameController: hit.Point=" + hit.Point);
		Debug.Log ("ARGameController: hit.Plane=" + hit.Plane);
		Debug.Log ("ARGameController: anchor=" + anchor);
		Debug.Log ("ARGameController: anchor.transform=" + anchor.transform);

		// Intanstiate Prefab object as a child of the anchor; it's transform will now benefit
		// from the anchor's tracking.
		var _object = Instantiate(m_objectPrefab, hit.Point, Quaternion.identity,
			anchor.transform);
		
		if (_object != null) {
			m_objectPrefab.SetActive (false);

			// Andy should look at the camera but still be flush with the plane.
			_object.transform.LookAt (m_firstPersonCamera.transform);
			_object.transform.rotation = Quaternion.Euler (
				0.0f, 
				_object.transform.rotation.eulerAngles.y, 
				_object.transform.rotation.z);
			
			//_object.transform.localScale = new Vector3 (0.0175f, 0.03125f, 0.015625f);
			_object.transform.localScale = new Vector3 (
				m_mainObjectSizeSlider.value, 
				m_mainObjectSizeSlider.value, 
				m_mainObjectSizeSlider.value);
			
			m_mainObjectPrefab = _object;

			Debug.Log ("ARGameController: PresentObject (m_mainObjectPrefab != null)=" + (m_mainObjectPrefab != null));
		} else {
			Debug.Log ("ARGameController=" + _object);
		}
		*/

		if (m_objectPrefab != null) {			

			// Andy should look at the camera but still be flush with the plane.
			m_objectPrefab.transform.LookAt (m_firstPersonCamera.transform);
			m_objectPrefab.transform.rotation = Quaternion.Euler (
				0.0f, 
				m_objectPrefab.transform.rotation.eulerAngles.y, 
				m_objectPrefab.transform.rotation.z);

			//_object.transform.localScale = new Vector3 (0.0175f, 0.03125f, 0.015625f);
			m_objectPrefab.transform.localScale = new Vector3 (
				m_mainObjectSizeSlider.value, 
				m_mainObjectSizeSlider.value, 
				m_mainObjectSizeSlider.value);

			m_mainObjectPrefab = m_objectPrefab;

			Debug.Log ("ARGameController: PresentObject (m_mainObjectPrefab != null)=" + (m_mainObjectPrefab != null));
		} else {
			Debug.Log ("ARGameController: m_objectPrefab=null");
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

	private void OnConnectClicked()
	{
		Debug.Log ("ARGameController: On Connect");
	}

	private void OnHostClicked()
	{
		Debug.Log ("ARGameController: On Host");
	}

}