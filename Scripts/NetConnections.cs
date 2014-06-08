using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using System.Collections;

public class NetConnections:uLink.MonoBehaviour{
	private static Dictionary<string, PlayerHolder> connections = new Dictionary<string, PlayerHolder>();

	public static void addNetworkPlayer(string ip, uLink.NetworkPlayer player){
		connections.Add(ip,new PlayerHolder(player));
	}

	public static void removeNetworkPlayer(string ip){
		connections.Remove(ip);
	}

	public static PlayerHolder getNetworkPlayerByIP(string ip){
		PlayerHolder tmp = null;
		connections.TryGetValue(ip, out tmp);
		return tmp;
	}

	public class PlayerHolder{
		public uLink.NetworkPlayer player;

		public PlayerHolder(uLink.NetworkPlayer player){
			this.player = player;
		}
	}
}
