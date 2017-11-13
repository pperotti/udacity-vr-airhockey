using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public abstract class AirHockeyNetworkBehaviour : NetworkBehaviour
{
	Vector3 scale;

	protected GameObject airHockeyRoot;

	private void Awake ()
	{
		airHockeyRoot = GameObject.FindGameObjectWithTag ("AirHockey");

		Debug.Log ("AirHockeyNetworkBehaviour.Awake=>" + airHockeyRoot);

		scale = airHockeyRoot.transform.localScale;

		transform.parent = airHockeyRoot.transform;

		transform.localScale = new Vector3 (
			transform.localScale.x * scale.x, 
			transform.localScale.y * scale.y, 
			transform.localScale.z * scale.z);
	}

	protected GameObject airHockeyInstantiate (GameObject prefab, Vector3 position, Quaternion rotation)
	{
		GameObject obj = Instantiate(prefab);
		Debug.Log ("AirHockeyNetworkBehaviour.airHockeyInstantiate => " + prefab);
		obj.transform.parent = airHockeyRoot.transform;
		obj.transform.localPosition = position;
		obj.transform.localRotation = rotation;

		return obj;
	}

	protected float scaledVelocityX (float x)
	{
		return x * scale.x;
	}

	protected Vector3 scaledVelocityVector (Vector3 vector)
	{
		Debug.Log ("ScaledVelocityVector=" + vector + "scale=" + scale);
		return vector;
	}

}
