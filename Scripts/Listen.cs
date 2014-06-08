using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Listen : uLink.MonoBehaviour {

    void Awake()
    {
        
    }

	void Start(){

	}

	void uLink_OnPlayerConnected(uLink.NetworkPlayer player){
		//Debug.Log("Player connected from " + player.internalIP + ":" + player.port);
		//StartCoroutine(CheckIP(player));
	}

    void uLink_OnPlayerDisconnected(uLink.NetworkPlayer player)
    {
        uLink.Network.DestroyPlayerObjects(player);
        uLink.Network.RemoveRPCs(player);
    }

	public IEnumerator CheckIP(uLink.NetworkPlayer player){
		WWW myExtIPWWW = new WWW("http://checkip.dyndns.org");
		yield return myExtIPWWW;
		string myExtIP=myExtIPWWW.text;
		myExtIP=myExtIP.Substring(myExtIP.IndexOf(":")+1);
		myExtIP=myExtIP.Substring(0,myExtIP.IndexOf("<"));
		NetConnections.addNetworkPlayer(myExtIP+":"+player.port, player);
    }

}
