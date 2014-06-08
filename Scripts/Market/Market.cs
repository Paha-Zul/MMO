using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MarketFunctions;
using MarketOrderFunctions;

public class Market : uLink.MonoBehaviour {
	public string marketType, ownerName, marketName;
    public Town townParent;
	private bool loaded=false, stopLoading=false;

	//private List<MarketOrder> sellList;
	//private List<MarketOrder> buyList;

    private Dictionary<string, Dictionary<string, Dictionary<string, List<MarketOrder>>>> sellOrders;
    private Dictionary<string, Dictionary<string, Dictionary<string, List<MarketOrder>>>> buyOrders;

    private static string[] names = new string[]{"Billy's","Sally's","Ian's","Will's","Brad's","Jake's","Ben's","Doug's","Timmy's","David's","Chucky's"};
    private static string[] modifiers = new string[] { "Bee's", "Pottery", "Meat", "Crafts", "Weapons", "Supplies", "Tools", "Ores", "Materials" };

    private bool[] sortDir = new bool[5];

    private List<Player> playerQueue = new List<Player>();
    private Tree filterTree = new Tree("filters");

	void Start(){
        if (marketType != "public") this.marketName = names[Random.Range(0, names.Length - 1)] + " " + modifiers[Random.Range(0, modifiers.Length - 1)];
        else this.marketName = "Public Market";

        FileParser.parseFilters(filterTree, "filters.txt");

        Transform possibleParent = gameObject.transform;
        while (!possibleParent.CompareTag("Town"))
            possibleParent = possibleParent.parent;

        this.townParent = possibleParent.GetComponent<Town>();
        if (townParent == null) Debug.Log("Town parent is null");

        sellOrders = new Dictionary<string, Dictionary<string, Dictionary<string, List<MarketOrder>>>>();
        buyOrders = new Dictionary<string, Dictionary<string, Dictionary<string, List<MarketOrder>>>>();

		//If this market is owned by the server, generate some test cases
		if(uLink.Network.isServer){
			//Randoms the number of orders to generate
            int rnd;

            if (this.marketType == "private") rnd = Random.Range(1, 10);
            else rnd = Random.Range(500, 5000);

			//Adds to the sell orders
			for(int i=0;i<rnd;i++){
				this.addSellOrder(new MarketOrder(ItemBank.getRandomItem(), Random.Range(1,1000), Random.Range(1,50), Random.Range(1,20), "sell", "vlad"));
			}

            if (this.marketType == "private") rnd = Random.Range(1, 10);
            else rnd = Random.Range(500, 5000);
			//Adds to the sell orders
			for(int i=0;i<rnd;i++){
				this.addBuyOrder(new MarketOrder(ItemBank.getRandomItem(), Random.Range(1,1000), Random.Range(1,50), Random.Range(1,20), "buy", "vlad"));
			}
			//Something...
			this.loaded=true;

		}
	}

    /// <summary>
    /// Sorts the buy or sell orders list directed by the 'type' variable. Will use the 'filter' variable to know which
    /// item list to sort.
    /// </summary>
    /// <param name="filter">The filter of the object. Ex: "weapon,1h,sword"</param>
    /// <param name="type">The type of the list, ie: "buy" or "sell"</param>
    /// <param name="propToSortBy">The property to sort by. This needs to be one of the following: "name" "price" "quantity" "duration"</param>
    public void sortItemList(string filter, string type, string propToSortBy)
    {
        string[] filters = filter.Split(',');

        if (propToSortBy == "name") sortDir[0] = !sortDir[0];
        else if (propToSortBy == "price") sortDir[1] = !sortDir[1];
        else if (propToSortBy == "quantity") sortDir[2] = !sortDir[2];
        else if (propToSortBy == "duration") sortDir[3] = !sortDir[3];
        else if (propToSortBy == "owner") sortDir[4] = !sortDir[4];

        Dictionary<string, Dictionary<string, Dictionary<string, List<MarketOrder>>>> orders;
        if (type == "buy") orders = buyOrders;
        else orders = sellOrders;

        //If the top filter doesn't exist, add it.
        if (!orders.ContainsKey(filters[0])) return;
        //if the sub filter doesn't exist, add it.
        if (!orders[filters[0]].ContainsKey(filters[1])) return;
        //if the sub sub filter doesn't exist, add it.
        if (!orders[filters[0]][filters[1]].ContainsKey(filters[2])) return;

        List<MarketOrder> sortedList = orders[filters[0]][filters[1]][filters[2]];

        if (type == "buy")
        {
            //Sorts the list.
            sortedList.Sort(
                delegate(MarketOrder o1, MarketOrder o2)
                {
                    if(propToSortBy == "name"){
                        if (sortDir[0]) return o1.getItem().getItemName().CompareTo(o2.getItem().getItemName());
                        return o2.getItem().getItemName().CompareTo(o1.getItem().getItemName());
                    }else if (propToSortBy == "price"){
                        if(sortDir[1]) return o1.getPrice().CompareTo(o2.getPrice());
                        return o2.getPrice().CompareTo(o1.getPrice());
                    }else if (propToSortBy == "quantity"){
                        if (sortDir[2]) return o1.getQuantity().CompareTo(o2.getQuantity());
                        return o2.getQuantity().CompareTo(o1.getQuantity());
                    }else if (propToSortBy == "duration"){
                        if (sortDir[3]) return o1.getDuration().CompareTo(o2.getDuration());
                        return o2.getDuration().CompareTo(o1.getDuration());
                    }

                    if (sortDir[4]) return o1.getOwnerName().CompareTo(o2.getOwnerName());
                    return o2.getOwnerName().CompareTo(o1.getOwnerName());
                }
            );
            buyOrders[filters[0]][filters[1]][filters[2]] = sortedList;
        }
        //Otherwise we need to sort the sell order.
        else
        {
            //Sorts the list.
            sortedList.Sort(
                delegate(MarketOrder o1, MarketOrder o2)
                {
                    if (propToSortBy == "name")
                    {
                        if (sortDir[0]) return o1.getItem().getItemName().CompareTo(o2.getItem().getItemName());
                        return o2.getItem().getItemName().CompareTo(o1.getItem().getItemName());
                    }
                    else if (propToSortBy == "price")
                    {
                        if (sortDir[1]) return o1.getPrice().CompareTo(o2.getPrice());
                        return o2.getPrice().CompareTo(o1.getPrice());
                    }
                    else if (propToSortBy == "quantity")
                    {
                        if (sortDir[2]) return o1.getQuantity().CompareTo(o2.getQuantity());
                        return o2.getQuantity().CompareTo(o1.getQuantity());
                    }
                    else if (propToSortBy == "duration")
                    {
                        if (sortDir[3]) return o1.getDuration().CompareTo(o2.getDuration());
                        return o2.getDuration().CompareTo(o1.getDuration());
                    }

                    if (sortDir[4]) return o1.getOwnerName().CompareTo(o2.getOwnerName());
                    return o2.getOwnerName().CompareTo(o1.getOwnerName());
                }
            );
            sellOrders[filters[0]][filters[1]][filters[2]] = sortedList;
        }
    }

    public void addOrder(MarketOrder order)
    {
        string[] filters = order.getItem().getItemFilters().Split(','); //Splits the filters, there should be three
        Dictionary<string, Dictionary<string, Dictionary<string, List<MarketOrder>>>> orders;

        if(order.getOrderType() == "buy") orders = buyOrders;
        else orders = sellOrders;

        //If the top filter doesn't exist, add it.
        if (!orders.ContainsKey(filters[0])) orders.Add(filters[0], new Dictionary<string, Dictionary<string, List<MarketOrder>>>());
        //if the sub filter doesn't exist, add it.
        if (!orders[filters[0]].ContainsKey(filters[1])) orders[filters[0]].Add(filters[1], new Dictionary<string, List<MarketOrder>>());
        //if the sub sub filter doesn't exist, add it.
        if (!orders[filters[0]][filters[1]].ContainsKey(filters[2])) orders[filters[0]][filters[1]].Add(filters[2], new List<MarketOrder>());

        if (uLink.Network.isServer)
            if (order.getOrderType() == "buy") filterTree.getNode(order.getItem().getItemFilters(), Tree.RAW_FILTER).addAmountToNode(1, Tree.BUY_AMOUNT);
            else filterTree.getNode(order.getItem().getItemFilters(), Tree.RAW_FILTER).addAmountToNode(1, Tree.SELL_AMOUNT);

        //Randomizes the version number for the server.
        if (uLink.Network.isServer)
            filterTree.getNode(order.getItem().getItemFilters(), Tree.RAW_FILTER).setValue(Tree.VERSION, Random.Range(0, int.MaxValue).ToString());

        orders[filters[0]][filters[1]][filters[2]].Add(order); //Adds the order
    }

	public void addBuyOrder(MarketOrder order){
        addOrder(order);
	}

	public void addSellOrder(MarketOrder order){
        addOrder(order);
	}

    public void removeOrder(MarketOrder order){
        string[] filters = order.getItem().getItemFilters().Split(','); //Splits the filters, there should be three
        Dictionary<string, Dictionary<string, Dictionary<string, List<MarketOrder>>>> orders;

        if(order.getOrderType() == "buy") orders = buyOrders;
        else orders = sellOrders;

        //Randomizes the version number for the server.
        if(uLink.Network.isServer) filterTree.getNode(order.getItem().getItemFilters()).setValue(Tree.VERSION, Random.Range(0, int.MaxValue).ToString());

        orders[filters[0]][filters[1]][filters[2]].Remove(order); //Removes the order.
    }

    /// <summary>
    /// Removes a buy order.
    /// </summary>
    /// <param name="order">The MarketOrder object to remove.</param>
	public void removeBuyOrder(MarketOrder order){
        removeOrder(order);
	}

    /// <summary>
    /// Removes a sell order.
    /// </summary>
    /// <param name="order">The MarketOrder to remove.</param>
	public void removeSellOrder(MarketOrder order){
        removeOrder(order);
	}

    /// <summary>
    /// Gets the number of total buy orders.
    /// </summary>
    /// <returns>An integer which is the total number of buy orders.</returns>
	public int numberOfBuyOrders(){
        int num = 0;
        foreach (Dictionary<string, Dictionary<string, List<MarketOrder>>> dict in buyOrders.Values)
            foreach (Dictionary<string, List<MarketOrder>> dict2 in dict.Values)
                foreach (List<MarketOrder> list in dict2.Values)
                     num += list.Count;

        return num;
	}

    /// <summary>
    /// Gets the number of buy orders specified by the filters.
    /// </summary>
    /// <returns>An integer which is the total number of buy orders in the specified filters.</returns>
    public int numberOfBuyOrders(string[] filters)
    {
        int num = 0;
        num = this.getListIfExists(filters, "buy").Count;

        return num;
    }

    /// <summary>
    /// Gets the number of total sell orders.
    /// </summary>
    /// <returns>An integer which is the total number of sell orders.</returns>
	public int numberOfSellOrders(){
        int num = 0;
        foreach (Dictionary<string, Dictionary<string, List<MarketOrder>>> dict in sellOrders.Values)
            foreach (Dictionary<string, List<MarketOrder>> dict2 in dict.Values)
                foreach (List<MarketOrder> list in dict2.Values)
                    num += list.Count;

        return num;
	}

    /// <summary>
    /// Gets the number of sell orders specified by the filters.
    /// </summary>
    /// <returns>An integer which is the total number of sell orders in the specified filters.</returns>
    public int numberOfSellOrders(string[] filters)
    {
        int num = 0;
        num = this.getListIfExists(filters, "sell").Count;

        return num;
    }

    /// <summary>
    /// Gets the master dictionary of the buy orders.
    /// </summary>
    /// <returns>A Dictionary that holds a Dictionary that holds a Dictionary that holds a list.</returns>
    public Dictionary<string, Dictionary<string, Dictionary<string, List<MarketOrder>>>> getBuyOrders()
    {
		return this.buyOrders;
	}

    /// <summary>
    /// Gets the master dictionary of the sell orders.
    /// </summary>
    /// <returns>A Dictionary that holds a Dictionary that holds a Dictionary that holds a list.</returns>
    public Dictionary<string, Dictionary<string, Dictionary<string, List<MarketOrder>>>> getSellOrders()
    {
		return this.sellOrders;
	}

    /// <summary>
    /// Gets the list of items which is specified by the filters string.
    /// </summary>
    /// <param name="filters">The filters to get items from. Example: "resource,ore,iron_ore"</param>
    /// <returns>The list of items of that item type.</returns>
    public List<MarketOrder> getBuyOrdersItemList(string filters)
    {
        string[] filters2 = filters.Split(','); //Splits the filters
        if (!this.filterExists(filters2, buyOrders)) return new List<MarketOrder>(0); //If the filter doesn't exist, return an empty list.

        return buyOrders[filters2[0]][filters2[1]][filters2[2]]; //If it exists, return the list.
    }

    /// <summary>
    /// Gets the list of items which is specified by the filters string.
    /// </summary>
    /// <param name="filters">The filters to get items from. Example: "resource,ore,iron_ore"</param>
    /// <returns>The list of items of that item type.</returns>
    public List<MarketOrder> getSellOrdersItemList(string filters)
    {
        string[] filters2 = filters.Split(','); //Splits the filters

        if (!this.filterExists(filters2, sellOrders)) return new List<MarketOrder>(0); //If the filter doesn't exist, return an empty list.

        return sellOrders[filters2[0]][filters2[1]][filters2[2]]; //If it exists, return the list.
    }


	public bool isLoaded(){
		return this.loaded;
	}


	public void clearOwner(){
		this.ownerName = "none";
        foreach (Dictionary<string, Dictionary<string, List<MarketOrder>>> dict in buyOrders.Values)
            foreach (Dictionary<string, List<MarketOrder>> dict2 in dict.Values)
                foreach (List<MarketOrder> list in dict2.Values)
                    list.Clear();

        foreach (Dictionary<string, Dictionary<string, List<MarketOrder>>> dict in sellOrders.Values)
            foreach (Dictionary<string, List<MarketOrder>> dict2 in dict.Values)
                foreach (List<MarketOrder> list in dict2.Values)
                    list.Clear();
	}

	public void clearContents(){
        foreach (Dictionary<string, Dictionary<string, List<MarketOrder>>> dict in buyOrders.Values)
            foreach (Dictionary<string, List<MarketOrder>> dict2 in dict.Values)
                foreach (List<MarketOrder> list in dict2.Values)
                    list.Clear();

        foreach (Dictionary<string, Dictionary<string, List<MarketOrder>>> dict in sellOrders.Values)
            foreach (Dictionary<string, List<MarketOrder>> dict2 in dict.Values)
                foreach (List<MarketOrder> list in dict2.Values)
                    list.Clear();

		//this.stopLoading=true;
	}

    /// <summary>
    /// Clears the specified list from the market.
    /// </summary>
    /// <param name="filters">The filters of the item type.</param>
    /// <param name="type">The type of order. This will be "sell" or "buy"</param>
    public void clearFilterContents(string filters, string type)
    {
        string[] filters2 = filters.Split(',');

        this.getListIfExists(filters2, type).Clear();
    }

    public Tree getMarketTree()
    {
        return this.filterTree;
    }

    public string getMarketType()
    {
        return marketType;
    }

    public void getOrderAmounts()
    {
        networkView.RPC("getOrderAmountsRPC", uLink.RPCMode.Server, "");
    }

    private void changeVersionNumber(Tree.Node node, int versionNum)
    {
        node.setValue(Tree.VERSION, versionNum.ToString());
    }

    [RPC]
    private void getOrderAmountsRPC(string arg1, uLink.NetworkMessageInfo info)
    {
        if (uLink.Network.isServer)
        {
            List<Tree.Node> leafList = filterTree.getAllLeafNodes();
            string message = "";
            foreach (Tree.Node node in leafList)
                message += node.getValues()[Tree.SELL_AMOUNT] + " " + node.getValues()[Tree.BUY_AMOUNT] + "|";
            message = message.TrimEnd('|');
            networkView.RPC("getOrderAmountsRPC", info.sender, message);
        }
        else
        {
            this.filterTree.SetAllNodesValue(new int[] { Tree.BUY_AMOUNT, Tree.SELL_AMOUNT }, "0"); //Reset the buy and sell order amounts.
            string[] amounts = arg1.Split('|'); //Split the string.
            List<Tree.Node> leaves = this.filterTree.getAllLeafNodes();

            //For all leaf nodes/amounts.
            for (int i = 0; i < leaves.Count; i++)
            {
                string[] vals = amounts[i].Split(' ');
                leaves[i].addAmountToNode(int.Parse(vals[0]), Tree.SELL_AMOUNT); //Add to the buy amount.
                leaves[i].addAmountToNode(int.Parse(vals[1]), Tree.BUY_AMOUNT); //Add to the sell amount.
            }
        }
    }

	/// <summary>
	/// Gets the contents specified by 'type'. The following actions happen for the server and client.
    /// 
    /// Server: The server will take in an order that consists of "orderType itemType versionNum" (ex: "buy resource,ore,iron_ore 1023849"). The server 
    /// first checks if the versionNum in the arguments matches its version number. If so, the function is terminated. If they do not match, the server will
    /// compile a list of contents based on the orderType and itemType. orderType "buy" and "sell" will gather from the buy and sell orders respectively.
    /// An itemType of "none" will gather from every existing filter that the order structure has. Otehrwise, an itemType of type "resource,ore,iron_ore" will be
    /// passed in. This will gather MarketOrders only from that filter type. These contents will then be returned to the client for processing if the versionNum does
    /// not match.
    /// 
    /// Client: The client will receive a string that follows the style of "versionNum=itemName-itemInfo2,itemInfo3,..." and will process it.
    /// If the client is receiving this information, it means the client's version did not match the servers. The versionNum will be assigned
    /// and the itemInfos will be created into MarketOrders to be displayed for the client.
	/// </summary>
    /// <param name="orderType">The type of the contents. This value should either be 'buy' or 'sell'.</param>
    /// <param name="itemType">The type of the item. This is the filter of the item. Ex: "weapon,2h,sword".</param>
    /// <param name="versionNum">The version number of the market. This will be matched against the server's market number.</param>
	public void getContents(string orderType, string itemType, string versionNum){
        this.stopLoading = false;
        networkView.RPC("getContentsRPC", uLink.RPCMode.Server, orderType + " " + itemType + " " + versionNum);
	}

	/// <summary>
	/// Gets the contents of the market and sends the contents to the client requesting it.
	/// </summary>
	/// <param name="arg1">The string of information for the RPC</param>
	[RPC]
	void getContentsRPC(string arg1, uLink.NetworkMessageInfo info){
        string[] parts = arg1.Split(' ');

		//If the owner of this object is the server, generate a single string containing all the requested
		//orders (which should be in the format "iteminfo,orderinfo-...") and call this RPC function again.
		if(uLink.Network.isServer){
            //If the market version number sent from the client matches the server's version, don't send the info.
            string version = this.filterTree.getNode(parts[1], Tree.RAW_FILTER).getValues()[Tree.VERSION];
            if (parts[2] == version) return;

            string contents = version+"=";
            Dictionary<string, Dictionary<string, Dictionary<string, List<MarketOrder>>>> orders;
            if (parts[0] == "sell") orders = sellOrders;
            else orders = buyOrders;

            //If we have no filter type, gather everything from every filter type.
            if (parts[1] == "none")
            {
                foreach (Dictionary<string, Dictionary<string, List<MarketOrder>>> dict in orders.Values)
                    foreach (Dictionary<string, List<MarketOrder>> dict2 in dict.Values)
                        foreach (List<MarketOrder> list in dict2.Values)
                            foreach (MarketOrder order in list)
                                contents += order.getOrderData() + "-";
            }
            //If there is a filter type, gather only from the specified filter.
            else
            {
                string[] filters = parts[1].Split(',');
                //If the subitem actually exists (this tests for filter->subfilter->itemType
                if (this.filterExists(filters, orders))
                    //Get each order.
                    foreach (MarketOrder order in this.getListIfExists(filters, parts[0]))
                        contents += order.getOrderData() + "-";
                else
                    Debug.Log("Filter didn't exist for some reason: " + filters[0] + " " + filters[1] + " " + filters[2] + " numOrders: " + this.numberOfSellOrders());
            }

            //Sends the RPC with the information just gathered
			networkView.RPC("getContentsRPC", info.sender, contents);

		//If the owner is the client, the data needs to be decoded and formed into orders.
		}else if(uLink.Network.isClient){
			//We call a coroutine to build the list over many frames. This prevents slowdown of frames.
			StartCoroutine(generateList(arg1));
		}
	}

    /// <summary>
    /// Calls a request for the server to place an order with the parameters passed in. It will use the item data plus the
    /// market data (orderType, price, quantitity, duration) to form a order request to be sent via RPC.
    /// </summary>
    /// <param name="item"> The Item that an order will be posted for.</param>
    /// <param name="orderType">The order type. This will be "buy" or "sell"</param>
    /// <param name="price"> The asking price of the order.</param>
    /// <param name="quantity"> The amount of the item.</param>
    /// <param name="duration"> The duration (in days) of the order.</param>
    public void addOrderRequest(Item item, string orderType, string price, string quantity, string duration, string playerID, string playerName, Player player)
    {
        this.playerQueue.Add(player);
        string contents = item.getData() + ":" + orderType + "|" + price + "|" + quantity + "|" + duration + "|" + playerID + "|" + playerName;
		//Triggers to RPC
		networkView.RPC("addOrderRequestRPC", uLink.RPCMode.Server, contents);
	}

	/// <summary>
	/// Adds the order request RP.
	/// </summary>
	/// <param name="args1">Args1.</param>
	[RPC]
    private void addOrderRequestRPC(string arg1, uLink.NetworkMessageInfo info)
    {
		if(uLink.Network.isServer){
			string[] parts = arg1.Split(':'); //Splits the string into 
            string[] itemInfo = parts[0].Split('|'); //This holds all the item info.
            string[] orderInfo = parts[1].Split('|'); //This holds all the order info.
            string playerName = orderInfo[5];

            int quantityWanted = int.Parse(orderInfo[2]);

            bool buy = orderInfo[0] == "buy"; //A bool to check for buy or sell type orders;

            Dictionary<string, Dictionary<string, Dictionary<string, List<MarketOrder>>>> orderStruct;

            //If we're buying, trying to buy from the sell orders. If selling, try to sell to buy orders.
            if (buy) orderStruct = sellOrders;
            else orderStruct = buyOrders;

            //Get the item from the item bank and it's filters.
            Item tmpItem = ItemBank.getItemByName(itemInfo[0]);
            string[] filters = tmpItem.getItemFilters().Split(',');

            //Find the order by price which is of the correct type (buy or sell)
            MarketOrder orderToMatch = this.getOrderAtPrice(filters, int.Parse(orderInfo[1]), orderInfo[0], itemInfo[0]);

            int quantity = int.Parse(orderInfo[2]), overflow = -1;
            string playerBeingFulfilled;

            //If there is a matching order, try to subtract the quantity we want from it first. If we need more than the matching order has,
            //create a new order in the market for the remaining amount.
            if (orderToMatch != null)
            {
                if (buy) playerBeingFulfilled = playerName;
                else playerBeingFulfilled = orderToMatch.getOwnerName();

                //This will be positive if the order has enough to fullfill our order. Neagative if we need more than the order has.
                overflow = orderToMatch.getQuantity() - quantityWanted;

                //If the overflow is 0 or negative, remove the order from the list.
                if (overflow <= 0)
                {
                    quantity = orderToMatch.getQuantity(); //Record the amount that is being fullfilled by this order before destroying it.
                    this.getListIfExists(filters, orderStruct).Remove(orderToMatch); //Remove the order from the list.
                }
                //If the overflow is positive, partially fulfill the orders.
                else
                {
                    quantity = quantityWanted;
                    orderToMatch.addQuantity(-quantityWanted);
                    changeVersionNumber(filterTree.getNode(tmpItem.getItemFilters(), Tree.RAW_FILTER), Random.Range(0, int.MaxValue));
                }

                orderToMatch.fulfillOrder(quantity, playerBeingFulfilled, this.townParent);
            }

            //If the overflow is negative, we need to create a new order with the overflow amount (-overflow makes it positive).
            if (overflow < 0) { Debug.Log("Adding a new order"); this.addOrder(new MarketOrder(tmpItem, int.Parse(orderInfo[1]), -overflow, int.Parse(orderInfo[3]), orderInfo[0], playerName)); }

            //Compiles a message that looks like itemName|quantity|playerID|orderType
            if (buy) networkView.RPC("addOrderRequestRPC", info.sender, tmpItem.getItemName() + "|" + -overflow + "|" + orderInfo[4] + "|" + orderInfo[0]);
            else networkView.RPC("addOrderRequestRPC", info.sender, tmpItem.getItemName() + "|" + orderInfo[2] + "|" + orderInfo[4] + "|" + orderInfo[0]);
		}else{
            string[] parts = arg1.Split('|');
            string itemName = parts[0];
            string quantity = parts[1];
            string playerID = parts[2];
            string orderType = parts[3];
            long ID = long.Parse(playerID);

            bool buy = orderType == "buy"; //Keeps a bool. Less computation than string comparison for every check.

            //Searches for the ID of the player that sent this request. Traverses in reverse to allow removing while iterating.
            for (int i = playerQueue.Count - 1; i >= 0; i--)
            {
                Player player = playerQueue[i]; //Get the player
                if (player.getID() == ID) //If the ID matches
                {
                    if (!buy) player.removeItemFromPlayerInventory(itemName, int.Parse(quantity)); //Remove a quantity from the item.
                    else
                    {
                        Item item = ItemBank.getItemByName(itemName); //Get the item from the bank.
                        player.addItemToPlayerInventory(item, int.Parse(quantity)); //Add the item and quantity to the player.
                    }
                    playerQueue.RemoveAt(i); //Remove the player if it is found.
                    break;
                }
            }
		}
	}

	/********************************** COROUTINES ***********************************/
	private IEnumerator generateList(string arg1){
		//The single giant list is split by the delimeter '-', which separates different packets of information (Ex: info1-info2-info3)
        string[] versionAndInfo = arg1.Split('=');
        string[] tmp = versionAndInfo[1].Split('-');
		int counter=0;
        string filters="";
        bool flag = false;
        Tree.Node nodeToCheck = null;

		//For each string we just split.
		foreach(string split in tmp){
			//This prevents the last empty string in array from executing any further.
			if(split=="") break;

            if (this.stopLoading) { this.stopLoading = false; break; }

			string[] parts = split.Split(':'); 	//Splits the string into itemInfo and orderInfo
            string[] itemInfo = parts[0].Split('|'); //Splits the string by the | character. 

			string itemName = itemInfo[0]; //Records the (item)name
			//string itemFilters = itemInfo[1]; //Records the (item)filters
            //string itemTags = itemInfo[2]; //Records the (item)Tags

			string[] orderInfo = parts[1].Split('|'); //Splits the order info
			
			//Create the item using the name and tags
            Item tmpItem = ItemBank.getItemByName(itemName);
            filters = tmpItem.getItemFilters();

            if (!flag)
            {
                nodeToCheck = filterTree.getNode(filters, Tree.RAW_FILTER);
                if (versionAndInfo[0] != nodeToCheck.getValues()[Tree.VERSION]) this.clearFilterContents(tmpItem.getItemFilters(), orderInfo[0]);
                flag = true;
            } 

			//Construct the buy order with the info and add it to the list.
            if (orderInfo[0] == "buy") this.addBuyOrder(new MarketOrder(tmpItem, int.Parse(orderInfo[1]), int.Parse(orderInfo[2]), int.Parse(orderInfo[3]), "buy", orderInfo[5], long.Parse(orderInfo[4])));
            else this.addSellOrder(new MarketOrder(tmpItem, int.Parse(orderInfo[1]), int.Parse(orderInfo[2]), int.Parse(orderInfo[3]), "sell", orderInfo[5], long.Parse(orderInfo[4])));

			counter++;

			if(counter > 5){
				counter=0;
				//Yields this function until the next frame.
				yield return null;
			}
		}

        this.filterTree.getNode(filters, Tree.RAW_FILTER).setValue(Tree.VERSION, versionAndInfo[0]);
		//A flag for something...
		this.loaded=true;
	}
}