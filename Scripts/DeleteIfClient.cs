using UnityEngine;
using System.Collections;

public class DeleteIfClient : MonoBehaviour {

	// Use this for initialization
	void Start () {
        if (uLink.Network.isClient) Destroy(this.gameObject);
        else Destroy(this);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
