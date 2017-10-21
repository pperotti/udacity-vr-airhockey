using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(MeshCollider))]
public class Player : AirHockeyNetworkBehaviour
{
    private const float maxMovementSpeed = 0.75f;

	private GameObject plane;

	bool isHost = false;
    bool painted = false;

	float limit = 3.5f;

    void Start()
    {
		Debug.Log ("Player.Start");
    }

    public override void OnStartServer()
    {
		Debug.Log ("Player.OnStartServer");
		isHost = true;
    }

    public override void OnStartLocalPlayer()
    {
		Debug.Log ("Player.OnStartLocalPlayer");

		plane = GameObject.FindGameObjectWithTag ("plane");
		Debug.Log ("Plane=" + plane);

		prepareSpawnPoint();

        prepareCameraToActivate();
    }

    void Update()
    {
        if (!painted) {
            painted = true;            
		}

		if (isLocalPlayer) 
		{
			GetComponent<MeshRenderer> ().material.color = Color.red;

			var currentX = transform.localPosition.x;
			var offsetX = Input.GetAxis ("Horizontal") * 0.1f;

			if (offsetX < 0) { //Left

				Debug.Log ("offset=" + offsetX);
				moveLeft (offsetX);

			} else if (offsetX > 0) { //Right

				Debug.Log ("offset=" + offsetX);
				moveRight (offsetX);
			}

			if (isHost && Input.GetKeyDown(KeyCode.Space)) 
			{
				startMovingDisk ();
			}
		}
    }

	void OnDestroy() {
		print("DragPlayer destroyed");
	}

	void prepareSpawnPoint()
    {		
		if (isHost) {
			GetComponent<MeshRenderer> ().material.color = Color.red;
		}

		GameObject localPlayerPosition;
		if (isHost) {
			localPlayerPosition = GameObject.FindGameObjectWithTag ("clientPlayer");
		} else {
			localPlayerPosition = GameObject.FindGameObjectWithTag ("hostPlayer");
		}

		transform.localPosition = new Vector3 (
			localPlayerPosition.transform.localPosition.x, 
			localPlayerPosition.transform.localPosition.y,
			localPlayerPosition.transform.localPosition.z);
    }

    void prepareCameraToActivate()
    {
		GameObject clientCameraGameObject = GameObject.FindGameObjectWithTag ("clientCamera");
		GameObject hostCameraGameObject = GameObject.FindGameObjectWithTag ("hostCamera");

		if (clientCameraGameObject != null) 
		{
			clientCameraGameObject.SetActive (!isServer);
		}

		if (hostCameraGameObject != null) 
		{
			hostCameraGameObject.SetActive (isServer);
		}
    }

    void startMovingDisk()
    {
		Debug.Log ("startMovingDisk");
        var players = GameObject.FindGameObjectsWithTag("Player");
        //if (players.Length == 2)
        {
            GameObject disk = GameObject.FindGameObjectWithTag("disk");
			var velocity = disk.GetComponent<Rigidbody> ().velocity;
			Debug.Log ("Velocity=" + velocity + " vector3.zero=" + Vector3.zero);

			/*
			if (Vector3.zero == velocity)  {
				disk.GetComponent<Rigidbody> ().velocity = scaledVelocityVector (new Vector3 (4, 0, 4));
				Debug.Log ("New Velocity=" + disk.GetComponent<Rigidbody> ().velocity);
			} else {
				Debug.Log ("No new velocity");
			}*/

			//disk.GetComponent<Rigidbody> ().velocity = scaledVelocityVector (new Vector3 (6, 0, 6));

			float x = Random.Range(3, 5);

			Vector3 newVector = new Vector3 (x, 0, x);
			disk.GetComponent<Rigidbody> ().AddForce(newVector, ForceMode.Impulse);

        }
    }

	void moveLeft(float offsetX) 
	{
		Debug.Log ("move left" + transform.localPosition.x);

		var currentX = transform.localPosition.x;

		if (currentX + offsetX >= -limit) {
			transform.Translate (offsetX, 0, 0);
		} else {
			transform.localPosition = new Vector3(-limit, 
				transform.localPosition.y,
				transform.localPosition.z);
		}
	}

	void moveRight(float offsetX) 
	{
		var currentX = transform.localPosition.x;

		if (currentX + offsetX <= limit) {
			transform.Translate (offsetX, 0, 0);
		} else {
			transform.localPosition = new Vector3 (limit, 
				transform.localPosition.y,
				transform.localPosition.z);
		}

		Debug.Log ("move right " + transform.localPosition.x);
	}
}
