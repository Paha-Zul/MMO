using UnityEngine;
using System.Collections;

public class EnterArea : uLink.MonoBehaviour {

	// Use this for initialization
	void Start () {

        this.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.name == "ore_deposit")
        {
            string message = col.gameObject.name + "|" + "create";
            networkView.RPC("deleteObjectRPC", uLink.RPCMode.Owner, message, col.GetComponent<uLinkNetworkView>().viewID); //Sends the RPC back to the server with the allocated ID.
            Debug.Log("Ore enter area, trying to create for client");
        }
    }

    void onTriggerExit(Collider col)
    {
        if (col.gameObject.name == "ore_deposit")
        {
            string message = col.gameObject.name + "|" + "create";
            networkView.RPC("createObjectRPC", uLink.RPCMode.Server, col.gameObject.name + "|" + "destroy", col.GetComponent<uLinkNetworkView>().viewID, col.transform.position, col.transform.rotation); //Sends the RPC back to the server with the allocated ID.
            Debug.Log("Ore left area, trying to destroy for client");
        }
    }

    [RPC]
    void deleteObjectRPC(uLink.NetworkViewID viewID)
    {
        uLink.NetworkView view = uLink.NetworkView.Find(viewID);
        Destroy(view.gameObject);
    }

    [RPC]
    void createObjectRPC(string arg, uLink.NetworkViewID viewID, Vector3 position, Quaternion rotation, uLink.NetworkPlayer server)
    {
        GameObject toSpawn = GameObjectBank.getObjectByName(arg);
        GameObject spawned = Instantiate(toSpawn, position, rotation) as GameObject;
        spawned.GetComponent<uLinkNetworkView>().SetViewID(viewID, server);
    }


}
