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
		Debug.Log ("On Collission Enter " + other + " rb=" + other.gameObject.tag);
        if (collisionsManagedByHost)
        {
			handleHostCollisions(other);
        }
        else
        {
            Collider c1 = transform.GetComponent<Collider>();
            Collider oc = other.transform.GetComponent<Collider>();
            Physics.IgnoreCollision(c1, oc);
            Destroy(c1);
            Destroy(oc);
        }
    }

	private void handleHostCollisions(Collision other)
    {
		Debug.Log ("Collision => " + other);
		if ("hostGoalLine".Equals (other.gameObject.tag)) {
			MatchStats.instance.scoreInHostGoalLine ();
			sendScoreToClient ();
		} else if ("clientGoalLine".Equals (other.gameObject.tag)) {
			MatchStats.instance.scoreInClientGoalLine ();
			sendScoreToClient ();
		} else if ("Player".Equals (other.gameObject.tag)) {
			float x = Random.Range(1, 3) * 50f * Time.deltaTime;
			float z = Random.Range(1, 3) * 50f * Time.deltaTime;
			Debug.Log ("PLAYER IMPULSE!!!! ---------------------- x=" + x + " z=" + z);
			GetComponent<Rigidbody> ().AddForce(x, 0f, z, ForceMode.Impulse);
		} else {
			Debug.Log ("Not score collision ...");
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
