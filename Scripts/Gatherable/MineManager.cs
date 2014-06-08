using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MineManager : MonoBehaviour {
    public List<Transform> oreSpawnSpots = new List<Transform>();
    public List<GameObject> oreToSpawn = new List<GameObject>();
    public int chanceToSpawn = 10, currentlySpawned = 0, spawnLimit=1;
    public float timeDelayToRespawn = 5, nextSpawnTime = 0;

	// Use this for initialization
	void Start () {
        //Only spawn ore for the server. This will network.instantiate it for all clients.
        if (uLink.Network.isServer)
        {
            foreach (Transform child in transform)
                if(child.gameObject.activeSelf) oreSpawnSpots.Add(child);

            spawnOre();
            this.nextSpawnTime = Time.time + timeDelayToRespawn;
        }
        else
            Destroy(this); //Removes this script.
	}
	
	// Update is called once per frame
	void Update () {
        //Spawn ore on a tick.
        if (Time.time > this.nextSpawnTime)
        {
            spawnOre();
            this.nextSpawnTime = Time.time + this.timeDelayToRespawn;
        }
	}
    
    private void spawnOre()
    {
        for (int i = oreSpawnSpots.Count-1; i >= 0; i--)
        {
            if (this.currentlySpawned >= this.spawnLimit) break;
            int rnd = Random.Range(0, 100);
            if (rnd < chanceToSpawn)
            {
                uLink.NetworkViewID viewID = uLink.Network.AllocateViewID();
                GameObject spawned = Instantiate(oreToSpawn[0], oreSpawnSpots[i].position, oreSpawnSpots[i].transform.rotation) as GameObject;
                spawned.uLinkNetworkView().SetViewID(viewID, uLink.NetworkPlayer.server);
                spawned.transform.parent = oreSpawnSpots[i];
                oreSpawnSpots.RemoveAt(i);
                currentlySpawned++;
            }
        }
    }

    public void decrementAmountCurrentlySpawned()
    {
        this.currentlySpawned--;
    }
}
