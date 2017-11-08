using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	public static GameController Instance { get; private set; }

	public int clientScore;
	public int hostScore;

	public GameObject resultDialog;

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
	void Start () {
		resultDialog = GameObject.FindGameObjectWithTag("resultDialog");
		resultDialog.SetActive (false);
	}

	public void incrementClientScore()
	{
		Debug.Log ("incrementClientScore=" + ++clientScore);
		hasWon (clientScore, true);
	}

	public void incrementHostScore()
	{
		Debug.Log ("incrementHostScore=" + ++hostScore);
		hasWon (hostScore, false);
	}

	private void hasWon(int score, bool isClient) {

		//TODO: See how to transmit the result over the network
		if (score == 1) {

			//TODO: Improve text descriptions
			var who = isClient ? "Client" : "Host";
			who += " WON!!!";

			resultDialog.transform.Find ("ResultLabel").GetComponent<UnityEngine.UI.Text> ().text = who;
			resultDialog.SetActive (true);
		}

	}
}
