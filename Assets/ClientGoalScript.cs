using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientGoalScript : MonoBehaviour {
	
	void OnCollisionEnter() 
	{
		Debug.Log ("ClientGoalScript.OnCollisionEnter " + GameController.Instance.clientScore);
		GameController.Instance.incrementClientScore ();
	}

}
