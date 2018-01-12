using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using GoogleARCore;

public class Utilities : MonoBehaviour {

	/*
	/// <summary>
	/// Quit the application if there was a connection error for the ARCore session.
	/// </summary>
	public static void QuitOnConnectionErrors()
	{
		// Do not update if ARCore is not tracking.
		 if (Session.ConnectionState == SessionConnectionState.UserRejectedNeededPermission) {
			Utilities.ShowAndroidToastMessage ("Camera permission is needed to run this application.");
			Application.Quit ();
		} else if (Session.ConnectionState == SessionConnectionState.ConnectToServiceFailed) {
			Utilities.ShowAndroidToastMessage ("ARCore encountered a problem connecting.  Please start the app again.");
			Application.Quit ();
		}
	}

	/// <summary>
	/// Show an Android toast message.
	/// </summary>
	/// <param name="message">Message string to show in the toast.</param>
	/// <param name="length">Toast message time length.</param>
	public static void ShowAndroidToastMessage(string message)
	{
		AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

		if (unityActivity != null)
		{
			AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
			unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
				{
					AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
						message, 0);
					toastObject.Call("show");
				}));
		}
	}

	public static void ShowDialog() {


	}
	*/
}
