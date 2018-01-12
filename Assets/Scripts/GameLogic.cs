using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameLogic : MonoBehaviour 
{

	public static GameLogic Instance { get; private set; }

	/**
	 * Determine whether the UI elements needs to always look at the user.
	 */
	public bool forceRelocatingUI = true;

	/**
	 * Client & Host Scores
	 */
	public int clientScore;
	public int hostScore;

	/**
	 * Dialog to present results.
	 */
	public GameObject resultDialog;

	/**
	 * HUD.
	 */
	public GameObject hudPanel;
	private HUD hud;

	/**
	 * Score Panel
	 */
	public GameObject scorePanel;

	/**
	 * Current Disk in place
	 */
	private Disk disk;

	/**
	 * Network manager to control all the networking.
	 */
	private NetworkManager networkManager;

	/**
	 * Players involved in the game.
	 */
	private Player localPlayer;

	private const int MAX_SCORE = 1;

	/**
	 * Offset to use when left or right click are pressed.
	 */
	public float incrementOffset = 0.05f;

	public void Awake()
	{
		Debug.Log ("GL.Awake");
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
	public void Start () 
	{
		Debug.Log ("GL.Start incrementOffset=" + incrementOffset);

		resultDialog.SetActive (false);

		//Network Manager 
		networkManager = GetComponent<NetworkManager> ();

		hud = hudPanel.GetComponent <HUD> ();

		if (forceRelocatingUI) 
		{
			//hudPanel.transform.Rotate(new Vector3 (0, -180,	0));
			//scorePanel.transform.Rotate(new Vector3 (0, -180, 0));
		}

	}

	public void IncrementClientScore()
	{
		++clientScore;
		Debug.Log ("GL.incrementClientScore=" + clientScore);
	}

	public void IncrementHostScore()
	{
		++hostScore;
		Debug.Log ("GL.incrementHostScore=" + hostScore);	
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
		} 
	}

	private void resetScores() 
	{
		hostScore = 0;
		clientScore = 0;

		scorePanel.transform.Find ("hostScore").GetComponent<UnityEngine.UI.Text> ().text = "Host: 0";
		scorePanel.transform.Find ("clientScore").GetComponent<UnityEngine.UI.Text> ().text = "Client: 0";
	}

	public bool IsGameOver()
	{
		Debug.Log ("GL.IsGameOver=" + (clientScore == MAX_SCORE || hostScore == MAX_SCORE));
		Debug.Log ("GL.clientScore=" + clientScore + " hostScore=" + hostScore);
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
			hud.UpdateDisconnectUI();
			resetScores();
		});
	}

	public void ShowClientLostDialog()
	{
		ShowDialog ("Client Lost!", Color.red, delegate {
			resultDialog.SetActive (false);
			hud.UpdateDisconnectUI();
			resetScores();
		});
	}

	public void ShowHostWinDialog() 
	{
		ShowDialog ("Host Win!", Color.green, delegate {
			resultDialog.SetActive (false);
			hud.UpdateDisconnectUI();
			resetScores();
		});
	}

	public void ShowHostLostDialog()
	{
		ShowDialog ("Host Lost!", Color.red, delegate {
			resultDialog.SetActive (false);
			hud.UpdateDisconnectUI();
			resetScores();
		});
	}

	public void StartServer(string networkAddress)
	{
		Debug.Log ("GL.StartServer");
		networkManager.networkAddress = networkAddress;
		networkManager.StartHost ();
	}

	public void StartClient(string networkAddress) 
	{
		Debug.Log ("GL.StartClient");
		networkManager.networkAddress = networkAddress;
		networkManager.networkPort = 7777;
		networkManager.StartClient ();
	}

	public void StopHost() 
	{
		Debug.Log ("GL.StopHost");
		NetworkManager.singleton.StopHost ();
	}

	public void StopClient()
	{
		Debug.Log ("GL.Disconnect");
		networkManager.StopClient ();
	}

	public bool IsNetworkActive()
	{
		return networkManager.isNetworkActive;
	}

	public bool IsNetworkActiveAndEnabled()
	{
		return networkManager.isActiveAndEnabled;
	}

	public void setDisk(Disk currentDisk)
	{
		disk = currentDisk;
	}

	public Disk getDisk()
	{
		return disk;
	}

	public void RegisterLocalPlayer(Player player) 
	{
		this.localPlayer = player;
	}

	public void UnRegisterLocalPlayer()
	{
		this.localPlayer = null;
	}

	public void OnLeftClick()
	{
		if (localPlayer != null) 
		{
			localPlayer.MoveLeft ();
		}
	}

	public void OnRightClick()
	{
		if (localPlayer != null) 
		{
			localPlayer.MoveRight ();
		}
	}

	public float getIncrement() 
	{
		return incrementOffset;
	}
}
