using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDHandler : MonoBehaviour {

	GameObject hudContainer;

	public UnityEngine.UI.InputField inputField;

	public UnityEngine.UI.Button startServerButton;
	public UnityEngine.UI.Button startClientButton;
	public UnityEngine.UI.Button stopServerButton;

	public HUDHandler(GameObject hud)
	{
		hudContainer = hud;

		Debug.Log ("Init");
	}


}
