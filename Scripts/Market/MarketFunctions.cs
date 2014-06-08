using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MarketFunctions
{
    public static class MarketFunctions
    {
        /// <summary>
        /// Searches for a market order in a list that matches the 'price' and 'itemName' parameter passed in.
        /// </summary>
        /// <param name="filters">The current filters of what is being looked at.</param>
        /// <param name="price">The price of the order being placed/looked for.</param>
        /// <param name="orderType">The type of order to look for. "buy" will look in buyOrders and "sell" will look in sellOrders</param>
        /// <param name="itemName">The name of the item to match.</param>
        /// <returns>The MarketOrder that matches the wanted price. If no MarketOrder is found, null is returned.</returns>
        public static MarketOrder getOrderAtPrice(this Market market, string[] filters, int price, string orderType, string itemName)
        {
            MarketOrder orderAtPrice = null;
            List<MarketOrder> list;

            //This needs to reverse the lists. Ex: type "sell" needs to assign the buyOrders list to the tmp list.
            if (orderType == "buy") list = market.getListIfExists(filters, market.getSellOrders());
            else list = market.getListIfExists(filters, market.getBuyOrders());

            foreach (MarketOrder order in list)
            {
                if (price == order.getPrice() && order.getItem().getItemName() == itemName)
                {
                    orderAtPrice = order;
                    break;
                }
            }
            return orderAtPrice;
        }

        /// <summary>
        /// Gets the highest priced MarketOrder in the buy orders.
        /// </summary>
        /// <param name="filters">The current filters of what is being looked at.</param>
        /// <param name="name">The name of the Item to match.</param>
        /// <returns>The highest priced MarketOrder in the buy orders. If no MarketOrders exist or the filters didn't exist, null will be returned.</returns>
        public static MarketOrder getHighestOrder(this Market market, string[] filters, string itemName)
        {
            MarketOrder highestOrder = null;
            float currPrice = -1;

            //If the filter exists, scan the list for the lowest price.
            foreach (MarketOrder order in market.getListIfExists(filters, market.getBuyOrders()))
            {
                if (itemName != "")
                {
                    if ((currPrice == -1 || currPrice < order.getPrice()) && order.getItem().getItemName() == itemName)
                    {
                        highestOrder = order;
                        currPrice = order.getPrice();
                    }
                }
                else
                    if (currPrice == -1 || currPrice < order.getPrice())
                    {
                        highestOrder = order;
                        currPrice = order.getPrice();
                    }

            }
            return highestOrder;
        }

        public static MarketOrder getHighestOrder(this Market market, string[] filters)
        {
            return getHighestOrder(market, filters, "");
        }

        /// <summary>
        /// Gets the lowest priced MarketOrder in the sell orders.
        /// </summary>
        /// <param name="filters">The current filters of what is being looked at.</param>
        /// <param name="name">The name of the Item to match.</param>
        /// <returns>The lowest priced MarketOrder in the sell orders. If no MarketOrders exist or the filters didn't exist, null will be returned.</returns>
        public static MarketOrder getLowestOrder(this Market market, string[] filters, string itemName)
        {
            MarketOrder lowestOrder = null;
            int currPrice = -1;

            //If the filter exists, scan the list for the lowest price.
            if (market.filterExists(filters, market.getSellOrders()))
            {
                foreach (MarketOrder order in market.getListIfExists(filters, market.getSellOrders()))
                {
                    //If the item name is not empty.
                    if (itemName != "")
                    {
                        //If the currPrice has not been initialized yet, or the curr price is less than the order AND the item names match.
                        if ((currPrice == -1 || currPrice > order.getPrice()) && order.getItem().getItemName() == itemName)
                        {
                            lowestOrder = order; //Set the lowest priced order.
                            currPrice = order.getPrice(); //Set the currPrice
                        }
                    }
                    //If the item name is empty, get the lowest price from any named item in the list.
                    else
                        if (currPrice == -1 || currPrice > order.getPrice())
                        {
                            lowestOrder = order;
                            currPrice = order.getPrice();
                        }
                }
            }
            return lowestOrder;
        }

        public static MarketOrder getLowestOrder(this Market market, string[] filters)
        {
            return market.getLowestOrder(filters, "");
        }

        /// <summary>
        /// Checks each level of the order structure to check if each level exists.
        /// </summary>
        /// <param name="market">The Market object to check.</param>
        /// <param name="filters">The filters as an array of strings.</param>
        /// <param name="list">The order structure.</param>
        /// <returns>True if the filter exists in the order structure, false otherwise.</returns>
        public static bool filterExists(this Market market, string[] filters, Dictionary<string, Dictionary<string, Dictionary<string, List<MarketOrder>>>> list)
        {
            if (list == null) { Debug.Log("List is null"); return false; }
            if (!list.ContainsKey(filters[0])) { /*Debug.Log(filters[0]+" can't be found");*/ return false; }
            if (!list[filters[0]].ContainsKey(filters[1])) { /*Debug.Log(filters[1]+" can't be found");*/ return false; }
            if (!list[filters[0]][filters[1]].ContainsKey(filters[2])) { /*Debug.Log(filters[2] + " can't be found");*/ return false; }
            return true;
        }

        /// <summary>
        /// Takes in a 3D Dictionary that holds a list of MarketOrders and returns the list if the filters exist in the dictionary structure.
        /// </summary>
        /// <param name="filters">The filters of the item type being searched for.</param>
        /// <param name="structure">The Dictionary structure. This is a list held by a dictionary held by a dictionary held by a dictionary.</param>
        /// <returns>The List that holds MarketOrders if the filters exist. Otherwise, an empty list.</returns>
        public static List<MarketOrder> getListIfExists(this Market market, string[] filters, Dictionary<string, Dictionary<string, Dictionary<string, List<MarketOrder>>>> structure)
        {
            List<MarketOrder> list = new List<MarketOrder>(); //Create an empty list.

            //These are separate because it's good for debugging.
            if (structure == null) { Debug.Log("List is null"); return list; }
            if (!structure.ContainsKey(filters[0])) { Debug.Log(filters[0] + " can't be found"); return list; }
            if (!structure[filters[0]].ContainsKey(filters[1])) { Debug.Log(filters[1] + " can't be found"); return list; }
            if (!structure[filters[0]][filters[1]].ContainsKey(filters[2])) { Debug.Log(filters[2] + " can't be found"); return list; }

            list = structure[filters[0]][filters[1]][filters[2]];
            return list;
        }

        /// <summary>
        /// Takes in a 3D Dictionary that holds a list of MarketOrders and returns the list if the filters exist in the dictionary structure.
        /// </summary>
        /// <param name="filters">The filters of the item type being searched for.</param>
        /// <param name="structure">The Dictionary structure. This is a list held by a dictionary held by a dictionary held by a dictionary.</param>
        /// <returns>The List that holds MarketOrders if the filters exist. Otherwise, an empty list.</returns>
        public static List<MarketOrder> getListIfExists(this Market market, string[] filters, string type)
        {
            List<MarketOrder> list = new List<MarketOrder>(); //Create an empty list.
            Dictionary<string, Dictionary<string, Dictionary<string, List<MarketOrder>>>> structure;

            if (type == "sell") structure = market.getSellOrders();
            else structure = market.getBuyOrders();

            //These are separate because it's good for debugging.
            if (structure == null) {return list; }
            if (!structure.ContainsKey(filters[0])) {  return list; }
            if (!structure[filters[0]].ContainsKey(filters[1])) {return list; }
            if (!structure[filters[0]][filters[1]].ContainsKey(filters[2])) {return list; }

            list = structure[filters[0]][filters[1]][filters[2]];
            return list;
        }
    }
}
