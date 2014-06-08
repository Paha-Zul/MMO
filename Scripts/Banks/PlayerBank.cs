using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using System.Collections;

public class PlayerBank
{
    private static Dictionary<string, Player> players = new Dictionary<string, Player>();

    public static void addPlayer(Player player)
    {
        if (players.ContainsKey(player.getPlayerInfo().playerName)) return;

        players.Add(player.getPlayerInfo().playerName, player);
    }

    public static void removePlayer(string name)
    {
        players.Remove(name);
    }

    public static Player getPlayerByName(string name)
    {
        Player p = null;
        players.TryGetValue(name, out p);
        return p;
    }

    public static Player getRandomPlayer()
    {
        List<Player> values = players.Values.ToList();
        return values[Random.Range(0, values.Count - 1)];
    }
}
