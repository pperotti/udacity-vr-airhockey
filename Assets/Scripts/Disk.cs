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

	private GameObject tableBase;

    void Start()
    {
		Debug.Log ("Disk.Start()");

        rigidBody = GetComponent<Rigidbody>();
        hostScore = GameObject.FindGameObjectWithTag("hostScore");
        clientScore = GameObject.FindGameObjectWithTag("clientScore");

		tableBase = GameObject.FindGameObjectWithTag("plane");
		Debug.Log ("tableBase=" + tableBase);
    }

	void Update()
	{
		//Debug.Log ("Update " + gameObject.transform.localPosition);

		//;

		/*
		float x = tableBase.gameObject.transform.localPosition.x;
		float z = tableBase.gameObject.transform.localPosition.z;

		if (x < x - 4 || x > x + 4) 
		{
			transform.localPosition = Vector3.zero;
		} 
		else if (z < z - 6 || z > z + 6) 
		{
			transform.localPosition = Vector3.zero;
		}
		*/
			

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
		if ("hostGoalLine".Equals (other.gameObject.tag)) {
			MatchStats.instance.scoreInHostGoalLine ();
			sendScoreToClient ();
		} else if ("clientGoalLine".Equals (other.gameObject.tag)) {
			MatchStats.instance.scoreInClientGoalLine ();
			sendScoreToClient ();
		} else if ("Player".Equals (other.gameObject.tag)) {
			if (rigidBody.velocity == Vector3.zero) {
				addImpulse ();
			}
		} else if ("wall".Equals (other.gameObject.tag)) {
			Debug.Log ("rigidBody.velocity=>" + rigidBody.velocity);

			float x = System.Math.Min (3, rigidBody.velocity.x);
			float z = System.Math.Min (3, rigidBody.velocity.z);

			Debug.Log ("Updated rigidBody.velocity=>" + rigidBody.velocity);
			rigidBody.velocity = new Vector3 (x, 0, z);
					
		}
    }

	public void addImpulse() {		
		float x = Random.Range(3, 5) * 30f * Time.deltaTime;
		float z = Random.Range(2, 4) * 30f * Time.deltaTime;

		x = System.Math.Min (5, x);
		z = System.Math.Min (5, z);

		Debug.Log ("\n\nNEW IMPULSE!!!! x=" + x + " z=" + z + "\n\n");
		GetComponent<Rigidbody> ().AddForce(x, 0f, z, ForceMode.Impulse);
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
