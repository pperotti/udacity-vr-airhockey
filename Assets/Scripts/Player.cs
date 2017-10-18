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
		Debug.Log ("Start");
    }

    public override void OnStartServer()
    {
		Debug.Log ("On Start Server");
		isHost = true;
    }

    public override void OnStartLocalPlayer()
    {
		Debug.Log ("On Start Local Player");

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

				if (currentX + offsetX >= -limit) {
					transform.Translate (offsetX, 0, 0);
				} else {
					transform.localPosition = new Vector3(-limit, 
						transform.localPosition.y,
						transform.localPosition.z);
				}

			} else if (offsetX > 0) { //Right

				Debug.Log ("offset=" + offsetX);

				if (currentX + offsetX <= limit) {
					transform.Translate (offsetX, 0, 0);
				} else {
					transform.localPosition = new Vector3 (limit, 
						transform.localPosition.y,
						transform.localPosition.z);
				}

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
			transform.localPosition.y,
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

			float x = Random.Range(3, 6);

			Vector3 newVector = new Vector3 (x, 0, x);
			//Debug.Log (newVector);
			disk.GetComponent<Rigidbody> ().AddForce(newVector, ForceMode.Impulse);
        }
    }

	void moveLeft() 
	{
		Debug.Log ("move left");
		transform.GetComponent<Rigidbody>().AddForce(new Vector3 (-0.5f, 0, 0));
	}

	void moveRight() 
	{
		Debug.Log ("move right " + transform.localPosition.x);
		transform.Translate (transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
		//transform.GetComponent<Rigidbody>().AddForce(new Vector3 (0.5f, 0, 0));
	}
}
