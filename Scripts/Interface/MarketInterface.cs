using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions; // needed for Regex
using MarketFunctions;

public class MarketInterface {
    public Texture2D background, orderBackground;
    public Texture2D[] buttonBackground;
    public GUIStyle style;

    private Market market;
    private bool activeWindow, rightClicked=true;

    private Rect marketRect, orderRect, sellOrdersGroupRect, buyOrdersGroupRect, sellOrdersRect, buyOrdersRect, sellOrdersScrollRect, buyOrdersScrollRect;
    private Rect orderListingRect;

    private Vector2 sellOrderScrollPosition = Vector2.zero, buyOrderScrollPosition = Vector2.zero;
    
    private int selectedFilter = -1, selectedSubFilter = -1, selectedSubSubFilter = -1;
    private string selFilter = "", selSubFilter = "", selSubSubFilter = "";

    private List<int> numItems = new List<int>();
    private Interface inter;

    public OrderInfoStruct orderInfo;
    public ComboBox comboBox;

    public MarketInterface(Interface inter)
    {
        this.inter = inter;

        //Defins some rectangles that are going to be used.
        //Rect for the entire market window
        this.marketRect = new Rect(50, 50, 600, 500);

        //Rect for placing orders. This is where information about a buy/sell order will be entered.
        this.orderRect = new Rect(300, 150, 200, 200);

        //Rects for the individual areas. This will be where orders are displayed for each category
        this.sellOrdersGroupRect = new Rect(marketRect.width * 0.3f, 20f, marketRect.width - marketRect.width * 0.31f, marketRect.height * 0.48f - 20f);
        this.buyOrdersGroupRect = new Rect(marketRect.width * 0.3f, sellOrdersGroupRect.height + sellOrdersGroupRect.y, marketRect.width - marketRect.width * 0.31f, marketRect.height * 0.48f - 20);
        
        //Rects for the background of the orders. These are the same width and height as the order groups but they start at 0,0.
        //Since these will be displayed inside the groups, 0,0 is at the origin of the parent group.
        this.sellOrdersRect = new Rect(0f, 0f, this.sellOrdersGroupRect.width, this.sellOrdersGroupRect.height);
        this.buyOrdersRect = new Rect(0f, 0f, this.buyOrdersGroupRect.width, this.buyOrdersGroupRect.height);

        //These are the viewable scroll areas. This is where orders can be viewed without having to scroll.
        this.sellOrdersScrollRect = new Rect(0, 40, sellOrdersRect.width, sellOrdersRect.height - 40);
        this.buyOrdersScrollRect = new Rect(0, 40, buyOrdersRect.width, buyOrdersRect.height - 40);

        orderInfo = new OrderInfoStruct();
        orderInfo.orderType = "none";
        orderInfo.price = "0";
        orderInfo.itemName = "";
        orderInfo.quantity = "1";
        orderInfo.duration = "1";
    }

    public void update()
    {
        if (Input.GetMouseButtonDown(1)) this.rightClicked = true;
        else this.rightClicked = false;
    }

    public void run()
    {
        style.alignment = TextAnchor.MiddleCenter;
        marketRect = GUI.Window(0, marketRect, DoMyWindow, this.market.marketName);

        if (this.orderInfo.orderType != "none") //If the user has clicked the place order button, draw the place order window.
            orderRect = GUI.Window(1, orderRect, DoMyWindow, "Place " + this.orderInfo.orderType + " Order");
    }

    public void toggleActive(Market market)
    {
        this.activeWindow = !this.activeWindow;
        this.setMarket(market);

        //If the interface has been activated
        if (this.activeWindow) this.getMarket().getOrderAmounts();
        else this.setMarket(null);
    }

    void DoMyWindow(int windowID)
    {
        if (windowID == 0)
        {
            drawFilters();

            //Makes a group, then a box for the background (graphical only really), then a scroll area, and the orders go inside the scroll area.
            GUI.BeginGroup(sellOrdersGroupRect, ""); //Begins the group. This is used mainly for organization and clipping content
            GUI.Box(sellOrdersRect, "Sell Orders (" + this.market.numberOfSellOrders(this.getCurrentFiltersArray()) + ")"); //Draws the box. Just a simple background
            this.drawColumnLabels(sellOrdersScrollRect.x, sellOrdersScrollRect.y-20, "sell"); //Draws the column labels.
            sellOrderScrollPosition = GUI.BeginScrollView(sellOrdersScrollRect, sellOrderScrollPosition, //Draws the scroll view. This will clip items and allow the user to scroll to see them.
                                                          new Rect(0, 0, sellOrdersScrollRect.width, this.market.numberOfSellOrders(this.getCurrentFiltersArray()) * 18));
            drawOrders("sell"); //Draws all the info for sell orders
            GUI.EndScrollView(); //Ends the scroll view
            GUI.EndGroup(); //Ends the group.

            //Makes a group, then a box for the background (graphical only really), then a scroll area, and the orders go inside the scroll area.
            GUI.BeginGroup(buyOrdersGroupRect, "");
            GUI.Box(buyOrdersRect, "Buy Orders (" + this.market.numberOfBuyOrders(this.getCurrentFiltersArray()) + ")");
            this.drawColumnLabels(buyOrdersScrollRect.x, buyOrdersScrollRect.y - 20, "buy");
            buyOrderScrollPosition = GUI.BeginScrollView(buyOrdersScrollRect, buyOrderScrollPosition,
                                                         new Rect(0, 0, buyOrdersGroupRect.width, this.market.numberOfBuyOrders(this.getCurrentFiltersArray()) * 18));
            drawOrders("buy"); //Draws all the info for sell orders
            GUI.EndScrollView();
            GUI.EndGroup();

            Rect buttonRect = new Rect(marketRect.width - 90, marketRect.height - 40, 80, 30);

            if (this.validFilters())
            {
                //Draws the button for placing an order.
                if (this.market.marketType == "public" && GUI.Button(buttonRect, "Add Buy Order", style))
                {
                    this.orderInfo.orderType = "buy";
                    string filters = this.getCurrentFilters();
                    string[] itemNames = ItemBank.getItemNamesWithFilters(filters);

                    GUIContent[] content = new GUIContent[itemNames.Length];
                    for (int i = 0; i < itemNames.Length; i++)
                        content[i] = new GUIContent(itemNames[i]);

                    this.comboBox = new ComboBox(new Rect(0,0,100,20), content[0], content, style); //Initializes a new combo box.
                    this.orderInfo.invItem = new Inventory.InventoryItem(ItemBank.getItemByName(content[0].text), 99999999); //Creates a new InventoryItem to store temporarily.
                    this.orderInfo.price = this.market.getLowestOrder(filters.Split(','), orderInfo.invItem.getItem().getItemName()).getPrice().ToString(); //Gets the highest buy order.

                    //this.orderInfo.invItem;
                    //GUI.combo
                }
            }

            /*
            buttonRect = new Rect(marketRect.width - 180, marketRect.height - 40, 80, 30);
            //Draws the button for placing an order.
            if (this.market.type == "public" && GUI.Button(buttonRect, "Add Buy Order", style))
                this.orderInfo.orderType = "buy";
            */

        }
        else if (windowID == 1)
            drawOrderInfo(); //Draws all the info for placing an order

        //Allows the window to be dragged
        GUI.DragWindow(new Rect(0, 0, 10000, 10000)); //Drags the window
    }

    /// <summary>
    /// Draws the labels for each column
    /// </summary>
    /// <param name="x">The X location.</param>
    /// <param name="y">The Y location</param>
    void drawColumnLabels(float x, float y, string type)
    {
        this.style.normal.background = null;
        this.style.hover.background = null;
        this.style.active.background = null;

        if (GUI.Button(new Rect(x, y, 80, 20), "Name", style))
            if (this.selectedFilter != -1 && this.selectedSubFilter != -1 && this.selectedSubSubFilter != -1)
                this.market.sortItemList(getCurrentFilters(), type, "name");
        if(GUI.Button(new Rect(x + 120, y, 50, 20), "Quantity", style))
            if (this.selectedFilter != -1 && this.selectedSubFilter != -1 && this.selectedSubSubFilter != -1)
                this.market.sortItemList(getCurrentFilters(), type, "quantity");
        if (GUI.Button(new Rect(x + 180, y, 50, 20), "Price", style))
            if (this.selectedFilter != -1 && this.selectedSubFilter != -1 && this.selectedSubSubFilter != -1)
                this.market.sortItemList(getCurrentFilters(), type, "price");
        if(GUI.Button(new Rect(x + 240, y, 50, 20), "Duration(days)", style))
            if (this.selectedFilter != -1 && this.selectedSubFilter != -1 && this.selectedSubSubFilter != -1)
                this.market.sortItemList(getCurrentFilters(), type, "duration");
        if (GUI.Button(new Rect(x + 300, y, 50, 20), "Owner", style))
            if (this.selectedFilter != -1 && this.selectedSubFilter != -1 && this.selectedSubSubFilter != -1)
                this.market.sortItemList(getCurrentFilters(), type, "owner");
    }

    /// <summary>
    /// Draws the buy/sell orders to the screen that exist in the market that the interface references.
    /// </summary>
    /// <param name="type">The type of order to display ("buy" or "sell")</param>
    void drawOrders(string type)
    {
        float x = 5, y = 0;
        float width = 380, height = 20;

        //Sets the background textures
        this.style.normal.background = orderBackground;
        this.style.hover.background = orderBackground;
        this.style.active.background = orderBackground;


        //Gets the list of all the sell orders from the market that is being referenced.
        //The list can either hold buy or sell roders
        List<MarketOrder> list = new List<MarketOrder>();
        Rect bounds;
        Vector2 pos;

        if (type == "sell")
        {
            if (this.selectedFilter != -1 && this.selectedSubFilter != -1 && this.selectedSubSubFilter != -1)
                list = this.market.getSellOrdersItemList(getCurrentFilters());
               
            bounds = this.sellOrdersScrollRect;
            pos = this.sellOrderScrollPosition;
        }
        else
        {
            if (this.selectedFilter != -1 && this.selectedSubFilter != -1 && this.selectedSubSubFilter != -1)
                list = this.market.getBuyOrdersItemList(getCurrentFilters());

            bounds = this.buyOrdersScrollRect;
            pos = this.buyOrderScrollPosition;
        }


        //For each order, draw the info for each.
        for (int i = 0; i < list.Count; i++)
        {
            MarketOrder order = list[i];
            orderListingRect = new Rect(x, y, width, height);

            //This checks if the y position of the element being drawn is not within the bounds of the viewable area. So if the 
            //box is above or below the viewable area, increment the y value and continue
            if (y < pos.y - height || y > pos.y + bounds.height)
            {
                y += height - 2;
                continue;
            }

            GUI.DrawTexture(orderListingRect, orderBackground);
            GUI.Label(new Rect(x, y, 80, height), "" + order.getItem().getItemName(), style); //Name
            GUI.Label(new Rect(x + 120, y, 50, height), "" + order.getQuantity(), style); //Quantity
            GUI.Label(new Rect(x + 180, y, 50, height), "" + order.getPrice(), style); //Price
            GUI.Label(new Rect(x + 240, y, 50, height), "" + order.getDuration(), style); //Duration
            GUI.Label(new Rect(x + 300, y, 50, height), "" + order.getOwnerName(), style); //Name


            y += height - 2;
        }

        //Sets the background textures for the button
        this.style.normal.background = buttonBackground[0];
        this.style.hover.background = buttonBackground[1];
        this.style.active.background = buttonBackground[2];
        //Creates the rect for the button
    }

    /// <summary>
    /// Draws the information to the screen about posting a buy/sell order.
    /// </summary>
    void drawOrderInfo()
    {
        Rect nameRect, priceRect, quantityRect, durationRect;
        //bool sellOrder = this.orderInfo.orderType == "sell";

        //Initializes the rects to be used.
        nameRect = new Rect(10, 90, 350, 20);
        priceRect = new Rect(10, 110, 120, 20);
        quantityRect = new Rect(10, 130, 120, 20);
        durationRect = new Rect(10, 150, 120, 20);

        //Draws the icon texture
        GUI.DrawTexture(new Rect(10, 25, 64, 64), this.orderInfo.invItem.getItem().getIcon());

        if(this.orderInfo.orderType == "buy"){
            //Sets the rect for the combo box.
            this.comboBox.rect = new Rect(70, 20, 130, 30);
            this.comboBox.Show();

            if (this.comboBox.getChanged())
            {
                this.orderInfo.invItem = new Inventory.InventoryItem(ItemBank.getItemByName(this.comboBox.getCurrentSelectedContent().text), 99999999);
                this.orderInfo.price = this.market.getLowestOrder(this.getCurrentFiltersArray(), this.orderInfo.invItem.getItem().getItemName()).getPrice().ToString();
            }
        }

        //Draws the labels. This includes name/price/quantity/duration
        GUI.Label(nameRect, "Item Name: " + this.orderInfo.invItem.getItem().getItemName());
        GUI.Label(priceRect, "Price:");
        GUI.Label(quantityRect, "Quantity(1-" + this.orderInfo.invItem.getQuantity() + "):");

        GUI.Label(durationRect, "Duration(1-20 days):");

        //Modifies the x and width values of the rects.
        nameRect.x = priceRect.x = quantityRect.x = durationRect.x = 140;
        priceRect.width = quantityRect.width = durationRect.width = 40;

        this.orderInfo.itemName = this.orderInfo.invItem.getItem().getItemName();

        //Produces the three input text field's needed
        this.orderInfo.price = GUI.TextField(priceRect, this.orderInfo.price, 25);
        if (this.orderInfo.price == "") this.orderInfo.price = "1";
        this.orderInfo.price = Regex.Replace(this.orderInfo.price, @"[^0-9 ]", ""); //Replaces any non-digit character with nothing(empty).

        //The quantity text field. This area makes sure it is not blank and is clamped between the correct values and only contains digits.
        this.orderInfo.quantity = GUI.TextField(quantityRect, this.orderInfo.quantity, 25);
        if (this.orderInfo.quantity == "") this.orderInfo.quantity = "1";
        this.orderInfo.quantity = Regex.Replace(this.orderInfo.quantity, @"[^0-9 ]", ""); //Replaces any non-digit character with nothing(empty).
        this.orderInfo.quantity = Mathf.Clamp(int.Parse(this.orderInfo.quantity), 1, this.orderInfo.invItem.getQuantity()).ToString(); //Clamps the value within the allowed amount.

        //The duration text field. Makes sure it's not blanked, is clamped between values, and only contains digits.
        this.orderInfo.duration = GUI.TextField(durationRect, this.orderInfo.duration, 25);
        if (this.orderInfo.duration == "") this.orderInfo.duration = "1";
        this.orderInfo.duration = Regex.Replace(this.orderInfo.duration, @"[^0-9 ]", ""); //Replaces any non-digit character with nothing(empty).
        this.orderInfo.duration = Mathf.Clamp(int.Parse(this.orderInfo.duration), 1, 20).ToString(); //Clamps the value within the allowed amount.

        int throwaway = 0;
        //Draws the post button.
        //If the name is empty or the price is 0, cancel the submit.
        if (GUI.Button(new Rect(orderRect.width - 80, orderRect.height - 30, 70, 20), "Post", style))
        {
            if (this.orderInfo.itemName == "" || this.orderInfo.price == "0" ||
                !int.TryParse(this.orderInfo.price, out throwaway) || !int.TryParse(this.orderInfo.quantity, out throwaway) || !int.TryParse(this.orderInfo.duration, out throwaway))
                return;

            //Gets the item from the item bank
            Item item = this.orderInfo.invItem.getItem();

            if (item == null) return; //If the item is null, don't do anything when this button is pressed.

            //Requests that the market calls the server to submit the order.
            market.addOrderRequest(item, this.orderInfo.orderType, this.orderInfo.price, this.orderInfo.quantity,
                this.orderInfo.duration, this.inter.getPlayerScript().getID().ToString(), Player.PlayerInfo.staticPlayerName, this.inter.getPlayerScript());

            //Reset some values.
            this.orderInfo.orderType = "none";
            this.orderInfo.itemName = "";
            this.orderInfo.price = "0";
            this.orderInfo.quantity = "1";
            this.orderInfo.duration = "20";
        }
    }

    /// <summary>
    /// Draws the filters along the left side of the market window. This will be a cascade type of system, where the client will select
    /// the filter->subfilter->subsubfilter. When the subsubfilter is selected, the market will be populated from the server's instance of the market.
    /// </summary>
    void drawFilters()
    {
        //Different x,y,width,and height for each type of section/filter.
        int x = 10, y = 20;
        int width = 170, height = 20;
        int subX = 20, subWidth = 160, subHeight = 20;
        int subSubX = 30, subSubWidth = 150, subSubHeight = 20;

        style.normal.background = this.inter.marketButtonBackground[0];
        style.hover.background = this.inter.marketButtonBackground[1];
        style.active.background = this.inter.marketButtonBackground[2];

        List<Tree.Node> tempFilters = this.getMarket().getMarketTree().getNode("").getChildren();

        //for each 'main' filter.
        for (int i = 0; i < tempFilters.Count; i++)
        {
            string filter = tempFilters[i].getNodeName();
            string buttonVal = filter + " (" + tempFilters[i].getValues()[Tree.SELL_AMOUNT] + "|" + tempFilters[i].getValues()[Tree.BUY_AMOUNT] + ")";

            if (tempFilters[i].getValues()[Tree.SELL_AMOUNT] == "0" && tempFilters[i].getValues()[Tree.BUY_AMOUNT] == "0" && this.getMarket().getMarketType() == "private")
                continue;

            //Draw each major filter.
            if (GUI.Button(new Rect(x, y, width, height), buttonVal, style))
            {
                //If this button was pressed, record the index (i) that it's currentlty at. This will be used for
                //drawing the subfilter buttons. Also, if it was already activated and we clicked it again, unactivate it.
                //All subfilters under this filter need to be reset.
                if (this.selectedFilter == i) {
                    this.selectedFilter = -1; 
                    this.selectedSubFilter = -1; 
                    this.selectedSubSubFilter = -1;
                    this.selFilter = "";
                    this.selSubFilter = "";
                    this.selSubSubFilter = "";
                }
                else { 
                    this.selectedFilter = i; 
                    this.selectedSubFilter = -1; 
                    this.selectedSubSubFilter = -1;
                    this.selFilter = filter;
                    this.selSubFilter = "";
                    this.selSubSubFilter = "";
                }
            }
            //Increment the height
            y += height;

            //If a subFilter is active, draw the subFilter that matches the button that was pressed.
            if (this.selectedFilter == i)
            {
                List<Tree.Node> tempSubFilters = this.getMarket().getMarketTree().getNode(selFilter).getChildren();

                //For each filter in this subsection
                for (int j = 0; j < tempSubFilters.Count; j++)
                {
                    string subFilter = tempSubFilters[j].getNodeName(); //Get the subFilter string
                    buttonVal = subFilter + " (" + tempSubFilters[j].getValues()[Tree.SELL_AMOUNT] + "|" + tempSubFilters[j].getValues()[Tree.BUY_AMOUNT] + ")";

                    if (tempSubFilters[j].getValues()[Tree.SELL_AMOUNT] == "0" && tempSubFilters[j].getValues()[Tree.BUY_AMOUNT] == "0" && this.getMarket().getMarketType() == "private")
                        continue;

                   //Draw the subFilter buttons
                    if (GUI.Button(new Rect(subX, y, subWidth, subHeight), buttonVal, style))
                    {
                        //If this button was pressed, record the index (i) that it's currentlty at. This will be used for
                        //drawing the subSubFilter buttons. Also, if it was already activated and we clicked it again, unactivate it.
                        if (this.selectedSubFilter == j) {
                            this.selectedSubFilter = -1; 
                            this.selectedSubSubFilter = -1;
                            this.selSubFilter = "";
                            this.selSubSubFilter = "";
                        }
                        else { 
                            this.selectedSubFilter = j; 
                            this.selectedSubSubFilter = -1;
                            this.selSubFilter = subFilter;
                            this.selSubSubFilter = "";
                        }
                    }
                    //Increment the height
                    y += subHeight;

                    //If a subSubFilter is active, draw the subFilter that matches the button that was pressed.
                    if (this.selectedSubFilter == j)
                    {
                        List<Tree.Node> tempSubSubFilters = this.getMarket().getMarketTree().getNode(selFilter + "," + selSubFilter).getChildren();

                        //for each sub sub filter in this subsection.
                        for (int k = 0; k < tempSubSubFilters.Count; k++)
                        {
                            string subSubFilter = tempSubSubFilters[k].getNodeName(); //Get the subFilter string using the selectedFilter and selectedSubFilter.
                            buttonVal = subSubFilter + " (" + tempSubSubFilters[k].getValues()[Tree.SELL_AMOUNT] + "|" + tempSubSubFilters[k].getValues()[Tree.BUY_AMOUNT] + ")";

                            if (tempSubSubFilters[k].getValues()[Tree.SELL_AMOUNT] == "0" && tempSubSubFilters[k].getValues()[Tree.BUY_AMOUNT] == "0" && this.getMarket().getMarketType() == "private")
                                continue;

                            //Draw a button
                            if (GUI.Button(new Rect(subSubX, y, subSubWidth, subSubHeight), buttonVal, style))
                            {
                                this.selectedSubSubFilter = k;
                                this.selSubSubFilter = subSubFilter;
                                string itemFilters = this.getMarket().getMarketTree().getNode(selFilter + "," + selSubFilter + "," + selSubSubFilter).getParentPathValues(0);
                                string marketVerNum =  this.getMarket().getMarketTree().getNode(itemFilters, Tree.RAW_FILTER).getValues()[Tree.VERSION];
                                this.market.getContents("sell", itemFilters, marketVerNum);
                                this.market.getContents("buy", itemFilters, marketVerNum);
                            }

                            y += subSubHeight;
                        }
                    }
                }
            }
        }
    }

    public bool validFilters()
    {
        return this.selectedFilter != -1 && this.selectedSubFilter != -1 && this.selectedSubSubFilter != -1;
    }

    private string getCurrentFilters()
    {
        return this.getMarket().getMarketTree().getNode(selFilter + "," + selSubFilter + "," + selSubSubFilter).getParentPathValues(0);
    }

    public string[] getCurrentFiltersArray()
    {
        if (selFilter == "" || selSubFilter == "" || selSubSubFilter == "") return new string[]{""};
        string filters = this.getMarket().getMarketTree().getNode(selFilter + "," + selSubFilter + "," + selSubSubFilter).getParentPathValues(0);
        return filters.Split(',');
    }

    public void setMarket(Market market)
    {
        this.market = market;
    }

    public Market getMarket()
    {
        return this.market;
    }

    public void setActive(bool val)
    {
        this.activeWindow = val;
    }

    public bool isWindowActive()
    {
        return this.activeWindow;
    }

    public void resetFilters()
    {

    }

    public void resetSubFilters()
    {

    }

    public void resetSubSubFilters()
    {

    }

    public struct OrderInfoStruct{
        public string itemName, price, quantity, duration, orderType;
        public int priceToMatch;
        public MarketOrder order;
        public Inventory.InventoryItem invItem;
        public Item item;
    }
}
