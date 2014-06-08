using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Gatherable : uLink.MonoBehaviour
{
    public string gatherableType, gatherableName;
    public int totalResourceMax = 100, totalResourceMin = 20, amountPerTick = 1, timePerTick = 1;

    private List<Player> playerList = new List<Player>();

    public int totalResource, currentResource;

    void Start()
    {
        totalResource = Random.Range(totalResourceMin, totalResourceMax);
        this.currentResource = this.totalResource;

        this.enabled = false;
    }

    public void run(Player playerScript)
    {
        playerList.Add(playerScript);
        subtractResource(playerScript);
    }

    /// <summary>
    /// Subtracts the resources from this resource node. Sends the name of the player that is gathering from it. The amount
    /// gathered will be handled by the server and server values.
    /// </summary>
    /// <param name="amount"></param>
    private void subtractResource(Player player)
    {
        networkView.RPC("subtractResourceRPC", uLink.RPCMode.Server, player.getPlayerInfo().playerName);
    }

    [RPC]
    private void subtractResourceRPC(string arg, uLink.NetworkMessageInfo info)
    {
        if (uLink.Network.isServer)
        {
            Player player = PlayerBank.getPlayerByName(arg); //Gets the player by name from the PlayerBank of connected players.

            int amount = this.amountPerTick; //Assigns a temp variable the amountPerTick. This can be altered by modifiers alter (better skill of the player?)
            int amountGained = 0; //This is the overflow of the amount in case we gather more than the resource has.
            
            if (this.currentResource - amount > 0) amountGained = amount; //If the subtraction is positive, give the player the whole amount.
            else amountGained = amount - this.currentResource; //If the subtraction was negative, give the player the difference of the two.

            this.currentResource -= amount; //Subtract the amount from the node.
            player.addItemToPlayerInventory(ItemBank.getItemByName(this.gatherableName), amountGained);

            //networkView.RPC("subtractResourceRPC", info.sender, amountGained.ToString()); //Send the RPC with the amount gathered.
            if (this.currentResource <= 0) //If the resource node has less than 0, destroy it.
            {
                uLink.Network.Destroy(this.gameObject); //Destroy the node for all connected clients.
                this.transform.parent.parent.GetComponent<MineManager>().decrementAmountCurrentlySpawned(); //Decrement the amount spawned from the MineManager.
            }
        }
        else
        {
            Item itemToAdd = ItemBank.getItemByName(this.gatherableName);
            playerList[0].addItemToPlayerInventory(itemToAdd);
            playerList.RemoveAt(0);
        }
        
    }
}
