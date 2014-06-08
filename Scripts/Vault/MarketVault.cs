using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MarketVault : uLink.MonoBehaviour {
    Dictionary<string, Inventory> playerVaults = new Dictionary<string, Inventory>();
    Inventory playerInventory;

	// Use this for initialization
	void Start () {
        if (uLink.Network.isClient)
        {
            playerInventory = new Inventory(0, true);
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void addItemToVault(string playerName, Item item, int quantity)
    {
        if (!playerVaults.ContainsKey(playerName)) playerVaults.Add(playerName, new Inventory(0, true, false)); //Adds a new inventory if one doesn't exist for this player
        Inventory vault = playerVaults[playerName]; // Saves a local copy of the inventory.
        vault.addItem(item, quantity); //Attempts to add the item.
    }

    public void getItemsFromVault()
    {
        networkView.RPC("getItemsFromVaultRPC", uLink.RPCMode.Server, Player.PlayerInfo.staticPlayerName);
    }

    [RPC]
    private void getItemsFromVaultRPC(string arg1, uLink.NetworkMessageInfo info){
        if (uLink.Network.isServer)
        {
            string contents="";
            Inventory vault = null;
            if(this.playerVaults.ContainsKey(arg1))
                vault = this.playerVaults[arg1];

            if (vault == null) contents = "";
            else
            {
                foreach (Inventory.InventoryItem item in vault.getInventoryList())
                {
                    contents += item.getItem().getItemName() + ":" + item.getQuantity() + "|";
                }

                contents.TrimEnd('|');
            }

            networkView.RPC("getItemsFromVaultRPC", info.sender, contents);
        }
        else
        {
            if (arg1 == "") return;

            string[] contents = arg1.Split('|');
            Inventory.InventoryItem[] itemList = new Inventory.InventoryItem[contents.Length];
            int i=0;

            foreach (string itemInfo in contents)
            {
                if (itemInfo == "") break;

                string[] parts = itemInfo.Split(':');
                itemList[i] = new Inventory.InventoryItem(ItemBank.getItemByName(parts[0]), int.Parse(parts[1]));
                i++;
            }

            this.playerInventory.setItems(itemList);
        }
    }

    public Inventory getInventory()
    {
        return this.playerInventory;
    }
}
