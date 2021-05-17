using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetConfiguration : MonoBehaviour
{
	[Header("Configuration")]
	public BuildType buildType;
	public string buildId = "";
	public string ipAddress = "";
	public ushort port = 0;
	public bool playFabDebugging = false;

	[Header("Server Configuration")]
	public float shutdownTimeout = 900f;

	private void Awake()
    {
		var playfabManager = GameObject.Find("PlayfabManager");
		if (playfabManager != null)
        {
			Debug.Log("[CONFIGURATION]: Playfab Manager found!");
			var matchmaker = playfabManager.GetComponent<PlayfabMatchmaker>();
			ipAddress = matchmaker.serverIp;
			port = (ushort)matchmaker.serverPort;
        }
    }
}
public enum BuildType
{
	LOCAL,
	REMOTE_CLIENT,
	REMOTE_SERVER
}