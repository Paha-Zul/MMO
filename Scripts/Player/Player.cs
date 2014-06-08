using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Player : uLink.MonoBehaviour
{
    public Inventory inventory;

    private Interface inter;
    private Color startColor;
    //private GameObject active;

    private long uniqueID = (long)Random.Range(0, long.MaxValue);
    public PlayerInfo pInfo;

    void Awake()
    {
        if (uLink.Network.isClient && networkView.isOwner)
        {
            pInfo = new PlayerInfo();
            pInfo.playerName = Player.PlayerInfo.staticPlayerName;
            Player.PlayerInfo.clientPlayer = this;
        }
    }
    

    // Use this for initialization
    void Start()
    {
        //Create a new inventory with 20 spaces.
        this.inventory = new Inventory(20, true);
        /*
        for (int i = 0; i < 800; i++) //Generate 800 random items to put in the inventory.
            this.inventory.addItem(ItemBank.getRandomItem());
         */

        //If this is the client, assign the name to it's info and register it with the server.
        if (uLink.Network.isClient) StartCoroutine(setupPlayer());

        this.enabled = false; //Disable this script, nothing in the update or gui calls.
    }

    public bool addItemToPlayerInventory(Item item, int amount)
    {
        bool added = this.inventory.addItem(item, amount);
        if (this.inventory.updateOnChange) updateInventory(); //If the inventory is to be updated on each change, call the RPC.
        return added;
    }

    public bool addItemToPlayerInventory(Item item)
    {
        //if(this.inventory.updateOnChange) networkView.RPC("updateInventoryRPC", uLink.Network.player, ""); //If the inventory is to be updated on each change, call the RPC.
        return addItemToPlayerInventory(item, 1);
    }

    public Item removeItemFromPlayerInventory(string name, int amount)
    {
        Item itemRemoved = this.inventory.removeItem(name, amount);
        if (this.inventory.updateOnChange) updateInventory();
        return itemRemoved;
    }

    public Item removeItemFromPlayerInventory(string name)
    {
        return removeItemFromPlayerInventory(name, 1);
    }

    private void updateInventory()
    {
        if (uLink.Network.isServer)
        {
            string contents = "";
            foreach (Inventory.InventoryItem item in this.inventory.getInventoryList())
            {
                //If the item is null, it's an empty spot in the inventory. We wish to preserve this on the client end. Therefore, a space will
                //indicate an empty spot for the client.
                if (item == null)
                {
                    contents += " " + "|"; //Done for clarity. Add a space and the separater.
                    continue;
                }
                contents += item.getItem().getItemName() + "," + item.getQuantity() + "|";
            }
            contents = contents.TrimEnd('|');
            networkView.RPC("updateInventoryRPC", uLink.RPCMode.Owner, contents);
        }
    }

    [RPC]
    private void updateInventoryRPC(string arg)
    {
        if (uLink.Network.isServer) Debug.Log("Called on server (shouldn't be)");
        else
        {
            string[] items = arg.Split('|');
            List<Inventory.InventoryItem> itemsToAdd = new List<Inventory.InventoryItem>(items.Length);
            foreach (string itemString in items)
            {
                if (itemString == "") break; //If it's an empty string, break. It's the end of the string.
                if (itemString == " ") { itemsToAdd.Add(null); continue; } //If a space, add a null and continue;
                string[] itemInfo = itemString.Split(','); //Otherwise, split the string for info.
                itemsToAdd.Add(new Inventory.InventoryItem(ItemBank.getItemByName(itemInfo[0]), int.Parse(itemInfo[1]))); //Add the item.
            }
            this.inventory.setItems(itemsToAdd); //Overwrites the players inventory.
        }
    }

    private void registerWithServer(string name)
    {
        Debug.Log("Sending name to server: " + name);
        networkView.RPC("registerWithServerRPC", uLink.RPCMode.Server, name);
    }

    [RPC]
    private void registerWithServerRPC(string arg, uLink.NetworkMessageInfo info)
    {
        //Only the server should be receiving this.
        this.pInfo.playerName = arg; //Assign the name to the server player.
        this.pInfo.netPlayer = info.sender; //Assigns the netPlayer
        PlayerBank.addPlayer(this); //Add the player to the PlayerBank on the server's end.
    }
     
    public long getID()
    {
        return this.uniqueID;
    }

    public PlayerInfo getPlayerInfo()
    {
        return this.pInfo;
    }

    /// <summary>
    /// Calls an RPC on the owner of this Player which will create a GameObject with the given viewID, position, and rotation.
    /// </summary>
    /// <param name="name">The name of the GameObject. This will be used to find the GameObject in the GameObjectBank.</param>
    /// <param name="viewID">The ViewID to give to the GameObject.</param>
    /// <param name="pos">The Vector3 position to give to the object.</param>
    /// <param name="rotation">The Quaternion rotation to give to the object.</param>
    public void createGameObject(GameObject obj){
        Debug.Log("Creating some kind of object: " + obj.name);
        networkView.RPC("createGameObjectRPC", uLink.RPCMode.Owner, obj.name, obj.uLinkNetworkView().viewID, obj.transform.position, obj.transform.rotation, obj.uLinkNetworkView().owner); //Send this RPC to the player owner.
    }

    [RPC]
    private void createGameObjectRPC(string name, uLink.NetworkViewID viewID, Vector3 pos, Quaternion rotation, uLink.NetworkPlayer owner)
    {
        //Time to strip the (clone) tag
        string strippedName = name.Substring(0, name.IndexOf('('));
        if (strippedName == "ServerPrefab") strippedName = "ProxyPrefab";

        GameObject toSpawn = GameObjectBank.getObjectByName(strippedName);
        if (toSpawn == null)
        {
            Debug.Log(name + " doesn't exist and can't be instantiated");
            return;
        }
        else
            Debug.Log(name + " is being instantiated!");

        GameObject spawned = Instantiate(toSpawn, pos, rotation) as GameObject;
        spawned.uLinkNetworkView().SetViewID(viewID, owner);
    }

    /// <summary>
    /// Destroys a GameObject on the client's end.
    /// </summary>
    /// <param name="obj">The GameObject to destroy. This will provide to viewID for the client to find and destroy.</param>
    public void destroyGameObject(GameObject obj)
    {
        networkView.RPC("destroyGameObjectRPC", uLink.RPCMode.Owner, obj.uLinkNetworkView().viewID); //Send this RPC to the player owner.
    }

    [RPC]
    private void destroyGameObjectRPC(uLink.NetworkViewID viewID)
    {
        uLink.NetworkView objToDestroy = uLink.NetworkView.Find(viewID);
        if (objToDestroy == null || objToDestroy.gameObject == null) return;
        objToDestroy.gameObject.uLinkNetworkView().DeallocateViewID();
        Destroy(objToDestroy.gameObject);
    }

    public void OnDestroy()
    {
        if (uLink.Network.isServer) PlayerBank.removePlayer(this.pInfo.playerName);
    }

    private IEnumerator setupPlayer()
    {
        yield return new WaitForSeconds(.5f);
        this.pInfo.playerName = Player.PlayerInfo.staticPlayerName; //Assign the player name from the login screen.
        this.registerWithServer(this.pInfo.playerName); //Call the registerWithServer function.
        PlayerBank.addPlayer(this); //Add this player to the player bank.
    }

    public struct PlayerInfo
    {
        public static string staticPlayerName;
        public static Player clientPlayer;
        public string playerName;
        public uLink.NetworkPlayer netPlayer;
    }
}

