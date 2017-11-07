using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//[RequireComponent(typeof(Collider))]
public class Player : AirHockeyNetworkBehaviour
{
	public const float movementSpeed = 10f;

	private GameObject plane;
	private GameObject disk;

	private GameObject leftMarker;
	private GameObject rightMarker;

	bool isHost = false;
    bool painted = false;

	float limit = 3.5f;

	private Rigidbody playerRigidBody;

    void Start()
    {
		Debug.Log ("Player.Start");

		playerRigidBody = GetComponent<Rigidbody> ();
		disk = GameObject.FindGameObjectWithTag("disk");

		leftMarker = GameObject.FindGameObjectWithTag("leftMarker");
		rightMarker = GameObject.FindGameObjectWithTag("rightMarker");
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

			float inputX = Input.GetAxis ("Horizontal"); // * 0.1f;
			float inputZ = Input.GetAxis ("Vertical");
			//float moveX = inputX * movementSpeed * Time.deltaTime;
			//float moveZ = inputZ * movementSpeed * Time.deltaTime;

			if (inputX != 0) {

				var posX = playerRigidBody.transform.localPosition.x;
				var offsetX = inputX;

				Debug.Log ("posX=" + posX
					+ " offsetX=" + offsetX 
					+ " left=" + leftMarker.transform.localPosition.x
					+ " right=" + rightMarker.transform.localPosition.x
				);

				if (posX + inputX < leftMarker.transform.localPosition.x) { //Left					
					//offsetX = posX - leftMarker.transform.localPosition.x;
					Debug.Log ("Move Left");
					transform.localPosition = new Vector3(leftMarker.transform.localPosition.x, 
						transform.localPosition.y,
						transform.localPosition.z);
				} else if (posX + inputX > rightMarker.transform.localPosition.x) { //Right
					//offsetX = posX - rightMarker.transform.localPosition.x;
					Debug.Log ("Move Right");
					transform.localPosition = new Vector3(rightMarker.transform.localPosition.x, 
						transform.localPosition.y,
						transform.localPosition.z);
				} else {
					transform.Translate (offsetX, 0, 0);
				}
			}			  

			/*
			//approach 2
			if (offsetX < 0) { //Left
                   Debug.Log ("offset=" + offsetX);
                   moveLeft (offsetX);
            } else if (offsetX > 0) { //Right
                   Debug.Log ("offset=" + offsetX);
                   moveRight (offsetX);
            }*/

			if (isHost 
				&& Input.GetKeyDown(KeyCode.Space) 
				&& disk.transform.localPosition.x == 0
				&& disk.transform.localPosition.z == 0)
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
        //var players = GameObject.FindGameObjectsWithTag("Player");
        //if (players.Length == 2)
        {
            
			var velocity = disk.GetComponent<Rigidbody> ().velocity;

			/*float x = Random.Range(3, 5);

			Vector3 newVector = new Vector3 (x, 0, x);
			disk.GetComponent<Rigidbody> ().AddForce(newVector, ForceMode.Impulse);*/

			//float x = 10f * Time.deltaTime;
			float x = Random.Range(1, 3) * 50f * Time.deltaTime;
			float z = Random.Range(1, 3) * 50f * Time.deltaTime;
			Debug.Log ("x=" + x + " z=" + z);
			disk.GetComponent<Rigidbody> ().AddForce(x, 0f, z, ForceMode.Impulse);

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
