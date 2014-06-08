using
	UnityEngine;
public class
ConnectionGUI : MonoBehaviour {
	public string serverIP = "127.0.0.1", playerName="";
	public int serverPort = 7100;

	private bool connected = false;
    private bool loaded = false;

    void Start()
    {
        ItemBank.addItem(new Item("Iron Ore", "resource,ore,iron_ore", "marketable"));
        ItemBank.addItem(new Item("Pristine Iron Ore", "resource,ore,iron_ore", "marketable"));
        ItemBank.addItem(new Item("Copper Ore", "resource,ore,copper_ore", "marketable"));
        ItemBank.addItem(new Item("Tin Ore", "resource,ore,tin_ore", "marketable"));
        ItemBank.addItem(new Item("Silver Ore", "resource,ore,silver_ore", "marketable"));
        ItemBank.addItem(new Item("Gold Ore", "resource,ore,gold_ore", "marketable"));
        ItemBank.addItem(new Item("Grain", "resource,food,grain", "marketable"));
        ItemBank.addItem(new Item("Stone", "resource,ore,stone", "marketable"));
        ItemBank.addItem(new Item("Slab of Meat", "resource,food,meat", "marketable"));
        ItemBank.addItem(new Item("Apple", "resource,food,apple", "marketable"));
        ItemBank.addItem(new Item("Pear", "resource,food,pear", "marketable"));
        ItemBank.addItem(new Item("Plum", "resource,food,plum", "marketable"));
        ItemBank.addItem(new Item("Cow", "resource,livestock,cow", "marketable"));
        ItemBank.addItem(new Item("Red Cow", "resource,livestock,cow", "marketable"));
        ItemBank.addItem(new Item("Black Cow", "resource,livestock,cow", "marketable"));
        ItemBank.addItem(new Item("Chicken", "resource,livestock,chicken", "marketable"));
        ItemBank.addItem(new Item("Goat", "resource,livestock,goat", "marketable"));
        ItemBank.addItem(new Item("Horse", "resource,livestock,horse", "marketable"));
        ItemBank.addItem(new Item("Shortsword", "weapon,1h,sword", "marketable"));
        ItemBank.addItem(new Item("Bardiche", "weapon,2h,polearm", "marketable"));
        ItemBank.addItem(new Item("Claymore", "weapon,2h,sword", "marketable"));
        ItemBank.addItem(new Item("Bloody Claymore", "weapon,2h,sword", "marketable"));
        ItemBank.addItem(new Item("Bow", "weapon,ranged,bow", "marketable"));
        ItemBank.addItem(new Item("Dagger", "weapon,1h,dagger", "marketable"));

    }

    void Update()
    {
        if (loaded)
        {
            if(uLink.Network.isServer)
                Application.LoadLevel("MMO");
            if(uLink.Network.isClient)
                Application.LoadLevel("MMO");
        }
    }

	void OnGUI(){
		// Checking if you are connected to the server or not
		if(uLink.Network.peerType == uLink.NetworkPeerType.Disconnected)
		{
			// Show fields to insert ip address and port
			serverIP = GUI.TextField(new Rect(120,10,100,20),serverIP);
			serverPort =int.Parse(GUI.TextField(new Rect(230,10,40,20),serverPort.ToString()));
            playerName = GUI.TextField(new Rect(280, 10, 100, 20), playerName);

			if(GUI.Button (new Rect(10,10,100,30),"Connect"))
			{
                if (playerName == "") return;
				//This code is run on the client
				uLink.Network.Connect(serverIP, serverPort);
			}
			if(GUI.Button (new Rect(10,50,100,30),"Start Server")){
				//This code is run on the server
				uLink.Network.InitializeServer(32, serverPort);
			}
		}else{
			if(connected){
                if (!loaded)
                {
                    //This code is run when a connection is established
                    string ipaddress = uLink.Network.player.ipAddress;
                    string port = uLink.Network.player.port.ToString();
                    GUI.Label(new Rect(140, 20, 250, 40), "IP Adress: " + ipaddress + ":" + port);
                    if (uLink.Network.isServer)
                        GUI.Label(new Rect(140, 60, 350, 40), "Running as a server");
                    else if (uLink.Network.isClient)
                    {
                        GUI.Label(new Rect(140, 60, 350, 40), "Running as a client");
                        Player.PlayerInfo.staticPlayerName = playerName;
                    }
                    loaded = true;
                    return;
                }

			}else{
				GUI.Label(new Rect(140,20,250,40),"Trying to connect...");
			}
		}
	}

	void uLink_OnServerInitialized(){
		this.connected=true;
		Debug.Log("Server successfully started");
	}

	void uLink_OnConnectedToServer(){
		this.connected=true;
		Debug.Log("Now connected to server");
		Debug.Log("Local Port = " + uLink.Network.player.port.ToString());
	}

	void uLink_OnPlayerDisconnected(uLink.NetworkPlayer player){
		uLink.Network.DestroyPlayerObjects(player);
		uLink.Network.RemoveRPCs(player);
	}

	void uLink_OnFailedToConnect(uLink.NetworkConnectionError error){
		Debug.LogError("uLink got error: "+ error);
	}

	void uLink_OnPlayerConnected(uLink.NetworkPlayer player){
		Debug.Log("Player connected from " + player.ipAddress + ":" + player.port);
	}
}