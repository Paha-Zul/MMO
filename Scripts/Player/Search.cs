using UnityEngine;
using System.Collections;

public class Search : MonoBehaviour {
    public string hitMessage;
    private Interface inter;
    private Player player;

	// Use this for initialization
	void Start () {
        this.inter = this.gameObject.GetComponent<Interface>();
        this.player = this.gameObject.GetComponent<Player>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        searchForInteractable();
        checkForKeyHits();
    }

    public void searchForInteractable()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        bool flag = false;
        if (Physics.Raycast(ray, out hit, 5))
        {
            flag = true;
            if (hit.collider.gameObject.CompareTag("Interactable"))
            {
                Interactable interScript = hit.collider.gameObject.GetComponent<Interactable>();
                if (interScript.interactableType == "market_public") this.hitMessage = "Press F to open the market window";
                else if (interScript.interactableType == "market_private") this.hitMessage = "Press F to open the market window";
                else if (interScript.interactableType == "vault_market") this.hitMessage = "Press F to open your market vault";
                else if (interScript.interactableType == "gatherable") this.hitMessage = "Press F to gather this resource";
                else this.hitMessage = "";

                if (Input.GetKeyDown(KeyCode.F))
                {
                    if (interScript.interactableType == "market_public")
                    {
                        Transform parent = hit.collider.gameObject.transform.parent;
                        //Traverses the parents until it hits the town.
                        while (!parent.CompareTag("Town"))
                            parent = parent.parent;
                        this.inter.toggleMarketInterface(parent.GetComponent<Market>());
                    }
                    else if (interScript.interactableType == "market_private")
                    {
                        this.inter.toggleMarketInterface(hit.collider.transform.parent.GetComponent<Market>());
                    }
                    else if (interScript.interactableType == "vault_market")
                    {
                        Transform parent = hit.collider.gameObject.transform.parent;
                        //Traverses the parents until it hits the town.
                        while (!parent.CompareTag("Town"))
                            parent = parent.parent;
                        this.inter.toggleWindow("MarketVault");
                        MarketVault vault = parent.GetComponent<MarketVault>();
                        vault.getItemsFromVault();
                        this.inter.setWindowTarget("MarketVault", vault.getInventory());
                    }
                    else if (interScript.interactableType == "gatherable")
                    {
                        hit.collider.GetComponent<Gatherable>().run(player);
                    }
                }
            }
            else
                this.hitMessage = "";
        }
        else
            hitMessage = "";

        if (!flag)
            if (Input.GetKeyDown(KeyCode.F))
                this.gameObject.GetComponent<Interface>().setMarketInterfaceActive(false, null);
    }

    public void checkForKeyHits()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            this.inter.toggleWindow("playerInv");
            this.inter.setWindowTarget("playerInv", player.inventory);
        }
    }
}
