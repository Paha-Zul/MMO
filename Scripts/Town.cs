using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Town : MonoBehaviour {
    public MarketVault marketVault;

	// Use this for initialization
	void Start () {
        marketVault = this.GetComponent<MarketVault>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public MarketVault getMarketVault()
    {
        return this.marketVault;
    }
}
