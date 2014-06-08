using UnityEngine;
using System.Collections;

//An Item is something that can be in an inventory (bank, shop, player, etc.)
public class Item{
	private string itemName, filters, tags;
	private Texture2D icon;

	public Item(string name, string filters, string tags){
		this.itemName = name;
        this.filters = filters;
		this.tags = tags;
		this.icon = Resources.Load<Texture2D>("Icons/Icon");
	}

    public Item(string name, string filters, string tags, Texture2D icon)
    {
        this.itemName = name;
        this.filters = filters;
        this.tags = tags;
        this.icon = icon;
    }

	public string getItemName(){
		return this.itemName;
	}

    public string getItemFilters()
    {
        return this.filters;
	}

	public string getTags(){
		return this.tags;
	}

    public Texture2D getIcon()
    {
        return this.icon;
    }

    /// <summary>
    /// Returns a compiled string of the necessary information. This currently includes name filters tags. Ex: "Warhammer(name)|weapons,2h,sword(filters)|Marketable(tags)" 
    /// </summary>
    /// <returns>A string compiled as itemName|filters|tags.</returns>
	public string getData(){
        return this.itemName + "|" + this.filters + "|" + this.tags;
	}

    /// <summary>
    /// Checks through the item tags for a match to 'tag'.
    /// </summary>
    /// <param name="tag">The tag to search for.</param>
    /// <returns>True if a tag is found, false otherwise.</returns>
	public bool hasTag(string tag){
		string[] split = this.tags.Split(' ');
		foreach(string word in split){
			if(word == tag)
				return true;
		}
		return false;
	}

    /// <summary>
    /// Checks through the types of the item to match 'type'.
    /// </summary>
    /// <param name="type">The string to check for.</param>
    /// <returns>True if the type is found, false otherwise.</returns>
    public bool hasType(string type)
    {
        string[] split = this.filters.Split(' ');
        foreach (string word in split)
        {
            if (word == type)
                return true;
        }
        return false;
    }

}
