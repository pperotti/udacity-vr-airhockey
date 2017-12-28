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

		GameLogic.Instance.setDisk (this);

        rigidBody = GetComponent<Rigidbody>();
        hostScore = GameObject.FindGameObjectWithTag("hostScore");
        clientScore = GameObject.FindGameObjectWithTag("clientScore");

		tableBase = GameObject.FindGameObjectWithTag("plane");
		Debug.Log ("tableBase=" + tableBase);
    }

	void OnDestroy()
	{
		GameLogic.Instance.setDisk (null);
	}

    private void MsgFromServer(NetworkMessage netMsg)
    {
		Debug.Log ("Message from server");
        var msg = netMsg.ReadMessage<ScoresMessage>();
        RefreshScore(msg);

		GameLogic.Instance.CheckClientScore ();

		if (GameLogic.Instance.IsGameOver()) 
		{
			GameLogic.Instance.StopClient ();
		}
    }

    private void RefreshScore(ScoresMessage msg)
    {		
		if (hostScore != null) {
			hostScore.GetComponent<Text> ().text = "Host: " + msg.hostScore;
		}
		if (clientScore != null) {
			clientScore.GetComponent<Text> ().text = "Client: " + msg.clientScore;
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
            NetworkManager.singleton.client.RegisterHandler(1001, MsgFromServer);
        }
    }

    void OnCollisionEnter(Collision other)
    {
		//Debug.Log ("On Collission Enter " + other + " rb=" + other.gameObject.tag);
        if (collisionsManagedByHost)
        {
			HandleHostCollisions(other);
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

	private void HandleHostCollisions(Collision other)
    {		
		if ("hostGoalLine".Equals (other.gameObject.tag)) {
			GameLogic.Instance.IncrementHostScore();
			SendScoreToClient ();
		} else if ("clientGoalLine".Equals (other.gameObject.tag)) {
			GameLogic.Instance.IncrementClientScore ();
			SendScoreToClient ();
		} else if ("Player".Equals (other.gameObject.tag)) {
			if (rigidBody.velocity == Vector3.zero) {
				AddImpulse ();
			}
		} else if ("wall".Equals (other.gameObject.tag)) {
			float x = System.Math.Min (3, rigidBody.velocity.x);
			float z = System.Math.Min (3, rigidBody.velocity.z);
			rigidBody.velocity = new Vector3 (x, 0, z);					
		}
    }

	public void AddImpulse() {		
		float x = Random.Range(3, 5) * 50f * Time.deltaTime;
		float z = Random.Range(2, 4) * 50f * Time.deltaTime;

		x = System.Math.Min (5, x);
		z = System.Math.Min (5, z);

		GetComponent<Rigidbody> ().AddForce(x, 0f, z, ForceMode.Impulse);
	}

    private void SendScoreToClient()
    {
        var msg = new ScoresMessage();        
		msg.hostScore = GameLogic.Instance.hostScore;        
		msg.clientScore = GameLogic.Instance.clientScore;
        NetworkServer.SendToAll(1001, msg);
        RefreshScore(msg);

		GameLogic.Instance.CheckHostScore ();
		Debug.Log ("DISK.client score=" + msg.clientScore + " hostScore=" + msg.hostScore);
		Debug.Log ("DISK.isGameOver=>" + GameLogic.Instance.IsGameOver());
		if (GameLogic.Instance.IsGameOver()) 
		{
			GameLogic.Instance.StopHost ();
		}
    }

    public class ScoresMessage : MessageBase
    {
        public int hostScore;
        public int clientScore;
    }

}
