using UnityEngine;
using System.Collections;

public class MovingGridEntity : uLink.MonoBehaviour {
    public float timePerTick = 1; //In Seconds
    private float nextTick;

    private Grid.GridSquare currGridSquare = null;

	// Use this for initialization
	void Start () {
        if (uLink.Network.isClient) Destroy(this);

        currGridSquare = Grid.addObjectToGrid(this.gameObject, false);
	}
	
	// Update is called once per frame
	void Update () {
        //If the time is up, check the currGridSquare
        if (Time.time > nextTick)
        {
            //createList 
            this.currGridSquare = Grid.checkGridSquare(this.gameObject, currGridSquare, false);
            nextTick = Time.time + timePerTick;
        }
	}
}
