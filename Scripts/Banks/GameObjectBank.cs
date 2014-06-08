using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using System.Collections;

public class GameObjectBank : MonoBehaviour
{
    //This will be for adding GameObjects through the inspector.
    public List<GameObject> gameObjectList = new List<GameObject>();

    private static Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>();

    void Start()
    {
        //Add add the inspector added objects to the has table.
        foreach (GameObject obj in gameObjectList)
            addObject(obj);

        this.enabled = false; //Disable the script.
    }

    public static void addObject(GameObject obj)
    {
        /*
        Debug.Log("Printing items in item bank ------------------------------- adding: " + item.getItemName());
        foreach (Item tmp in items.Values)
            Debug.Log(tmp.getItemName());
         */

        if (gameObjects.ContainsKey(obj.name)) return;

        gameObjects.Add(obj.name, obj);
    }

    public static void removeItem(string name)
    {
        gameObjects.Remove(name);
    }

    public static GameObject getObjectByName(string name)
    {
        GameObject tmp = null;
        gameObjects.TryGetValue(name, out tmp);
        return tmp;
    }

    public static GameObject getRandomItem()
    {
        List<GameObject> values = gameObjects.Values.ToList();
        return values[Random.Range(0, gameObjects.Count - 1)];
    }
}
