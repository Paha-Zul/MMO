using UnityEngine;
using System.Collections;
using System;

public class MarketOrder
{
    private long uniqueID;
	private Item item;
	private int price, quanitity, duration;
    private string type, ownerName;

    /// <summary>
    /// Constructs this market order with a specified uniqueID
    /// </summary>
    /// <param name="item"> The Item object this order will hold.</param>
    /// <param name="price"> The price of this order.</param>
    /// <param name="quantity"> The quantities of items in this order.</param>
    /// <param name="duration"> The duration in days that this order will exist for.</param>
    /// <param name="type"> The type of order. This will be either "buy" or "sell"</param>
    /// <param name="uniqueID"> The unique ID of the market order.</param>
	public MarketOrder(Item item, int price, int quantity, int duration, string type, string owner, long uniqueID){
		this.item = item;
		this.price = price;
		this.quanitity = quantity;
		this.duration = duration;
		this.type = type;
        this.ownerName = owner;

        this.uniqueID = uniqueID;
	}

    /// <summary>
    /// Constructs this market order without a specified uniqueID. This will cause this order to create a new ID.
    /// </summary>
    /// <param name="item"> The Item object this order will hold.</param>
    /// <param name="price"> The price of this order.</param>
    /// <param name="quantity"> The quantities of items in this order.</param>
    /// <param name="duration"> The duration in days that this order will exist for.</param>
    /// <param name="type"> The type of order. This will be either "buy" or "sell"</param>
    public MarketOrder(Item item, int price, int quantity, int duration, string type, string owner)
        : this(item, price, quantity, duration, type, owner, (long)UnityEngine.Random.Range(0, long.MaxValue))
    {

    }

	public Item getItem(){
		return this.item;
	}

	public int getPrice(){
		return this.price;
	}

	public int getQuantity(){
		return this.quanitity;
	}

    public void addQuantity(int amt)
    {
        this.quanitity += amt;
    }

	public int getDuration(){
		return this.duration;
	}

	public string getOrderType(){
		return this.type;
	}

    public long getUniqueID()
    {
        return this.uniqueID;
    }

    public string getOwnerName()
    {
        return this.ownerName;
    }

	public string getOrderData(){
		return this.item.getData()+":"+this.type+"|"+this.price+"|"+this.quanitity+"|"+this.duration+"|"+this.uniqueID+"|"+this.ownerName;
	}

}
