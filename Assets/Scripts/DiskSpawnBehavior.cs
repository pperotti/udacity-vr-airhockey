using UnityEngine;
using UnityEngine.Networking;

public class DiskSpawnBehavior : AirHockeyNetworkBehaviour {

	public GameObject diskPrefab;

    public override void OnStartServer()
	{
		Debug.Log ("DiskSpawnBehavior.OnStartServer");

		NetworkServer.Spawn(airHockeyInstantiate(diskPrefab, Vector3.zero, Quaternion.Euler(Vector3.zero)));
	}

}
