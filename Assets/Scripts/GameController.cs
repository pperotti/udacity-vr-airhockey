using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameController : MonoBehaviour {

	public static GameController Instance { get; private set; }

	public int clientScore;
	public int hostScore;

	/**
	 * Dialog to present results.
	 */
	public GameObject resultDialog;

	/**
	 * Scores.
	 */
	public GameObject hudPanel;
	private HUD hud;

	/**
	 * Network manager to control all the networking.
	 */
	private NetworkManager networkManager;

	private const int MAX_SCORE = 1;

	private void Awake()
	{
		if (Instance == null) {
			Instance = this;
			DontDestroyOnLoad (gameObject);
		} 
		else 
		{
			Destroy (gameObject);
		}
	}

	// Use this for initialization
	void Start () 
	{
		//resultDialog = GameObject.FindGameObjectWithTag("resultDialog");
		resultDialog.SetActive (false);

		//Network Manager 
		networkManager = GetComponent<NetworkManager> ();

		//resultDialog.transform.Find ("Quit").

		hud = hudPanel.GetComponent <HUD> ();
	}

	public void IncrementClientScore()
	{
		++clientScore;
		Debug.Log ("incrementClientScore=" + clientScore);
	}

	public void IncrementHostScore()
	{
		++hostScore;
		Debug.Log ("incrementHostScore=" + hostScore);	
	}

	public void CheckHostScore()
	{
		if (IsGameOver()) 
		{
			if (DidHostWin()) {
				ShowHostWinDialog ();
			} else {
				ShowHostLostDialog ();
			}
			resetScores ();
		} 
	}

	public void CheckClientScore() 
	{
		if (IsGameOver()) 
		{
			if (DidClientWin()) {
				ShowClientWinDialog ();
			} else {
				ShowClientLostDialog ();
			}
			resetScores ();
		} 
	}

	private void resetScores() 
	{
		hostScore = 0;
		clientScore = 0;
	}

	public bool IsGameOver()
	{
		return clientScore == MAX_SCORE || hostScore == MAX_SCORE;
	}

	public bool DidClientWin()
	{
		return clientScore == MAX_SCORE;
	}

	public bool DidHostWin()
	{
		return hostScore == MAX_SCORE;
	}

	private void ShowDialog(string message, UnityEngine.Color color, UnityEngine.Events.UnityAction action)
	{
		resultDialog.transform.Find ("ResultLabel").GetComponent<UnityEngine.UI.Text> ().color = color;
		resultDialog.transform.Find ("ResultLabel").GetComponent<UnityEngine.UI.Text> ().text = message;
		resultDialog.transform.Find ("Quit").GetComponent<UnityEngine.UI.Button> ().onClick.AddListener (action);
		resultDialog.SetActive (true);
	}

	private void HideDialog()
	{
		resultDialog.SetActive (false);
	}

	public void ShowClientWinDialog()
	{	
		ShowDialog ("Client Won!", Color.green, delegate {
			resultDialog.SetActive (false);
			hud.UpdateStopServerUI();
		});
	}

	public void ShowClientLostDialog()
	{
		ShowDialog ("Client Lost!", Color.red, delegate {
			resultDialog.SetActive (false);
			hud.UpdateStopServerUI();
		});
	}

	public void ShowHostWinDialog() 
	{
		ShowDialog ("Host Win!", Color.green, delegate {
			resultDialog.SetActive (false);
			hud.UpdateStopServerUI();
		});
	}

	public void ShowHostLostDialog()
	{
		ShowDialog ("Host Lost!", Color.red, delegate {
			resultDialog.SetActive (false);
			hud.UpdateStopServerUI();
		});
	}

	public void StartServer(string networkAddress)
	{
		networkManager.networkAddress = networkAddress;
		networkManager.StartHost ();
	}

	public void StartClient(string networkAddress) 
	{
		networkManager.networkAddress = networkAddress;
		networkManager.networkPort = 7777;
		networkManager.StartClient ();
	}

	public void StopHost() 
	{
		//networkManager.StopHost ();
		NetworkManager.singleton.StopHost ();
	}

	public void StopClient()
	{
		NetworkManager.singleton.client.Disconnect ();
	}

	public bool IsNetworkActive()
	{
		return networkManager.isNetworkActive;
	}

	public bool IsNetworkActiveAndEnabled()
	{
		return networkManager.isActiveAndEnabled;
	}
}
