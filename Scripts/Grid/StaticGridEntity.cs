using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StaticGridEntity : uLink.MonoBehaviour {
    public int objRadius = 5, playerRadius = 5;

	// Use this for initialization
	void Start () {
        if (uLink.Network.isClient) Destroy(this);

        //Add this GameObject to the grid.
        Grid.addObjectToGrid(this.gameObject, false);
        sendCreationToAllPlayers(); //Sends it's creation to all players.

        this.enabled = false;
	}

    private void sendCreationToAllPlayers()
    {
        //gets the list of all nearby players on the server.
        List<GameObject> playerList = Grid.getObjectsInRadius(this.gameObject, 1, true);
        foreach (GameObject obj in playerList)
        {
            Player playerScript = obj.GetComponent<Player>();
            playerScript.createGameObject(this.gameObject);
        }
    }

    void OnDestroy()
    {
        Grid.removeObjectFromGrid(this.gameObject, false);
    }
}
