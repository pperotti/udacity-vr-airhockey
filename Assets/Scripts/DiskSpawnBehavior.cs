using UnityEngine;
using UnityEngine.Networking;

public class DiskSpawnBehavior : AirHockeyNetworkBehaviour {

	public GameObject diskPrefab;

    public override void OnStartServer()
	{
		Debug.Log ("DiskSpawnBehavior.OnStartServer -> " + diskPrefab);

		NetworkServer.Spawn(airHockeyInstantiate(
			diskPrefab, 
			diskPrefab.transform.localPosition, 
			diskPrefab.transform.localRotation));
	}
}
