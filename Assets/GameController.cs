using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	public static GameController Instance { get; private set; }

	public int clientScore;
	public int hostScore;

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
		
	}

	public void incrementClientScore()
	{
		clientScore++;
	}

	public void incrementHostScore()
	{
		hostScore++;
	}
}
