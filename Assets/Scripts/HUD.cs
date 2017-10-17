using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HUD : MonoBehaviour 
{
	private NetworkManager networkManager;

	public InputField inputField;

	public Button startServerButton;
	public Button startClientButton;
	public Button stopServerButton;

	// Use this for initialization
	void Start () 
	{
		networkManager = GetComponent<NetworkManager> ();

		if (startServerButton != null) 
		{
			startServerButton.onClick.AddListener(delegate {StartServer();});
		}

		if (startClientButton != null) 
		{
			startClientButton.onClick.AddListener(delegate {StartClient();});
		}

		if (stopServerButton != null) 
		{
			stopServerButton.onClick.AddListener(delegate {StopServer();});
		}

		startServerButton.gameObject.SetActive (true);
		startClientButton.gameObject.SetActive (true);
		stopServerButton.gameObject.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	void StartServer()
	{
		Debug.Log ("Start Server networkManager.isNetworkActive=" + networkManager.isNetworkActive 
			+ " networkManager.isActiveAndEnabled=" + networkManager.isActiveAndEnabled
			+ " ip=" + inputField.text
		);

		if (networkManager.isNetworkActive == false) 
		{
			networkManager.networkAddress = inputField.text;
			networkManager.StartHost ();

			startServerButton.gameObject.SetActive (false);
			startClientButton.gameObject.SetActive (false);
			stopServerButton.gameObject.SetActive (true);
		}
			
	}

	void StopServer()
	{
		Debug.Log ("Stop Server networkManager.isNetworkActive=" + networkManager.isNetworkActive);

		if (networkManager.isNetworkActive) 
		{
			networkManager.StopHost ();	

			startServerButton.gameObject.SetActive (true);
			startClientButton.gameObject.SetActive (true);
			stopServerButton.gameObject.SetActive (false);
		}
	}

	void StartClient()
	{
		Debug.Log ("Start Client networkManager.isNetworkActive=" + networkManager.isNetworkActive 
			+ " networkManager.isNetworkActive=" + networkManager.isNetworkActive
			+ " ip=" + inputField.text
		);

		if (networkManager.isNetworkActive == false) 
		{
			networkManager.networkAddress = inputField.text;
			networkManager.networkPort = 7777;
			networkManager.StartClient ();

			startServerButton.gameObject.SetActive (false);
			startClientButton.gameObject.SetActive (false);
			stopServerButton.gameObject.SetActive (true);
		}
	}
}
