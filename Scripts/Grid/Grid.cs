using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour {
    public int _worldStartX = -50000, _worldStartY = -50000;
    public int _worldWidth = 100000, _worldHeight = 100000;
    public int _squareWidth = 1000, _squareHeight = 1000;
    public int numX, numZ;

    private static int worldStartX, worldStartY;
    private static int worldWidth, worldHeight;
    private static int squareWidth, squareHeight;

    private static GridSquare[,] grid;


    void Awake()
    {
        //Assign the public to the static values
        worldStartX = _worldStartX;
        worldStartY = _worldStartY;
        worldWidth = _worldWidth;
        worldHeight = _worldHeight;
        squareWidth = _squareWidth;
        squareHeight = _squareHeight;

        this.numX = (worldWidth / squareWidth);
        this.numZ = (worldHeight / squareHeight);

        grid = new GridSquare[(worldWidth / squareWidth), (worldHeight / squareHeight)]; //Initializes the 2D array.
        //Fills the 2D array with the GridSquare objects.
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int z = 0; z < grid.GetLength(1); z++)
            {
                //Assigns each GridSquare the correct X and Y coordinate along with the width and height.
                grid[x, z] = new GridSquare(worldStartX + x, worldStartY + z, squareWidth, squareHeight, x, z);
            }
        }

        this.enabled = false; //Disable the script.
    }

    /// <summary>
    /// Adds a GameObject to the GridSquare using it's world coordinates. Calculates the index using the X and Z coordinates of the GameObject.
    /// </summary>
    /// <param name="obj">The GameObject to add to the Grid.</param>
    /// <returns>The GridSquare that the GameObject was added to.</returns>
    public static GridSquare addObjectToGrid(GameObject obj, bool isPlayer)
    {
        Vector2 index = computeIndex(obj);
        return grid[(int)index.x, (int)index.y].addGameObjectToList(obj, isPlayer);
    }

    /// <summary>
    /// Removes a GameObject from the Grid. Calculates the index using the X and Z coordinates of the GameObject.
    /// </summary>
    /// <param name="obj"> The GameObject to remove from the Grid.</param>
    /// <returns>True if the object was found and removed, false otherwise.</returns>
    public static bool removeObjectFromGrid(GameObject obj, bool isPlayer)
    {
        Vector2 index = computeIndex(obj);
        return grid[(int)index.x, (int)index.y].removeGameObjectFromList(obj, isPlayer);
    }

    public static GridSquare checkGridSquare(GameObject obj, GridSquare currSquare, bool isPlayer)
    {
        //If we are not still within the bounds, get a new square.
        if (!currSquare.checkWithinBounds(obj))
        {
            //If the object could not be removed, throw an exception.
            if (!currSquare.removeGameObjectFromList(obj, isPlayer)) throw new Exception("GridSquare did not contain the GameObject");
            Vector2 index = computeIndex(obj); //Get the current index for the GameObject.
            currSquare = grid[(int)index.x, (int)index.y].addGameObjectToList(obj, isPlayer); //Add it to GridSquare and get the square it was added to.
        }

        return currSquare; //Return either the old GridSquare if it's still in the right spot, or a new GridSquare.
    }

    public static GridSquare checkGridSquare(GameObject obj, GridSquare currSquare, bool isPlayer, List<GameObject> createList,
        List<GameObject> destroyList, List<GameObject> playerCreateList, List<GameObject> playerDestroyList, int radius)
    {
        GridSquare tempSquare = currSquare;
        //If we are not still within the bounds, get a new square.
        if (!currSquare.checkWithinBounds(obj))
        {
            Player playerScript = obj.GetComponent<Player>();

            /*
            //Destroy all GameObjects from this GridSquare.
            if (playerScript != null)
            {
                createList.Clear(); //Clear the List.
                List<GameObject> objList = Grid.getAllObjectsInRadius(currSquare, 0); //Get the GameObjects
                foreach(GameObject nearbyObj in objList) //Assign them to the createList.
                    createList.Add(nearbyObj);
            }
             */

            //Change the GridSquare
            if (!currSquare.removeGameObjectFromList(obj, isPlayer)) throw new Exception("GridSquare did not contain the GameObject");//If the object could not be removed, throw an exception.
            Vector2 index = computeIndex(obj); //Get the current index for the GameObject.
            tempSquare = grid[(int)index.x, (int)index.y].addGameObjectToList(obj, isPlayer); //Add it to GridSquare and get the square it was added to.

            //Create all GameObjects from this GridSquare.
            if (playerScript != null)
            {
                //Clears all lists.
                createList.Clear();
                destroyList.Clear();
                playerCreateList.Clear();
                playerDestroyList.Clear();
                //Gets the objects needed for creation and deletion.
                getAllObjectsInDifference(currSquare, tempSquare, createList, destroyList, playerCreateList, playerDestroyList, radius);
                /*
                destroyList.Clear(); //Clear the List.
                List<GameObject> objList = Grid.getAllObjectsInRadius(currSquare, 0); //Get the GameObjects
                foreach (GameObject nearbyObj in objList) //Assign them to the createList.
                    destroyList.Add(nearbyObj);
                 */
            }
        }
        return tempSquare;
    }

    private static void getAllObjectsInDifference(GridSquare firstSquare, GridSquare secondSquare, List<GameObject> createList, 
        List<GameObject> destroyList, List<GameObject> playerCreateList, List<GameObject> playerDestroyList, int radius)
    {
        int xDiff = (int)firstSquare.index.x - (int)secondSquare.index.x;
        int zDiff = (int)firstSquare.index.y - (int)secondSquare.index.y;
        
        /*
        //Variables for the first square.
        //Get the startX, startZ, endX, endZ
        int startX = (int)secondSquare.index.x - radius, endX = (int)secondSquare.index.x + radius;
        int startZ = (int)secondSquare.index.y - radius, endZ = (int)secondSquare.index.y + radius;

        //Check bounds.
        if (startX < 0) startX = 0;
        if (endX > grid.GetLength(0) - 1) endX = grid.GetLength(0) - 1;
        if (startZ < 0) startZ = 0;
        if (endZ > grid.GetLength(1) - 1) endZ = grid.GetLength(1) - 1;
         */

        //Variables for the first square.
        //Get the startX, startZ, endX, endZ
        int startX = (int)secondSquare.index.x - radius, endX = (int)secondSquare.index.x + radius;
        int startZ = (int)secondSquare.index.y - radius, endZ = (int)secondSquare.index.y + radius;

        //Check bounds.
        if (startX < 0) startX = 0;
        if (endX > grid.GetLength(0) - 1) endX = grid.GetLength(0) - 1;
        if (startZ < 0) startZ = 0;
        if (endZ > grid.GetLength(1) - 1) endZ = grid.GetLength(1) - 1;

        //Variables for the second square.
        //Get the startX, startZ, endX, endZ
        int startX2 = (int)firstSquare.index.x - radius, endX2 = (int)firstSquare.index.x + radius;
        int startZ2 = (int)firstSquare.index.y - radius, endZ2 = (int)firstSquare.index.y + radius;

        //Check bounds.
        if (startX2 < 0) startX2 = 0;
        if (endX2 > grid.GetLength(0) - 1) endX2 = grid.GetLength(0) - 1;
        if (startZ2 < 0) startZ2 = 0;
        if (endZ2 > grid.GetLength(1) - 1) endZ2 = grid.GetLength(1) - 1;

        List<GridSquare> createSquares = new List<GridSquare>(5);
        List<GridSquare> destroySquares = new List<GridSquare>(5);

        for (int x = startX; x <= endX; x++)
        {
            for (int z = startZ; z <= endZ; z++)
            {
                //If the X and Z coords are inside the bounds of the second square, do nothing (they are overlapping).
                if (x >= startX2 && x <= endX2 && z >= startZ2 && z <= endZ2)
                {
                    continue;
                }
                else //If the X and Z coords are outside the bounds of the second square, add it to the createSquares list. We will be creating objects in this square.
                {
                    createSquares.Add(grid[x,z]);
                }
            }
        }

        for (int x = startX2; x <= endX2; x++)
        {
            for (int z = startZ2; z <= endZ2; z++)
            {
                //If the X and Z coords are inside the bounds of the first square, do nothing (they are overlapping).
                if (x >= startX && x <= endX && z >= startZ && z <= endZ)
                {
                    continue;
                }
                else //If the X and Z coords are outside the bounds of the first square, add it to the createSquares list. We will be creating objects in this square.
                {
                    destroySquares.Add(grid[x,z]);
                }
            }
        }

        foreach (GridSquare square in createSquares)
        {
            foreach (GameObject obj in square.getObjectList(false))
                createList.Add(obj);
            foreach (GameObject obj in square.getObjectList(true))
            {
                createList.Add(obj);
                playerCreateList.Add(obj);
            }
        }
        foreach (GridSquare square in destroySquares)
        {
            foreach (GameObject obj in square.getObjectList(false))
                destroyList.Add(obj);
            foreach (GameObject obj in square.getObjectList(true))
            {
                destroyList.Add(obj);
                playerDestroyList.Add(obj);
            }
        }

        /*
        //To get what we must create, we can simply find the right direction the square has moved and get the correct index. For example, if the
        //square moved to the right, we only need the right-most squares along the Z axis.

        //To get what me must destroy, we do the reverse direction of above except we extend one additional level out. This will be the area that was lost
        //by moving in the direction that was chosen. If we can't extend out that far for some reason, don't destroy anything.

        //First we have to figure out which of the 8 directions we moved.
        if ((xDiff == 0 || zDiff == 0) && !(xDiff == 0 && zDiff == 0)) //If either the X or Z difference is 0, it's a left/right/up/down shift. But not both 0.
        {
            int loopStart, loopEnd, constant, deleteConstant;
            if (xDiff == 0) //xDiff is 0, the change is in zDiff
            {
                if (zDiff < 0) //If zDiff is negative, that means that the first square is above the second square. (up)
                {
                    loopStart = startX; //The start of the loop
                    loopEnd = endX; //The end of the loop.
                    constant = endZ; //The constant axis variable. (for creating)
                    deleteConstant = startZ - 1; //The opposite constant variable. (for destroying)
                    Debug.Log("Normal case: up");
                }
                else //If zDiff is positive, that means the first square is below the sezond square. (down)
                {
                    loopStart = startX; //The start of the loop
                    loopEnd = endX; //The end of the loop.
                    constant = startZ; //The constant axis variable. (for creating)
                    deleteConstant = endZ + 1; //The opposite constant variable. (for destroying)
                    Debug.Log("Normal case: down");
                }

                //Adds all objects in the GridSquares from startX to endX along the startZ only. (Z is 0 - nonchanging).
                for (int i = loopStart; i <= loopEnd; i++)
                {
                    //Adds all the GameObjects to the createList
                    foreach (GameObject obj in grid[i, constant].getObjectList(false))
                        createList.Add(obj);
                    foreach (GameObject obj in grid[i, constant].getObjectList(true))
                    {
                        createList.Add(obj);
                        playerCreateList.Add(obj);
                    }

                    //Adds all the GameObjects to the destroyList
                    foreach (GameObject obj in grid[i, deleteConstant].getObjectList(false))
                        destroyList.Add(obj);
                    foreach (GameObject obj in grid[i, deleteConstant].getObjectList(true))
                    {
                        destroyList.Add(obj);
                        playerDestroyList.Add(obj);
                    }
                }
            }
            else //zDiff is 0 and xDiff is -1 or 1. We need to move along the Z axis with the X being constants (left or right)
            {
                if (xDiff < 0) //If xDiff is negative, the first square is to the left. (left)
                {
                    loopStart = startZ; //The start of the loop
                    loopEnd = endZ; //The end of the loop.
                    constant = endX; //The constant axis variable. (for creating)
                    deleteConstant = startX - 1; //The opposite constant variable. (for destroying)
                    Debug.Log("Normal case: left");
                }
                else //If xDiff is positive, the first square is to the right. (right).
                {
                    loopStart = startZ; //The start of the loop
                    loopEnd = endZ; //The end of the loop.
                    constant = startX; //The constant axis variable. (for creating)
                    deleteConstant = endX + 1; //The opposite constant variable. (for destroying)
                    Debug.Log("Normal case: right");
                }

                //Adds all objects in the GridSquares from startX to endX along the startZ only. (Z is 0 - nonchanging).
                for (int i = loopStart; i <= loopEnd; i++)
                {
                    //Adds the GameObjects to the create lists.
                    foreach (GameObject obj in grid[constant, i].getObjectList(false))
                        createList.Add(obj);
                    foreach (GameObject obj in grid[constant, i].getObjectList(true))
                    {
                        createList.Add(obj);
                        playerCreateList.Add(obj);
                    }

                    //Adds the GameObjects to the destroy lists.
                    foreach (GameObject obj in grid[deleteConstant, i].getObjectList(false))
                        destroyList.Add(obj);
                    foreach (GameObject obj in grid[deleteConstant, i].getObjectList(true))
                    {
                        destroyList.Add(obj);
                        playerDestroyList.Add(obj);
                    }
                }
            }
        }
        //This is a corner case.
        else if(Math.Abs(xDiff) > 0 && Math.Abs(zDiff) > 0) //If xDiff and zDiff greater than 0, it's a corner case (topleft/topright/bottomleft/bottomright).
        {
            Debug.Log("Diagonal case");
            int xConst, zConst;
            int xDelConst, zDelConst;
            int bonusX, bonusZ;

            if (xDiff > 0) { zConst = startZ; zDelConst = endZ + 1;  bonusZ = 1;}
            else { zConst = endZ; zDelConst = startZ - 1;  bonusZ = -1;}
            if (zDiff > 0) { xConst = startX; xDelConst = endX + 1;  bonusX = 1;}
            else { xConst = endX; xDelConst = startX - 1;  bonusX = -1; }

            //Creates bounds from where we stepped from (for destroying entities).
            int startX2 = startX + bonusX, endX2 = endX + bonusX;
            int startZ2 = startZ + bonusZ, endZ2 = endZ + bonusZ;

            //Loop through the radius.
            for (int x = startX-1; x <= endX+1; x++) 
            {
                for (int z = startZ-1; z <= endZ+1; z++)
                {
                    //If either of the X or Z vals equal the constants, and it's within the normal bounds, the GameObjects should be added.
                    if ((x == xConst || z == zConst) && (x >= startX && x<= endX && z >= startZ && z<= endZ))
                    {
                        Debug.Log("adding to create: " + x + " " + z);
                        //Adds the GameObjects to the create lists.
                        foreach (GameObject obj in grid[x, z].getObjectList(false))
                            createList.Add(obj);
                        foreach (GameObject obj in grid[x, z].getObjectList(true))
                        {
                            createList.Add(obj);
                            playerCreateList.Add(obj);
                        }
                    //This is for destroy objects. If either X or Z equal the constants, and it is OUTSIDE the normal bounds (destroy from squares that
                    //have been left), add it to the destroy lists.
                    }

                    if ((x == xDelConst || z == zDelConst) && (x >= startX2 && x <= endX2 && z >= startZ2 && z <= endZ2))
                    {
                        Debug.Log("Adding these objects to destroy lists x: " + x + " z: " + z + " bonusX: " +bonusX + " bonusz: "+bonusZ + "xDelConst: " + xDelConst + " zDelConst: " + zDelConst);
                        //adds the gameobjects to the create lists.
                        foreach (GameObject obj in grid[x, z].getObjectList(false))
                            destroyList.Add(obj);
                        foreach (GameObject obj in grid[x, z].getObjectList(true))
                        {
                            destroyList.Add(obj);
                            playerDestroyList.Add(obj);
                        }
                    }
                }
            }
        }
         * 
         */
    }

    /// <summary>
    /// Gets ALL of the GameObjects in a radius.
    /// </summary>
    /// <param name="currSquare">The current GridSquare to search from.</param>
    /// <param name="radius">The radius of the search.</param>
    /// <returns>A List holding the GameObjects.</returns>
    public static List<GameObject> getAllObjectsInRadius(GridSquare currSquare, int radius)
    {
        return getAllObjectsInRadius((int)currSquare.index.x, (int)currSquare.index.y, radius);
    }

    /// <summary>
    /// Gets ALL of the GameObjects in a radius.
    /// </summary>
    /// <param name="obj">The GameObject to search around. The Object's position will be used to calculated the base index.</param>
    /// <param name="radius">The radius of the search.</param>
    /// <returns>A List holding the GameObjects.</returns>
    public static List<GameObject> getAllObjectsInRadius(GameObject obj, int radius)
    {
        Vector2 index = computeIndex(obj);
        return getAllObjectsInRadius((int)index.x, (int)index.y, radius);
    }

    /// <summary>
    /// Gets ALL of the GameObjects in a radius.
    /// </summary>
    /// <param name="xIndex">The xIndex to search from.</param>
    /// <param name="zIndex">The yIndex to search from.</param>
    /// <param name="radius">The radius of the search.</param>
    /// <returns>A List holding the GameObjects.</returns>
    public static List<GameObject> getAllObjectsInRadius(int xIndex, int zIndex, int radius)
    {
        List<GameObject> objList = new List<GameObject>();

        //Get the startX, startZ, endX, endZ
        int startX = xIndex - radius, endX = xIndex + radius;
        int startZ = zIndex - radius, endZ = zIndex + radius;

        //Check bounds.
        if (startX < 0) startX = 0;
        if (endX > grid.GetLength(0)-1) endX = grid.GetLength(0)-1;
        if (startZ < 0) startZ = 0;
        if (endZ > grid.GetLength(1)-1) endZ = grid.GetLength(1)-1;

        //Get the list...
        for (int x = startX; x <= endX; x++)
        {
            for (int z = startZ; z <= endZ; z++)
            {
                //Add every entity from the grid's player list to the objList.
                foreach (GameObject obj in grid[x, z].getObjectList(true))
                    objList.Add(obj);
                foreach (GameObject obj in grid[x, z].getObjectList(false))
                    objList.Add(obj);
            }
        }

        return objList;
    }

    /// <summary>
    /// Gets either regular or player GameObjects in an area.
    /// </summary>
    /// <param name="currSquare">The GridSquare to search from.</param>
    /// <param name="radius">The distance to search.</param>
    /// <param name="player">If it is to return players or regular GameObject.</param>
    /// <returns>A List of either regular or player GameObjects</returns>
    public static List<GameObject> getObjectsInRadius(GridSquare currSquare, int radius, bool player)
    {
        return getObjectsInRadius((int)currSquare.index.x, (int)currSquare.index.y, radius, player);
    }

    /// <summary>
    /// Gets either regular or player GameObjects in an area.
    /// </summary>
    /// <param name="obj">The GameObject to search from.</param>
    /// <param name="radius">The distance to search.</param>
    /// <param name="player">If it is to return players or regular GameObject.</param>
    /// <returns>A List of either regular or player GameObjects</returns>
    public static List<GameObject> getObjectsInRadius(GameObject obj, int radius, bool player)
    {
        Vector2 index = computeIndex(obj);
        return getObjectsInRadius((int)index.x, (int)index.y, radius, player);
    }

    /// <summary>
    /// Gets either regular or player GameObjects in an area.
    /// </summary>
    /// <param name="xIndex">The xIndex to search from.</param>
    /// <param name="zIndex">The zIndex to search from.</param>
    /// <param name="radius">The distance to search.</param>
    /// <param name="player">If it is to return players or regular GameObject.</param>
    /// <returns>A List of either regular or player GameObjects</returns>
    public static List<GameObject> getObjectsInRadius(int xIndex, int zIndex, int radius, bool player)
    {
        List<GameObject> objList = new List<GameObject>();

        //Get the startX, startZ, endX, endZ
        int startX = xIndex - radius, endX = xIndex + radius;
        int startZ = zIndex - radius, endZ = zIndex + radius;

        //Check bounds.
        if (startX < 0) startX = 0;
        if (endX > grid.GetLength(0) - 1) endX = grid.GetLength(0) - 1;
        if (startZ < 0) startZ = 0;
        if (endZ > grid.GetLength(1) - 1) endZ = grid.GetLength(1) - 1;

        //Get the list...
        for (int x = startX; x <= endX; x++)
        {
            for (int z = startZ; z <= endZ; z++)
            {
                //Add every entity from the grid's player list to the objList.
                foreach (GameObject obj in grid[x, z].getObjectList(player))
                    objList.Add(obj);
            }
        }
        return objList;
    }

    /// <summary>
    /// Computes the index for the Grid using the GameObejct's X and Z coordinates.
    /// </summary>
    /// <param name="obj">The GameObject to use for computing the index.</param>
    /// <returns>A Vector2 holding the X and Z index.</returns>
    private static Vector2 computeIndex(GameObject obj)
    {
        int x = (int)obj.transform.position.x, z = (int)obj.transform.position.z; //Local copy of X and Z coordinates.
        int xIndex = (x + -worldStartX) / squareWidth, yIndex = (z + -worldStartY) / squareHeight; //Calculates the index.
        return new Vector2(xIndex, yIndex); //Returns a Vector2.
    }

    public class GridSquare
    {
        List<GameObject> gameObjectList = new List<GameObject>();
        List<GameObject> playerGameObjectList = new List<GameObject>();
        public Vector2 position; //Real World X and Y coordinate.
        public Vector2 dimensions; //Width and Height of the square.
        public Vector2 index; //The index of the array this square resides.

        public GridSquare(int x, int y, int width, int height, int indexX, int indexY)
        {
            position = new Vector2(x, y);
            dimensions = new Vector2(width, height);
            index = new Vector2(indexX, indexY);
        }

        /// <summary>
        /// Adds a GameObject to this GridSquare.
        /// </summary>
        /// <param name="obj">The GameObject to add.</param>
        /// <param name="player">If this object is a player or not.</param>
        public GridSquare addGameObjectToList(GameObject obj, bool player)
        {
            if (!player) this.gameObjectList.Add(obj); //If it's not a player, add to the general object list.
            else this.playerGameObjectList.Add(obj); //If it's a player, add to the player object list.
            return this;
        }

        /// <summary>
        /// Gets the object list dictated by the player boolean passed in.
        /// </summary>
        /// <param name="player">A bool indicating if we need to retrieve the object list or player object list.</param>
        /// <returns></returns>
        public List<GameObject> getObjectList(bool player)
        {
            if(!player) return this.gameObjectList;
            return this.playerGameObjectList;
        }

        /// <summary>
        /// Removes a GameObject from this GridSquare.
        /// </summary>
        /// <param name="obj">The GameObject to remove.</param>
        /// <returns>True if the GameObject was found and removed, false otherwise.</returns>
        public bool removeGameObjectFromList(GameObject obj, bool player)
        {
            if (!player) return this.gameObjectList.Remove(obj);
            return this.playerGameObjectList.Remove(obj);
        }

        public bool checkWithinBounds(GameObject obj)
        {
            Vector2 objIndex = computeIndex(obj); //Get the index of this GameObject.
            if (objIndex.x == index.x && objIndex.y == index.y) return true; //If the GameObject's index matches this squares index, return true.
            return false; //Otherwise, return false.
        }
    }
}
