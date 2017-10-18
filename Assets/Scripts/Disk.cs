using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Disk : AirHockeyNetworkBehaviour
{
    GameObject hostScore;
    GameObject clientScore;

    private const float maxSpeed = 10f;
    private const float minSpeed = 10f;

    private Rigidbody rigidBody;

    private bool collisionsManagedByHost;

    void Start()
    {
		Debug.Log ("Disk.Start()");

        rigidBody = GetComponent<Rigidbody>();
        hostScore = GameObject.FindGameObjectWithTag("hostScore");
        clientScore = GameObject.FindGameObjectWithTag("clientScore");

		//transform.position = new Vector3 (0.0f, 0.5f, 0.0f);
		//rigidBody.mass = scaledVelocityX (rigidBody.mass);
    }

    private void msgFromServer(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<ScoresMessage>();
        refreshScore(msg);
    }

    private void refreshScore(ScoresMessage msg)
    {
		if (hostScore != null) {
			hostScore.GetComponent<Text> ().text = "" + msg.hostScore;
		}
		if (clientScore != null) {
			clientScore.GetComponent<Text> ().text = "" + msg.clientScore;
		}
    }

    public override void OnStartServer()
    {
		Debug.Log ("Disk.OnStartServer()");

        collisionsManagedByHost = !this.isClient;
    }

    public override void OnStartClient()
    {
		Debug.Log ("Disk.OnStartClient()");

        if (!collisionsManagedByHost) {
            NetworkManager.singleton.client.RegisterHandler(1001, msgFromServer);
        }
    }

    void OnCollisionEnter(Collision other)
    {
		Debug.Log ("On Collission Enter " + other);
        /*if (collisionsManagedByHost)
        {
			// scores managed by server only
			manageScores(other);

			Vector3 vel = rigidBody.velocity;

            // limit bouncing speed
			Vector3 limitedVelocity = ClampMagnitude (rigidBody.velocity, scaledVelocityX (maxSpeed), scaledVelocityX(minSpeed));
			rigidBody.velocity = limitedVelocity;
        }
        else
        {
            Collider c1 = transform.GetComponent<Collider>();
            Collider oc = other.transform.GetComponent<Collider>();
            Physics.IgnoreCollision(c1, oc);
            Destroy(c1);
            Destroy(oc);
        }*/
    }

	// from https://forum.unity.com/threads/clampmagnitude-why-no-minimum.388488/
	public static Vector3 ClampMagnitude(Vector3 v, float max, float min)
	{
		double sm = v.sqrMagnitude;
		if(sm > (double)max * (double)max) return v.normalized * max;
		else if(sm < (double)min * (double)min) return v.normalized * min;
		return v;
	}

    private void manageScores(Collision other)
    {
        if ("hostGoalLine".Equals(other.gameObject.tag))
        {
            MatchStats.instance.scoreInHostGoalLine();
            sendScoreToClient();
        }
        else if ("clientGoalLine".Equals(other.gameObject.tag))
        {
            MatchStats.instance.scoreInClientGoalLine();
            sendScoreToClient();
        }
    }

    private void sendScoreToClient()
    {
        var msg = new ScoresMessage();
        msg.hostScore = MatchStats.instance.Host;
        msg.clientScore = MatchStats.instance.Client;
        NetworkServer.SendToAll(1001, msg);
        refreshScore(msg);
    }

    public class ScoresMessage : MessageBase
    {
        public int hostScore;
        public int clientScore;
    }

}
