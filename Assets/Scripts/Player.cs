using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//[RequireComponent(typeof(Collider))]
public class Player : AirHockeyNetworkBehaviour
{
	public const float movementSpeed = 10f;

	private const float horizontalLimit = 3.5f;
	private float increment = 0f;

	private Disk disk;

	bool isHost = false;
    bool painted = false;

	private Rigidbody playerRigidBody;

    void Start()
    {
		Debug.Log ("Player.Start");

		playerRigidBody = GetComponent<Rigidbody> ();
		GameObject diskObject = GameObject.FindGameObjectWithTag("disk");
		disk = diskObject.GetComponent<Disk>();

		Debug.Log ("Player.Started");
    }

    public override void OnStartServer()
    {
		Debug.Log ("Player.OnStartServer");
		isHost = true;

		if (isLocalPlayer) 
		{
			GameLogic.Instance.RegisterLocalPlayer (this);
		}

		increment = GameLogic.Instance.getIncrement ();
		Debug.Log ("Player.increment=" + increment);
    }

	public override void OnStartLocalPlayer()
    {
		Debug.Log ("Player.OnStartLocalPlayer");

		prepareSpawnPoint();

        prepareCameraToActivate();

		if (isLocalPlayer)
		{
			GameLogic.Instance.RegisterLocalPlayer (this);
		}

		increment = GameLogic.Instance.getIncrement ();
		Debug.Log ("Player.increment=" + increment);
    }

	void Destroy()
	{
		Debug.Log ("Destroy()");
		GameLogic.Instance.UnRegisterLocalPlayer ();
	}

    void Update()
    {
        if (!painted)
		{
            painted = true;            
		}

		if (isLocalPlayer) 
		{
			GetComponent<MeshRenderer> ().material.color = Color.red;

			float inputX = Input.GetAxis ("Horizontal"); 

			if (isHost)
			{
				handleHostInput (inputX);
			} 
			else 
			{
				handleClientInput (inputX);
			}

			if (isHost
			    && Input.GetKeyDown (KeyCode.Space)
				&& disk.GetComponent<Rigidbody>().velocity == Vector3.zero) {
				moveDisk ();
			}
		} 
    }

	void OnDestroy() {
		print("DragPlayer destroyed");
	}

	void handleHostInput(float inputX) {
		if (inputX != 0) {

			var posX = playerRigidBody.transform.localPosition.x;
			var offsetX = (inputX>0) ? increment : -increment;

			handleInput (inputX, offsetX, posX);
		}
	}

	bool isLeft(float inputX) {
		return (isHost) ? inputX < 0 : inputX > 0;
	}

	bool isRight(float inputX) {
		return (isHost) ? inputX > 0 : inputX < 0;
	}

	void handleInput(float inputX, float offsetX, float posX) {
		if (isLeft(inputX) && posX - increment < -horizontalLimit) { //Left
			transform.localPosition = new Vector3 (
				-horizontalLimit, 
				transform.localPosition.y,
				transform.localPosition.z);
		} else if (isRight(inputX) && posX + increment > horizontalLimit) { //Right			
			transform.localPosition = new Vector3 (
				horizontalLimit, 
				transform.localPosition.y,
				transform.localPosition.z);
		} else {
			transform.Translate (offsetX, 0, 0);
		}
	}

	public void MoveLeft()
	{
		float offsetX = transform.localPosition.x - increment;
		Debug.Log ("Player.MoveRight -> offsetX=" + offsetX);
		if (offsetX < -horizontalLimit)
		{
			offsetX = -horizontalLimit;
		}
		transform.localPosition = new Vector3 (
			offsetX, 
			transform.localPosition.y,
			transform.localPosition.z);
	}

	public void MoveRight()
	{
		float offsetX = transform.localPosition.x + increment;
		Debug.Log ("Player.MoveRight -> offsetX=" + offsetX);
		if (offsetX > horizontalLimit)
		{
			offsetX = horizontalLimit;
		}
		transform.localPosition = new Vector3 (
			offsetX, 
			transform.localPosition.y,
			transform.localPosition.z);
	}

	void handleClientInput(float inputX) {
		if (inputX != 0) {

			var posX = playerRigidBody.transform.localPosition.x;
			var offsetX = (inputX>0) ? -increment : increment;

			handleInput (inputX, offsetX, posX);
		}
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
		/*GameObject clientCameraGameObject = GameObject.FindGameObjectWithTag ("clientCamera");
		GameObject hostCameraGameObject = GameObject.FindGameObjectWithTag ("hostCamera");

		if (clientCameraGameObject != null) 
		{
			clientCameraGameObject.SetActive (!isServer);
		}

		if (hostCameraGameObject != null) 
		{
			hostCameraGameObject.SetActive (isServer);
		}*/
    }

    void moveDisk()
    {
		var players = GameObject.FindGameObjectsWithTag("Player");
        // if (players.Length == 2)
        {
			disk.AddImpulse ();
        }
    }
}
