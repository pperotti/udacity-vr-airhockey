using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostGoalScript : MonoBehaviour {

	void OnCollisionEnter() 
	{
		Debug.Log ("ClientGoalScript.OnCollisionEnter " + GameController.Instance.clientScore);
		GameController.Instance.incrementHostScore ();
	}
}
