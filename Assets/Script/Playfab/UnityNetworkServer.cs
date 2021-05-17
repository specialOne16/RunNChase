namespace PlayFab.Networking
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using Mirror;
	using UnityEngine.Events;

	public class UnityNetworkServer : NetworkBehaviour
	{
		public NetConfiguration configuration;

		public PlayerEvent OnPlayerAdded = new PlayerEvent();
		public PlayerEvent OnPlayerRemoved = new PlayerEvent();

		public int MaxConnections = 100;
		public int Port = 7777;

		public List<UnityNetworkConnection> Connections
		{
			get { return _connections; }
			private set { _connections = value; }
		}
		private List<UnityNetworkConnection> _connections = new List<UnityNetworkConnection>();

		public class PlayerEvent : UnityEvent<string> { }

		void Awake()
		{
			if (configuration.buildType == BuildType.REMOTE_SERVER)
			{
				AddRemoteServerListeners();
			}
		}

		private void AddRemoteServerListeners()
		{
			Debug.Log("[UnityNetworkServer].AddRemoteServerListeners");
			NetworkServer.RegisterHandler<ReceiveAuthenticateMessage>(OnReceiveAuthenticate);
		}

		public void StartServer()
		{
			NetworkServer.Listen(Port);
		}

		private void OnApplicationQuit()
		{
			NetworkServer.Shutdown();
		}

		private void OnReceiveAuthenticate(NetworkConnection netConn, ReceiveAuthenticateMessage netMsg)
		{
			var conn = _connections.Find(c => c.ConnectionId == netConn.connectionId);
			if (conn != null)
			{
				var message = netMsg;
				conn.PlayFabId = message.PlayFabId;
				conn.IsAuthenticated = true;
				OnPlayerAdded.Invoke(message.PlayFabId);
			}
		}

		public void OnServerConnect(NetworkConnection conn)
		{
			if (configuration.buildType == BuildType.LOCAL) return;

			Debug.LogWarning("Client Connected");
			var uconn = _connections.Find(c => c.ConnectionId == conn.connectionId);
			if (uconn == null)
			{
				_connections.Add(new UnityNetworkConnection()
				{
					Connection = conn,
					ConnectionId = conn.connectionId,
					LobbyId = PlayFabMultiplayerAgentAPI.SessionConfig.SessionId
				});
			}
		}

		public void OnServerDisconnect(NetworkConnection conn)
		{
			if (configuration.buildType == BuildType.LOCAL) return;

			var uconn = _connections.Find(c => c.ConnectionId == conn.connectionId);
			if (uconn != null)
			{
				if (!string.IsNullOrEmpty(uconn.PlayFabId))
				{
					OnPlayerRemoved.Invoke(uconn.PlayFabId);
				}
				_connections.Remove(uconn);
			}
		}
	}

	[Serializable]
    public class UnityNetworkConnection
    {
        public bool IsAuthenticated;
        public string PlayFabId;
        public string LobbyId;
        public int ConnectionId;
        public NetworkConnection Connection;
    }

    public struct ReceiveAuthenticateMessage : NetworkMessage
    {
        public string PlayFabId;
    }

    public struct ShutdownMessage : NetworkMessage { }

    [Serializable]
    public struct MaintenanceMessage : NetworkMessage
    {
        public DateTime ScheduledMaintenanceUTC;
    }

    public static class MaintenanceMessageFunctions
    {
        public static MaintenanceMessage Deserialize(this NetworkReader reader)
        {
            var json = PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
            DateTime ScheduledMaintenanceUTC = json.DeserializeObject<DateTime>(reader.ReadString());
            MaintenanceMessage value = new MaintenanceMessage
            {
                ScheduledMaintenanceUTC = ScheduledMaintenanceUTC
            };

            return value;
        }

        public static void Serialize(this NetworkWriter writer, MaintenanceMessage value)
        {
            var json = PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
            var str = json.SerializeObject(value.ScheduledMaintenanceUTC);
            writer.Write(str);
        }
    }
}