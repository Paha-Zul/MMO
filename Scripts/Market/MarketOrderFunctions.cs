using UnityEngine;
using System.Collections;

namespace MarketOrderFunctions
{
    public static class MarketOrderFunctions
    {
        /// <summary>
        /// Fulfills the order with quantity amount given to player with playerName. This will add the items to the town market vault.
        /// </summary>
        /// <param name="order">The order that is being fulfilled.</param>
        /// <param name="quantity">The quantity of the item.</param>
        /// <param name="playerName">The name of the player who's order is being fulfilled.</param>
        /// <param name="town">The Town which holds the market vault.</param>
        public static void fulfillOrder(this MarketOrder order, int quantity, string playerName, Town town){
            MarketVault vault = town.getMarketVault();
            vault.addItemToVault(playerName, order.getItem(), quantity);
        }
    }
}
