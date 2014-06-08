using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Inventory{
    List<InventoryItem> inventory;
    public bool expandable = false, updateOnChange = false;

    /// <summary>
    /// Sets the initial size of the inventory as well as the expandable boolean. If this inventory is expandable, the addItem method will be allowed
    /// to push onto the list if there is no more space.
    /// </summary>
    /// <param name="size">The initial size of the list.</param>
    /// <param name="expandable">If this inventory is expandable or not.</param>
    /// <param name="updateOnChange">A bool indicating if the server inventory should update to the client on change.</param>
    public Inventory(int size, bool expandable, bool updateOnChange)
    {
        this.inventory = new List<InventoryItem>(size);
        for (int i = 0; i < size; i++) this.inventory.Add(null);

        this.expandable = expandable;
        this.updateOnChange = updateOnChange;
    }

    /// <summary>
    /// Sets the initial size of the inventory as well as the expandable boolean. If this inventory is expandable, the addItem method will be allowed
    /// to push onto the list if there is no more space.
    /// </summary>
    /// <param name="size">The initial size of the list.</param>
    /// <param name="expandable">If this inventory is expandable or not.</param>
    public Inventory(int size, bool updateOnChange) : this(size, false, updateOnChange) { }

    /// <summary>
    /// Initializes the inventory with the 'size' parameter. Sets the inventory to not expandable.
    /// </summary>
    /// <param name="size"></param>
    public Inventory(int size) : this(size, false, false) { }

    /// <summary>
    /// Adds an item with the desired amount. This function will first check for any existing items, then any open spaces, then will psuh onto
    /// the list if this is an expandable/infinite inventory.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="amount"></param>
    public bool addItem(Item item, int amount)
    {
        foreach (InventoryItem invItem in this.inventory)
        {
            if (invItem == null)
                continue;

            if (invItem.getItem().getItemName() == item.getItemName())
            {
                invItem.addQuantity(amount);
                return true;
            }
        }

        //This only executes if an already existing match was not found.
        for (int i = 0; i < this.inventory.Count; i++)
            if (this.inventory[i] == null)
            {
                this.inventory[i] = new InventoryItem(item);
                this.inventory[i].addQuantity(amount);
                return true;
            }

        //If the item couldn't be added and this inventory is expandable, push onto the list.
        if (this.expandable)
        {
            this.inventory.Add(new InventoryItem(item, amount));
            return true;
        }

        return false;
    }

    /// <summary>
    /// Adds an item to the inventory with a quantity of 1.
    /// </summary>
    /// <param name="item">The Item to add.</param>
    public bool addItem(Item item)
    {
        return this.addItem(item, 1);
    }

    /// <summary>
    /// This functions similarly to addItem, except that instead of checking for empty spots or already existing items, it will clear the inventory (nothing in it)
    /// and will add items in the order they are received.
    /// </summary>
    /// <returns>True if the items could be added, false otherwise.</returns>
    public bool setItems(InventoryItem[] items)
    {
        this.inventory.Clear();
        try
        {
            foreach (InventoryItem itemToAdd in items)
            {
                this.inventory.Add(itemToAdd);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

       return true;
    }

    /// <summary>
    /// Overwrites the inventory with the passed in list of items.
    /// </summary>
    /// <param name="items">A List of items to set to the inventory.</param>
    /// <returns>True if the action was performed, false otherwise.</returns>
    public bool setItems(List<InventoryItem> items)
    {
        this.inventory.Clear();
        this.inventory = items;

        return true;
    }

    public Item removeItem(string itemName, int quantity)
    {
        Item itemRemoved = null;
        for (int i =0; i < this.inventory.Count; i++)
        {
            if (this.inventory[i] == null) continue; //Skip over null/empty spots.

            if (this.inventory[i].getItem().getItemName() == itemName) //If the names match
            {
                this.inventory[i].addQuantity(-quantity); //Add a negative quantity.
                itemRemoved = this.inventory[i].getItem();
                if (this.inventory[i].getQuantity() <= 0) //If the quantity is 0 or negative (somehow)
                { 
                    this.inventory[i] = null; //Set it to null.
                    break;
                }
            }
        }

        return itemRemoved;
    }

    public List<InventoryItem> getInventoryList()
    {
        return this.inventory;
    }

    public int getNumItems()
    {
        return this.inventory.Count;
    }

    public Item getItem(int index)
    {
        if (this.inventory[index] == null) return null;
        return this.inventory[index].getItem();
    }

    public InventoryItem getInventoryItem(int index)
    {
        return this.inventory[index];
    }

    public class InventoryItem
    {
        private Item item=null;
        private int quantity=0;

        public InventoryItem(Item item, int quantity)
        {
            this.item = item;
            this.quantity = quantity;
        }

        public InventoryItem(Item item) : this(item, 0){}

        public void addQuantity(int amount)
        {
            this.quantity += amount;
        }

        public int getQuantity()
        {
            return this.quantity;
        }

        public Item getItem()
        {
            return this.item;
        }
    }
}
