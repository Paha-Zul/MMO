using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using MarketFunctions;

public class ContextMenu{
    private string type;
    private Interface inter;
    private List<ContextEvent> contextEventList = new List<ContextEvent>();

    private Rect windowRect;
    private object objectToExecute;

    public ContextMenu(string type, Interface inter, Vector2 location, object o)
    {
        this.type = type;
        this.inter = inter;
        this.objectToExecute = o;

        if (type == "item_rightClick")
        {
            contextEventList.Add(new ContextEvent("sell", "Sell Item", "Sells the currently selected item", this));
        }
        else if (type == "order_rightClick")
        {
            contextEventList.Add(new ContextEvent("buy", "Buy Item", "Buys the currently selected market order", this));
        }

        windowRect = new Rect(location.x, location.y, 75, 20 + contextEventList.Count*20);
    }

    public void update()
    {
       if (Input.GetMouseButtonDown(1))
            this.inter.setContextMenu(null);
    }

    public void draw()
    {
        GUI.Window(10, windowRect, DoMyWindow, "sell");
    }

    private void DoMyWindow(int windowID)
    {
        drawEvents();
    }

    private void drawEvents(){
        float x=0,y=20, width=windowRect.width, height=20;

        foreach (ContextEvent context in contextEventList)
        {
            if (!context.satisfied()) continue;
            if (GUI.Button(new Rect(x, y, width, height), context.cName))
            {
                context.execute(this.objectToExecute);
            }
            y += height;
        }
    }

    public class ContextEvent
    {
        public string cType, cName, description;
       
        private ContextMenu parent;

        public ContextEvent(string type, string name, string description, ContextMenu parent)
        {
            this.cType = type;
            this.cName = name;
            this.description = description;
            this.parent = parent;
        }

        public void execute(object o)
        {
            if (cType == "sell")
            {
                Inventory.InventoryItem invItem = o as Inventory.InventoryItem;
                if (invItem == null) throw new Exception("invItem is not and InventoryItem.");

                int priceToMatch = this.parent.inter.marketInterface.getMarket().getHighestOrder(this.parent.inter.marketInterface.getCurrentFiltersArray(), invItem.getItem().getItemName()).getPrice();
                this.parent.inter.marketInterface.orderInfo.price = priceToMatch.ToString();

                this.parent.inter.marketInterface.orderInfo.orderType = "sell";
                this.parent.inter.marketInterface.orderInfo.invItem = invItem;

                this.parent.inter.setContextMenu(null);
            }
            else if (cType == "buy")
            {
                MarketOrder order = o as MarketOrder;
                if (order == null) throw new Exception("order is not a MarketOrder");

                int priceToMatch = this.parent.inter.marketInterface.getMarket().getHighestOrder(this.parent.inter.marketInterface.getCurrentFiltersArray()).getPrice();
                this.parent.inter.marketInterface.orderInfo.price = priceToMatch.ToString();

                this.parent.inter.marketInterface.orderInfo.orderType = "buy";

                Inventory.InventoryItem invItem = new Inventory.InventoryItem(order.getItem());
                invItem.addQuantity(999999999);

                if (invItem.getItem() == null) Debug.Log("Item is null");

                this.parent.inter.marketInterface.orderInfo.invItem = invItem;
                this.parent.inter.setContextMenu(null);
            }
        }

        public bool satisfied()
        {
            if (cType == "sell")
                return this.parent.inter.marketInterface.isWindowActive();
            return true;
        }
    }
}
