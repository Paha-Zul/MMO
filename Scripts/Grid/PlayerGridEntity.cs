using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerGridEntity : uLink.MonoBehaviour {
    public float timePerTick = 1; //In Seconds
    public int radiusEnter = 1, radiusLeave = 2;
    private float nextTick;

    private List<GameObject> createList;
    private List<GameObject> destroyList;
    private List<GameObject> playerCreateList;
    private List<GameObject> playerDestroyList;

    private Grid.GridSquare currGridSquare = null;
    private Player player;

	// Use this for initialization
	void Start () {
        if (uLink.Network.isClient) Destroy(this);

        this.createList = new List<GameObject>();
        this.destroyList = new List<GameObject>();
        this.playerCreateList = new List<GameObject>();
        this.playerDestroyList = new List<GameObject>();

        player = this.GetComponent<Player>();
        currGridSquare = Grid.addObjectToGrid(this.gameObject, true);
        sendCreationToAllPlayers();
        createAllObjectsAtLocation(radiusEnter);
	}
	
	// Update is called once per frame
	void Update () {
        //If the time is up, check the currGridSquare
        if (Time.time > nextTick)
        {
            //createList 
            this.currGridSquare = Grid.checkGridSquare(this.gameObject, currGridSquare, true, createList, destroyList, playerCreateList, playerDestroyList, 1);
            if (createList.Count > 0) createObjects();
            if (destroyList.Count > 0) destroyObjects();
            nextTick = Time.time + timePerTick;
        }
	}

    void createObjects()
    {
        //Send create messages to the owner of this script (GameObject).
        foreach (GameObject obj in createList)
            if(obj != this.gameObject) player.createGameObject(obj);
        foreach (GameObject obj in playerCreateList)
        {
            if (obj == this.gameObject) continue;
            Player playerScript = obj.GetComponent<Player>(); //Get the Player script from the player GameObject
            if (playerScript == null) throw new Exception("This GameObject is not a player"); //Throw an exception if no playerscript was found.
            playerScript.createGameObject(this.gameObject); //Tell the playerScript to create me (us/this gameobject).
        }
        playerCreateList.Clear();
        createList.Clear();
    }

    void destroyObjects()
    {
        //Send destroy messages to the owner of this script (GameObject).
        foreach (GameObject obj in destroyList)
            if (obj != this.gameObject)  player.destroyGameObject(obj);
        foreach (GameObject obj in playerDestroyList)
        {
            if (obj == this.gameObject) continue;
            Player playerScript = obj.GetComponent<Player>(); //Get the Player script from the player GameObject
            if (playerScript == null) throw new Exception("This GameObject is not a player"); //Throw an exception if no playerscript was found.
            playerScript.destroyGameObject(this.gameObject); //Tell the playerScript to create me (us/this gameobject).
        }
        destroyList.Clear();
        playerDestroyList.Clear();
    }

    private void sendCreationToAllPlayers()
    {
        //gets the list of all nearby players on the server.
        List<GameObject> playerList = Grid.getObjectsInRadius(this.gameObject, 1, true);
        foreach (GameObject obj in playerList)
        {
            if (obj == null) continue;
            Player playerScript = obj.GetComponent<Player>();
            if (playerScript.getPlayerInfo().playerName == this.player.getPlayerInfo().playerName)
                continue;
            playerScript.createGameObject(this.gameObject);
        }
    }

    private void createAllObjectsAtLocation(int radius)
    {
        List<GameObject> objsToCreate = Grid.getAllObjectsInRadius(this.gameObject, radius);
        foreach (GameObject obj in objsToCreate)
        {
            if (obj == this.gameObject) continue;
            this.player.createGameObject(obj);
        }
    }

    void OnDestroy()
    {
        Grid.removeObjectFromGrid(this.gameObject, true);
    }

}
