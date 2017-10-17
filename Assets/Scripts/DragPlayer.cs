using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(MeshCollider))]
public class DragPlayer : AirHockeyNetworkBehaviour
{
    private const float maxMovementSpeed = 0.75f;

	bool isHost = false;
    bool painted = false;

    Vector3 previousPos;

    void Start()
    {
		Debug.Log ("Start");
		previousPos = Vector3.zero;
    }

    public override void OnStartServer()
    {
		Debug.Log ("On Start Server");
		isHost = true;
    }

    public override void OnStartLocalPlayer()
    {
		Debug.Log ("On Start Local Player");

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

			var offsetX = Input.GetAxis ("Horizontal") * 0.1f;

			if (offsetX != 0) {
				transform.Translate (offsetX, 0, 0);
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
		Debug.Log ("isServer=" + isServer);

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

	/*
    void checkMaxMovementSpeed(ref Vector3 curPosition)
    {
        if (previousPos != Vector3.zero)
        {
            float x = curPosition.x;
            if (Mathf.Abs(x - previousPos.x) > maxMovementSpeed)
            {
                x = previousPos.x + Mathf.Sign(x - previousPos.x) * maxMovementSpeed;
            }
            float z = curPosition.z;
            if (Mathf.Abs(z - previousPos.z) > maxMovementSpeed)
            {
                z = previousPos.z + Mathf.Sign(z - previousPos.z) * maxMovementSpeed;
            }
            curPosition = new Vector3(x, curPosition.y, z);
        }
    }*/

    void startMovingDisk()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        //if (players.Length == 2)
        {
            GameObject disk = GameObject.FindGameObjectWithTag("disk");
			if (disk.GetComponent<Rigidbody>().velocity == Vector3.zero)
            {
				disk.GetComponent<Rigidbody>().velocity = scaledVelocityVector(new Vector3(4, 0, 4));
            }
        }
    }

	void moveLeft() 
	{
		var x = transform.localPosition.x * 0.1f;

		transform.Translate (x, transform.localPosition.y, transform.localPosition.z);
	}

	void moveRight() 
	{
		var x = transform.localPosition.x * 0.1f;

		transform.Translate (x, transform.localPosition.y, transform.localPosition.z);
	}
}
