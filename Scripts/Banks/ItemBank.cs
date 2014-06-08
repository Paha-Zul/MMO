using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using System.Collections;

public class ItemBank{
	private static Dictionary<string, Item> items = new Dictionary<string, Item>();
	
	public static void addItem(Item item){
        
        /*
        Debug.Log("Printing items in item bank ------------------------------- adding: " + item.getItemName());
        foreach (Item tmp in items.Values)
            Debug.Log(tmp.getItemName());
         */

        if (items.ContainsKey(item.getItemName())) return;

		items.Add(item.getItemName(), item);
	}
	
	public static void removeItem(string name){
		items.Remove(name);
	}
	
	public static Item getItemByName(string name){
		Item tmp = null;
		items.TryGetValue(name, out tmp);
		return tmp;
	}

	public static Item getRandomItem(){
		List<Item> values = items.Values.ToList();
		return values[Random.Range(0,values.Count-1)];
	}

    public static string[] getItemNamesWithFilters(string filters)
    {
        List<string> itemNames = new List<string>();
        foreach (Item item in items.Values)
        {
            if (item.getItemFilters() == filters)
                itemNames.Add(item.getItemName());
        }

        string[] namesArray = new string[itemNames.Count];
        for (int i = 0; i < itemNames.Count; i++)
            namesArray[i] = itemNames[i];

        return namesArray;
    }
}
