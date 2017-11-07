﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreScript : MonoBehaviour {

	GameObject hostScore;
	GameObject clientScore;

	// Use this for initialization
	void Update () {
		if (hostScore != null) {
			hostScore.GetComponent<TextMesh> ().text = "" + GameController.Instance.hostScore;
		}

		if (clientScore != null) {
			clientScore.GetComponent<TextMesh> ().text = "" + GameController.Instance.clientScore;
		}
	}

}
