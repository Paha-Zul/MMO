using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Instantiate : uLink.MonoBehaviour {
	public GameObject PlayerPrefab, ServerPrefab, proxyPrefab;

	void Start(){

	}

	void uLink_OnPlayerConnected(uLink.NetworkPlayer player){
        StartCoroutine(create(player));
		//uLink.Network.Instantiate(player, ServerPrefab, PlayerPrefab, ServerPrefab, transform.position, transform.rotation, 0);
        //networkView.RPC("spawnPlayerRPC", uLink.RPCMode.All, "");
		Debug.Log("Network aware game object is now created by client.");
	}

    private IEnumerator create(uLink.NetworkPlayer player)
    {
        yield return new WaitForSeconds(1f);
        //GameObject serverSpawnedObject = uLink.Network.Instantiate(player, ServerPrefab, PlayerPrefab, ServerPrefab, transform.position, transform.rotation, 0);
        
        spawnPlayer(player);
    }


    private void spawnPlayer(uLink.NetworkPlayer player)
    {
        //Allocates a viewID on the server.
        uLink.NetworkViewID tempViewID = uLink.Network.AllocateViewID();
        //Sends an RPC to instantiate the object. Sends the player that's connected and the viewID.
        networkView.RPC("spawnPlayerRPC", player, tempViewID, player); //Send RPCs out.
    }

    [RPC]
    private void spawnPlayerRPC(uLink.NetworkViewID viewID, uLink.NetworkPlayer player, uLink.NetworkMessageInfo info)
    {
        if (uLink.Network.isClient)
        {
            //This area instantiates the player object and tricks the client/server into thinking that the client is the owner.
            //Takes the NetworkPlayer passed in (which is the client that connected) and assigns it as the owner and creator.
            GameObject spawned = Instantiate(PlayerPrefab, this.transform.position, this.transform.rotation) as GameObject; //Create the object.
            Player.PlayerInfo.clientPlayer = spawned.GetComponent<Player>(); //Assign the client player as the script from the object just spawned.
            Player.PlayerInfo.clientPlayer.pInfo.netPlayer = player; //Assign the player (client) to the pInfo.

            spawned.GetComponent<uLinkNetworkView>().SetViewID(viewID, player); //Assigns the viewID and sets the player as owner and creator.
            networkView.RPC("spawnPlayerRPC", uLink.RPCMode.Server, viewID, player); //Sends the RPC back to the server with the allocated ID.
        }
        else
        {
            GameObject spawned = Instantiate(ServerPrefab, this.transform.position, this.transform.rotation) as GameObject;
            spawned.uLinkNetworkView().SetViewID(viewID, player); //Assigns the viewID and sets the player as owner and creator.
        }
    }

    [RPC]
    private void spawnServerPlayerRPC(uLink.NetworkViewID viewID)
    {
        GameObject spawned = Instantiate(ServerPrefab, this.transform.position, this.transform.rotation) as GameObject;
        spawned.GetComponent<uLinkNetworkView>().viewID = viewID;
        Debug.Log("Owner: " + spawned.uLinkNetworkView().owner);
    }
}