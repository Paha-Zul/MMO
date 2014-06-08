using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class FileParser {

    public static void parseFilters(Tree tree, string fileName)
    {
        var sr = new StreamReader(Application.dataPath + "/" + fileName);
        var fileContents = sr.ReadToEnd();
        sr.Close();

        //Splits by the \n character
        string[] lines = fileContents.Split('\n');

        bool f = false, sf = false, ssf = false;
        string currMain = "", currSub = "", currSubSub = "";


        foreach (string line in lines)
        {
            //Removes any trailing return carriages or newlines.
            string line2 = line.TrimEnd('\r', '\n');
            string[] parts = line2.Split(':');

            if ((line2 == "S:") == true)
            {
                f = true;
                sf = false;
                ssf = false;
                continue;
            }
            else if (line2 == "SB:")
            {
                f = false;
                sf = true;
                ssf = false;
                continue;
            }
            else if (line2 == "SBB:")
            {
                f = false;
                sf = false;
                ssf = true;
                continue;
            }

            if (f)
            {
                currMain = parts[0];
                List<string> temp = new List<string>();
                temp.Add(parts[1]);
                temp.Add("0"); //Buy orders
                temp.Add("0"); //Sell orders
                temp.Add("0"); //Version number
                tree.addNode("", currMain, temp);
            }
            else if (sf)
            {
                currSub = parts[0];
                List<string> temp = new List<string>();
                temp.Add(parts[1]);
                temp.Add("0"); //Buy orders
                temp.Add("0"); //Sell orders
                temp.Add("0"); //Version number
                tree.addNode(currMain, currSub, temp);
            }
            else if (ssf)
            {
                currSubSub = parts[0];
                List<string> temp = new List<string>();
                temp.Add(parts[1]);
                temp.Add("0"); //Buy orders
                temp.Add("0"); //Sell orders
                temp.Add("0"); //Version number
                tree.addNode(currMain + "," + currSub, currSubSub, temp);
            }
        }

       //Debug.Log(tree.printTree());
    }
}
