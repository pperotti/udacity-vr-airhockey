using UnityEngine;
using UnityEngine.Networking;

public class DiskSpawnScript : AirHockeyNetworkBehaviour {

	public GameObject diskPrefab;

    public override void OnStartServer()
	{
		NetworkServer.Spawn(airHockeyInstantiate(diskPrefab, Vector3.zero, Quaternion.Euler(Vector3.zero)));
	}

}
