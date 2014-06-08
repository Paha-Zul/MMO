using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tree {
    public const int RAW_FILTER = 0, BUY_AMOUNT = 1, SELL_AMOUNT = 2, VERSION = 3;

    Node root;

    /// <summary>
    /// Creates a new tree with a root node.
    /// </summary>
    /// <param name="rootName">The name of the root.</param>
    public Tree(string rootName)
    {
        this.root = new Node(rootName, null);
    }

    /// <summary>
    /// Adds a new node to the tree.
    /// </summary>
    /// <param name="path">A string which is the path to the soon-to-be parent of the node being added. For example, path "Weapons,2H" with node
    /// "Swords" will result in the path "Weapons,2H,Swords".</param>
    /// <param name="name">The name of the new node.</param>
    public void addNode(string path, string name)
    {
        Node nodeToAddChildTo = this.getNode(path); //Gets the node using 'path'.
        Node child = new Node(name, nodeToAddChildTo); //Creates the new node.
        nodeToAddChildTo.addChild(child); //Adds the new node to the children of the node found earlier.
    }

    /// <summary>
    /// Adds a new node to the tree.
    /// </summary>
    /// <param name="path">A string which is the path to the soon-to-be parent of the node being added. For example, path "Weapons,2H" with node
    /// "Swords" will result in the path "Weapons,2H,Swords".</param>
    /// <param name="name">The name of the new node.</param>
    /// <param name="values">The values to assign to the new node.</param>
    public void addNode(string path, string name, List<string> values)
    {
        Node nodeToAddChildTo = this.getNode(path); //Gets the node using 'path'
        Node child = new Node(name, nodeToAddChildTo); //Creas the child node.
        child.setValues(values); //Assigns the values passed in.
        nodeToAddChildTo.addChild(child); //Adds the child to the node found.
    }

    /// <summary>
    /// Gets the node by using the path string passed in. The path string needs to have the path of node names including the child separated by commas.
    /// For example: Weapons,2H.Swords will result in getting the node with the name "Swords".
    /// </summary>
    /// <param name="path">The path to the node.</param>
    /// <returns>The node that was found. If the child could not be found, null is returned.</returns>
    public Node getNode(string path)
    {
        string[] path2 = path.Split(','); //Splits the string.
        Node currNode = root; //Starts at the root.

        //Traverses up the parents.
        foreach (string childName in path2)
        {
            if (childName == "") break;
            currNode = currNode.getChild(childName);
        }

        //If the last traversed node is the node we wanted, return it. Otherwise, return null.
        if (path2[0] == "" || currNode.getNodeName() == path2[path2.Length - 1]) return currNode;
        return null;
    }

    /// <summary>
    /// Gets the node by using the path string passed in and the index passed in. The index is used for getting a value from the Node's value list. 
    /// The path string needs to have the path of node values including the child separated by commas.
    /// For example:' weapon,2h.sword' (raw values) will result in getting the node with the value "sword".
    /// </summary>
    /// <param name="path">The path to the node.</param>
    /// <returns>The node that was found. If the child could not be found, null is returned.</returns>
    public Node getNode(string path, int index)
    {
        string[] path2 = path.Split(','); //Splits the string.
        Node currNode = root; //Starts at the root.

        //Traverses up the parents.
        foreach (string value in path2)
        {
            if (value == "") break;
            currNode = currNode.getChild(value, index);
        }

        //If the last traversed node is the node we wanted, return it. Otherwise, return null.
        if (path2[0] == "" || currNode.getValues()[index] == path2[path2.Length - 1]) return currNode;
        return null;
    }

    /// <summary>
    /// Adds a new value to the list of values in the node.
    /// </summary>
    /// <param name="path">The path to the node.</param>
    /// <param name="value">The value to add to the node.</param>
    public void addValueToNode(string path, string value)
    {
        Node node = this.getNode(path); //Get the node.
        node.addValue(value); //Add the value.
    }

    /// <summary>
    /// Gets all leaf nodes of the Tree starting at the root inorder (left to right).
    /// </summary>
    /// <returns>A List of Nodes that are leaf nodes.</returns>
    public List<Node> getAllLeafNodes()
    {
        List<Node> leafNodes = new List<Node>();
        Queue<Node> queue = new Queue<Node>();
        queue.Enqueue(this.root);

        while (queue.Count > 0) //While the queue is not empty
        {
            Node potentialLeaf = queue.Dequeue(); //Dequeue the head.
            if (potentialLeaf.isLeaf()) //If it is a leaf, add it to the leaf list and continue from the while loop.
            {
                leafNodes.Add(potentialLeaf);
                continue;
            }

            //If it wasn't a leaf, add all it's children to the queue.
            foreach(Node child in potentialLeaf.getChildren())
                queue.Enqueue(child);
        }
        return leafNodes;
    }

    public void SetAllNodesValue(int[] indices, string value)
    {
        Queue<Node> queue = new Queue<Node>();
        foreach (Node child in root.getChildren())
            queue.Enqueue(child);

        while (queue.Count > 0) //While the queue is not empty
        {
            Node potentialLeaf = queue.Dequeue(); //Dequeue the head.
            foreach(int index in indices)
                potentialLeaf.setValue(index, value); //Set the value.

            if (potentialLeaf.isLeaf()) //If it is a leaf, continue.
                continue;
            //If it wasn't a leaf, add all it's children to the queue.
            foreach (Node child in potentialLeaf.getChildren())
                queue.Enqueue(child);
        }
    }

    public void SetAllNodesValue(int index, string value)
    {
        SetAllNodesValue(new int[] { index }, value);
    }

    private string getNames(Node node)
    {
        string names = "";
        foreach (Node child in node.getChildren())
        {
            names += child.getNodeName()+" ";
            names += getNames(child);
        }
        return names;
    }

    public string printTree()
    {
        return getNames(this.root);
    }

    public Node getRootNode()
    {
        return this.root;
    }

    public class Node
    {
        string nodeName;
        Node parent;
        List<Node> children = new List<Node>();
        List<string> values = new List<string>();

        /// <summary>
        /// Instantiates a new node with a name and a parent node.
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <param name="parent">The parent node of this node.</param>
        public Node(string name, Node parent)
        {
            this.nodeName = name;
            this.parent = parent;
        }

        /// <summary>
        /// </summary>
        /// <returns>The name of the node.</returns>
        public string getNodeName()
        {
            return this.nodeName;
        }

        /// <summary>
        /// Adds a child Node to this Node.
        /// </summary>
        /// <param name="child">The child Node to add.</param>
        public void addChild(Node child)
        {
            this.children.Add(child);
        }

        /// <summary>
        /// Adds a value to this Node.
        /// </summary>
        /// <param name="value">The value to add.</param>
        public void addValue(string value)
        {
            this.values.Add(value);
        }

        /// <summary>
        /// Gets the list of values for this Node.
        /// </summary>
        /// <returns>A List<string> of the Node's values.</returns>
        public List<string> getValues()
        {
            return this.values;
        }

        /// <summary>
        /// Sets the value at 'index' to the 'value' passed in.
        /// </summary>
        /// <param name="index">The index to use for the list of values.</param>
        /// <param name="value">The string value to replace at index.</param>
        public void setValue(int index, string value)
        {
            this.values[index] = value;
        }

        /// <summary>
        /// Assigns the values of the Node.
        /// </summary>
        /// <param name="values">A List of string values.</param>
        public void setValues(List<string> values)
        {
            this.values = values;
        }

        /// <summary>
        /// </summary>
        /// <returns>A bool value indicating if this Node is a leaf or not (has children or does not).</returns>
        public bool isLeaf()
        {
            if(children.Count < 1)
                return true;
            return false;
        }

        /// <summary>
        /// </summary>
        /// <returns>The parent Node of this Node.</returns>
        public Node getParent()
        {
            return this.parent;
        }

        /// <summary>
        /// Gets a child of this Node by name.
        /// </summary>
        /// <param name="name">The name of the child Node.</param>
        /// <returns>The child Node if it was found, null otherwise.</returns>
        public Node getChild(string name)
        {
            foreach (Node child in children)
            {
                if (child.nodeName == name) return child;
            }

            return null;
        }

        /// <summary>
        /// Gets a child of this Node by a value at 'index' index.
        /// </summary>
        /// <param name="name">The name of the child Node.</param>
        /// <returns>The child Node if it was found, null otherwise.</returns>
        public Node getChild(string value, int index)
        {
            foreach (Node child in children)
            {
                if (child.values[index] == value) return child;
            }

            return null;
        }

        /// <summary>
        /// </summary>
        /// <returns>A List of Nodes which are the children of this Node.</returns>
        public List<Node> getChildren()
        {
            return this.children;
        }

        /// <summary>
        /// Compiles a string separated by commas in reverse of the Node and all parents values at the 'index' index.
        /// For example, calling this function on Swords which is under 2H which is under Weapons will using index 0 will
        /// result in the string "Weapons,2H,Swords".
        /// </summary>
        /// <param name="index">The index to use in gathering values.</param>
        /// <returns>A string separated by commas of all values from the Node to the root in reverse.</returns>
        public string getParentPathValues(int index)
        {
            string path = "";
            Node currNode = this;

            while (currNode != null)
            {
                if (currNode.parent != null && currNode.parent.parent != null)
                    if(path!= "") path = currNode.values[index] + "," + path;
                    else path = currNode.values[index];
                else if(currNode.parent != null)
                    path = currNode.values[index] + "," + path;

                currNode = currNode.parent;
            }

            return path;
        }

        /// <summary>
        /// Adds an integer amount to this Node. The index parameter is to allow the programmer to specify which value index
        /// the amount should be added to.
        /// </summary>
        /// <param name="amount">The amount to add to the node's existing value.</param>
        /// <param name="index">The index to add the value at.</param>
        /// <returns></returns>
        public bool addAmountToNode(int amount, int index)
        {
            this.getValues()[index] = (int.Parse(this.getValues()[index]) + amount).ToString(); //Parses the values and adds them together, and reassigns.

            trickleAmountUp(amount, index, this.getParent());
            return true;
        }

        /// <summary>
        /// This is a private method used internally by the tree. This will take an integer amount and add it to every parent of the passed in node recursively.
        /// </summary>
        /// <param name="amount">The amount to add to the node.</param>
        /// <param name="index">The index to at the value at.</param>
        /// <param name="nextNode">The next parent to add the node to.</param>
        private void trickleAmountUp(int amount, int index, Node nextNode)
        {
            //Since we don't want to add any values to the root node, this will stop the recursive calls at the node right before the root.
            if (nextNode.getParent() == null) return;

            nextNode.getValues()[index] = (int.Parse(nextNode.getValues()[index]) + amount).ToString(); //Parses the values and adds them together, and reassigns.
            trickleAmountUp(amount, index, nextNode.getParent()); //Recursively trickles up.
        }
    }
}
